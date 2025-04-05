namespace Una.Drawing.Script.Parser;

internal class TagsNodeAttributeParser : INodeAttributeParser
{
    public bool Apply(Node node, string name, dynamic value)
    {
        if (name != "tags" || value is not string str) return false;
     
        var classList = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var className in classList) {
            node.TagsList.Add(className);
        }

        return true;
    }
}
