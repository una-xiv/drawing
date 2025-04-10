using System.Text;

namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private MeasuredText MeasureMultiLine(
        string text,
        int    fontSize,
        float  maxWidth,
        float  lineHeightFactor
    )
    {
        List<Chunk>  chunks = GenerateChunks(text);
        List<string> lines  = [];

        float lineHeight       = GetLineHeight(fontSize);
        float currentLineWidth = 0;
        float maxLineWidth     = 0;
        float totalHeight      = 0;
        var   lineBuffer       = new StringBuilder();
        
        foreach (var chunk in chunks) {
            SKFont chunkFont = GetFont(chunk, fontSize);
            string[] words =
                chunk.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            float spaceWidth = chunkFont.MeasureText(" ");

            foreach (string word in words) {
                float wordWidth         = chunkFont.MeasureText(word);
                float widthWithNextWord = currentLineWidth;

                if (lineBuffer.Length > 0) widthWithNextWord += spaceWidth;

                widthWithNextWord += wordWidth;

                if (lineBuffer.Length > 0 && widthWithNextWord + 1f >= maxWidth) {
                    string completedLine = lineBuffer.ToString();
                    lines.Add(completedLine);
                    totalHeight  += (lineHeight * lineHeightFactor);
                    maxLineWidth =  MathF.Max(maxLineWidth, currentLineWidth);

                    lineBuffer.Clear().Append(word);
                    currentLineWidth = wordWidth;
                    continue;
                }

                if (lineBuffer.Length > 0) {
                    lineBuffer.Append(' ');
                    currentLineWidth += spaceWidth;
                }

                lineBuffer.Append(word);
                currentLineWidth += wordWidth;
            }
        }

        if (lineBuffer.Length > 0) {
            string lastLine = lineBuffer.ToString();
            lines.Add(lastLine);
            totalHeight  += (lineHeight * lineHeightFactor);
            maxLineWidth =  MathF.Max(maxLineWidth, currentLineWidth);
        }

        return new() {
            Lines     = lines.ToArray(),
            Size      = new(MathF.Ceiling(maxLineWidth), MathF.Ceiling(totalHeight)),
            LineCount = (uint)lines.Count,
        };
    }
}