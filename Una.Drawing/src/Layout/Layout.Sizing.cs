using System.Linq;

namespace Una.Drawing;

internal static partial class Layout
{
    /// <summary>
    /// Computes the sizes of the given node and its children.
    /// </summary>
    private static void ComputeSizes(Node node)
    {
        ComputeFixedAndFitSizes(node);

        if (!ComputeGrowingSizes(node)) {
            return; // No need to stabilize.
        }

        // Second pass to stabilize for text wrapping & overflow.
        ComputeFixedAndFitSizes(node);
        ComputeGrowingSizes(node);
    }

    private static void ComputeFixedAndFitSizes(Node node)
    {
        bool isHorizontal = node.ComputedStyle.Flow == Flow.Horizontal;

        // Use clearer names reflecting the final calculated dimension
        int childrenCalculatedWidth;
        int childrenCalculatedHeight;

        // Use temporary accumulators based on flow direction
        int mainAxisAccumulatedSize = 0;
        int crossAxisMaxSize        = 0;

        bool isAutoWidth  = node.ComputedStyle.Size.Width <= 0;
        bool isAutoHeight = node.ComputedStyle.Size.Height <= 0;

        int visibleChildCount = 0; // Needed for correct gap calculation

        foreach (Node child in node.ChildNodes) {
            if (child.IsDisposed || !child.ComputedStyle.IsVisible) continue;

            visibleChildCount++;            // Count only visible children for gap calculation.
            ComputeFixedAndFitSizes(child); // Recurse reverse-breadth-first.

            int childOuterWidth  = child.Bounds.PaddingSize.Width; // Using OuterWidth/Height based on context
            int childOuterHeight = child.Bounds.PaddingSize.Height;

            if (isHorizontal) {
                mainAxisAccumulatedSize += childOuterWidth;                              // Sum widths
                crossAxisMaxSize        =  Math.Max(crossAxisMaxSize, childOuterHeight); // Max height
            } else {
                mainAxisAccumulatedSize += childOuterHeight;                            // Sum heights
                crossAxisMaxSize        =  Math.Max(crossAxisMaxSize, childOuterWidth); // Max width
            }
        }

        mainAxisAccumulatedSize += (node.ComputedStyle.Gap > 0 && visibleChildCount > 1)
            ? (visibleChildCount - 1) * node.ComputedStyle.Gap
            : 0;

        if (isHorizontal) {
            childrenCalculatedWidth  = mainAxisAccumulatedSize; // Sum of Widths + Gaps from children.
            childrenCalculatedHeight = crossAxisMaxSize;        // Max Height from children.
        } else {
            childrenCalculatedWidth  = crossAxisMaxSize;        // Max Width from children.
            childrenCalculatedHeight = mainAxisAccumulatedSize; // Sum of Heights + Gaps from children.
        }

        Size textSize = node.ComputeContentSizeFromText(); // Assuming this returns Content Size needed for text

        var finalContentWidth = isAutoWidth
            ? Math.Max(childrenCalculatedWidth, textSize.Width)
            : node.ComputedStyle.Size.Width - node.ComputedStyle.Padding.HorizontalSize;

        int finalContentHeight = isAutoHeight
            ? Math.Max(childrenCalculatedHeight, textSize.Height)
            : node.ComputedStyle.Size.Height - node.ComputedStyle.Padding.VerticalSize;

        node.Bounds.ContentSize = new Size( // Assuming Point(width, height)
            Math.Max(0, finalContentWidth),
            Math.Max(0, finalContentHeight)
        );

        node.Bounds.PaddingSize = new Size(
            node.Bounds.ContentSize.Width + node.ComputedStyle.Padding.HorizontalSize,
            node.Bounds.ContentSize.Height + node.ComputedStyle.Padding.VerticalSize
        );
    }

    private static bool ComputeGrowingSizes(Node node)
    {
        Flow axis          = node.ComputedStyle.Flow;
        Flow crossAxis     = axis == Flow.Horizontal ? Flow.Vertical : Flow.Horizontal;
        bool mustStabilize = false;

        foreach (List<Node> children in node.AnchorToChildNodes.Values) {
            GrowChildrenAlongAxis(node, axis, children);
            GrowChildrenAlongCrossAxis(node, crossAxis, children);
        }

        foreach (Node child in node.ChildNodes) {
            if (ComputeGrowingSizes(child)) {
                mustStabilize = true;
            }
        }

        // Reset and recompute text size after layout if necessary.
        if (node.ComputedStyle.WordWrap || !node.ComputedStyle.TextOverflow) {
            node.ClearTextCache();
            Size newTextSize = node.ComputeContentSizeFromText();

            if ((node.ComputedStyle.AutoSize.Horizontal == AutoSize.Fit && node.ComputedStyle.Size.Width == 0 &&
                 newTextSize.Width != node.Bounds.ContentSize.Width) ||
                (node.ComputedStyle.AutoSize.Vertical == AutoSize.Fit && node.ComputedStyle.Size.Height == 0 &&
                 newTextSize.Height != node.Bounds.ContentSize.Height)
            ) {
                return true;
            }
        }

        return mustStabilize;
    }

