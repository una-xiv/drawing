using System.Linq;
using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal class IntegerStyleAttributeParser : IStyleAttributeParser
{
    public bool Apply(Style style, PropertyInfo property, Type attributeType, List<Token> tokens)
    {
        if (attributeType != typeof(int)) return false;
        if (tokens.Count != 1) {
            throw new Exception("Expected a single token for an integer attribute.");
        }

        try {
            property.SetValue(style, Convert.ToInt32(tokens.First().Value));
        } catch (Exception ex) {
            throw new Exception($"Failed to set property '{property.Name}': {ex.Message}");
        }
        
        return true;
    }
}