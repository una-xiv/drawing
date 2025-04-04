using System.Linq;

namespace Una.Drawing.Script.Parser;

internal partial class ScriptParser
{
    private static readonly INodeAttributeParser[] NodeAttributeParsers = [
        new IdNodeAttributeParser(),
        new ClassNodeAttributeParser(),
        new TagsNodeAttributeParser(),
        new ValueNodeAttributeParser(),
        new DynamicNodeAttributeParser(),
    ];

    /// <summary>
    /// Parse a collection of nodes.
    /// </summary>
    private List<Node> ParseNodeList()
    {
        var nodes = new List<Node>();

        while (!_stream.IsEof) {
            // Stop if we have reached the beginning of a closing tag.
            if (_stream.SequenceEquals([TokenType.LessThan, TokenType.Slash])) break;

            if (_stream.SequenceEquals([TokenType.LessThan, TokenType.Identifier])) {
                nodes.Add(ParseNode());
                continue;
            }

            throw new Exception($"Unexpected token: {_stream.Current}");
        }

        return nodes;
    }

    /// <summary>
    /// Parses a node at the current position.
    /// </summary>
    private Node ParseNode()
    {
        _stream.Consume(TokenType.LessThan);
        string identifier = _stream.Consume(TokenType.Identifier).Value.ToString()?.ToLowerInvariant() ?? "";

        if (Activator.CreateInstance(ElementRegistry.GetElementType(identifier)) is not Node node) {
            throw new Exception($"Element '{identifier}' could not be constructed.");
        }

        // Continue consuming attributes until we reach the end of the start tag.
        while (!_stream.IsEof &&
               _stream.Peek()?.Type != TokenType.GreaterThan &&
               !_stream.SequenceEquals([TokenType.Slash, TokenType.GreaterThan])
        ) {
            // Check for attributes with assignment.
            if (_stream.SequenceEquals([TokenType.Identifier, TokenType.Assignment])) {
                var attribute = _stream.Consume(TokenType.Identifier);
                _stream.Consume(TokenType.Assignment);
                var value = ParseAttributeValue();

                if (value == null) {
                    throw new Exception($"Failed to parse attribute value of {attribute}");
                }

                string name  = attribute.Value.ToString()?.ToLowerInvariant() ?? "";
                bool   found = NodeAttributeParsers.Any(parser => parser.Apply(node, name, value));

                if (!found) {
                    throw new Exception($"Invalid attribute '{name}' for node '{identifier}'.");
                }

                continue;
            }

            // Check for implicit boolean attributes.
            if (_stream.Peek()?.Type == TokenType.Identifier) {
                var attribute = _stream.Consume(TokenType.Identifier);

                string name  = attribute.Value.ToString()?.ToLowerInvariant() ?? "";
                bool   found = NodeAttributeParsers.Any(parser => parser.Apply(node, name, true));

                if (!found) {
                    throw new Exception($"Invalid attribute '{name}' for node '{identifier}'.");
                }
            } else {
                throw new Exception($"Unexpected token: {_stream.Current}");
            }
        }

        // === End of Opening Tag ===
        // Check if the tag is self-closing.
        if (_stream.SequenceEquals([TokenType.Slash, TokenType.GreaterThan])) {
            _stream.Advance(2);
            return node;
        }

        // Otherwise, expect a normal closing of the start tag.
        if (_stream.Peek()?.Type == TokenType.GreaterThan) {
            _stream.Consume(TokenType.GreaterThan);
        } else {
            throw new Exception("Expected '>' or '/>' at the end of the start tag.");
        }

        foreach (var child in ParseNodeList()) {
            node.AppendChild(child);
        }

        _stream.Consume(TokenType.LessThan);
        _stream.Consume(TokenType.Slash);

        string closingIdentifier = _stream.Consume(TokenType.Identifier).Value.ToString()?.ToLowerInvariant() ?? "";

        if (closingIdentifier != identifier) {
            throw new Exception($"Unexpected closing tag: {closingIdentifier}");
        }

        _stream.Consume(TokenType.GreaterThan);

        return node;
    }
}