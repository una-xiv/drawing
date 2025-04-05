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
        (int originX, int originY) = GetNodeOrigin(anchor, root, children);

        int x   = originX, y = originY;
        int gap = root.ComputedStyle.Gap;

        Node lastChild = children.Last();

        if (anchor.IsCenter) {
            x = GetCenterStart(root, children);
        }

        if (anchor.IsTop) {
            y = originY;
        }
        
        foreach (Node node in children) {
            if (node.IsDisposed || !node.IsVisible) continue;
            
            if (anchor.IsMiddle) {
                y = originY - (int)Math.Ceiling(node.OuterHeight / 2f);
            } else if (anchor.IsBottom) {
                y = originY - node.OuterHeight;
            }

            node.Bounds.PaddingRect = new Rect(x, y, node.Bounds.PaddingSize);
            node.Bounds.ContentRect = new Rect(x + node.ComputedStyle.Padding.Left,
                y + node.ComputedStyle.Padding.Top, node.Bounds.ContentSize);

            if (node.Equals(lastChild)) {
                ComputePositions(node);
                break;
            }

            switch (root.ComputedStyle.Flow) {
                case Flow.Vertical:
                    // X-axis
                    if (anchor.IsLeft) {
                        x = originX;
                    } else if (anchor.IsCenter) {
                        x = originX - (int)Math.Ceiling(node.OuterWidth / 2f);
                    } else if (anchor.IsRight) {
                        x = originX - node.OuterWidth;
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
                    }

                    break;
            }

            ComputePositions(node);
        }
    }

    private static (int, int) GetNodeOrigin(Anchor anchor, Node root, List<Node> children)
    {
        int x = 0;
        int y = 0;

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

    private static int GetCenterStart(Node root, List<Node> children)
    {
        return root.Bounds.ContentRect.X1 + Math.Abs(
            (int)Math.Ceiling((root.Width / 2f) - (GetChildrenWidth(root, children) / 2f)));
    }

    private static int GetMiddle(Node root)
    {
        return root.Bounds.ContentRect.Y1 + (int)Math.Ceiling(root.Bounds.ContentSize.Height / 2f);
    }

    private static int GetChildrenWidth(Node root, List<Node> children)
    {
        int width = children
                   .Where(node => node is { IsDisposed: false, ComputedStyle.IsVisible: true })
                   .Sum(node => node.OuterWidth);

        width += root.ComputedStyle.Gap > 0 ? (children.Count - 1) * root.ComputedStyle.Gap : 0;

        return width;
    }
}