    private static void GrowChildrenAlongAxis(Node node, Flow axis, List<Node> children)
    {
        List<Node>               growableChildren;
        Func<Node, int>          getOuterSize;
        Func<ComputedStyle, int> getPaddingSize;
        int                      parentContentSize;
        int                      gap = node.ComputedStyle.Gap;

        // Configure based on axis.
        if (axis == Flow.Horizontal) {
            growableChildren = children.Where(n => n is
                { IsDisposed: false, ComputedStyle: { AutoSize.Horizontal: AutoSize.Grow, IsVisible: true } }).ToList();
            getOuterSize      = n => n.OuterWidth;
            getPaddingSize    = cs => cs.Padding.HorizontalSize;
            parentContentSize = node.Bounds.ContentSize.Width;
        } else {
            growableChildren = children.Where(n => n is
                { IsDisposed: false, ComputedStyle: { AutoSize.Vertical: AutoSize.Grow, IsVisible: true } }).ToList();
            getOuterSize      = n => n.OuterHeight;
            getPaddingSize    = cs => cs.Padding.VerticalSize;
            parentContentSize = node.Bounds.ContentSize.Height;
        }

        // Abort if there is nothing to grow.
        if (growableChildren.Count == 0) return;

        // Calculate space used by non-growable items and gaps.
        int nonGrowableSize = children
                             .Where(n => !growableChildren.Contains(n) &&
                                         n is { IsDisposed: false, ComputedStyle.IsVisible: true })
                             .Sum(n => getOuterSize(n));

        int visibleChildCount = children.Count(n => n is { IsDisposed: false, ComputedStyle.IsVisible: true });
        int totalGapSize      = children.Count > 1 ? gap * (visibleChildCount - 1) : 0;

        // Total space the growable items should collectively occupy.
        int availableSizeForGrowableGroup = parentContentSize - nonGrowableSize - totalGapSize;
        availableSizeForGrowableGroup = Math.Max(0, availableSizeForGrowableGroup);

        // Distribute available size.
        int baseTargetOuterSize = availableSizeForGrowableGroup / growableChildren.Count;
        int remainderSize       = availableSizeForGrowableGroup % growableChildren.Count;

        foreach (Node child in growableChildren) {
            if (child.IsDisposed || !child.ComputedStyle.IsVisible) continue;

            int targetOuterSize  = baseTargetOuterSize + (remainderSize > 0 ? 1 : 0);
            int childPaddingSize = getPaddingSize(child.ComputedStyle);
            int newContentSize   = targetOuterSize - childPaddingSize;

            newContentSize = Math.Max(0, newContentSize);

            child.Bounds.ContentSize = axis == Flow.Horizontal
                ? new Size(newContentSize, child.Bounds.ContentSize.Height)
                : new Size(child.Bounds.ContentSize.Width, newContentSize);

            child.Bounds.PaddingSize = child.Bounds.ContentSize + child.ComputedStyle.Padding.Size;

            if (remainderSize > 0) { remainderSize--; }
        }
    }

    private static void GrowChildrenAlongCrossAxis(Node node, Flow axisToStretch, List<Node> children)
    {
        int parentContentSize = (axisToStretch == Flow.Horizontal)
            ? node.Bounds.ContentSize.Width
            : node.Bounds.ContentSize.Height;

        Func<Node, bool>         shouldStretchChild;
        Func<ComputedStyle, int> getPaddingSize;

        if (axisToStretch == Flow.Horizontal) {
            shouldStretchChild = n =>
                n.ComputedStyle.AutoSize.Horizontal is AutoSize.Grow && n.ComputedStyle.Size.Width <= 0;
            getPaddingSize = cs => cs.Padding.HorizontalSize;
        } else {
            shouldStretchChild = n =>
                n.ComputedStyle.AutoSize.Vertical is AutoSize.Grow && n.ComputedStyle.Size.Height <= 0;
            getPaddingSize = cs => cs.Padding.VerticalSize;
        }

        foreach (Node child in children) {
            if (child.IsDisposed || !child.ComputedStyle.IsVisible) continue;
            if (!shouldStretchChild(child)) continue;

            int childPaddingSize = getPaddingSize(child.ComputedStyle);
            int newContentSize   = parentContentSize - childPaddingSize;

            newContentSize = Math.Max(0, newContentSize);

            // Get current content size on the axis to potentially update.
            int currentContentSize = (axisToStretch == Flow.Horizontal)
                ? child.Bounds.ContentSize.Width
                : child.Bounds.ContentSize.Height;

            // Abort if the content size is already the same.
            if (newContentSize == currentContentSize) continue;

            child.Bounds.ContentSize = axisToStretch == Flow.Horizontal
                ? new Size(newContentSize, child.Bounds.ContentSize.Height)
                : new Size(child.Bounds.ContentSize.Width, newContentSize);

            child.Bounds.PaddingSize = child.Bounds.ContentSize + child.ComputedStyle.Padding.Size;
        }
    }
}