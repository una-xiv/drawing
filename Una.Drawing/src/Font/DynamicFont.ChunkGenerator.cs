using System.Text;

namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private static List<Chunk> GenerateChunks(string text)
    {
        var chunks = new List<Chunk>();

        if (string.IsNullOrEmpty(text)) return chunks;

        var buffer = new StringBuilder();

        Chunk.Kind lastKind = CharIsGlyph(text[0]) ? Chunk.Kind.Glyph : Chunk.Kind.Text;
        buffer.Append(text[0]);

        for (int i = 1; i < text.Length; i++) {
            char c = text[i];

            Chunk.Kind currentKind = CharIsGlyph(c) ? Chunk.Kind.Glyph : Chunk.Kind.Text;

            if (currentKind != lastKind) {
                chunks.Add(new(lastKind, buffer.ToString()));
                buffer.Clear();
                lastKind = currentKind;
            }

            buffer.Append(c);
        }

        if (buffer.Length > 0) chunks.Add(new(lastKind, buffer.ToString()));

        return chunks;
    }

    private static bool CharIsGlyph(char c)
    {
        return c >= 0xE020 && c <= 0xE0DB;
    }

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