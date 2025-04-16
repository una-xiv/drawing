namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private MeasuredText MeasureSingleLine(
        object text,
        int    fontSize,
        float  maxWidth,
        bool   textOverflow,
        Color  textColor,
        Color  edgeColor
    )
    {
        List<Chunk> chunks = GenerateChunks(text, fontSize, textColor, edgeColor);
        List<Chunk> line   = [];

        float width  = 0;
        float height = (int)Math.Ceiling(GetLineHeight(fontSize));

        if (textOverflow || maxWidth == 0 || maxWidth >= float.MaxValue - 1f) {
            foreach (var chunk in chunks) {
                width += chunk.Width;
                line.Add(chunk);
            }
        } else {
            foreach (var chunk in chunks) {
                if (BreakChunkAt(chunk, width, maxWidth, fontSize, out var result)) {
                    width += chunk.Width;
                    line.Add(result);
                } else {
                    width += chunk.Width;
                    width =  Math.Min(width, maxWidth);
                    line.Add(result);
                    break;
                }
            }
        }

        if (maxWidth == 0) maxWidth = float.MaxValue;

        return new() { Lines = [line.ToArray()], Size = new((int)Math.Min(maxWidth, Math.Ceiling(width)), (int)height), LineCount = 1, };
    }
}