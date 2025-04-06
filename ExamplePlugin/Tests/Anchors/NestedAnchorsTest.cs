using ImGuiNET;
using System.Numerics;
using Una.Drawing;

namespace ExamplePlugin.Tests.Basic;

public class NestedAnchorsTest : DrawingTest
{
    private Node? _node;
    
    /// <inheritdoc/>
    public override string Name => "Nested Anchors";

    /// <inheritdoc/>
    public override string Category => "Anchors";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("anchors.nested.tpl");
    }

    /// <inheritdoc/>
    public override void OnDeactivate() { }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Tests nested anchors. The root node contains child nodes with different anchor points. Each child in the root node also contains 9 nodes that each have a different anchor point. If everything works correctly, the child nodes should be positioned correctly relative to the root node and each other.");
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        _node?.Render(dl, new Vector2(10, 10));
    }
}