using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.BoxModel;

public class BoxModelTest : DrawingTest
{
    private Node? _node;
    private bool  _wasDebugEnabled;

    /// <inheritdoc/>
    public override string Name => "Box Model Tests";

    /// <inheritdoc/>
    public override string Category => "Box Model";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _wasDebugEnabled = Node.DrawDebugInfo;
        _node            = CreateNodeFromFile("box_model.test.tpl");

        Node.DrawDebugInfo = true;
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        Node.DrawDebugInfo = _wasDebugEnabled;
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped(
            "Tests the box model by rendering nodes with different padding/margin configurations. Each test is drawn inside a box to properly differentiate the distances between the nodes. Drawing of debug bounds is enabled automatically when activating this test to see if the box model is calculated correctly.");
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        _node?.Render(dl, new(10, 10));
    }
}