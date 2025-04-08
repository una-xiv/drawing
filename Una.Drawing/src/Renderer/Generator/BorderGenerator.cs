namespace Una.Drawing.Generator;

internal class BorderGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 10;

    /// <inheritdoc/>
    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        Size          size  = node.Bounds.PaddingSize;
        ComputedStyle style = node.ComputedStyle;

        DrawStroke(canvas, size, style, origin);

        if (null == style.BorderColor) return false;
        if (style.BorderWidth is { HorizontalSize: 0, VerticalSize: 0 }) return false;

        EdgeSize inset       = style.BorderInset;
        float    topWidth    = style.BorderWidth.Top;
        float    rightWidth  = style.BorderWidth.Right;
        float    bottomWidth = style.BorderWidth.Bottom;
        float    leftWidth   = style.BorderWidth.Left;

        // FIXME: This isn't right. Corner radius should respect individual edge sizes now.
        float topCornerRadius    = Math.Max(0, (style.BorderRadius) - (style.BorderInset.Top));
        float rightCornerRadius  = Math.Max(0, (style.BorderRadius) - (style.BorderInset.Right));
        float bottomCornerRadius = Math.Max(0, (style.BorderRadius) - (style.BorderInset.Bottom));
        float leftCornerRadius   = Math.Max(0, (style.BorderRadius) - (style.BorderInset.Left));

        var rect = new SKRect(
            origin.X + inset.Left,
            origin.Y + inset.Top,
            origin.X + size.Width - inset.Right,
            origin.Y + size.Height - inset.Bottom
        );

        Color? topColor    = style.BorderColor.Value.Top;
        Color? rightColor  = style.BorderColor.Value.Right;
        Color? leftColor   = style.BorderColor.Value.Left;
        Color? bottomColor = style.BorderColor.Value.Bottom;

        using SKPaint paint = new SKPaint();
        paint.IsAntialias = node.ComputedStyle.IsAntialiased;
        paint.IsStroke    = true;
        paint.Style       = SKPaintStyle.Stroke;

        if (topWidth > 0 && topColor is not null) {
            paint.Color       = Color.ToSkColor(topColor);
            paint.StrokeWidth = topWidth;

            canvas.DrawLine(
                rect.Left + (style.RoundedCorners.HasFlag(RoundedCorners.TopLeft) ? topCornerRadius : 0),
                rect.Top,
                rect.Right - (style.RoundedCorners.HasFlag(RoundedCorners.TopRight) ? topCornerRadius : 0),
                rect.Top,
                paint
            );

            if (style.RoundedCorners.HasFlag(RoundedCorners.TopLeft)) {
                canvas.DrawArc(
                    new(rect.Left, rect.Top, rect.Left + 2 * leftCornerRadius, rect.Top + 2 * topCornerRadius),
                    -135,
                    45,
                    false,
                    paint
                );
            }

            if (style.RoundedCorners.HasFlag(RoundedCorners.TopRight)) {
                canvas.DrawArc(
                    new(rect.Right - 2 * rightCornerRadius, rect.Top, rect.Right, rect.Top + 2 * topCornerRadius),
                    -90,
                    45,
                    false,
                    paint
                );
            }
        }

        if (rightWidth > 0 && rightColor is not null) {
            paint.Color       = Color.ToSkColor(rightColor);
            paint.StrokeWidth = rightWidth;

            canvas.DrawLine(
                rect.Right,
                rect.Top + (style.RoundedCorners.HasFlag(RoundedCorners.TopRight) ? topCornerRadius : 0),
                rect.Right,
                rect.Bottom - (style.RoundedCorners.HasFlag(RoundedCorners.BottomRight) ? bottomCornerRadius : 0),
                paint
            );

            if (style.RoundedCorners.HasFlag(RoundedCorners.TopRight)) {
                canvas.DrawArc(
                    new(rect.Right - 2 * rightCornerRadius, rect.Top, rect.Right, rect.Top + 2 * topCornerRadius),
                    -45,
                    45,
                    false,
                    paint
                );
            }

            if (style.RoundedCorners.HasFlag(RoundedCorners.BottomRight)) {
                canvas.DrawArc(
                    new(
                        rect.Right - 2 * rightCornerRadius,
                        rect.Bottom - 2 * bottomCornerRadius,
                        rect.Right,
                        rect.Bottom
                    ),
                    0,
                    45,
                    false,
                    paint
                );
            }
        }

        if (bottomWidth > 0 && bottomColor is not null) {
            paint.Color       = Color.ToSkColor(bottomColor);
            paint.StrokeWidth = bottomWidth;

            canvas.DrawLine(
                rect.Left + (style.RoundedCorners.HasFlag(RoundedCorners.BottomLeft) ? leftCornerRadius : 0),
                rect.Bottom,
                rect.Right - (style.RoundedCorners.HasFlag(RoundedCorners.BottomRight) ? bottomCornerRadius : 0),
                rect.Bottom,
                paint
            );

            if (style.RoundedCorners.HasFlag(RoundedCorners.BottomRight)) {
                canvas.DrawArc(
                    new(
                        rect.Right - 2 * rightCornerRadius,
                        rect.Bottom - 2 * bottomCornerRadius,
                        rect.Right,
                        rect.Bottom
                    ),
                    45,
                    45,
                    false,
                    paint
                );
            }

            if (style.RoundedCorners.HasFlag(RoundedCorners.BottomLeft)) {
                canvas.DrawArc(
                    new(rect.Left, rect.Bottom - 2 * leftCornerRadius, rect.Left + 2 * bottomCornerRadius, rect.Bottom),
                    90,
                    45,
                    false,
                    paint
                );
            }
        }

        if (leftWidth > 0 && leftColor is not null) {
            paint.Color       = Color.ToSkColor(leftColor);
            paint.StrokeWidth = leftWidth;

            canvas.DrawLine(
                rect.Left,
                rect.Top + (style.RoundedCorners.HasFlag(RoundedCorners.TopLeft) ? topCornerRadius : 0),
                rect.Left,
                rect.Bottom - (style.RoundedCorners.HasFlag(RoundedCorners.BottomLeft) ? bottomCornerRadius : 0),
                paint
            );

            if (style.RoundedCorners.HasFlag(RoundedCorners.TopLeft)) {
                canvas.DrawArc(
                    new(rect.Left, rect.Top, rect.Left + 2 * leftCornerRadius, rect.Top + 2 * topCornerRadius),
                    180,
                    45,
                    false,
                    paint
                );
            }

            if (style.RoundedCorners.HasFlag(RoundedCorners.BottomLeft)) {
                canvas.DrawArc(
                    new(rect.Left, rect.Bottom - 2 * leftCornerRadius, rect.Left + 2 * bottomCornerRadius, rect.Bottom),
                    135,
                    45,
                    false,
                    paint
                );
            }
        }

        return true;
    }

    private static bool DrawStroke(SKCanvas canvas, Size size, ComputedStyle style, Vector2 origin)
    {
        if (!(style.StrokeColor?.IsVisible ?? false) || style.StrokeWidth == 0) return false;

        using var paint = new SKPaint();

        float  inset = style.StrokeInset + (style.StrokeWidth / 2f);
        SKRect rect  = new(origin.X + inset, origin.Y + inset, origin.X + (size.Width - inset), origin.Y + (size.Height - inset));

        paint.IsAntialias = style.IsAntialiased;
        paint.Color       = Color.ToSkColor(style.StrokeColor);
        paint.Style       = SKPaintStyle.Stroke;
        paint.StrokeWidth = style.StrokeWidth;

        if (style.BorderRadius == 0) {
            canvas.DrawRect(rect, paint);
            return true;
        }

        float   radius   = style.StrokeRadius ?? style.BorderRadius;
        SKPoint topLeft  = style.RoundedCorners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : new(0, 0);
        SKPoint topRight = style.RoundedCorners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : new(0, 0);

        SKPoint bottomRight =
            style.RoundedCorners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : new(0, 0);

        SKPoint bottomLeft = style.RoundedCorners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : new(0, 0);

        using SKRoundRect roundRect = new SKRoundRect(rect, radius, radius);
        roundRect.SetRectRadii(rect, [topLeft, topRight, bottomRight, bottomLeft]);

        canvas.DrawRoundRect(roundRect, paint);

        return true;
    }
}