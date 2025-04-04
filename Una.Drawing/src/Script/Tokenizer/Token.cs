using System.Globalization;

namespace Una.Drawing.Script;

public record struct Token
{
    public TokenType Type;
    public object    Value;
    public int       LineStart;
    public int       ColumnStart;
    public int       LineEnd;
    public int       ColumnEnd;

    public override string ToString()
    {
        string value = (Type == TokenType.Double ? ((double)Value!).ToString("F4", CultureInfo.InvariantCulture) : Value?.ToString()) ?? "NULL";
        return $"{Type}({value}) at {LineStart}:{ColumnStart} - {LineEnd}:{ColumnEnd}";
    }
}
