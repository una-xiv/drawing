using System;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class TextTest : ITest
{
    public string Name => "Text";

    private readonly Node _node = new()
    {
        Style = new()
        {
            Anchor          = Anchor.TopLeft,
            Size            = new(800, 400),
            BackgroundColor = new(0xFF_000000),
            Padding         = new(10),
            Gap             = 10,
        },
        ChildNodes =
        [
            CreateNode(Anchor.TopLeft, Anchor.TopLeft, "Fixed:64.", null, 64),
            CreateNode(Anchor.TopLeft, Anchor.TopLeft, "Top-left."),

            CreateNode(Anchor.TopCenter, Anchor.TopCenter, "Top-center."),
            CreateNode(Anchor.TopCenter, Anchor.TopCenter, "Fixed:64.", null, 64),

            CreateNode(Anchor.TopRight, Anchor.TopRight, "Fixed:64.", null, 64),
            CreateNode(Anchor.TopRight, Anchor.TopRight, "Top-right."),

            CreateNode(Anchor.MiddleLeft, Anchor.MiddleLeft, "Middle-left."),
            CreateNode(Anchor.MiddleLeft, Anchor.MiddleLeft, "Fixed:64.", null, 64),

            CreateNode(Anchor.MiddleCenter, Anchor.MiddleCenter, "Middle-center with some wrapped text here.", 150,
                150),
            CreateNode(Anchor.MiddleCenter, Anchor.MiddleCenter, "Middle-center with some wrapped text here.", 150, 0),

            CreateNode(Anchor.MiddleRight, Anchor.MiddleRight, "Fixed:64.", null, 64),
            CreateNode(Anchor.MiddleRight, Anchor.MiddleRight, "Middle-right."),

            CreateNode(Anchor.BottomLeft, Anchor.BottomLeft, "Fixed:64.", null, 64),
            CreateNode(Anchor.BottomLeft, Anchor.BottomLeft, "Bottom-left"),

            CreateNode(Anchor.BottomCenter, Anchor.BottomCenter, "Bottom-center."),
            CreateNode(Anchor.BottomCenter, Anchor.BottomCenter, "Fixed:64.", null, 64),

            CreateNode(Anchor.BottomRight, Anchor.BottomRight, "Fixed:64.", null, 64),
            CreateNode(Anchor.BottomRight, Anchor.BottomRight, "Bottom-right."),
        ]
    };

    public void OnActivate()
    {
    }

    public void Render()
    {
        _node.Render(ImGui.GetBackgroundDrawList(), new(100, 100));
    }

    private static Node CreateNode(Anchor anchor, Anchor textAlign, string text, int? width = null, int? height = null)
    {
        return new()
        {
            NodeValue = text,
            Style = new()
            {
                Size            = new(width ?? 0, height ?? 0),
                Anchor          = anchor,
                TextAlign       = textAlign,
                BackgroundColor = new(0xFF_A00000),
                StrokeColor     = new(0xFF_00FF00),
                StrokeWidth     = 1,
                Padding         = new(10),
                FontSize        = 12,
                WordWrap        = width is not null,
                BorderColor     = new(new(0xA0FFFFFF)),
                BorderWidth     = new(1),
                BorderInset     = new(10),
            }
        };
    }
}