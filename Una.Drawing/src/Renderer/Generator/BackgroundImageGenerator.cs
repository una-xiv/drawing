﻿using Una.Drawing.Texture;

namespace Una.Drawing.Generator;

public class BackgroundImageGenerator : IGenerator
{
    public int RenderOrder => 1000;

    /// <inheritdoc/>
    public void Generate(SKCanvas canvas, Node node)
    {
        if (null == node.ComputedStyle.BackgroundImage) return;

        using var paint = new SKPaint();

        Size     size   = node.Bounds.PaddingSize;
        EdgeSize inset  = node.ComputedStyle.BackgroundImageInset;
        Vector2  scale  = node.ComputedStyle.BackgroundImageScale;
        Color    color  = node.ComputedStyle.BackgroundImageColor;
        SKImage? image  = LoadImage(node.ComputedStyle.BackgroundImage);

        if (null == image) return;

        paint.Color = Color.ToSkColor(node.ComputedStyle.BackgroundImageColor);
        paint.Style = SKPaintStyle.Fill;

        SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees(node.ComputedStyle.BackgroundImageRotation);
        SKMatrix scaleMatrix    = SKMatrix.CreateScale(1f / scale.X, 1f / scale.Y);
        SKMatrix matrix         = rotationMatrix.PreConcat(scaleMatrix);

        using var shader = image
            .ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix)
            .WithColorFilter(SKColorFilter.CreateBlendMode(Color.ToSkColor(color), (SKBlendMode)node.ComputedStyle.BackgroundImageBlendMode));

        paint.Shader = shader;

        canvas.DrawRegion(new (new SKRectI(
            inset.Left,
            inset.Top,
            size.Width - inset.Right,
            size.Height - inset.Bottom
        )), paint);
    }

    private static SKImage? LoadImage(object? image)
    {
        return image switch {
            byte[] bytes => TextureLoader.LoadFromBytes(bytes),
            uint iconId  => TextureLoader.LoadIcon(iconId),
            _            => null
        };
    }
}
