using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.Overflow;

public class OverflowBasicTest : DrawingTest
{
    private Node? _node;
    
    /// <inheritdoc/>
    public override string Name => "Overflow Basic Test";

    /// <inheritdoc/>
    public override string Category => "Overflow";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("overflow_basic.tpl");
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Renders a node with overflow disabled, causing a scrollbar to appear if the inner node of this node exceeds to maximum size of the available content region.");
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        _node?.Render(dl, new (10, 10));
    }
}