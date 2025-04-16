using Dalamud.Game.Text.SeStringHandling;
using System.Linq;
using Una.Drawing.Font;

namespace Una.Drawing.Generator;

internal class TextGenerator : IGenerator
{
    public int RenderOrder => 999;

    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        MeasuredText? measurement = node.NodeValueMeasurement;
        if (null == measurement || measurement.Value.LineCount == 0) return false;

        Size  size       = node.NodeValueMeasurement!.Value.Size;
        IFont font       = FontRegistry.Fonts[node.ComputedStyle.Font];
        int   fontSize   = node.ComputedStyle.FontSize;
        float lineHeight = font.GetLineHeight(fontSize);
        var   metrics    = font.GetMetrics(node.ComputedStyle.FontSize);

        var y = origin.Y + (lineHeight + node.ComputedStyle.TextOffset.Y) - metrics.Descent - 1.5f;
        var x = origin.X + node.ComputedStyle.TextOffset.X;

        y = (int)Math.Ceiling(y);

        if (node.ComputedStyle.TextAlign.IsTop) y    += node.ComputedStyle.Padding.Top;
        if (node.ComputedStyle.TextAlign.IsLeft) x   += node.ComputedStyle.Padding.Left;
        if (node.ComputedStyle.TextAlign.IsRight) x  -= node.ComputedStyle.Padding.Right;
        if (node.ComputedStyle.TextAlign.IsMiddle) y += (node.Height - size.Height) / 2f;
        if (node.ComputedStyle.TextAlign.IsBottom) y += node.Height - size.Height;

        foreach (Chunk[] line in node.NodeValueMeasurement!.Value.Lines) {
            PrintLine(canvas, font, node, line, x, y);
            y += (lineHeight * node.ComputedStyle.LineHeight);
        }

        return true;
    }

    private static void PrintLine(SKCanvas canvas, IFont font, Node node, Chunk[] line, float x, float y)
    {
        float lineWidth = line.Sum(chunk => chunk.Width);

        if (node.ComputedStyle.TextAlign.IsCenter) x += (node.Bounds.PaddingSize.Width - lineWidth) / 2;
        if (node.ComputedStyle.TextAlign.IsRight) x  += (node.Bounds.PaddingSize.Width - lineWidth);

        using SKPaint paint        = new();
        SKPoint       point        = new(x, y);
        Color         textColor    = node.ComputedStyle.Color;
        Color         outlineColor = node.ComputedStyle.OutlineColor ?? new(0);

        if (node.ComputedStyle.OutlineSize > 0 && null != node.ComputedStyle.OutlineColor) {
            paint.ImageFilter = null;
            paint.Color       = Color.ToSkColor(node.ComputedStyle.OutlineColor);
            paint.Style       = SKPaintStyle.Stroke;
            paint.StrokeWidth = node.ComputedStyle.OutlineSize;

            if (node.ComputedStyle.OutlineSize > 1) {
                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, node.ComputedStyle.OutlineSize);
            } else {
                paint.StrokeWidth = 3.0f; // 1.5f on each side.
            }

            font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, line, outlineColor, outlineColor);
        }

        if (node.ComputedStyle.TextShadowSize > 0) {
            paint.ImageFilter = paint.ImageFilter = SKImageFilter.CreateDropShadow(
                0,
                0,
                node.ComputedStyle.TextShadowSize,
                node.ComputedStyle.TextShadowSize,
                Color.ToSkColor(node.ComputedStyle.TextShadowColor ?? new(0xFF000000))
            );
        }

        paint.Color       = Color.ToSkColor(node.ComputedStyle.Color);
        paint.Style       = SKPaintStyle.Fill;
        paint.StrokeWidth = 0;
        paint.MaskFilter  = null;

        font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, line, textColor, outlineColor);
    }
}