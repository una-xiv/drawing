using Una.Drawing.Script.Parser;

namespace Una.Drawing.Script;

public class ScriptSource
{
    public static ScriptSource FromCode(string code)
    {
        return ScriptParser.Parse(code);
    }

    public Node?       RootNode   { get; }
    public Stylesheet? Stylesheet { get; }

    internal ScriptSource(Node? node, Stylesheet? stylesheet)
    {
        RootNode   = node;
        Stylesheet = stylesheet;
    }
}
