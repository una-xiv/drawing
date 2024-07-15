namespace Una.Drawing.Font;

internal partial class DynamicFont(SKTypeface textTypeface, SKTypeface glyphTypeface, float sizeOffset) : IFont
{
    private SKTypeface TextTypeface  { get; } = textTypeface;
    private SKTypeface GlyphTypeface { get; } = glyphTypeface;
    private float      SizeOffset    { get; } = sizeOffset;

    /// <inheritdoc/>
    public MeasuredText MeasureText(
        string text,
        int    fontSize     = 14,
        float  outlineSize  = 0,
        float? maxLineWidth = null,
        bool   wordWrap     = false,
        bool   textOverflow = true,
        float  lineHeight   = 1.2f
    )
    {
        if (wordWrap == false || maxLineWidth is null or 0) {
            return MeasureSingleLine(text, fontSize, outlineSize, maxLineWidth ?? 0, textOverflow);
        }

        return MeasureMultiLine(text, fontSize, outlineSize, maxLineWidth.Value, lineHeight);
    }

    /// <inheritdoc/>
    public void DrawText(SKCanvas canvas, SKPaint paint, SKPoint pos, int fontSize, string text)
    {
        if (text.Length == 0) return;

        List<Chunk> chunks = GenerateChunks(text);

        float x = pos.X;

        foreach (var chunk in chunks) {
            SKFont font = chunk.Type == Chunk.Kind.Glyph ? GetGlyphFont(fontSize) : GetTextFont(fontSize);
            canvas.DrawText(chunk.Text, x, pos.Y, font, paint);
            x += font.MeasureText(chunk.Text);
        }
    }

    /// <inheritdoc/>
    public SKFontMetrics GetMetrics(int fontSize)
    {
        return GetTextFont(fontSize).Metrics;
    }

    /// <inheritdoc/>
    public float GetLineHeight(int fontSize)
    {
        return (96 / 72) * GetTextFont(fontSize).Size;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (SKFont font in TextFontCache.Values) font.Dispose();
        foreach (SKFont font in GlyphFontCache.Values) font.Dispose();

        TextFontCache.Clear();
        GlyphFontCache.Clear();
    }
}
