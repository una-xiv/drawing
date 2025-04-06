using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests.AutoSize;

public class TextAnchorsTest : DrawingTest
{
    private Node? _node;
    private int   _width  = 640;
    private int   _height = 480;
    
    /// <inheritdoc/>
    public override string Name => "Text Anchors";

    /// <inheritdoc/>
    public override string Category => "Anchors";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("anchors.text.tpl");
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Tests the different text anchors.");

        int width  = _width;
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