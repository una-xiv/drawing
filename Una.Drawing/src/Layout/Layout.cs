namespace Una.Drawing;

internal static partial class Layout
{
    public static void ComputeLayout(Node node, Vector2 origin)
    {
        if (node.ParentNode != null) {
            node = node.RootNode;
        }

        if (node.IsDisposed || !node.ComputedStyle.IsVisible) return;

        
        ComputeSizes(node);
        
        node.Bounds.PaddingRect = new Rect(origin, node.Bounds.PaddingSize);
        node.Bounds.ContentRect = new Rect(
            (int)Math.Ceiling(origin.X) + node.ComputedStyle.Padding.Left,
            (int)Math.Ceiling(origin.Y) + node.ComputedStyle.Padding.Top,
            node.Bounds.ContentSize
        );
        
        ComputePositions(node);
    }
}