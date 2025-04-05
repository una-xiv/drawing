namespace Una.Drawing.Script.Parser;

internal class IdNodeAttributeParser : INodeAttributeParser
{
    public bool Apply(Node node, string name, dynamic value)
    {
        if (name != "id" || value is not string str) return false;

        node.Id = str.Trim();
        
        return true;
    }
}
