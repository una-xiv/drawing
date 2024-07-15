namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private SKFont GetFont(Chunk chunk, int fontSize)
    {
        return chunk.Type == Chunk.Kind.Glyph ? GetGlyphFont(fontSize) : GetTextFont(fontSize);
    }

    /// <summary>
    /// Returns the width of the given chunk based on the specified font size.
    /// </summary>
    private float GetChunkWidth(Chunk chunk, int fontSize)
    {
        return GetFont(chunk, fontSize).MeasureText(chunk.Text);
    }

    /// <summary>
    /// Computes the width of the given chunk based on the specified font size
    /// and returns a new chunk with a cut-off text that fits within the
    /// specified maximum width.
    /// </summary>
    /// <returns>True if the original chunk fits, false otherwise.</returns>
    public bool BreakChunkAt(Chunk chunk, float x, float maxWidth, int fontSize, out Chunk result)
    {
        var font  = GetFont(chunk, fontSize);
        int bytes = font.BreakText(chunk.Text, maxWidth - x);

        if (bytes < chunk.Text.Length) {
            result = new(chunk.Type, bytes > 2 ? (chunk.Text[..(bytes - 2)] + '…') : chunk.Text[..bytes]);
            return false;
        }

        result = chunk;
        return true;
    }
}
