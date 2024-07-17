/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using Una.Drawing.Texture;

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// <para>
    /// A callback that is invoked before the node is drawn.
    /// </para>
    /// <para>
    /// This method is guaranteed to be called before the node is drawn,
    /// regardless of whether the node is visible or not, or if it needs to
    /// reflow.
    /// </para>
    /// </summary>
    public Action<Node>? BeforeDraw;

    /// <summary>
    /// <para>
    /// A callback that is invoked after the node is drawn.
    /// </para>
    /// <para>
    /// This method is guaranteed to be called before the node is drawn,
    /// regardless of whether the node is visible or not, or if it needs to
    /// reflow.
    /// </para>
    /// </summary>
    public Action<Node>? AfterDraw;

    /// <summary>
    /// Whether children of this node should overflow the bounds of the node.
    /// Defaults to <c>true</c>. Settings this to false will clip children to
    /// the bounds of the node and show a scrollbar if necessary.
    /// </summary>
    /// <remarks>
    /// A side effect of setting this to <c>false</c> is that the node will
    /// render its children in a separate draw list. Child nodes that are not
    /// visible due to them being outside the bounds of the parent node will
    /// be set to "invisible" and will not be drawn. This may cause issues
    /// if the child nodes have their visibility toggled manually.
    /// </remarks>
    public bool Overflow { get; set; } = true;

    /// <summary>
    /// Shows a horizontal scrollbar if the children exceed the width of the
    /// bounds of the node. Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="Overflow"/> is set to <c>false</c>.
    /// </remarks>
    public bool HorizontalScrollbar { get; set; } = false;

    /// <summary>
    /// Represents the current horizontal scroll position of the node.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="Overflow"/> is set to <c>false</c>.
    /// </remarks>
    public uint ScrollX { get; private set; }

    /// <summary>
    /// Represents the current vertical scroll position of the node.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="Overflow"/> is set to <c>false</c>.
    /// </remarks>
    public uint ScrollY { get; private set; }

    /// <summary>
    /// Represents the current width of the scroll area of the node.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="Overflow"/> is set to <c>false</c>.
    /// </remarks>
    public uint ScrollWidth { get; private set; }

    /// <summary>
    /// Represents the current height of the scroll area of the node.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="Overflow"/> is set to <c>false</c>.
    /// </remarks>
    public uint ScrollHeight { get; private set; }

    private static uint _globalInstanceId;
    private        uint InstanceId { get; } = _globalInstanceId++;
    private        uint _colorThemeVersion;

    private          IDalamudTextureWrap? _texture;
    private          NodeSnapshot         _snapshot;
    private readonly List<ImDrawListPtr>  _drawLists = [];

    ~Node()
    {
        Dispose();
    }

    public void Render(ImDrawListPtr drawList, Point position, bool forceSynchronousStyleComputation = false)
    {
        if (ParentNode is not null)
            throw new InvalidOperationException("Cannot render a node that has a parent or is not a root node.");

        if (!_mustReflow && _hasComputedStyle && !forceSynchronousStyleComputation && UseThreadedStyleComputation) {
            Task.Run(ComputeStyle);
        } else {
            ComputeStyle();
        }

        if (!_isUpdatingStyle) {
            ComputedStyle = _intermediateStyle;
        }

        lock (_lockObject) {
            Reflow(position);
            Draw(drawList);
        }
    }

    /// <summary>
    /// Returns true if this node is visible and its bounds are not empty.
    /// </summary>
    public bool IsVisible => ComputedStyle.IsVisible && !Bounds.PaddingSize.IsZero;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Draw(ImDrawListPtr drawList)
    {
        BeforeDraw?.Invoke(this);

        if (Color.ThemeVersion != _colorThemeVersion) {
            _colorThemeVersion = Color.ThemeVersion;
            _texture?.Dispose();
            _texture           = null;
        }

        if (Style.IsVisible is false) return;

        if (!IsVisible) {
            _isVisibleSince = 0;
            return;
        }

        if (_isVisibleSince == 0) _isVisibleSince = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        PushDrawList(drawList);
        BeginOverflowContainer();
        SetupInteractive(drawList);

        if (UpdateTexture()) RenderShadow(drawList);

        if (null != _texture) {
            drawList.AddImage(
                _texture.ImGuiHandle,
                Bounds.PaddingRect.TopLeft,
                Bounds.PaddingRect.BottomRight,
                Vector2.Zero,
                Vector2.One,
                GetRenderColor()
            );
        }

        DrawDebugBounds();

        var childDrawList = _drawLists.Last();

        OnDraw(childDrawList);

        foreach (var childNode in ChildNodes) {
            childNode.Draw(childDrawList);
        }

        EndInteractive();
        EndOverflowContainer();

        AfterDraw?.Invoke(this);

        PopDrawList();
    }

    private bool UpdateTexture()
    {
        if (NodeValue is null && !ComputedStyle.HasDrawables()) {
            _consecutiveRedraws = 0;
            return false;
        }

        NodeSnapshot snapshot = CreateSnapshot();

        if ((_texture is null || !snapshot.Equals(ref _snapshot)) && Width > 0 && Height > 0) {
            _texture?.Dispose();
            _texture  = Renderer.CreateTexture(this);
            _snapshot = snapshot;
            _consecutiveRedraws++;
        } else {
            _consecutiveRedraws = 0;
        }

        if (_consecutiveRedraws > 30) {
            DebugLogger.Log(
                $"WARNING: Node {this} is redrawing on every frame (value={_nodeValue}). Please check for unnecessary state changes."
            );

            _consecutiveRedraws = 0;
        }

        return true;
    }

    private int _consecutiveRedraws;

    /// <summary>
    /// Allows the node to draw custom content on the node's draw list.
    /// </summary>
    protected virtual void OnDraw(ImDrawListPtr drawList) { }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void BeginOverflowContainer()
    {
        if (Overflow) return;

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,      new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,     new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,   0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,     ComputedStyle.BorderRadius);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,     ComputedStyle.BorderRadius);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize,     10);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize,   0);

        ImGui.PushStyleColor(ImGuiCol.FrameBg,              0);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg,          ComputedStyle.ScrollbarTrackColor.ToUInt());
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab,        ComputedStyle.ScrollbarThumbColor.ToUInt());
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive,  ComputedStyle.ScrollbarThumbActiveColor.ToUInt());
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, ComputedStyle.ScrollbarThumbHoverColor.ToUInt());

        ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);

        ImGui.BeginChildFrame(
            InstanceId,
            Bounds.PaddingSize.ToVector2() + new Vector2(1, 0),
            (HorizontalScrollbar ? ImGuiWindowFlags.HorizontalScrollbar : ImGuiWindowFlags.None)
        );

        PushDrawList(ImGui.GetWindowDrawList());
        ImGui.SetCursorPos(Vector2.Zero);

        var scrollX      = (uint)ImGui.GetScrollX();
        var scrollY      = (uint)ImGui.GetScrollY();
        var scrollWidth  = (uint)ImGui.GetScrollMaxX();
        var scrollHeight = (uint)ImGui.GetScrollMaxY();

        ScrollX      = scrollX;
        ScrollY      = scrollY;
        ScrollWidth  = scrollWidth;
        ScrollHeight = scrollHeight;

        Vector2 pos = ImGui.GetCursorScreenPos();

        foreach (var child in _childNodes) {
            child.ComputeBoundingRects(new((int)pos.X, (int)pos.Y));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EndOverflowContainer()
    {
        if (Overflow) return;

        var totalSize = GetTotalSizeOfChildren(_childNodes);
        var maxSize   = GetMaxSizeOfChildren(_childNodes);

        Vector2 size = new(
            ComputedStyle.Flow == Flow.Horizontal ? totalSize.Width : maxSize.Width,
            ComputedStyle.Flow == Flow.Vertical ? totalSize.Height : maxSize.Height
        );

        ImGui.SetCursorPos(size);
        ImGui.EndChildFrame();
        ImGui.PopStyleVar(8);
        ImGui.PopStyleColor(5);

        PopDrawList();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private NodeSnapshot CreateSnapshot()
    {
        return new() {
            Width       = OuterWidth,
            Height      = OuterHeight,
            ValueWidth  = NodeValueMeasurement?.Size.Width ?? 0,
            ValueHeight = NodeValueMeasurement?.Size.Height ?? 0,
            Layout      = ComputedStyle.LayoutStyleSnapshot,
            Paint       = ComputedStyle.PaintStyleSnapshot
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private uint GetRenderColor()
    {
        float opacity = ComputedStyle.Opacity;

        Node? parent = ParentNode;

        while (parent is not null
               && opacity > 0.0f) {
            opacity *= parent.ComputedStyle.Opacity;
            parent  =  parent.ParentNode;
        }

        opacity = Math.Clamp(opacity, 0.0f, 1.0f);

        return (uint)(opacity * 255) << 24 | 0x00FFFFFF;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void RenderShadow(ImDrawListPtr drawList)
    {
        if (ComputedStyle.ShadowSize.IsZero) return;

        uint color = GetRenderColor();
        if (color == 0) return;

        var rect = Bounds.PaddingRect.Copy();

        if (ComputedStyle.ShadowInset > 0) rect.Shrink(new(ComputedStyle.ShadowInset));

        rect.X1 += (int)ComputedStyle.ShadowOffset.X;
        rect.Y1 += (int)ComputedStyle.ShadowOffset.Y;
        rect.X2 += (int)ComputedStyle.ShadowOffset.X;
        rect.Y2 += (int)ComputedStyle.ShadowOffset.Y;

        const float uv0 = 0.0f;
        const float uv1 = 0.333333f;
        const float uv2 = 0.666666f;
        const float uv3 = 1.0f;

        ImDrawListPtr dl   = drawList;
        IntPtr        id   = TextureLoader.GetEmbeddedTexture("Shadow.png").ImGuiHandle;
        Vector2       p    = rect.TopLeft;
        Vector2       s    = new(rect.Width, rect.Height);
        Vector2       m    = new(p.X + s.X, p.Y + s.Y);
        EdgeSize      side = ComputedStyle.ShadowSize;

        if (IsInWindowDrawList(dl)) dl.PushClipRectFullScreen();

        if (side.Top > 0 || side.Left > 0)
            dl.AddImage(id, new(p.X - side.Left, p.Y - side.Top), new(p.X, p.Y), new(uv0, uv0), new(uv1, uv1), color);

        if (side.Top > 0)
            dl.AddImage(id, p with { Y = p.Y - side.Top }, new(m.X, p.Y), new(uv1, uv0), new(uv2, uv1), color);

        if (side.Top > 0 || side.Right > 0)
            dl.AddImage(
                id,
                m with { Y = p.Y - side.Top },
                p with { X = m.X + side.Right },
                new(uv2, uv0),
                new(uv3, uv1),
                color
            );

        if (side.Left > 0)
            dl.AddImage(id, p with { X = p.X - side.Left }, new(p.X, m.Y), new(uv0, uv1), new(uv1, uv2), color);

        if (side.Right > 0)
            dl.AddImage(id, new(m.X, p.Y), m with { X = m.X + side.Right }, new(uv2, uv1), new(uv3, uv2), color);

        if (side.Bottom > 0 || side.Left > 0)
            dl.AddImage(
                id,
                m with { X = p.X - side.Left },
                p with { Y = m.Y + side.Bottom },
                new(uv0, uv2),
                new(uv1, uv3),
                color
            );

        if (side.Bottom > 0)
            dl.AddImage(id, new(p.X, m.Y), m with { Y = m.Y + side.Bottom }, new(uv1, uv2), new(uv2, uv3), color);

        if (side.Bottom > 0 || side.Right > 0)
            dl.AddImage(
                id,
                new(m.X, m.Y),
                new(m.X + side.Right, m.Y + side.Bottom),
                new(uv2, uv2),
                new(uv3, uv3),
                color
            );

        if (IsInWindowDrawList(drawList)) dl.PopClipRect();
    }

    private void PushDrawList(ImDrawListPtr drawList)
    {
        _drawLists.Add(drawList);
    }

    private void PopDrawList()
    {
        _drawLists.RemoveAt(_drawLists.Count - 1);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NodeSnapshot
{
    internal int                 Width;
    internal int                 Height;
    internal int                 ValueWidth;
    internal int                 ValueHeight;
    internal LayoutStyleSnapshot Layout;
    internal PaintStyleSnapshot  Paint;

    internal bool Equals(ref readonly NodeSnapshot other)
    {
        return MemoryMarshal
            .AsBytes(new ReadOnlySpan<NodeSnapshot>(in this))
            .SequenceEqual(MemoryMarshal.AsBytes(new ReadOnlySpan<NodeSnapshot>(in other)));
    }
}
