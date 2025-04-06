using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.Overflow;

public class OverflowDialogTest : DrawingTest
{
    private Node? _node;
    
    /// <inheritdoc/>
    public override string Name => "Dialog with Overflow";

    /// <inheritdoc/>
    public override string Category => "Overflow";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("overflow.dialog.tpl");
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