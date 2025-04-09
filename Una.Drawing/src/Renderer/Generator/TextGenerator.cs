using Una.Drawing.Font;

namespace Una.Drawing.Generator;

internal class TextGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 999;

    /// <inheritdoc/>
    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        if (node.NodeValue is not string str || string.IsNullOrWhiteSpace(str)) return false;

        MeasuredText? measurement = node.NodeValueMeasurement;
        if (null == measurement || measurement.Value.LineCount == 0) return false;

        Size  size       = node.NodeValueMeasurement!.Value.Size;
        IFont font       = FontRegistry.Fonts[node.ComputedStyle.Font];
        int   fontSize   = node.ComputedStyle.FontSize;
        float lineHeight = font.GetLineHeight(fontSize);
        var   metrics    = font.GetMetrics(node.ComputedStyle.FontSize);

        var y = origin.Y + (lineHeight + node.ComputedStyle.TextOffset.Y) - metrics.Descent;
        var x = origin.X + node.ComputedStyle.TextOffset.X + 1;

        y = (int)Math.Ceiling(y);

        if (node.ComputedStyle.TextAlign.IsTop) y    += node.ComputedStyle.Padding.Top;
        if (node.ComputedStyle.TextAlign.IsLeft) x   += node.ComputedStyle.Padding.Left;
        if (node.ComputedStyle.TextAlign.IsRight) x  -= node.ComputedStyle.Padding.Right;
        if (node.ComputedStyle.TextAlign.IsMiddle) y += (node.Height - size.Height) / 2f;
        if (node.ComputedStyle.TextAlign.IsBottom) y += node.Height - size.Height;

        foreach (string line in node.NodeValueMeasurement!.Value.Lines) {
            PrintLine(canvas, font, node, line, x, y);
            y += (float)(lineHeight * node.ComputedStyle.LineHeight);
        }

        return true;
    }

    private static void PrintLine(SKCanvas canvas, IFont font, Node node, string line, float x, float y)
    {
        MeasuredText measurement = font.MeasureText(line, node.ComputedStyle.FontSize, node.ComputedStyle.OutlineSize);
        float        lineWidth   = measurement.Size.Width;

        if (node.ComputedStyle.TextAlign.IsCenter) x += (node.Bounds.PaddingSize.Width - lineWidth) / 2;
        if (node.ComputedStyle.TextAlign.IsRight)
            x += (node.Bounds.PaddingSize.Width - lineWidth) + (node.ComputedStyle.OutlineSize * 2);

        using SKPaint paint = new();
        SKPoint       point = new(x, y);

        if (node.ComputedStyle.OutlineSize > 0 && null != node.ComputedStyle.OutlineColor) {
            paint.ImageFilter = null;
            paint.Color       = Color.ToSkColor(node.ComputedStyle.OutlineColor);
            paint.Style       = SKPaintStyle.Stroke;
            paint.StrokeWidth = node.ComputedStyle.OutlineSize;
            paint.MaskFilter  = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, node.ComputedStyle.OutlineSize);
            font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, line);
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

        font.DrawText(canvas, paint, point, node.ComputedStyle.FontSize, line);
    }
}