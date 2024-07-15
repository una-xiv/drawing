/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing.Generator;

internal class BackgroundGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 0;

    /// <inheritdoc/>
    public void Generate(SKCanvas canvas, Node node)
    {
        if (null == node.ComputedStyle.BackgroundColor) return;

        using var paint = new SKPaint();

        Size size = node.Bounds.PaddingSize;

        paint.Color       = Color.ToSkColor(node.ComputedStyle.BackgroundColor);
        paint.Style       = SKPaintStyle.Fill;

        if (node.ComputedStyle.BorderRadius == 0) {
            canvas.DrawRect(0, 0, size.Width, size.Height, paint);
            return;
        }

        paint.IsAntialias = node.ComputedStyle.IsAntialiased;

        var style  = node.ComputedStyle;
        var radius = (float)style.BorderRadius;

        RoundedCorners corners     = style.RoundedCorners;
        SKRect         rect        = new(0, 0, size.Width, size.Height);
        SKPoint        topLeft     = corners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : new(0, 0);
        SKPoint        topRight    = corners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomRight = corners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomLeft  = corners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : new(0, 0);

        using SKRoundRect roundRect = new SKRoundRect(rect, radius, radius);

        roundRect.SetRectRadii(rect, [topLeft, topRight, bottomRight, bottomLeft]);
        canvas.DrawRoundRect(roundRect, paint);
    }
}
