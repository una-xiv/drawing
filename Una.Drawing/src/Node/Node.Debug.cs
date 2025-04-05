/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using Dalamud.Interface.Utility;
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
        string type        = GetType().Name;
        string id          = string.IsNullOrWhiteSpace(Id) ? "" : $" Id=\"{Id}\"";
        string classes     = ClassList.Count == 0 ? "" : $" Class=\".{string.Join(".", ClassList)}\"";
        string tags        = TagsList.Count == 0 ? "" : $" Tags=\":{string.Join(":", TagsList)}\"";
        string disabled    = IsDisabled ? " Disabled" : "";
        string inheritTags = InheritTags ? " InheritTags" : "";
        string size = Bounds.PaddingSize.IsZero
            ? ""
            : $" Size=\"{Bounds.PaddingSize.Width} x {Bounds.PaddingSize.Width}\"";

        return $"<{type}{id}{classes}{tags}{disabled}{inheritTags}{size}/>";
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
    // [Conditional("DEBUG")]
    private void DrawDebugBounds(ImDrawListPtr drawList)
    {
        if (!DrawDebugInfo) return;

        if (Bounds.ContentRect.IsEmpty) return;
        if (Bounds.PaddingRect.IsEmpty) return;

        Vector2 mousePos = ImGui.GetMousePos();
        int     depth    = GetNodeDepth();

        if (!Bounds.PaddingRect.Contains(mousePos)) {
            // dl.AddRect(Bounds.PaddingRect.TopLeft, Bounds.PaddingRect.BottomRight, 0xAAFFFF00);
            drawList.AddRectFilled(Bounds.ContentRect.TopLeft, Bounds.ContentRect.BottomRight, DebugColors[depth]);
            return;
        }

        drawList.AddRect(Bounds.PaddingRect.TopLeft, Bounds.PaddingRect.BottomRight, 0xC0FFFF00);
        drawList.AddRect(Bounds.ContentRect.TopLeft, Bounds.ContentRect.BottomRight, 0xFF00FF00);

        ImDrawListPtr dl = ImGui.GetForegroundDrawList();
        string text = $"Layer #{depth}: Content Size = {Bounds.ContentSize}\tPadding Size = {Bounds.PaddingSize}";
        Vector2 textPos = mousePos + new Vector2(32f, 32f + (depth * 16f));
        Vector2 textSize = ImGui.CalcTextSize(text);

        dl.AddRectFilled(textPos, textPos + textSize, 0xFF000000);
        dl.AddText(textPos + new Vector2(1, 1), 0xFF000000, text);
        dl.AddText(textPos, 0xFFFFFFFF, text);
    }

    private int GetNodeDepth()
    {
        int   depth   = 0;
        Node? current = ParentNode;

        while (current != null) {
            depth++;
            current = current.ParentNode;
        }

        return depth;
    }

    private static readonly Dictionary<int, uint> DebugColors = new() {
        { 0, 0xC00000FF },
        { 1, 0xC000FF00 },
        { 2, 0xC0FF0000 },
        { 3, 0xC0FFFF00 },
        { 4, 0xC000FFFF },
        { 5, 0xC0FF00FF },
        { 6, 0xC0000000 },
        { 7, 0xC0FFFFFF },
        { 8, 0xC0808080 },
        { 9, 0xC0808000 },
        { 10, 0xC0800080 },
        { 11, 0xC0008080 },
        { 12, 0xC0000080 },
        { 13, 0xC0800000 },
        { 14, 0xC0008000 },
        { 15, 0xC0000040 },
        { 16, 0xC0404040 },
        { 17, 0xC0404000 },
        { 18, 0xC0400040 },
        { 19, 0xC0004040 },
        { 20, 0xC0004000 },
        { 21, 0xC0000020 },
        { 22, 0xC0202020 },
        { 23, 0xC0202000 },
        { 24, 0xC0200020 },
        { 25, 0xC0002020 },
        { 26, 0xC0002000 },
        { 27, 0xC0000010 },
        { 28, 0xC0101010 },
        { 29, 0xC0101000 },
        { 30, 0xC0100010 },
        { 31, 0xC0001010 },
        { 32, 0xC0001000 },
    };
}