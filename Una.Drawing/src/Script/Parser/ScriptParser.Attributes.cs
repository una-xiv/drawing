using System.Collections;
using System.Linq;
using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal partial class ScriptParser
{
    private List<Token> ParseStyleAttributeValue()
    {
        List<Token> result  = [];
        Token?      current = _stream.Peek();

        if (current?.Type == TokenType.Semicolon) {
            throw new Exception("Attribute value cannot be empty.");
        }

        while (!_stream.IsEof) {
            result.Add(_stream.Consume(_stream.Peek()?.Type ?? TokenType.Semicolon));

            if (_stream.Peek()?.Type == TokenType.Semicolon) {
                return result;
            }
        }

        throw new Exception($"Expected a semicolon at {current}");
    }

    private dynamic ParseAttributeValue()
    {
        Token? token = _stream.Peek();

        return token?.Type switch {
            TokenType.Boolean => (bool)_stream.Consume(TokenType.Boolean).Value,
            TokenType.String  => _stream.Consume(TokenType.String).Value.ToString()!,
            TokenType.Integer => Convert.ToInt32(token.Value),
            TokenType.Double  => Convert.ToDouble(token.Value),
            TokenType.UInt    => Convert.ToUInt32(token.Value),
            _                 => throw new Exception($"Unexpected {token}")
        };
    }
}