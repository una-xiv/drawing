using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.AutoSize;

public class AutoSizeLayoutTest : DrawingTest
{
    private Node? _node;
    private int   _width = 640;
    private int   _height = 480;
    
    /// <inheritdoc/>
    public override string Name => "Auto Size Layout";

    /// <inheritdoc/>
    public override string Category => "Auto Size";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("autosize_layout.tpl");

        foreach (var item in _node.QuerySelectorAll(".menu-item, .button")) {
            item.OnClick += n => { };
        }
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Renders a node that contains elements that should grow to fit the available space.");

        int width = _width;
        int height = _height;

        if (ImGui.DragInt("Width", ref width, 1, 340, 1260)) {
            _width = width;
        }
        
        if (ImGui.DragInt("Height", ref height, 1, 128, 880)) {
            _height = height;
        }
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        if (null == _node) return;
        
        _node.Style.Size = new(_width, _height); 
        _node.Render(dl, new (10, 10));
    }
}