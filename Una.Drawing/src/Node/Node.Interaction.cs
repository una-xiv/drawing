/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using ImGuiNET;

namespace Una.Drawing;

public partial class Node
{
    public event Action<Node>? OnClick;
    public event Action<Node>? OnMiddleClick;
    public event Action<Node>? OnRightClick;
    public event Action<Node>? OnMouseEnter;
    public event Action<Node>? OnMouseLeave;
    public event Action<Node>? OnMouseDown;
    public event Action<Node>? OnMouseUp;
    public event Action<Node>? OnDelayedMouseEnter;

    /// <summary>
    /// True if the element has any interactive event listeners attached to it.
    /// </summary>
    public bool IsInteractive =>
        null != Tooltip
        || null != OnClick
        || null != OnMiddleClick
        || null != OnRightClick
        || null != OnMouseEnter
        || null != OnMouseLeave
        || null != OnMouseUp;

    /// <summary>
    /// True if the mouse cursor is currently inside the bounding box of the element.
    /// </summary>
    public bool IsMouseOver { get; private set; }

    /// <summary>
    /// True if one of the mouse buttons in held down while the cursor is over the element.
    /// </summary>
    public bool IsMouseDown { get; private set; }
    public bool IsMiddleMouseDown { get; private set; }
    public bool IsRightMouseDown { get; private set; }

    /// <summary>
    /// True if this element currently has focus.
    /// </summary>
    public bool IsFocused { get; private set; }

    /// <summary>
    /// True if this node cannot be interacted with, regardless of bound event
    /// listeners.
    /// </summary>
    public bool IsDisabled { get; set; }

    private bool _isInWindowOrInteractiveParent;
    private bool _didStartInteractive;
    private bool _didStartDelayedMouseEnter;
    private long _mouseOverStartTime;
    private long _isVisibleSince;

    private void SetupInteractive(ImDrawListPtr drawList)
    {
        _didStartInteractive = false;

        switch (IsDisabled) {
            case true when !_tagsList.Contains("disabled"):
                _tagsList.Add("disabled");
                break;
            case false when _tagsList.Contains("disabled"):
                _tagsList.Remove("disabled");
                break;
        }

        if (IsDisabled || !IsInteractive || !IsVisible) return;

        if (_isVisibleSince == 0) _isVisibleSince = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        // Debounce the interactive state to prevent unintentional clicks
        // when toggling visibility on elements on the same position.
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _isVisibleSince < 100) return;

