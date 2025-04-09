namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private MeasuredText MeasureSingleLine(
        string text,
        int    fontSize,
        float  outlineSize,
        float  maxWidth,
        bool   textOverflow
    )
    {
        List<Chunk> chunks = GenerateChunks(text);
        float       width  = 0;
        float       height = (int)Math.Ceiling(GetLineHeight(fontSize) + (outlineSize * 2.0f));
        var         buffer = string.Empty;

        if (textOverflow || maxWidth == 0 || maxWidth >= float.MaxValue - 1f) {
            foreach (var chunk in chunks) {
                // Allow overflowing text over max width if specified.
                width  += GetChunkWidth(chunk, fontSize);
                buffer += chunk.Text;
            }
        } else {
            foreach (var chunk in chunks) {
                if (BreakChunkAt(chunk, width, maxWidth, fontSize, out var result)) {
                    width  += GetChunkWidth(result, fontSize);
                    buffer += result.Text;
                } else {
                    buffer += result.Text;
                    width  += GetChunkWidth(result, fontSize);
                    width  =  Math.Min(width, maxWidth);
                    break;
                }
            }
        }

        if (maxWidth == 0) maxWidth = float.MaxValue;

        return new() {
            Lines     = [buffer],
            Size      = new((int)Math.Min(maxWidth, Math.Ceiling(width)), (int)height),
            LineCount = 1,
        };
    }
}