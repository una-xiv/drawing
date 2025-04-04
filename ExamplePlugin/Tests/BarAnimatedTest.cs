using System;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class BarAnimatedTest : ITest
{
    public string Name => "Bar (Animated)";

    private readonly Node _node = new()
    {
        Stylesheet = ExamplePlugin.GlobalStylesheet,
        Style = new()
        {
            Flow               = Flow.Horizontal,
            Size               = new(1920, 32),
            BackgroundColor    = new(0xFF2A2A2A),
            BackgroundGradient = GradientColor.Vertical(new(0xFF2F2F2F), new(0xFF1A1A1A)),
            BorderColor        = new(Top: new(0xFF4F4F4F)),
            BorderWidth        = new() { Top = 1 },
        },
        ChildNodes =
        [
            new()
            {
                Id = "Left",
                Style = new()
                {
                    Anchor  = Anchor.MiddleLeft,
                    Flow    = Flow.Horizontal,
                    Size    = new(0, 0),
                    Gap     = 6,
                    Padding = new() { Left = 6 },
                }
            },
            new()
            {
                Id = "Center",
                Style = new()
                {
                    Anchor = Anchor.MiddleCenter,
                    Flow   = Flow.Horizontal,
                    Size   = new(0, 0),
                    Gap    = 6,
                }
            },
            new()
            {
                Id = "Right",
                Style = new()
                {
                    Anchor  = Anchor.MiddleRight,
                    Flow    = Flow.Horizontal,
                    Size    = new(0, 0),
                    Gap     = 6,
                    Padding = new() { Right = 6 },
                }
            }
        ]
    };

    public BarAnimatedTest()
    {
        Left.AppendChild(CreateBox("Left1", "Button"));
        Left.AppendChild(CreateBox("Left2", "Button"));
        Left.AppendChild(CreateBox("Left3", "Button"));
        Left.AppendChild(CreateBox("Left4", "Button"));
        Left.AppendChild(CreateBox("Left5", "Button"));
        Left.AppendChild(CreateBox("Left6", "Button"));
        Left.AppendChild(CreateBox("Left7", "Button"));
        Left.AppendChild(CreateBox("Left8", "Button"));
        Left.AppendChild(CreateBox("Left9", "Button"));
        Left.AppendChild(CreateBox("Left10", "Button"));

        Center.AppendChild(CreateBox("Center1", "Button", Anchor.TopCenter));
        Center.AppendChild(CreateBox("Center2", "Button", Anchor.TopCenter));
        Center.AppendChild(CreateBox("Center3", "Button", Anchor.TopCenter));
        Center.AppendChild(CreateBox("Center4", "Button", Anchor.TopCenter));

        Right.AppendChild(CreateBox("Right1", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right2", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right3", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right4", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right5", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right6", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right7", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right8", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right9", "Button", Anchor.TopRight));
        Right.AppendChild(CreateBox("Right10", "Button", Anchor.TopRight));
    }

    private float _frameCount;

    public void OnActivate()
    {
    }

    public void Render()
    {
        _node.Render(ImGui.GetBackgroundDrawList(), new(100, 100));

        _frameCount += ImGui.GetIO().DeltaTime;

        int val = (int)MathF.Abs(MathF.Sin(_frameCount * 0.25f) * 100) + 26;
        Left.QuerySelector("Left3")!.Style.Size = new(val, 26);

        int val2 = (int)MathF.Abs(MathF.Sin((_frameCount + 64) * 0.66f) * 100) + 32;
        Center.QuerySelector("Center2")!.Style.Size = new(val2, 26);

        int val3 = (int)MathF.Abs(MathF.Sin((_frameCount + 64) * 0.9f) * 100) + 32;
        Right.QuerySelector("Right1")!.Style.Size = new(val3, 26);
        int val4 = (int)MathF.Abs(MathF.Sin((_frameCount + 87) * 0.75f) * 100) + 32;
        Right.QuerySelector("Right6")!.Style.Size = new(val4, 26);
    }

    private Node Left   => _node.QuerySelector("Left")!;
    private Node Center => _node.QuerySelector("Center")!;
    private Node Right  => _node.QuerySelector("Right")!;

    private static Node CreateBox(string id, string label, Anchor? anchor = null)
    {
        Node box = new()
        {
            Id        = id,
            NodeValue = label,
            ClassList = ["button"],
            Style = new()
            {
                Anchor = anchor ?? Anchor.TopLeft,
            },
        };

        box.OnMouseEnter += n => n.ClassList.Add("button-hover");
        box.OnMouseLeave += n => n.ClassList.Remove("button-hover");

        return box;
    }
}