        _didStartInteractive = true;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,    Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,     Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,  0);

        string  uuid              = GetInteractiveId();
        Vector2 boundingBoxSize   = Bounds.PaddingSize.ToVector2();
        Node?   interactiveParent = GetInteractiveParent();
        _isInWindowOrInteractiveParent = IsInWindowDrawList(drawList) || interactiveParent != null;

        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

        if (_isInWindowOrInteractiveParent) {
            ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
            ImGui.BeginChild(uuid, boundingBoxSize, false, InteractiveWindowFlags);
            ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
        } else {
            ImGui.SetNextWindowPos(Bounds.PaddingRect.TopLeft, ImGuiCond.Always);
            ImGui.SetNextWindowSize(boundingBoxSize, ImGuiCond.Always);
            ImGui.Begin(uuid, InteractiveWindowFlags);
        }

        bool wasHovered = IsMouseOver;

        ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
        ImGui.InvisibleButton($"{uuid}##Button", Bounds.PaddingSize.ToVector2());
        IsMouseOver = ImGui.IsItemHovered();
        IsFocused   = ImGui.IsItemFocused();

        switch (IsMouseOver) {
            case true when !_tagsList.Contains("hover"):
                _tagsList.Add("hover");
                break;
            case false when _tagsList.Contains("hover"):
                _tagsList.Remove("hover");
                break;
        }

        switch (IsFocused) {
            case true when !_tagsList.Contains("focus"):
                _tagsList.Add("focus");
                break;
            case false when _tagsList.Contains("focus"):
                _tagsList.Remove("focus");
                break;
        }

        switch (IsMouseDown) {
            case true when !_tagsList.Contains("active"):
                _tagsList.Add("active");
                break;
            case false when _tagsList.Contains("active"):
                _tagsList.Remove("active");
                break;
        }

        if (Tooltip != null && IsMouseOver && _mouseOverStartTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - 500) {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,  new Vector2(8, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6);
            ImGui.PushStyleColor(ImGuiCol.Border,   0xFF3F3F3F);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF252525);
            ImGui.PushStyleColor(ImGuiCol.Text,     0xFFCACACA);
            ImGui.BeginTooltip();
            ImGui.SetCursorPos(new(8, 4));
            ImGui.TextUnformatted(Tooltip);
            ImGui.EndTooltip();
            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(2);
        }

        switch (wasHovered) {
            case false when IsMouseOver:
                OnMouseEnter?.Invoke(this);
                _mouseOverStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                break;
            case true when !IsMouseOver:
                OnMouseLeave?.Invoke(this);
                _didStartDelayedMouseEnter = false;
                IsMouseDown                = false;
                IsMiddleMouseDown          = false;
                IsRightMouseDown           = false;
                break;
        }

        if (IsMouseOver) {
            if (_mouseOverStartTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - 50) {
                if (!_didStartDelayedMouseEnter) {
                    OnDelayedMouseEnter?.Invoke(this);
                    _didStartDelayedMouseEnter = true;
                }
            }

            if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) {
                if (!IsMouseDown) {
                    OnMouseDown?.Invoke(this);
                    OnClick?.Invoke(this);
                    IsMouseDown = true;
                }
            } else if (ImGui.IsMouseDown(ImGuiMouseButton.Middle)) {
                if (!IsMiddleMouseDown) {
                    OnMouseDown?.Invoke(this);
                    OnMiddleClick?.Invoke(this);
                    IsMiddleMouseDown = true;
                }
            } else if (ImGui.IsMouseDown(ImGuiMouseButton.Right)) {
                if (!IsRightMouseDown) {
                    OnMouseDown?.Invoke(this);
                    OnRightClick?.Invoke(this);
                    IsRightMouseDown = true;
                }
            } else {
                if (IsMouseDown) {
                    OnMouseUp?.Invoke(this);
                    IsMouseDown = false;
                }
                if (IsMiddleMouseDown) {
                    IsMiddleMouseDown = false;
                }
                if (IsRightMouseDown) {
                    IsRightMouseDown = false;
                }
            }
        }
    }

    private void EndInteractive()
    {
        if (!_didStartInteractive) return;

        if (_isInWindowOrInteractiveParent) {
            ImGui.EndChild();
        } else {
            ImGui.End();
        }

        ImGui.PopStyleVar(4);
    }

    /// <summary>
    /// Returns the top-most interactive parent element.
    /// </summary>
    /// <returns></returns>
    private Node? GetInteractiveParent()
    {
        Node? immediateParent   = ParentNode;
        Node? interactiveParent = null;

        while (immediateParent != null) {
            if (immediateParent.IsInteractive) interactiveParent = immediateParent;
            immediateParent = immediateParent.ParentNode;
        }

        return interactiveParent;
    }

    /// <summary>
    /// Returns true if the given drawList is not the foreground or background drawList.
    /// </summary>
    private static unsafe bool IsInWindowDrawList(ImDrawListPtr drawList)
    {
        return
            drawList.NativePtr != ImGui.GetForegroundDrawList().NativePtr
            && drawList.NativePtr != ImGui.GetBackgroundDrawList().NativePtr;
    }

    private static ImGuiWindowFlags InteractiveWindowFlags =>
        ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoBackground
        | ImGuiWindowFlags.NoDecoration
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoScrollWithMouse
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoDocking
        | ImGuiWindowFlags.NoNavFocus
        | ImGuiWindowFlags.NoNavInputs
        | ImGuiWindowFlags.NoFocusOnAppearing;

    private string? _interactiveId;

    private string GetInteractiveId()
    {
        if (null != _interactiveId) return _interactiveId;

        Guid guid = Guid.NewGuid();
        _interactiveId = guid.ToString();

        return _interactiveId;
    }
}
