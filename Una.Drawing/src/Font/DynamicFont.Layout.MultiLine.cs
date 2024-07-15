namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private MeasuredText MeasureMultiLine(
        string text,
        int    fontSize,
        float  outlineSize,
        float  maxWidth,
        float  lineHeightFactor
    )
    {
        List<Chunk>  chunks = GenerateChunks(text);
        List<string> lines  = [];

        float lineHeight = MathF.Ceiling(GetLineHeight(fontSize) + (outlineSize * 2.0f)) + 1;

        float width      = 0;
        float height     = 0;
        float spaceWidth = GetTextFont(fontSize).MeasureText(" ");
        var   buffer     = string.Empty;

        foreach (var chunk in chunks) {
            string[] words = chunk.Text.Split(' ');
            SKFont   font  = GetFont(chunk, fontSize);

            foreach (string word in words) {
                float wordWidth = font.MeasureText(word);

                if (width + wordWidth + spaceWidth > maxWidth) {
                    if (buffer.Length > 0) {
                        lines.Add(buffer.TrimStart());
                        height += (lineHeight * lineHeightFactor);
                    }

                    buffer = word;
                    width  = wordWidth;
                } else {
                    buffer += ' ' + word;
                    width  += spaceWidth + wordWidth;
                }
            }
        }

        if (buffer.Length > 0) {
            lines.Add(buffer.TrimStart());
            height += lines.Count == 1 ? lineHeight : (lineHeight / lineHeightFactor);
        }

        return new() {
            Lines     = lines.ToArray(),
            Size      = new((int)Math.Ceiling(width + (outlineSize * 2.0f)), (int)(height)),
            LineCount = (uint)lines.Count,
        };
    }
}
