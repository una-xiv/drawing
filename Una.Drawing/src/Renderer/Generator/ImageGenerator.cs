using Una.Drawing.Texture;
using SkiaSharp;

namespace Una.Drawing.Generator;

internal class ImageGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 3;

    /// <inheritdoc/>
    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        SKRect relativeRect = new(
            node.ComputedStyle.ImageInset?.Left ?? 0,
            node.ComputedStyle.ImageInset?.Top ?? 0,
            node.Bounds.PaddingSize.Width - (node.ComputedStyle.ImageInset?.Right ?? 0),
            node.Bounds.PaddingSize.Height - (node.ComputedStyle.ImageInset?.Bottom ?? 0)
        );
 
        if (relativeRect.IsEmpty) return false;

        SKImage? image = GetImage(node);
        if (image == null) return false;

        float scale = MathF.Max(0.1f, node.ComputedStyle.ImageScale) * Node.ScaleFactor;

        // Calculate the scale matrix based on the mode
        SKMatrix scaleMatrix = node.ComputedStyle.ImageScaleMode switch {
            // Scale to fit the relative rectangle dimensions
            ImageScaleMode.Adapt => SKMatrix.CreateScale(relativeRect.Width / image.Width * scale,
                relativeRect.Height / image.Height * scale),
            // Scale based on original image size
            ImageScaleMode.Original => SKMatrix.CreateScale(scale, scale),
            _ => throw new ArgumentOutOfRangeException(nameof(node.ComputedStyle.ImageScaleMode),
                node.ComputedStyle.ImageScaleMode, null)
        };

        // Calculate the pivot point for rotation and scaling, relative to the top-left of relativeRect (0,0)
        Vector2 pivot = node.ComputedStyle.ImageScaleMode switch {
            // Pivot at the center of the target rectangle
            ImageScaleMode.Adapt => new Vector2(relativeRect.Width / 2f, relativeRect.Height / 2f),
            // Pivot at the center of the original scaled image
            ImageScaleMode.Original => new Vector2((image.Width / 2f) * scale, (image.Height / 2f) * scale),
            _ => throw new ArgumentOutOfRangeException(nameof(node.ComputedStyle.ImageScaleMode),
                node.ComputedStyle.ImageScaleMode, null)
        };

        // Calculate the rotation matrix around the calculated pivot point
        SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees(node.ComputedStyle.ImageRotation, pivot.X, pivot.Y);

        // Calculate the translation matrix ONLY for the ImageOffset.
        SKMatrix shaderOffsetMatrix = SKMatrix.CreateTranslation(
            (node.ComputedStyle.ImageOffset?.X ?? 0),
            (node.ComputedStyle.ImageOffset?.Y ?? 0)
        );

        // Combine transformations for the shader matrix. Order: Scale -> Rotate -> Translate Offset
        SKMatrix imageTransformMatrix = shaderOffsetMatrix;
        imageTransformMatrix = imageTransformMatrix.PreConcat(rotationMatrix);
        imageTransformMatrix = imageTransformMatrix.PreConcat(scaleMatrix);

        SKShaderTileMode tileMode = node.ComputedStyle.ImageTileMode switch {
            ImageTileMode.Clamp  => SKShaderTileMode.Clamp,
            ImageTileMode.Repeat => SKShaderTileMode.Repeat,
            ImageTileMode.Mirror => SKShaderTileMode.Mirror,
            ImageTileMode.Decal  => SKShaderTileMode.Decal,
            _ => throw new ArgumentOutOfRangeException(nameof(node.ComputedStyle.ImageTileMode),
                node.ComputedStyle.ImageTileMode, null)
        };

        using SKPaint paint = new();
        paint.IsAntialias = node.ComputedStyle.IsAntialiased;
        paint.IsDither    = false;

        // Apply blur if specified
        if (node.ComputedStyle.ImageBlur != Vector2.Zero) {
            using SKImageFilter blur = SKImageFilter.CreateBlur(
                node.ComputedStyle.ImageBlur.X,
                node.ComputedStyle.ImageBlur.Y,
                tileMode
            );
            paint.ImageFilter = blur;
        }

        // Create the image shader with the final transformation matrix
        SKShader imageShader = SKShader.CreateImage(image, tileMode, tileMode, imageTransformMatrix);

        // Apply color filters
        imageShader = SKShader.CreateColorFilter(
            imageShader,
            SKColorFilter.CreateHighContrast(new() {
                Grayscale = node.ComputedStyle.ImageGrayscale, Contrast = node.ComputedStyle.ImageContrast
            })
        );
        imageShader = imageShader.WithColorFilter(
            SKColorFilter.CreateBlendMode(
                Color.ToSkColor(node.ComputedStyle.ImageColor),
                (SKBlendMode)node.ComputedStyle.ImageBlendMode
            )
        );
        paint.Shader = imageShader;

        int saveCount = canvas.Save();
        try {
            canvas.Translate(origin.X, origin.Y);

            float radius = node.ComputedStyle.ImageRounding;

            if (radius < 0.01f) {
                canvas.DrawRect(relativeRect, paint);
            } else {
                var            style   = node.ComputedStyle;
                RoundedCorners corners = style.ImageRoundedCorners;
                
                SKPoint topLeft     = corners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : SKPoint.Empty;
                SKPoint topRight    = corners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : SKPoint.Empty;
                SKPoint bottomRight = corners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : SKPoint.Empty;
                SKPoint bottomLeft  = corners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : SKPoint.Empty;

                using SKRoundRect roundRect = new();
                roundRect.SetRectRadii(relativeRect, [topLeft, topRight, bottomRight, bottomLeft]);

                canvas.DrawRoundRect(roundRect, paint);
            }
        } finally {
            canvas.RestoreToCount(saveCount);
        }

        return true;
    }

    private SKImage? GetImage(Node node)
    {
        if (node.ComputedStyle.ImageBytes is not null) {
            return TextureLoader.LoadFromBytes(node.ComputedStyle.ImageBytes);
        }

        if (node.ComputedStyle.IconId is not null) {
            return TextureLoader.LoadIcon(node.ComputedStyle.IconId.Value);
        }

        if (!string.IsNullOrWhiteSpace(node.ComputedStyle.UldResource) && node.ComputedStyle is
            { UldPartsId: not null, UldPartId: not null }) {
            var uld = TextureLoader.LoadUld(
                node.ComputedStyle.UldResource,
                node.ComputedStyle.UldPartsId.Value,
                node.ComputedStyle.UldPartId.Value,
                node.ComputedStyle.UldStyle ?? UldStyle.Default
            );

            return uld?.Texture.Subset(new SKRectI(
                (int)uld.Value.Rect.X1,
                (int)uld.Value.Rect.Y1,
                (int)uld.Value.Rect.X2,
                (int)uld.Value.Rect.Y2
            ));
        }

        return null;
    }
}