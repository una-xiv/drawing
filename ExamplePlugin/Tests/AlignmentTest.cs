using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class AlignmentTest : ITest
{
    public string Name => "Alignment";

    private readonly Node _node = new()
    {
        Style = new()
        {
            Anchor          = Anchor.MiddleCenter,
            Size            = new(600, 600),
            Padding         = new(10),
            Flow            = Flow.Vertical,
            BackgroundColor = new(0xFF303030),
            StrokeColor     = new(0xFFFFFFFF),
            StrokeWidth     = 2,
        },
        ChildNodes =
        [
            CreateContainer(150, Anchor.AnchorPoint.TopLeft),
            CreateContainer(150, Anchor.AnchorPoint.TopCenter),
            CreateContainer(150, Anchor.AnchorPoint.TopRight),
            CreateContainer(150, Anchor.AnchorPoint.MiddleLeft),
            CreateContainer(150, Anchor.AnchorPoint.MiddleCenter),
            CreateContainer(150, Anchor.AnchorPoint.MiddleRight),
            CreateContainer(150, Anchor.AnchorPoint.BottomLeft),
            CreateContainer(150, Anchor.AnchorPoint.BottomCenter),
            CreateContainer(150, Anchor.AnchorPoint.BottomRight),
        ]
    };

    public void OnActivate()
    {
    }

    public void Render()
    {
        _node.Render(ImGui.GetBackgroundDrawList(), new(50, 50));
    }

    private static Node CreateContainer(int size, Anchor.AnchorPoint point, bool createChildNodes = true)
    {
        Node box = new()
        {
            Style = new()
            {
                Anchor          = new(point),
                Size            = new(size, size),
                Padding         = new(4),
                BackgroundColor = new(0xFF101010),
                StrokeColor     = new(0xFF4FAFAF),
                StrokeWidth     = 1,
                BorderRadius    = 4,
                Gap             = 1,
            },
            ChildNodes = createChildNodes
                ?
                [
                    CreateContainer(20, Anchor.AnchorPoint.TopLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.TopLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.TopCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.TopCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.TopRight, false),
                    CreateContainer(20, Anchor.AnchorPoint.TopRight, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleRight, false),
                    CreateContainer(20, Anchor.AnchorPoint.MiddleRight, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomLeft, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomCenter, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomRight, false),
                    CreateContainer(20, Anchor.AnchorPoint.BottomRight, false),
                ]
                : []
        };

        box.OnMouseEnter += n => n.ClassList.Add("button-hover");
        box.OnMouseLeave += n => n.ClassList.Remove("button-hover");

        return box;
    }
}