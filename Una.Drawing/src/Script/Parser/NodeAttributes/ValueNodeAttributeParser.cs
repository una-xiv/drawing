namespace Una.Drawing.Script.Parser;

internal class ValueNodeAttributeParser : INodeAttributeParser
{
    public bool Apply(Node node, string name, dynamic value)
    {
        if (name != "value" || value is not string str) return false;

        node.NodeValue = str;
        
        return true;
    }
}