namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private static List<Chunk> GenerateChunks(string text)
    {
        var         buffer   = string.Empty;
        var         chunks   = new List<Chunk>();
        Chunk.Kind? lastKind = null;

        foreach (char c in text) {
            Chunk.Kind kind = CharIsGlyph(c) ? Chunk.Kind.Glyph : Chunk.Kind.Text;

            if (lastKind != kind) {
                if (lastKind != null && buffer.Length > 0) {
                    chunks.Add(new(lastKind.Value, buffer));
                }

                buffer   = string.Empty;
                lastKind = kind;
            }

            buffer += c;
        }

        if (buffer.Length > 0) {
            chunks.Add(new(lastKind ?? Chunk.Kind.Text, buffer));
        }

        return chunks;
    }

    private static bool CharIsGlyph(char c)
    {
        return c >= 0xE020 && c <= 0xE0DB;
    }

    // [GeneratedRegex(@"[\uE020-\uE0DB]")]

    internal readonly struct Chunk(Chunk.Kind kind, string text)
    {
        internal enum Kind
        {
            Text,
            Glyph,
        }

        internal Kind   Type { get; } = kind;
        internal string Text { get; } = text;
    }
}
