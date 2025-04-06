using ImGuiNET;
using System;
using System.Numerics;
using Una.Drawing;

namespace ExamplePlugin.Tests.Images;

public class UldWindowTest : DrawingTest
{
    private Node?    _node;
    private int      _width        = 640;
    private int      _height       = 480;
    private UldStyle _style        = UldStyle.Light;
    private Vector4  _bgColor      = new(.1f, .1f, .1f, 1);
    private Color    _bgColorValue = new(0xFF212021);

    /// <inheritdoc/>
    public override string Name => "ULD Window";

    /// <inheritdoc/>
    public override string Category => "Images";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        _node = CreateNodeFromFile("uld_window.tpl");
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        _node?.Dispose();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped("Creates a window made up of ULD textures.");

        int width  = _width;
        int height = _height;

        if (ImGui.DragInt("Width", ref width, 1, 340, 1260)) {
            _width = width;
        }

        if (ImGui.DragInt("Height", ref height, 1, 128, 880)) {
            _height = height;
        }

        UldStyle style = _style;
        if (ImGui.BeginCombo("ULD Style", style.ToString())) {
            foreach (UldStyle s in Enum.GetValues(typeof(UldStyle))) {
                if (ImGui.Selectable(s.ToString(), s == style)) {
                    _style = s;
                }
            }

            ImGui.EndCombo();
        }

        Vector4 color = _bgColor;
        if (ImGui.ColorPicker4("Background Color", ref color, ImGuiColorEditFlags.NoOptions)) {
            _bgColor      = color;
            _bgColorValue = new Color(GetColor());
        }
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        if (null == _node) return;

        foreach (var node in _node.QuerySelectorAll(".uld")) {
            node.Style.UldStyle = _style;
        }
        
        _node.Style.BackgroundColor = _bgColorValue;

        _node.Style.Size = new(_width, _height);
        _node.Render(dl, new(10, 10));
    }

    private uint GetColor()
    {
        // Convert the vector4 color to a uint color value (0xAABBGGRR)
        return (uint)((int)(_bgColor.W * 255) << 24 | (int)(_bgColor.Z * 255) << 16 | (int)(_bgColor.Y * 255) << 8 |
                      (int)(_bgColor.X * 255));
    }
}