/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Diagnostics;
using ImGuiNET;

namespace Una.Drawing;

public partial class Node
{
    public static bool DrawDebugInfo { get; set; }

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    public override string ToString()
    {
        string id      = string.IsNullOrWhiteSpace(Id) ? "" : $"{Id}";
        string classes = ClassList.Count == 0 ? "" : $".{string.Join(".", ClassList)}";
        string tags    = TagsList.Count  == 0 ? "" : $":{string.Join(":", TagsList)}";

        var result = $"{id}{classes}{tags}";

        return (result == "" ? "[/]" : result) + $" <{OuterWidth} x {OuterHeight}>";
    }

    /// <summary>
    /// Returns a string representation of the node tree.
    /// </summary>
    public string DumpTree()
    {
        return "Node tree:\n" + DumpNode(this, 0);

        string DumpNode(Node node, int depth)
        {
            string indent     = node.ParentNode == null ? "" : $"{new(' ', depth * 2)} - ";
            string nodeString = $"{indent}{node}";

            foreach (Node child in node.ChildNodes) {
                nodeString += $"\n{DumpNode(child, depth + 1)}";
            }

            return nodeString;
        }
    }

    /// <summary>
    /// Returns a string representation of the full path to this node.
    /// </summary>
    /// <returns></returns>
    public string GetFullNodePath()
    {
        List<string> breadcrumbs = [];
        Node?        current     = this;

        while (current != null) {
            breadcrumbs.Insert(0, current.ToString());
            current = current.ParentNode;
        }

        return string.Join(" > ", breadcrumbs);
    }

    /// <summary>
    /// Draws debug information for this node.
    /// </summary>
    [Conditional("DEBUG")]
    private void DrawDebugBounds()
    {
        if (! DrawDebugInfo) return;

        if (Bounds.ContentRect.IsEmpty) return;
        if (Bounds.PaddingRect.IsEmpty) return;
        if (Bounds.MarginRect.IsEmpty) return;

        ImDrawListPtr dl = ImGui.GetForegroundDrawList();

        dl.AddRectFilled(Bounds.ContentRect.TopLeft, Bounds.ContentRect.BottomRight, 0x3000FF00);
        dl.AddRect(Bounds.PaddingRect.TopLeft, Bounds.PaddingRect.BottomRight, 0xA0A0A0FF);
        dl.AddRect(Bounds.MarginRect.TopLeft,  Bounds.MarginRect.BottomRight,  0x60FFFFFF);
    }
}
