/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Una.Drawing.Font;
using Una.Drawing.Texture;

namespace Una.Drawing.Generator;

public class SeStringGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 999;

    /// <inheritdoc/>
    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        if (node.NodeValue is not SeString seString || seString.Payloads.Count == 0) return false;

        Size  size        = node.NodeValueMeasurement!.Value.Size;
        IFont font        = FontRegistry.Fonts[node.ComputedStyle.Font];
        var   outlineSize = (int)node.ComputedStyle.OutlineSize;
        var   metrics     = font.GetMetrics(node.ComputedStyle.FontSize);
        int   spaceWidth  = font.MeasureText("X", node.ComputedStyle.FontSize, node.ComputedStyle.OutlineSize).Size.Width;

        var y = (int)origin.X + (int)(metrics.CapHeight) + outlineSize;
        var x = (int)origin.Y + (int)node.ComputedStyle.TextOffset.X + 1;

        if (node.ComputedStyle.TextAlign.IsTop) y    += node.ComputedStyle.Padding.Top + outlineSize;
        if (node.ComputedStyle.TextAlign.IsLeft) x   += node.ComputedStyle.Padding.Left + outlineSize;
        if (node.ComputedStyle.TextAlign.IsRight) x  += -(node.ComputedStyle.Padding.Right + outlineSize);
        if (node.ComputedStyle.TextAlign.IsMiddle) y += (node.Height - size.Height) / 2 + outlineSize;
        if (node.ComputedStyle.TextAlign.IsBottom) y =  node.Height - size.Height - outlineSize;

        SKColor  prevColor     = Color.ToSkColor(node.ComputedStyle.Color);
        SKColor  color         = prevColor;
        SKColor? edgeColor     = null;
        SKColor? prevEdgeColor = null;

        foreach (var payload in seString.Payloads) {
            switch (payload) {
                case UIForegroundPayload fg:
                    uint rgb = fg.RGBA;

                    if (rgb == 0) {
                        color = prevColor;
                    } else {
                        prevColor = color;
                        color     = Color.ToSkColor(new(RgbaToAbgr(fg.UIColor.Value.Dark)));
                    }
                    continue;
                case UIGlowPayload glow:
                    uint rgba = glow.RGBA;

                    if (rgba == 0) {
                        edgeColor = prevEdgeColor;
                    } else {
                        prevEdgeColor = edgeColor;
                        edgeColor     = Color.ToSkColor(new(RgbaToAbgr(glow.UIColor.Value.Dark)));
                    }
                    continue;
                case TextPayload text:
                    x += DrawText(canvas, new(x, y + node.ComputedStyle.TextOffset.Y), color, edgeColor, node, text.Text ?? "", x, node.ComputedStyle.MaxWidth);
                    continue;
                case IconPayload icon: {
                    x += spaceWidth;

                    GfdIcon gfdIcon = GfdIconRepository.GetIcon(icon.Icon);

                    using SKPaint gfdPaint = new();

                    canvas.DrawImage(
                        gfdIcon.Texture,
                        new(gfdIcon.Uv0.X, gfdIcon.Uv0.Y, gfdIcon.Uv1.X, gfdIcon.Uv1.Y),
                        new SKRect(
                            x - 4,
                            y - (metrics.CapHeight) - 6,
                            x + 22,
                            y - (metrics.CapHeight) + 19
                        ),
                        gfdPaint
                    );

                    x += 20 + spaceWidth;
                    continue;
                }
            }
        }

        return true;
    }

    private static int DrawText(SKCanvas canvas, SKPoint point, SKColor color, SKColor? edgeColor, Node node, string text, int currentWidth, int? maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        IFont font = FontRegistry.Fonts[node.ComputedStyle.Font];

        MeasuredText measurements = font.MeasureText(text, node.ComputedStyle.FontSize, node.ComputedStyle.OutlineSize, maxWidth: maxWidth - currentWidth);

        if (measurements.Size.Width == 0) return 0;
        text = measurements.Lines[0];

        using SKPaint paint = new();

        if (node.ComputedStyle.OutlineSize > 0) {
            paint.Color       = edgeColor ?? Color.ToSkColor(node.ComputedStyle.OutlineColor ?? new(0xFF000000));
            paint.Style       = SKPaintStyle.Stroke;
            paint.StrokeWidth = node.ComputedStyle.OutlineSize * 2;
            paint.MaskFilter  = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, node.ComputedStyle.OutlineSize);
            font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, text);
        }

        if (node.ComputedStyle.TextShadowSize > 0) {
            paint.ImageFilter = paint.ImageFilter = SKImageFilter.CreateDropShadow(
                0,
                0,
                node.ComputedStyle.TextShadowSize * 2,
                node.ComputedStyle.TextShadowSize * 2,
                Color.ToSkColor(node.ComputedStyle.TextShadowColor ?? new(0xFF000000))
            );
        } else {
            paint.ImageFilter = null;
        }

        paint.MaskFilter  = null;
        paint.StrokeWidth = 0;
        paint.Style       = SKPaintStyle.Fill;
        paint.Color       = color;
        font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, text);

        return measurements.Size.Width;
    }

    internal static uint RgbaToAbgr(uint rgba)
    {
        uint tmp = ((rgba << 8) & 0xFF00FF00) | ((rgba >> 8) & 0xFF00FF);
        return (tmp << 16) | (tmp >> 16);
    }
}