﻿/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

[StructLayout(LayoutKind.Sequential)]
public partial struct ComputedStyle
{
    /// <inheritdoc cref="Style.IsVisible"/>
    public bool IsVisible;

    /// <inheritdoc cref="Style.Anchor"/>
    public Anchor Anchor;

    /// <inheritdoc cref="Style.Size"/>
    public Size Size;

    /// <inheritdoc cref="Style.Flow"/>
    public Flow Flow;

    /// <inheritdoc cref="Style.Gap"/>
    public int Gap;

    /// <inheritdoc cref="Style.Padding"/>
    public EdgeSize Padding;

    /// <inheritdoc cref="Style.Margin"/>
    public EdgeSize Margin;

    public bool Stretch;

    /// <inheritdoc cref="Style.Color"/>
    public Color Color;

    /// <inheritdoc cref="Style.Font"/>
    public uint Font;

    /// <inheritdoc cref="Style.FontSize"/>
    public int FontSize;

    /// <inheritdoc cref="Style.LineHeight"/>
    public float LineHeight;

    /// <inheritdoc cref="Style.WordWrap"/>
    public bool WordWrap;

    /// <inheritdoc cref="Style.TextAlign"/>
    public Anchor TextAlign;

    /// <inheritdoc cref="Style.OutlineSize"/>
    public float OutlineSize;

    /// <inheritdoc cref="Style.TextOffset"/>
    public Vector2 TextOffset;

    /// <inheritdoc cref="Style.TextOverflow"/>
    public bool TextOverflow;

    /// <inheritdoc cref="Style.BackgroundColor"/>
    public Color? BackgroundColor;

    /// <inheritdoc cref="Style.BorderColor"/>
    public BorderColor? BorderColor;

    /// <inheritdoc cref="Style.BorderInset"/>
    public EdgeSize BorderInset;

    /// <inheritdoc cref="Style.BorderRadius"/>
    public int BorderRadius;

    /// <inheritdoc cref="Style.BorderWidth"/>
    public EdgeSize BorderWidth;

    /// <inheritdoc cref="Style.StrokeColor"/>
    public Color? StrokeColor;

    /// <inheritdoc cref="Style.StrokeWidth"/>
    public int StrokeWidth;

    /// <inheritdoc cref="Style.StrokeInset"/>
    public float StrokeInset;

    /// <inheritdoc cref="Style.RoundedCorners"/>
    public float? StrokeRadius;

    /// <inheritdoc cref="Style.RoundedCorners"/>
    public RoundedCorners RoundedCorners;

    /// <inheritdoc cref="Style.BackgroundGradient"/>
    public GradientColor? BackgroundGradient;

    /// <inheritdoc cref="Style.BackgroundGradientInset"/>
    public EdgeSize BackgroundGradientInset;

    /// <inheritdoc cref="Style.BackgroundImage"/>
    public object? BackgroundImage;

    /// <inheritdoc cref="Style.BackgroundImageInset"/>
    public EdgeSize BackgroundImageInset;

    /// <inheritdoc cref="Style.BackgroundImageScale"/>
    public Vector2 BackgroundImageScale;

    /// <inheritdoc cref="Style.BackgroundImageColor"/>
    public Color BackgroundImageColor;

    /// <inheritdoc cref="Style.BackgroundImageRotation"/>
    public short BackgroundImageRotation;

    /// <inheritdoc cref="Style.BackgroundImageBlendMode"/>
    public BlendMode BackgroundImageBlendMode;

    /// <inheritdoc cref="Style.OutlineColor"/>
    public Color? OutlineColor;

    /// <inheritdoc cref="Style.TextShadowSize"/>
    public float TextShadowSize;

    /// <inheritdoc cref="Style.TextShadowColor"/>
    public Color? TextShadowColor;

    /// <inheritdoc cref="Style.IconId"/>
    public uint? IconId;

    /// <inheritdoc cref="Style.ImageBytes"/>
    public byte[]? ImageBytes;

    /// <inheritdoc cref="Style.ImageInset"/>
    public EdgeSize? ImageInset;

    /// <inheritdoc cref="Style.ImageOffset"/>
    public Vector2? ImageOffset;

    /// <inheritdoc cref="Style.ImageRounding"/>
    public float ImageRounding;

    /// <inheritdoc cref="Style.ImageRoundedCorners"/>
    public RoundedCorners ImageRoundedCorners;

    /// <inheritdoc cref="Style.ImageGrayscale"/>
    public bool ImageGrayscale;

    /// <inheritdoc cref="Style.ImageContrast"/>
    public float ImageContrast;

    /// <inheritdoc cref="Style.ImageRotation"/>
    public short ImageRotation;

    /// <inheritdoc cref="Style.ImageColor"/>
    public Color ImageColor;

    /// <inheritdoc cref="Style.ImageBlendMode"/>
    public BlendMode ImageBlendMode;

    /// <inheritdoc cref="Style.Opacity"/>
    public float Opacity;

    /// <inheritdoc cref="Style.ShadowSize"/>
    public EdgeSize ShadowSize;

    /// <inheritdoc cref="Style.ShadowInset"/>
    public int ShadowInset;

    /// <inheritdoc cref="Style.ShadowOffset"/>
    public Vector2 ShadowOffset;

    /// <inheritdoc cref="Style.IsAntialiased"/>
    public bool IsAntialiased;

    /// <inheritdoc cref="Style.ScrollbarTrackColor"/>
    public Color ScrollbarTrackColor;

    /// <inheritdoc cref="Style.ScrollbarThumbColor"/>
    public Color ScrollbarThumbColor;

    /// <inheritdoc cref="Style.ScrollbarThumbHoverColor"/>
    public Color ScrollbarThumbHoverColor;

    /// <inheritdoc cref="Style.ScrollbarThumbActiveColor"/>
    public Color ScrollbarThumbActiveColor;

    /// <inheritdoc cref="Style.UldResource"/>
    public string? UldResource;

    /// <inheritdoc cref="Style.UldPartsId"/>
    public int? UldPartsId { get; set; }

    /// <inheritdoc cref="Style.UldPartId"/>
    public int? UldPartId { get; set; }

    internal PaintStyleSnapshot  PaintStyleSnapshot;
    internal LayoutStyleSnapshot LayoutStyleSnapshot;
}
