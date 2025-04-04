using System;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class BasicTest : ITest
{
    public string Name => "Basic";

    private readonly Node _node = new()
    {
        NodeValue = "Hello World! Pretty graphics! This is very neat with word wrap enabled.",
        Style = new()
        {
            Size            = new(128, 128),
            Flow            = Flow.Vertical,
            Padding         = new(0, 0),
            Gap             = 4,
            BackgroundColor = new(0x504400FF),
            BorderColor = new()
            {
                Top    = new(0xFFFFAAAA),
                Bottom = new(0xFFFF00FF)
            },
            BorderInset             = new(4),
            BorderRadius            = 16,
            BorderWidth             = new(2),
            StrokeColor             = new(0xFF00FFFF),
            StrokeWidth             = 1,
            BackgroundGradient      = GradientColor.Vertical(new(0xA0A0A0A0), new(0x60FFA0FF)),
            BackgroundGradientInset = new(4),
            FontSize                = 12,
            LineHeight              = 1.2f,
            WordWrap                = true,
            TextAlign               = Anchor.MiddleCenter,
            OutlineColor            = new(0x600000FF),
            OutlineSize             = 2,
            TextOffset              = new(0, -5),
            TextShadowSize          = 16,
            TextShadowColor         = new(0xFFFFFFFF),
        },
        ChildNodes = []
    };

    private uint _frameCount = 0;

    public void OnActivate()
    {
    }

    public void Render()
    {
        _frameCount++;

        _node.NodeValue =
            $"Hello World! Pretty graphics! {DateTime.Now:h:mm:ss tt} Frame Count: {_frameCount}";

        _node.Render(ImGui.GetBackgroundDrawList(), new(100, 100));
    }
}