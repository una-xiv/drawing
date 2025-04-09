using ImGuiNET;

namespace Una.Drawing.Debugger;

internal partial class NodeDebugger
{
    private int _nodeTreeId;

    private void RenderNodeTree()
    {
        _nodeTreeId = 0;
        HoveredNode = null;

        ImGui.BeginChild(
            "NodeTree",
            new(300, ImGui.GetWindowHeight() - 60),
            false,
            ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.HorizontalScrollbar
        );
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));

        ImGui.BeginChild("Toolbar", new(-1, 19), false, ImGuiWindowFlags.AlwaysUseWindowPadding);

        ImGui.EndChild();

        RenderNodeTreeViewerNode(RootNode);
        ImGui.PopStyleVar(1);

        ImGui.EndChild();
    }

    private void RenderNodeTreeViewerNode(Node node)
    {
        _nodeTreeId++;
        ImGui.PushStyleColor(ImGuiCol.Text, node.IsVisible ? 0xFFFFFFFF : 0xFF808080);
        ImGui.PushID(_nodeTreeId);

        ImGuiTreeNodeFlags nodeFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;

        if (SelectedNode == node) nodeFlags |= ImGuiTreeNodeFlags.Selected;

        if (node.ChildNodes.Count == 0) {
            nodeFlags |= ImGuiTreeNodeFlags.Leaf;
            nodeFlags &= ~ImGuiTreeNodeFlags.OpenOnArrow;
        } else {
            nodeFlags |= ImGuiTreeNodeFlags.DefaultOpen;
        }

        bool nodeOpen = ImGui.TreeNodeEx($"{node}", nodeFlags);

        if (ImGui.IsItemClicked()) SelectedNode = node;
        if (ImGui.IsItemHovered()) HoveredNode  = node;

        if (nodeOpen) {
            foreach (var child in node.ChildNodes) RenderNodeTreeViewerNode(child);
            ImGui.TreePop();
        }

        ImGui.PopID();
        ImGui.PopStyleColor();
    }
}