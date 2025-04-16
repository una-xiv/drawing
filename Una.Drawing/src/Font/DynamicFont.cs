using Dalamud.Game.Text.SeStringHandling;
using Una.Drawing.Texture;

namespace Una.Drawing.Font;

internal partial class DynamicFont(SKTypeface textTypeface, SKTypeface glyphTypeface, float sizeOffset) : IFont
{
    private SKTypeface TextTypeface  { get; } = textTypeface;
    private SKTypeface GlyphTypeface { get; } = glyphTypeface;
    private float      SizeOffset    { get; } = sizeOffset;

    public MeasuredText MeasureText(
        object text,
        int    fontSize     = 14,
        float? maxLineWidth = null,
        bool   wordWrap     = false,
        bool   textOverflow = true,
        float  lineHeight   = 1.2f,
        float? maxWidth     = null,
        Color  textColor    = default,
        Color  edgeColor    = default
    )
    {
        if (maxWidth > 0) {
            maxLineWidth = maxWidth;
            wordWrap     = false;
            textOverflow = false;
        }

        if (wordWrap == false || maxLineWidth is null or 0) {
            return MeasureSingleLine(text, fontSize, maxLineWidth ?? 0, textOverflow, textColor, edgeColor);
        }
        return MeasureMultiLine(text, fontSize, maxLineWidth.Value, lineHeight, textColor, edgeColor);
    }

    public void DrawText(SKCanvas canvas, SKPaint paint, SKPoint pos, int fontSize, Chunk[] chunks, Color textColor, Color edgeColor)
    {
        if (chunks.Length == 0) return;

        float x = pos.X;
        float y = pos.Y;

        foreach (var chunk in chunks) {
            switch (chunk.Type) {
                case Chunk.Kind.Text:
                case Chunk.Kind.Glyph:
                    SKFont font = chunk.Type == Chunk.Kind.Glyph ? GetGlyphFont(fontSize) : GetTextFont(fontSize);
                    canvas.DrawText(chunk.Text, x, pos.Y, font, paint);
                    break;
                case Chunk.Kind.BitmapIcon: {
                    using SKPaint gfdPaint = new();

                    GfdIcon icon = chunk.BitmapIcon!.Value;

                    canvas.DrawImage(
                        icon.Texture,
                        new(icon.Uv0.X, icon.Uv0.Y, icon.Uv1.X, icon.Uv1.Y),
                        new SKRect(
                            x,
                            y - (icon.Size.Y / 2f) - 4,
                            x + icon.Size.X,
                            y + (icon.Size.Y / 2f) - 4
                        ),
                        gfdPaint
                    );
                    break;
                }
            }
            
            x += chunk.Width;
        }
    }

    public SKFontMetrics GetMetrics(int fontSize)
    {
        return GetTextFont(fontSize).Metrics;
    }

    public float GetLineHeight(int fontSize)
    {
        SKFontMetrics metrics = GetTextFont(fontSize).Metrics;
        return metrics.Descent - metrics.Ascent + metrics.Leading;
    }

    public void Dispose()
    {
        foreach (SKFont font in TextFontCache.Values) font.Dispose();
        foreach (SKFont font in GlyphFontCache.Values) font.Dispose();

        TextFontCache.Clear();
        GlyphFontCache.Clear();
    }
}