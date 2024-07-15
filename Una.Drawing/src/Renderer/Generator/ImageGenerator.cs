/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using Una.Drawing.Texture;

namespace Una.Drawing.Generator;

internal class ImageGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 3;

    /// <inheritdoc/>
    public void Generate(SKCanvas canvas, Node node)
    {
        SKImage? image;

        if (node.ComputedStyle.ImageBytes is not null) {
            image = TextureLoader.LoadFromBytes(node.ComputedStyle.ImageBytes);
        } else if (node.ComputedStyle.IconId is not null) {
            image = TextureLoader.LoadIcon(node.ComputedStyle.IconId.Value);
        } else if (!string.IsNullOrWhiteSpace(node.ComputedStyle.UldResource) && node.ComputedStyle is { UldPartsId: not null, UldPartId: not null }) {
            var uld = TextureLoader.LoadUld(node.ComputedStyle.UldResource, node.ComputedStyle.UldPartsId.Value, node.ComputedStyle.UldPartId.Value);

            if (!uld.HasValue) return;

            var uldVal = uld.Value;

            image = uldVal.Texture.Subset(uldVal.Rect);
        } else {
            return;
        }

        if (image == null) return;

        SKRect rect = new(
            node.ComputedStyle.ImageInset?.Left ?? 0,
            node.ComputedStyle.ImageInset?.Top ?? 0,
            node.Bounds.PaddingSize.Width - (node.ComputedStyle.ImageInset?.Right ?? 0),
            node.Bounds.PaddingSize.Height - (node.ComputedStyle.ImageInset?.Bottom ?? 0)
        );

        if (rect.IsEmpty) return;

        SKMatrix scaleMatrix    = SKMatrix.CreateScale(rect.Width / image.Width, rect.Height / image.Height);
        SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees(node.ComputedStyle.ImageRotation);

        SKMatrix transformMatrix = SKMatrix.CreateTranslation(
            (node.ComputedStyle.ImageOffset?.X ?? 0) + (node.ComputedStyle.ImageInset?.Left ?? 0),
            (node.ComputedStyle.ImageOffset?.Y ?? 0) + (node.ComputedStyle.ImageInset?.Top ?? 0)
        );

        SKMatrix matrix = rotationMatrix.PreConcat(scaleMatrix.PreConcat(transformMatrix));

        using SKPaint  paint  = new();
        using SKShader shader = SKShader.CreateImage(image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, matrix);

        paint.Shader      = shader;
        paint.IsAntialias = node.ComputedStyle.IsAntialiased;
        paint.Style       = SKPaintStyle.Fill;

        paint.Shader = SKShader
            .CreateColorFilter(
                SKShader.CreateImage(image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, matrix),
                SKColorFilter.CreateHighContrast(
                    new() {
                        Grayscale = node.ComputedStyle.ImageGrayscale,
                        Contrast  = node.ComputedStyle.ImageContrast
                    }
                )
            )
            .WithColorFilter(
                SKColorFilter.CreateBlendMode(
                    Color.ToSkColor(node.ComputedStyle.ImageColor),
                    (SKBlendMode)node.ComputedStyle.ImageBlendMode
                )
            );

        float radius = node.ComputedStyle.ImageRounding;

        rect = new(
            rect.Left + (node.ComputedStyle.ImageOffset?.X ?? 0),
            rect.Top + (node.ComputedStyle.ImageOffset?.Y ?? 0),
            rect.Right + (node.ComputedStyle.ImageOffset?.X ?? 0),
            rect.Bottom + (node.ComputedStyle.ImageOffset?.Y ?? 0)
        );

        if (radius < 0.01f) {
            canvas.DrawRect(rect, paint);
            return;
        }

        var style = node.ComputedStyle;

        RoundedCorners corners     = style.ImageRoundedCorners;
        SKPoint        topLeft     = corners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : new(0, 0);
        SKPoint        topRight    = corners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomRight = corners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : new(0, 0);
        SKPoint        bottomLeft  = corners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : new(0, 0);

        using SKRoundRect roundRect = new SKRoundRect(rect, radius, radius);

        roundRect.SetRectRadii(rect, [topLeft, topRight, bottomRight, bottomLeft]);
        canvas.DrawRoundRect(roundRect, paint);
    }
}
