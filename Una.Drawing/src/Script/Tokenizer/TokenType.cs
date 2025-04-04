namespace Una.Drawing.Script;

public enum TokenType : int
{
    Identifier,
    Selector,
    String,
    Integer,
    UInt,
    Double,
    Boolean,
    LessThan,
    GreaterThan,
    Colon,
    Semicolon,
    Assignment,
    Slash,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    OpenBracket,
    CloseBracket,
    Comma,
    Keyword,
    Ampersand,
}