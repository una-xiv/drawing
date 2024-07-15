/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing.Generator;

internal class GradientGenerator : IGenerator
{
    public int RenderOrder => 1;

    public void Generate(SKCanvas canvas, Node node)
    {
        ComputedStyle style = node.ComputedStyle;
        Size          size  = node.Bounds.PaddingSize;

        if (null == style.BackgroundGradient || style.BackgroundGradient.IsEmpty) return;

        EdgeSize inset = style.BackgroundGradientInset;

        using var paint = new SKPaint();
        SKRect    rect  = new(inset.Left, inset.Top, size.Width - inset.Right, size.Height - inset.Bottom);

        paint.IsAntialias = true;
        paint.Style       = SKPaintStyle.Fill;
        paint.Shader      = CreateShader(size, style.BackgroundGradient, inset);

        if (style.BorderRadius == 0) {
            canvas.DrawRect(rect, paint);
            return;
        }

        var radius = (float)style.BorderRadius;

        RoundedCorners corners     = style.RoundedCorners;
        SKPoint        topLeft     = corners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : new(0, 0);
        SKPoint        topRight    = corners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomRight = corners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomLeft  = corners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : new(0, 0);

        using SKRoundRect roundRect = new SKRoundRect(rect, radius, radius);

        roundRect.SetRectRadii(rect, [topLeft, topRight, bottomRight, bottomLeft]);
        canvas.DrawRoundRect(roundRect, paint);
    }

    private static SKShader CreateShader(Size size, GradientColor gradientColor, EdgeSize inset)
    {
        return gradientColor.Type switch {
            GradientType.Horizontal => SKShader.CreateLinearGradient(
                new(0, 0),
                new(size.Width - inset.HorizontalSize, 0),
                new[] { Color.ToSkColor(gradientColor.Color1), Color.ToSkColor(gradientColor.Color2) },
                null,
                SKShaderTileMode.Clamp
            ),
            GradientType.Vertical => SKShader.CreateLinearGradient(
                new(0, 0),
                new(0, size.Height - inset.VerticalSize),
                new[] { Color.ToSkColor(gradientColor.Color1), Color.ToSkColor(gradientColor.Color2) },
                null,
                SKShaderTileMode.Clamp
            ),
            GradientType.Radial => SKShader.CreateRadialGradient(
                new(size.Width / 2, size.Height / 2),
                (size.Width - inset.HorizontalSize) / 2,
                new[] { Color.ToSkColor(gradientColor.Color1), Color.ToSkColor(gradientColor.Color2) },
                null,
                SKShaderTileMode.Clamp
            ),
            _ => throw new InvalidOperationException(nameof(gradientColor.Type))
        };
    }
}
