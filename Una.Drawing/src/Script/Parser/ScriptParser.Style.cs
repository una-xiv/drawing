using System.Linq;
using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal partial class ScriptParser
{
    private static readonly IStyleAttributeParser[] StyleAttributeParsers = [
        new IntegerStyleAttributeParser(),
        new UnsignedIntegerStyleAttributeParser(),
        new BooleanStyleAttributeParser(), 
        new DoubleStyleAttributeParser(),
        new AutoSizeStyleAttributeParser(),
        new AnchorStyleAttributeParser(),
        new FlowStyleAttributeParser(),
        new SizeStyleAttributeParser(),
        new EdgeSizeStyleAttributeParser(),
        new BorderColorStyleAttributeParser(),
        new ColorStyleAttributeParser(),
        new RoundedCornersStyleAttributeParser(),
        new GradientColorStyleAttributeParser(),
        new Vector2StyleAttributeParser(),
    ];

    /// <summary>
    /// <para>
    /// Parses a stylesheet.
    /// </para>
    /// <para>
    /// At the root level, this can either contain an @import statement, a
    /// @mixin declaration or a rule that contains a selector and a block of
    /// properties.
    /// </para>
    /// </summary>
    /// <returns></returns>
    private void ParseStylesheet()
    {
        _stylesheet = new Stylesheet([]);

        while (!_stream.IsEof) {
            if (_stream.SequenceEquals([
                    TokenType.LessThan, TokenType.Slash, TokenType.Identifier, TokenType.GreaterThan
                ]) &&
                _stream.Peek(2)?.Value.ToString()?.ToLowerInvariant() == "style"
            ) {
                _stream.Advance(4);
                break;
            }

            if (_stream.Peek()?.Type == TokenType.Keyword) {
                ParseKeyword();
                continue;
            }

            if (_stream.Peek()?.Type == TokenType.Selector) {
                var rules = ParseRule();
                foreach (var rule in rules) {
                    _stylesheet.AddRule(rule.Query, rule.Style);
                }
                continue;
            }

            throw new Exception($"Unexpected token {_stream.Current}");
        }
    }

    private void ParseKeyword()
    {
        Token keyword = _stream.Consume(TokenType.Keyword);
        if (keyword.Value.ToString()?.ToLowerInvariant() == "import") {
            string name = _stream.Consume(TokenType.String).Value.ToString()!;

            if (!ScriptRegistry.Exists(name.ToLowerInvariant())) {
                throw new Exception(
                    $"Script '{name}' not found. Make sure to register it first via ScriptRegistry.Register()");
            }

            Stylesheet? sheet = ScriptSource.FromCode(ScriptRegistry.Get(name.ToLowerInvariant())).Stylesheet;
            if (sheet == null) {
                throw new Exception($"Script '{name}' does not contain a stylesheet.");
            }

            _stylesheet!.ImportFrom(sheet);

            _stream.Consume(TokenType.Semicolon);
            return;
        }

        throw new Exception($"Unexpected {keyword}");
    }

    private List<Stylesheet.StyleDefinition> ParseRule(string? parent = null, List<Stylesheet.StyleDefinition>? rules = null)
    {
        rules ??= [];
        string selector = (parent != null ? $"{parent}" : "") + ParseSelectorString();
        _stream.Consume(TokenType.OpenBrace);

        DebugLogger.Log($"Selector: {selector}");
        
        Style style     = new();
        Type  styleType = typeof(Style);
        
        rules.Add(new(selector, style));
        
        while (true) {
            if (_stream.IsEof) {
                throw new Exception("Unexpected end of file while parsing rule.");
            }

            if (_stream.Peek()?.Type == TokenType.CloseBrace) {
                _stream.Consume(TokenType.CloseBrace);
                break;
            }
            
            if (_stream.SequenceEquals([TokenType.Ampersand, TokenType.Selector])) {
                _stream.Consume(TokenType.Ampersand);
                ParseRule(selector, rules);
                continue;
            }

            if (_stream.Peek()?.Type == TokenType.Selector) {
                ParseRule($"{selector} > ", rules);
                continue;
            }

            if (_stream.SequenceEquals([TokenType.Identifier, TokenType.Colon])) {
                var identifier = _stream.Consume(TokenType.Identifier);
                _stream.Consume(TokenType.Colon);
                var value = ParseStyleAttributeValue();
                if (value == null) {
                    throw new Exception($"Failed to parse attribute value of {identifier}");
                }

                _stream.Consume(TokenType.Semicolon);

                PropertyInfo? property = FindStyleProperty(styleType, identifier.Value.ToString()!);
                if (property == null) {
                    throw new Exception($"Invalid property '{identifier}' for style '{selector}'.");
                }

                var  attrType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                bool found    = StyleAttributeParsers.Any(parser => parser.Apply(style, property, attrType, value));

                if (!found) {
                    throw new Exception($"Failed to apply style '{identifier}' to '{selector}'.");
                }
                
                continue;
            }

            throw new Exception($"Unexpected token {_stream.Current}");
        }
        
        return rules;
    }

    private string ParseSelectorString()
    {
        List<string> selectors = [];
        
        while (!_stream.IsEof) {
            if (_stream.Peek()?.Type == TokenType.OpenBrace) {
                break;
            }
            
            if (_stream.Peek()?.Type == TokenType.Selector) {
                selectors.Add(_stream.Consume(TokenType.Selector).Value.ToString()!);
                continue;
            }
            
            if (_stream.Peek()?.Type == TokenType.Identifier) {
                selectors.Add(_stream.Consume(TokenType.Identifier).Value.ToString()!);
                continue;
            }
            
            throw new Exception($"Unexpected token {_stream.Current}");
        }
        
        if (selectors.Count == 0) {
            throw new Exception("Expected selector.");
        }
        
        if (_stream.IsEof) {
            throw new Exception("Unexpected end of file. Expected an opening brace.");
        }
        
        return string.Join(" ", selectors);
    }

    private static PropertyInfo? FindStyleProperty(Type type, string name)
    {
        PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property != null) return property;

        // Allow kebab-case names (e.g. "border-radius" to "borderradius").
        name = name.Replace("-", "").ToLowerInvariant();

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .FirstOrDefault(prop => prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}