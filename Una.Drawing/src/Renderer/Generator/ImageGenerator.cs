using Una.Drawing.Texture;

namespace Una.Drawing.Generator;

internal class ImageGenerator : IGenerator
{
    /// <inheritdoc/>
    public int RenderOrder => 3;

    /// <inheritdoc/>
    public bool Generate(SKCanvas canvas, Node node, Vector2 origin)
    {
        // Target rectangle within the node's padding box for drawing
        SKRect relativeRect = new(
            node.ComputedStyle.ImageInset?.Left ?? 0,
            node.ComputedStyle.ImageInset?.Top ?? 0,
            node.Bounds.PaddingSize.Width - (node.ComputedStyle.ImageInset?.Right ?? 0),
            node.Bounds.PaddingSize.Height - (node.ComputedStyle.ImageInset?.Bottom ?? 0)
        );

        if (relativeRect.IsEmpty) return false;

        SKImage? image = GetImage(node);
        if (image == null) return false;

        // Base user-defined scale factor, ensuring it's not too small
        float userScale = MathF.Max(0.1f, node.ComputedStyle.ImageScale) * Node.ScaleFactor;

        // Determine the tile mode first, as it affects the shader matrix calculation
        SKShaderTileMode tileMode = node.ComputedStyle.ImageTileMode switch {
            ImageTileMode.Clamp  => SKShaderTileMode.Clamp,
            ImageTileMode.Repeat => SKShaderTileMode.Repeat,
            ImageTileMode.Mirror => SKShaderTileMode.Mirror,
            ImageTileMode.Decal  => SKShaderTileMode.Decal,
            _ => throw new ArgumentOutOfRangeException(nameof(node.ComputedStyle.ImageTileMode),
                node.ComputedStyle.ImageTileMode, null)
        };

        SKMatrix imageTransformMatrix;
        SKMatrix shaderOffsetMatrix = SKMatrix.CreateTranslation(
            (node.ComputedStyle.ImageOffset?.X ?? 0),
            (node.ComputedStyle.ImageOffset?.Y ?? 0)
        );

        // --- Shader Matrix Calculation ---
        if (tileMode == SKShaderTileMode.Repeat || tileMode == SKShaderTileMode.Mirror) {
            // --- Tiling Mode Shader Matrix ---
            // For tiling, the shader needs the image's intrinsic scale (modified by userScale).
            // The drawing rect (`relativeRect`) acts as the clipping boundary.

            // 1. Scale Matrix: Based *only* on userScale
            SKMatrix tileScaleMatrix = SKMatrix.CreateScale(userScale, userScale);

            // 2. Rotation Matrix: Rotate the entire pattern around the center of the drawing rectangle.
            Vector2 rotationPivot = new Vector2(relativeRect.Width / 2f, relativeRect.Height / 2f);
            SKMatrix rotationMatrix =
                SKMatrix.CreateRotationDegrees(node.ComputedStyle.ImageRotation, rotationPivot.X, rotationPivot.Y);

            // 3. Combine: Scale -> Rotate -> Translate Offset
            // Apply transforms in order: Scale original image, Rotate the scaled space, Translate
            imageTransformMatrix = SKMatrix.Identity;
            imageTransformMatrix = imageTransformMatrix.PreConcat(shaderOffsetMatrix); // Apply offset last
            imageTransformMatrix = imageTransformMatrix.PreConcat(rotationMatrix);     // Apply rotation second
            imageTransformMatrix = imageTransformMatrix.PreConcat(tileScaleMatrix);    // Apply scale first
        } else                                                                         // Clamp or Decal Mode
        {
            // --- Non-Tiling Mode Shader Matrix (Original Logic) ---
            // Use the original logic where Adapt mode scales the image to fit the rect.

            // 1. Scale Matrix: Based on ImageScaleMode
            SKMatrix scaleMatrix = node.ComputedStyle.ImageScaleMode switch {
                ImageScaleMode.Adapt => SKMatrix.CreateScale(
                    (relativeRect.Width / image.Width) * userScale,
                    (relativeRect.Height / image.Height) * userScale),
                ImageScaleMode.Original => SKMatrix.CreateScale(userScale, userScale),
                _                       => SKMatrix.Identity // Or throw, but Identity is safer fallback
            };

            // 2. Pivot Point for Rotation: Based on ImageScaleMode
            //    (Using the corrected logic from previous step)
            Vector2 rotationPivot = node.ComputedStyle.ImageScaleMode switch {
                // Pivot at the center of the target rectangle
                ImageScaleMode.Adapt => new Vector2(relativeRect.Width / 2f, relativeRect.Height / 2f),
                // Pivot at the center of the *original* image (before scaling) for Original mode rotation
                ImageScaleMode.Original => new Vector2(image.Width / 2f, image.Height / 2f),
                _ => new Vector2(relativeRect.Width / 2f, relativeRect.Height / 2f) // Default pivot
            };

            // 3. Rotation Matrix: Rotate around the calculated pivot
            SKMatrix rotationMatrix =
                SKMatrix.CreateRotationDegrees(node.ComputedStyle.ImageRotation, rotationPivot.X, rotationPivot.Y);


            // 4. Combine: Scale -> Rotate -> Translate Offset
            imageTransformMatrix = SKMatrix.Identity;
            imageTransformMatrix = imageTransformMatrix.PreConcat(shaderOffsetMatrix); // Apply offset last
            imageTransformMatrix = imageTransformMatrix.PreConcat(rotationMatrix);     // Apply rotation second
            imageTransformMatrix = imageTransformMatrix.PreConcat(scaleMatrix);        // Apply scale first
        }
        // --- End Shader Matrix Calculation ---


        using SKPaint paint = new();
        paint.IsAntialias = node.ComputedStyle.IsAntialiased;
        paint.IsDither    = false;

        // Apply blur if specified
        if (node.ComputedStyle.ImageBlur != Vector2.Zero) {
            // Note: Blur's tileMode might interact interestingly with shader's tileMode. Usually matching them is best.
            using SKImageFilter blur = SKImageFilter.CreateBlur(
                node.ComputedStyle.ImageBlur.X,
                node.ComputedStyle.ImageBlur.Y,
                tileMode // Use the same tileMode for blur consistency
            );
            paint.ImageFilter = blur;
        }

        // Create the image shader with the final transformation matrix
        // The matrix tells the shader how to map image texture coordinates onto the canvas *before* tiling happens.
        SKShader imageShader = SKShader.CreateImage(image, tileMode, tileMode, imageTransformMatrix);

        // Apply color filters (order matters here too)
        // 1. High Contrast Filter
        imageShader = imageShader.WithColorFilter( // Use WithColorFilter to chain if previous shader exists
            SKColorFilter.CreateHighContrast(new() {
                Grayscale = node.ComputedStyle.ImageGrayscale, Contrast = node.ComputedStyle.ImageContrast
            })
        );
        // 2. Blend Mode Color Filter
        imageShader = imageShader.WithColorFilter(
            SKColorFilter.CreateBlendMode(
                Color.ToSkColor(node.ComputedStyle.ImageColor), // Assuming Color.ToSkColor exists
                (SKBlendMode)node.ComputedStyle.ImageBlendMode
            )
        );
        paint.Shader = imageShader;


        // --- Drawing ---
        int saveCount = canvas.Save();
        try {
            // Translate canvas origin to the node's origin + relativeRect top-left offset
            // This ensures drawing happens relative to the node's calculated position.
            // The shader matrix handles image placement *within* the relativeRect space.
            canvas.Translate(origin.X + relativeRect.Left, origin.Y + relativeRect.Top);

            // Define the drawing area based on relativeRect size (width/height)
            // Since we translated, the top-left for drawing is now (0,0) in the translated space.
            SKRect drawingRect = SKRect.Create(0, 0, relativeRect.Width, relativeRect.Height);

            float radius = node.ComputedStyle.ImageRounding;

            if (radius < 0.01f) {
                // Draw a rectangle covering the relativeRect area. The paint/shader fills it.
                canvas.DrawRect(drawingRect, paint);
            } else {
                // Draw a rounded rectangle covering the relativeRect area.
                var            style   = node.ComputedStyle;
                RoundedCorners corners = style.ImageRoundedCorners;

                // Define radii based on corners flags
                SKPoint topLeft     = corners.HasFlag(RoundedCorners.TopLeft) ? new(radius, radius) : SKPoint.Empty;
                SKPoint topRight    = corners.HasFlag(RoundedCorners.TopRight) ? new(radius, radius) : SKPoint.Empty;
                SKPoint bottomRight = corners.HasFlag(RoundedCorners.BottomRight) ? new(radius, radius) : SKPoint.Empty;
                SKPoint bottomLeft  = corners.HasFlag(RoundedCorners.BottomLeft) ? new(radius, radius) : SKPoint.Empty;

                using SKRoundRect roundRect = new();
                // Set the rect using the drawingRect (relative to the translated canvas)
                roundRect.SetRectRadii(drawingRect, [topLeft, topRight, bottomRight, bottomLeft]);

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