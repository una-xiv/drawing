/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using Dalamud.Game.Text.SeStringHandling;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class StretchTest : ITest
{
    public string Name => "Stretch";

    private readonly Node _node = new()
    {
        Id = "Menu",
        Style = new()
        {
            Anchor                  = Anchor.TopCenter,
            Flow                    = Flow.Vertical,
            BackgroundColor         = new(0xFF212021),
            BorderColor             = new(new(0xFF4F4F4F)),
            BackgroundGradient      = GradientColor.Vertical(new(0xFF212021), new(0xFF443911)),
            BackgroundGradientInset = new(6) { Bottom = 0 },
            BorderWidth             = new() { Top     = 1, Right = 1, Left = 1, Bottom = 0 },
            BorderInset             = new(5) { Bottom = 0 },
            BorderRadius            = 9,
            RoundedCorners          = RoundedCorners.TopLeft | RoundedCorners.TopRight,
            Gap                     = 18,
            Padding                 = new(16),
            ShadowSize              = new(64, 64, 0, 64),
            ShadowInset             = 2,
            ShadowOffset            = new(0, 8),
            IsAntialiased           = true,
        },
        ChildNodes =
        [
            CreateItem("Item1", "JA: グッデイワールド", 1),
            CreateItem("Item2", "CN: 好日子世界", 2),
            CreateItem("Item3", "KR: 굿데이월드", 3),
            CreateItem("Item4", "Item 4", 4),
            CreateItem("Item5", "Short", 5),
            CreateItem("Item6", "Foobar", 6),
            CreateItem("Item7", "Another one with very large text that is larger than any other item", 7),
            CreateItem("Item8", null, 8),
            CreateItem("Item9", "This is item 9", 9),
            CreateItem("Item10", "This is item 10", 10),
            CreateItem("Item11", "This is item 11", 11),
            CreateItem("Item12", "This is item 12", 12),
            CreateItem("Item13", "This is item 13", 13),
        ]
    };

    private int _time;

    public StretchTest()
    {
        Color.AssignByName("stretch-test", 0xFF402070);

        SeString str = new SeStringBuilder()
                      .AddUiForeground(28)
                      .AddText("SeString test with ")
                      .AddUiForegroundOff()
                      .AddIcon(BitmapFontIcon.IslandSanctuary)
                      .AddText(" a very nice icon. Neat stuff!")
                      .Build();

        _node.QuerySelector("Item8_Label")!.NodeValue = str;
    }

    public void OnActivate()
    {
    }

    public void Render()
    {
        _time += 1;

        if (_time == 300)
        {
            _node.QuerySelector("Item4_Label")!.NodeValue =
                "Now this text is much much much much much longer, and should expand the total width of the menu.";
            _node.QuerySelector("Item6")!.Style.IsVisible = true;
            Color.AssignByName("stretch-test", 0xFF407070);

            _node.QuerySelector("Item6")!.SortIndex = 1;
        }

        if (_time == 600)
        {
            _node.QuerySelector("Item4_Label")!.NodeValue = "Yes";
            _node.QuerySelector("Item6")!.Style.IsVisible = false;
            _time                                         = 0;
            Color.AssignByName("stretch-test", 0xFF404050);

            _node.QuerySelector("Item6")!.SortIndex = 99;
        }

        _node.Render(ImGui.GetBackgroundDrawList(), new(500, 100));
    }

    private static Node CreateItem(string id, object? label, int sortIndex)
    {
        return new()
        {
            Id        = id,
            SortIndex = sortIndex,
            Style = new()
            {
                Anchor          = Anchor.TopLeft,
                AutoSize        = (AutoSize.Grow, AutoSize.Fit),
                Size            = new(0, 32),
                Padding         = new(0, 8),
                MaxWidth        = 200,
                Font            = 1,
                FontSize        = 14,
                TextAlign       = Anchor.MiddleCenter,
                BackgroundColor = new("stretch-test"),
                BorderRadius    = 4,
                TextShadowColor = new(0xFF000000),
                TextShadowSize  = 2,
                OutlineColor    = new(0xFF000000),
                OutlineSize     = 1,
                IsAntialiased   = false,
            },
            ChildNodes =
            [
                new()
                {
                    NodeValue = $"#{sortIndex}",
                    Style = new()
                    {
                        Anchor          = Anchor.TopLeft,
                        Size            = new(0, 32),
                        AutoSize        = (AutoSize.Fit, AutoSize.Fit),
                        Padding         = new(0, 8),
                        Font            = 1,
                        FontSize        = 14,
                        TextAlign       = Anchor.MiddleRight,
                        TextShadowColor = new(0xFF000000),
                        TextShadowSize  = 2,
                    }
                },
                new()
                {
                    Id        = $"{id}_Label",
                    NodeValue = label,
                    Style = new()
                    {
                        Anchor          = Anchor.TopLeft,
                        Size            = new(0, 32),
                        AutoSize        = (AutoSize.Grow, AutoSize.Fit),
                        Padding         = new(0, 8),
                        Font            = 1,
                        FontSize        = 14,
                        TextAlign       = Anchor.MiddleCenter,
                        TextShadowColor = new(0xFF000000),
                        TextShadowSize  = 2,
                        IsAntialiased   = false,
                        TextOverflow    = false,
                    }
                },
                new()
                {
                    NodeValue = "Right!",
                    Style = new()
                    {
                        Anchor          = Anchor.TopLeft,
                        Size            = new(64, 32),
                        Padding         = new(0, 8),
                        Font            = 1,
                        FontSize        = 14,
                        TextAlign       = Anchor.MiddleRight,
                        TextShadowColor = new(0xFF000000),
                        TextShadowSize  = 2,
                    }
                }
            ]
        };
    }
}