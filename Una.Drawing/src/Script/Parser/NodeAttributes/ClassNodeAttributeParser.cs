namespace Una.Drawing.Script.Parser;

internal class ClassNodeAttributeParser : INodeAttributeParser
{
    public bool Apply(Node node, string name, dynamic value)
    {
        if (name != "class" || value is not string str) return false;
     
        var classList = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var className in classList) {
            node.ClassList.Add(className);
        }

        return true;
    }
}
