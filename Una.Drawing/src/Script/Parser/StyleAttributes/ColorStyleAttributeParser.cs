using System.Linq;
using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal class ColorStyleAttributeParser : IStyleAttributeParser
{
    public bool Apply(Style style, PropertyInfo property, Type attributeType, List<Token> tokens)
    {
        if (attributeType != typeof(Color)) return false;
        if (tokens.Count != 1) {
            throw new Exception("Expected a single token for a Color attribute.");
        }

        Token token = tokens.First();

        if (token.Type != TokenType.Identifier && token.Type != TokenType.String && token.Type != TokenType.UInt) {
            throw new Exception(
                $"Expected an identifier, string or uint token for a Color attribute, but got {token.Type}.");
        }

        try {
            property.SetValue(style, token.Type switch {
                TokenType.Identifier or TokenType.String => new Color(token.Value.ToString()!),
                TokenType.UInt => new Color(Convert.ToUInt32(token.Value)),
                _ => throw new Exception($"Invalid token type '{token.Type}' for a Color attribute.")
            });
        } catch (Exception ex) {
            throw new Exception($"Invalid color value '{token.Value}': {ex.Message}");
        }

        return true;
    }
}