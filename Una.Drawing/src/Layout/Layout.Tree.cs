namespace Una.Drawing;

internal static partial class Layout
{
    private static bool IsLeafNode(Node node)
    {
        return node.ChildNodes.Count == 0;
    }
}