/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// <para>
    /// Invoked immediate after the bounding boxes of all nodes have been
    /// computed and immediately before the reflow process begins. This
    /// hook must return TRUE if it made any changes to ensure that the
    /// parent node's bounding boxes are recomputed.
    /// </para>
    /// <para>
    /// Use this hook to perform any resize operations on the Bounds of the
    /// node, for example, manually resizing the node with certain constraints.
    /// </para>
    /// <remarks>
    /// This callback is only invoked if the node needs to reflow. This only
    /// happens if any property of this node or any of its dependencies has
    /// been modified that would affect its layout.
    /// </remarks>
    /// </summary>
    public ReflowDelegate? BeforeReflow;

    public delegate bool ReflowDelegate(Node node);

    /// <summary>
    /// <para>
    /// Whether to inherit tags from the parent node.
    /// </para>
    /// <para>
    /// This can be useful if the parent node is interactive and children have
    /// style definitions that are affected by the parent's interactivity tags,
    /// such as ":hover", ":active" and ":disabled".
    /// </para>
    /// </summary>
    /// <remarks>
    /// Custom tags are overwritten by the parent's tags for as long as this
    /// option is enabled.
    /// </remarks>
    public bool InheritTags {
        get => _inheritTags;
        set {
            if (_inheritTags.Equals(value)) return;
            _inheritTags = value;
            SignalReflow();
        }
    }

    private readonly Dictionary<Anchor.AnchorPoint, List<Node>> _anchorToChildNodes = [];
    private readonly Dictionary<Node, Anchor.AnchorPoint>       _childNodeToAnchor  = [];

    private bool  _inheritTags;
    private bool  _mustReflow = true;
    private Point _position   = new(0, 0);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Reflow(Point? position = null)
    {
        if (!ComputedStyle.IsVisible) return;
        if (Style.IsVisible is false) return;

        lock (Bounds) {
            if (_scaleFactor != ScaleFactor) {
                _scaleFactor = ScaleFactor;
                _mustReflow  = true;
            }

            if (_scaleAffectsBorders != ScaleAffectsBorders) {
                _scaleAffectsBorders = ScaleAffectsBorders;
                _mustReflow          = true;
            }

            if (_mustReflow) {
                if (!UseThreadedStyleComputation) {
                    InheritTagsFromParent();
                }

                ComputeBoundingBox();
                ComputeStretchedNodeSizes();
                InvokeReflowHook();
            }

            if (_mustReflow || _position != position) {
                _position = position ?? new(0, 0);
                ComputeBoundingRects(_position);
            }

            _mustReflow = false;
        }
    }

    /// <summary>
    /// Recomputes the size of this node. This method is typically used from
    /// a Reflow hook of another node to recompute the size of this node based
    /// on its child nodes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void RecomputeSize()
    {
        ComputeNodeSize(true);
        ComputeStretchedNodeSizes(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private bool InvokeReflowHook()
    {
        var changed = false;

        foreach (Node child in _childNodes) {
            bool result         = child.InvokeReflowHook();
            if (result) changed = true;
        }

        if (changed) ComputeNodeSize(true);

        return (BeforeReflow?.Invoke(this) ?? false) || changed;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void InheritTagsFromParent()
    {
        if (Style.IsVisible is false) return;

        if (_inheritTags && ParentNode is not null) {
            TagsList = ParentNode.TagsList;
        }

        foreach (Node child in _childNodes) {
            child.InheritTagsFromParent();
        }
    }

    #region Reflow Stage #1

    /// <summary>
    /// <para>
    /// Reflow stage #1: Compute the bounding box of this node.
    /// </para>
    /// <para>
    /// This method ensures that the <see cref="NodeBounds.ContentSize"/>,
    /// <see cref="NodeBounds.PaddingSize"/> and <see cref="NodeBounds.MarginSize"/>
    /// of this node are computed correctly, except for nodes that are supposed
    /// to be stretched (See <see cref="ComputedStyle.Stretch"/>).
    /// </para>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ComputeBoundingBox()
    {
        foreach (Node child in _childNodes) {
            if (child.ComputedStyle.IsVisible)
                child.ComputeBoundingBox();
        }

        ComputeNodeSize();
    }

    /// <summary>
    /// Computes the size of this node based on its own value and the size of
    /// the child nodes, if any.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ComputeNodeSize(bool force = false)
    {
        if (!force && !_mustReflow) return;
        if (Style.IsVisible is false) return;

        // Always compute the content size from text, regardless of whether the
        // node size is fixed or not, since this also prepares the text for
        // rendering later on.
        Size contentSize = ComputeContentSizeFromText();

        if (false == ComputedStyle.Size.IsFixed) {
            Size childSpan = ComputeContentSizeFromChildren();

            Bounds.ContentSize = new(
                Math.Max(contentSize.Width,  childSpan.Width),
                Math.Max(contentSize.Height, childSpan.Height)
            );
        }

        Bounds.PaddingSize = Bounds.ContentSize + ComputedStyle.Padding.Size;

        // Readjust the content size based on the configured size constraints.
        if (ComputedStyle.Size.Width > 0)
            Bounds.ContentSize.Width = Bounds.PaddingSize.Width = ComputedStyle.Size.Width;

        if (ComputedStyle.Size.Height > 0)
            Bounds.ContentSize.Height = Bounds.PaddingSize.Height = ComputedStyle.Size.Height;

        Bounds.MarginSize = Bounds.PaddingSize + ComputedStyle.Margin.Size;
    }

    /// <summary>
    /// Computes the content (inner) size of this node based on the size of
    /// its child nodes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private Size ComputeContentSizeFromChildren()
    {
        Size result = new();

        lock (_anchorToChildNodes) {
            foreach (List<Node> childNodes in _anchorToChildNodes.Values) {
                var width  = 0;
                var height = 0;

                foreach (Node childNode in childNodes) {
                    if (!childNode.ComputedStyle.IsVisible) continue;

                    switch (ComputedStyle.Flow) {
                        case Flow.Horizontal:
                            width  += childNode.OuterWidth + ComputedStyle.Gap;
                            height =  Math.Max(height, childNode.OuterHeight);
                            break;
                        case Flow.Vertical:
                            width  =  Math.Max(width, childNode.OuterWidth);
                            height += childNode.OuterHeight + ComputedStyle.Gap;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                switch (ComputedStyle.Flow) {
                    case Flow.Horizontal when width > 0:
                        width -= ComputedStyle.Gap;
                        break;
                    case Flow.Vertical when height > 0:
                        height -= ComputedStyle.Gap;
                        break;
                }

                result.Width  = Math.Max(result.Width,  width);
                result.Height = Math.Max(result.Height, height);
            }
        }

        return result;
    }

    #endregion

    #region Reflow Stage #2

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ComputeStretchedNodeSizes(bool recursive = true)
    {
        // Start depth-first traversal of the node tree.
        if (recursive) {
            foreach (Node child in _childNodes) {
                if (child.ComputedStyle.IsVisible == false) continue;
                child.ComputeStretchedNodeSizes();
            }
        }

        if (!ComputedStyle.Stretch || ParentNode is null) return;

        Size size = ParentNode!.Bounds.ContentSize;
        Size newContentSize;

        if (ParentNode.ComputedStyle.Flow == Flow.Horizontal) {
            newContentSize = new Size(Bounds.ContentSize.Width, size.Height) - ComputedStyle.Padding.Size;
        } else {
            newContentSize = new Size(size.Width, Bounds.ContentSize.Height) - ComputedStyle.Padding.Size;
        }

        Bounds.ContentSize = newContentSize;
        Bounds.PaddingSize = Bounds.ContentSize + ComputedStyle.Padding.Size;
        Bounds.MarginSize  = Bounds.PaddingSize + ComputedStyle.Margin.Size;
    }

    #endregion

    #region Reflow Stage #3

    /// <summary>
    /// Computes the bounding rectangles that define the position and size of
    /// each child node within this node recursively.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ComputeBoundingRects(Point position)
    {
        if (ComputedStyle.IsVisible is false) return;

        // Use own position if configured; required for overflow nodes.
        if (_position.X != 0 && _position.Y != 0) position = _position;

        Bounds.MarginRect.X1 = position.X;
        Bounds.MarginRect.Y1 = position.Y;
        Bounds.MarginRect.X2 = Bounds.MarginRect.X1 + OuterWidth;
        Bounds.MarginRect.Y2 = Bounds.MarginRect.Y1 + OuterHeight;

        Bounds.PaddingRect.X1 = Bounds.MarginRect.X1 + ComputedStyle.Margin.Left;
        Bounds.PaddingRect.Y1 = Bounds.MarginRect.Y1 + ComputedStyle.Margin.Top;
        Bounds.PaddingRect.X2 = Bounds.MarginRect.X2 - ComputedStyle.Margin.Right;
        Bounds.PaddingRect.Y2 = Bounds.MarginRect.Y2 - ComputedStyle.Margin.Bottom;

        Bounds.ContentRect.X1 = Bounds.PaddingRect.X1 + ComputedStyle.Padding.Left;
        Bounds.ContentRect.Y1 = Bounds.PaddingRect.Y1 + ComputedStyle.Padding.Top;
        Bounds.ContentRect.X2 = Bounds.PaddingRect.X2 - ComputedStyle.Padding.Right;
        Bounds.ContentRect.Y2 = Bounds.PaddingRect.Y2 - ComputedStyle.Padding.Bottom;

        UpdateParentBounds();

        int originX = Bounds.ContentRect.X1;
        int originY = Bounds.ContentRect.Y1;

        lock (_anchorToChildNodes) {
            foreach (Anchor.AnchorPoint anchorPoint in _anchorToChildNodes.Keys) {
                List<Node> childNodes     = _anchorToChildNodes[anchorPoint];
                Size       maxChildSize   = GetMaxSizeOfChildren(childNodes);
                Size       totalChildSize = GetTotalSizeOfChildren(childNodes);
                Anchor     anchor         = new(anchorPoint);

                int x = originX;
                int y = originY;

                if (anchor.IsCenter) {
                    x += InnerWidth / 2
                        - (ComputedStyle.Flow == Flow.Horizontal ? totalChildSize.Width : maxChildSize.Width) / 2;
                }

                if (anchor.IsRight) x += InnerWidth;

                if (anchor.IsMiddle) {
                    y += (InnerHeight / 2)
                        - (ComputedStyle.Flow == Flow.Horizontal ? maxChildSize.Height : totalChildSize.Height) / 2;
                }

                if (anchor.IsBottom) y += Height;

                Node lastNode = childNodes[^1];

                foreach (Node childNode in childNodes) {
                    if (!childNode.IsVisible) continue;

                    var xOffset = 0;
                    var yOffset = 0;

                    if (anchor.IsMiddle) {
                        yOffset = ((maxChildSize.Height - childNode.OuterHeight) / 2) - ComputedStyle.Padding.Top;
                    } else if (anchor.IsBottom) {
                        yOffset -= (childNode.OuterHeight) + ComputedStyle.Padding.VerticalSize;
                    }

                    if (anchor.IsCenter) {
                        xOffset = -ComputedStyle.Padding.Left;
                    } else if (anchor.IsRight) {
                        xOffset -= (childNode.OuterWidth) + ComputedStyle.Padding.HorizontalSize;
                    }

                    childNode.ComputeBoundingRects(new(x + xOffset, y + yOffset));

                    if (childNode == lastNode) break;

                    switch (ComputedStyle.Flow) {
                        case Flow.Horizontal:
                            x = anchor.IsRight
                                ? x - childNode.OuterWidth
                                : x + childNode.OuterWidth;

                            if (lastNode != childNode) {
                                x += anchor.IsRight ? -ComputedStyle.Gap : ComputedStyle.Gap;
                            }

                            break;
                        case Flow.Vertical:
                            y = anchor.IsBottom
                                ? y - childNode.OuterHeight
                                : y + childNode.OuterHeight;

                            if (lastNode != childNode) {
                                y += anchor.IsTop ? ComputedStyle.Gap : -ComputedStyle.Gap;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown flow direction '{ComputedStyle.Flow}'.");
                    }
                }
            }
        }

        _mustReflow = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private Size GetMaxSizeOfChildren(IReadOnlyCollection<Node> nodes)
    {
        if (nodes.Count == 0) return new();

        var width  = 0;
        var height = 0;

        foreach (var node in nodes) {
            if (!node.IsVisible) continue;
            width  = Math.Max(width,  node.OuterWidth);
            height = Math.Max(height, node.OuterHeight);
        }

        if (ComputedStyle.Flow == Flow.Horizontal) {
            width += ComputedStyle.Gap * Math.Max(0, nodes.Count - 1);
        } else {
            height += ComputedStyle.Gap * Math.Max(0, nodes.Count - 1);
        }

        return new(width, height);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private Size GetTotalSizeOfChildren(IReadOnlyCollection<Node> nodes)
    {
        if (nodes.Count == 0) return new();

        var width  = 0;
        var height = 0;

        foreach (var node in nodes) {
            if (!node.IsVisible) continue;
            width  += node.OuterWidth;
            height += node.OuterHeight;
        }

        if (ComputedStyle.Flow == Flow.Horizontal) {
            width += ComputedStyle.Gap * Math.Max(0, nodes.Count - 1);
        } else {
            height += ComputedStyle.Gap * Math.Max(0, nodes.Count - 1);
        }

        return new(width, height);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ReassignAnchorNodes()
    {
        lock (_childNodeToAnchor) {
            lock (_anchorToChildNodes) {
                lock (_childNodes) {
                    _childNodeToAnchor.Clear();
                    _anchorToChildNodes.Clear();

                    foreach (Node child in _childNodes) {
                        if (child.ComputedStyle.Anchor == Anchor.AnchorPoint.None) continue;

                        if (!_anchorToChildNodes.ContainsKey(child.ComputedStyle.Anchor.Point)) {
                            _anchorToChildNodes[child.ComputedStyle.Anchor.Point] = new();
                        }

                        _anchorToChildNodes[child.ComputedStyle.Anchor.Point].Add(child);
                        _childNodeToAnchor[child] = child.ComputedStyle.Anchor.Point;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void UpdateParentBounds()
    {
        if (ParentNode is not null) return;

        var offsetX = 0;
        var offsetY = 0;

        if (ComputedStyle.Anchor.IsCenter) {
            offsetX = Bounds.MarginSize.Width / 2;
        } else if (ComputedStyle.Anchor.IsRight) {
            offsetX = Bounds.MarginSize.Width;
        }

        if (ComputedStyle.Anchor.IsMiddle) {
            offsetY = Bounds.MarginSize.Height / 2;
        } else if (ComputedStyle.Anchor.IsBottom) {
            offsetY = Bounds.MarginSize.Height;
        }

        Bounds.MarginRect.X1 -= offsetX;
        Bounds.MarginRect.Y1 -= offsetY;
        Bounds.MarginRect.X2 -= offsetX;
        Bounds.MarginRect.Y2 -= offsetY;

        Bounds.PaddingRect.X1 -= offsetX;
        Bounds.PaddingRect.Y1 -= offsetY;
        Bounds.PaddingRect.X2 -= offsetX;
        Bounds.PaddingRect.Y2 -= offsetY;

        Bounds.ContentRect.X1 -= offsetX;
        Bounds.ContentRect.Y1 -= offsetY;
        Bounds.ContentRect.X2 -= offsetX;
        Bounds.ContentRect.Y2 -= offsetY;
    }
}
