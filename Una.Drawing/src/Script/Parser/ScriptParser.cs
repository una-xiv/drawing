namespace Una.Drawing.Script.Parser;

internal partial class ScriptParser
{
    public static ScriptSource Parse(string code)
    {
        return (new ScriptParser(Tokenizer.Tokenize(code))).Parse();
    }

    private readonly TokenStream _stream;

    private Node?       _rootNode;
    private Stylesheet? _stylesheet;

    private ScriptParser(TokenStream stream)
    {
        _stream = stream;
        
        DebugLogger.Log(stream.ToString());
    }

    private ScriptSource Parse()
    {
        if (_stream.IsEmpty) return new ScriptSource(null, null);

        while (!_stream.IsEof) {
            // At the root level, we can either have a stylesheet or a single node.
            if (_stream.SequenceEquals([TokenType.LessThan, TokenType.Identifier, TokenType.GreaterThan]) &&
                _stream.Peek(1)?.Value.ToString()?.ToLowerInvariant() == "style"
            ) {
                _stream.Advance(3);
                ParseStylesheet();
                continue;
            }

            if (_stream.SequenceEquals([TokenType.LessThan, TokenType.Identifier])) {
                if (null != _rootNode) {
                    throw new Exception("Unexpected root node. Only one root node is allowed.");
                }

                _rootNode = ParseNode();
                continue;
            }

            throw new Exception($"Unexpected token {_stream.Current}");
        }

        if (_rootNode != null && _stylesheet != null) {
            _rootNode.Stylesheet = _stylesheet;
        }

        return new ScriptSource(_rootNode, _stylesheet);
    }
}