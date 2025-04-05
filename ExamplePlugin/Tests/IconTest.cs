using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class ImageTest : ITest
{
    public string Name => "Images";

    private static readonly Stylesheet ImageStyles = new([]);

    private readonly Node _node = new()
    {
        Stylesheet = ImageStyles,
        Style = new()
        {
            Flow = Flow.Vertical,
            Gap  = 4,
        },
        ChildNodes =
        [
            new()
            {
                NodeValue = SeIconChar.EorzeaTimeEn.ToIconString(),
                Style = new()
                {
                    Font         = 4,
                    FontSize     = 24,
                    Color        = new(0xFF00FFFF),
                    OutlineColor = new(0xFF0000FF),
                    OutlineSize  = 1,
                }
            },
            CreateBlock(62101),
            CreateBlock(62102),
            CreateBlock(62103),
            CreateBlock(62104),
            CreateBlock(62105),
            CreateBlock(62106),
            CreateBlock(62107),
            CreateBlock(62108),
            CreateBlock(62109),
            CreateBlock(62110),
        ]
    };

    public ImageTest()
    {
        ImageStyles.AddRule(
            ".icon-row",
            new()
            {
                Anchor          = Anchor.TopLeft,
                Size            = new(0, 40),
                Padding         = new(4),
                BorderRadius    = 12,
                BackgroundColor = new(0x80232223),
                StrokeColor     = new(0xFF6A6A6A),
                StrokeWidth     = 1,
                Gap             = 10,
            }
        );

        ImageStyles.AddRule(
            ".icon",
            new()
            {
                Size            = new(32, 32),
                BackgroundColor = new(0x80232223),
                BorderColor     = new(new(0xFFC0C0C0)),
                BorderWidth     = new(2),
                BorderInset     = new(1),
                BorderRadius    = 8,
                ImageRounding   = 16,
                ImageInset      = new(4),
                ImageGrayscale  = true,
            }
        );

        ImageStyles.AddRule(
            ".icon-desc",
            new()
            {
                Size         = new(0, 32),
                TextAlign    = Anchor.MiddleLeft,
                WordWrap     = false,
                Font         = 1,
                FontSize     = 14,
                OutlineColor = new(0xFF000000),
                OutlineSize  = 1,
                Padding      = new() { Right = 4 },
            }
        );
    }

    public void OnActivate()
    {
    }

    public void Render()
    {
        _node.Render(ImGui.GetBackgroundDrawList(), new(200, 200));
    }

    private static Node CreateBlock(uint iconId)
    {
        Node box = new()
        {
            ClassList = ["icon-row"],
            ChildNodes =
            [
                new()
                {
                    ClassList = ["icon"],
                    Style = new()
                    {
                        IconId = iconId,
                    },
                },
                new()
                {
                    ClassList = ["icon-desc"],
                    NodeValue = $"Image #{iconId}"
                },
            ]
        };

        box.OnMouseEnter += _ =>
        {
            box.QuerySelector(".icon")!.Style.ImageGrayscale = false;
            box.QuerySelector(".icon")!.Style.ImageInset     = new(-4);
        };

        box.OnMouseLeave += _ =>
        {
            box.QuerySelector(".icon")!.Style.ImageGrayscale = true;
            box.QuerySelector(".icon")!.Style.ImageInset     = new(4);
        };

        return box;
    }
}