using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.Overflow;

public class OverflowBasicVerticalTest : DrawingTest
{
    private Node? _node;
    
    /// <inheritdoc/>
    public override string Name => "Basic Vertical";

    /// <inheritdoc/>
    public override string Category => "Overflow";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("overflow.basic-v.tpl");
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Tests a vertical overflow.");
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        _node?.Render(dl, new (10, 10));
    }
}