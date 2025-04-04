using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class LayoutTest : ITest
{
    public string Name => "Layout";

    private Node Node = new()
    {
        Style = new()
        {
            Flow            = Flow.Horizontal,
            Size            = new(900, 256),
            BackgroundColor = new(0x500000FF),
            BorderColor     = new(new(0xFFFFFFFF)),
            BorderWidth     = new(2),
            Padding         = new(16, 10),
            Color           = new(0xFFFFFFFF),
            TextAlign       = Anchor.TopRight,
            TextShadowColor = new(0xFF000000),
            TextShadowSize  = 1f,
            Gap             = 10,
        },
        ChildNodes =
        [
            CreateNode(Anchor.TopLeft, new(0xFF00FFFF), new(100, 50), null, null, "Fixed size"),
            CreateNode(Anchor.TopLeft, new(0xFF0000FF), new(0, 64), new Color(0x80FF00FF),
                (AutoSize.Grow, AutoSize.Fit), "Fill width"),
            CreateNode(Anchor.TopLeft, new(0xFF0000FF), new(0, 0), new Color(0x80FF00FF),
                (AutoSize.Grow, AutoSize.Grow), "Fill width & height"),
            CreateNode(Anchor.TopLeft, new(0xFF00FFFF), new(100, 50), null, null, "Fixed size"),
            CreateNode(Anchor.TopLeft, new(0xFF00AAFF), new(100, 50), null, null, "Fixed #4"),
            CreateNode(Anchor.TopLeft, new(0xFF00EE00), new(100, 50), null, null, "Fixed #5"),
            CreateNode(Anchor.TopLeft, new(0xFFCCFFAA), new(100, 50), null, null, "Fixed #6"),
        ]
    };

    private int _frameNum;

    public void OnActivate()
    {
    }

    public void Render()
    {
        _frameNum++;

        Node.Render(ImGui.GetBackgroundDrawList(), new(100, 100));

        if (_frameNum == 50)
        {
            Node.ChildNodes[4].Style.IsVisible = !Node.ChildNodes[4].Style.IsVisible;
        }

        if (_frameNum == 100)
        {
            Node.ChildNodes[1].Style.IsVisible = false;
        }

        if (_frameNum == 150)
        {
            Node.ChildNodes[5].Style.IsVisible = !Node.ChildNodes[5].Style.IsVisible;
        }

        if (_frameNum == 300)
        {
            Node.ChildNodes[6].Style.IsVisible = !Node.ChildNodes[6].Style.IsVisible;
            Node.ChildNodes[1].Style.IsVisible = true;
            _frameNum                          = 0;
        }
    }

    private static Node CreateNode(Anchor                    anchor,   Color  color, Size size, Color? bgColor,
                                   (AutoSize h, AutoSize v)? autoSize, string text = "")
    {
        return new Node()
        {
            NodeValue = text,
            Style = new Style()
            {
                TextAlign       = Anchor.MiddleCenter,
                Color           = new Color(0xFFFFFFFF),
                OutlineColor    = new Color(0xFF000000),
                OutlineSize     = 2,
                TextShadowColor = new(0xFFFFFFFF),
                TextShadowSize  = 3,
                Anchor          = anchor,
                BackgroundColor = bgColor,
                BorderColor     = new(color),
                BorderWidth     = new(2),
                Size            = size,
                IsVisible       = true,
                AutoSize        = autoSize,
            }
        };
    }
}