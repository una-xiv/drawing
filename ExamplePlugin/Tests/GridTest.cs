using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class GridTest : ITest
{
    public string Name => "Grid";

    private readonly Node _node = new()
    {
        Style = new()
        {
            Flow = Flow.Vertical,
            Gap  = 4,
        },
        ChildNodes = []
    };

    public GridTest()
    {
        for (var y = 0; y < 50; y++)
        {
            Node row = new()
            {
                Style = new()
                {
                    Flow = Flow.Horizontal,
                    Gap  = 4,
                    BorderColor = new()
                    {
                        Bottom = new(0xFF0000FF),
                        Top    = new(0xFF00FF00),
                    },
                    BorderWidth = new(1),
                },
            };

            _node.AppendChild(row);

            for (var x = 0; x < 50; x++)
            {
                row.AppendChild(CreateBlock());
            }
        }
    }

    public void OnActivate()
    {
    }

    public void Render()
    {
        _node.Render(ImGui.GetBackgroundDrawList(), new(10, 100));
    }

    private Node CreateBlock()
    {
        return new()
        {
            Style = new()
            {
                Size            = new(32, 32),
                Padding         = new(4, 4),
                BackgroundColor = new(0x552479FF),
                BorderColor     = new(new(0xFFFFFFFF)),
                BorderWidth     = new(2),
                BorderRadius    = 0,
            },
        };
    }
}