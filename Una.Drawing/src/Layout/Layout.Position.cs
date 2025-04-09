using System.Linq;

namespace Una.Drawing;

internal static partial class Layout
{
    private static void ComputePositions(Node root)
    {
        lock (root.AnchorToChildNodes) {
            foreach (var anchorPoint in root.AnchorToChildNodes.Keys
                                            .Where(anchorPoint => root.AnchorToChildNodes[anchorPoint].Count > 0)
            ) {
                PositionNodesWithSameAnchor(anchorPoint, root, root.AnchorToChildNodes[anchorPoint]);
            }
        }
    }

    private static void PositionNodesWithSameAnchor(Anchor anchor, Node root, List<Node> children)
    {
        if (children.Count == 0) return;

        FlowOrder order = root.ComputedStyle.FlowOrder;
        Flow      flow  = root.ComputedStyle.Flow;

        int firstIndex = order is FlowOrder.Normal ? 0 : children.Count - 1;
        int lastIndex  = order is FlowOrder.Normal ? children.Count - 1 : 0;
        int step       = (order is FlowOrder.Normal ? 1 : -1);
        
        while (firstIndex < children.Count && !children[firstIndex].ComputedStyle.IsVisible) {
            firstIndex += step;
        }
        
        while (lastIndex >= 0 && !children[lastIndex].ComputedStyle.IsVisible) {
            lastIndex -= step;
        }
        
        // If all children are invisible, return.
        if (firstIndex > lastIndex) return;
        
        (float originX, float originY) = GetNodeOrigin(anchor, root, children);

        int   gap = root.ComputedStyle.Gap;
        float x   = originX, y = originY;

        float center = 0f;
        float middle = 0f;

        if (flow == Flow.Horizontal) {
            if (anchor.IsCenter) {
                center = GetCenter(root);
                x      = center - (GetChildrenWidth(root, children) / 2f);
            }
        } else {
            if (anchor.IsCenter) {
                center = GetCenter(root);
                x      = center - (children[firstIndex].OuterWidth / 2f);
            }
        }

        if (anchor.IsTop) {
            y = originY;
        } else if (anchor.IsBottom) {
            if (flow == Flow.Vertical) {
                y = originY - GetChildrenHeight(root, children);
            } else {
                y = originY - children[firstIndex].OuterHeight;
            }
        } else if (anchor.IsMiddle) {
            middle = GetMiddle(root);
            if (flow == Flow.Horizontal) {
                y = middle - (children[firstIndex].OuterHeight / 2f);
            } else {
                y = middle - (GetChildrenHeight(root, children) / 2f);
            }
        }

        for (var i = firstIndex; i <= lastIndex; i += step) {
            Node node = children[i];

            if (node.IsDisposed || !node.IsVisible) continue;

            // Prohibit sub-pixel positioning to prevent blurry images.
            x = MathF.Round(x);
            y = MathF.Round(y);

            node.Bounds.MarginRect = new Rect(x, y, node.Bounds.MarginSize);
            node.Bounds.PaddingRect = new Rect(
                x + node.ComputedStyle.Margin.Left,
                y + node.ComputedStyle.Margin.Top,
                node.Bounds.PaddingSize
            );

            node.Bounds.ContentRect = new Rect(
                x + node.ComputedStyle.Margin.Left + node.ComputedStyle.Padding.Left,
                y + node.ComputedStyle.Margin.Top + node.ComputedStyle.Padding.Top,
                node.Bounds.ContentSize
            );

            if (i == lastIndex) {
                ComputePositions(node);
                break;
            }

            Node nextNode = children[i + step];
            while (!nextNode.ComputedStyle.IsVisible || i == lastIndex) {
                i += step;
                if (i == lastIndex) break;
                nextNode = children[i + step];
            }

            switch (flow) {
                case Flow.Vertical:
                    // X-axis
                    if (anchor.IsLeft) {
                        x = originX;
                    } else if (anchor.IsCenter) {
                        x = center - (nextNode.OuterWidth / 2f);
                    } else if (anchor.IsRight) {
                        x = originX - nextNode.OuterWidth;
                    }

                    // Y-axis
                    y += node.OuterHeight + gap;
                    break;
                case Flow.Horizontal:
                default:
                    // X-axis
                    x += node.OuterWidth + gap;

                    // Y-axis
                    if (anchor.IsTop) {
                        y = originY;
                    } else if (anchor.IsMiddle) {
                        y = middle - (nextNode.OuterHeight / 2f);
                    } else if (anchor.IsBottom) {
                        y = originY - nextNode.OuterHeight;
                    }

                    break;
            }

            ComputePositions(node);
        }
    }

    private static (float, float) GetNodeOrigin(Anchor anchor, Node root, List<Node> children)
    {
        float x = 0;
        float y = 0;

        if (anchor.IsLeft) {
            x = root.Bounds.ContentRect.X1;
        } else if (anchor.IsRight) {
            x = root.Bounds.ContentRect.X2 - GetChildrenWidth(root, children);
        } else if (anchor.IsCenter) {
            x = GetCenterStart(root, children);
        }

        if (anchor.IsTop) {
            y = root.Bounds.ContentRect.Y1;
        } else if (anchor.IsBottom) {
            y = root.Bounds.ContentRect.Y2;
        } else if (anchor.IsMiddle) {
            y = GetMiddle(root);
        }

        return (x, y);
    }

    private static float GetCenterStart(Node root, List<Node> children)
    {
        return root.Bounds.ContentRect.X1 + Math.Abs(
            (int)Math.Ceiling((root.InnerWidth / 2f) - (GetChildrenWidth(root, children) / 2f)));
    }

    private static float GetCenter(Node root)
    {
        return root.Bounds.ContentRect.X1 + MathF.Ceiling(root.Bounds.ContentSize.Width / 2f);
    }

    private static float GetMiddle(Node root)
    {
        return root.Bounds.ContentRect.Y1 + MathF.Ceiling(root.Bounds.ContentSize.Height / 2f);
    }

    private static float GetChildrenWidth(Node root, List<Node> children)
    {
        var visibleChildren = children.Where(node => node is { IsDisposed: false, ComputedStyle.IsVisible: true });
        var enumerable      = visibleChildren as Node[] ?? visibleChildren.ToArray();

        float width = enumerable.Sum(node => node.OuterWidth);

        if (root.ComputedStyle.Flow == Flow.Horizontal) {
            width += root.ComputedStyle.Gap > 0 ? (enumerable.Length - 1) * root.ComputedStyle.Gap : 0;
        }

        return width;
    }

    private static float GetChildrenHeight(Node root, List<Node> children)
    {
        var visibleChildren = children.Where(node => node is { IsDisposed: false, ComputedStyle.IsVisible: true });
        var enumerable      = visibleChildren as Node[] ?? visibleChildren.ToArray();
        
        float height = enumerable.Sum(node => node.OuterHeight);

        if (root.ComputedStyle.Flow == Flow.Vertical) {
            height += root.ComputedStyle.Gap > 0 ? (enumerable.Length - 1) * root.ComputedStyle.Gap : 0;
        }

        return height;
    }
}