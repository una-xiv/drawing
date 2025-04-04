using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal interface IStyleAttributeParser
{
    internal bool Apply(Style style, PropertyInfo property, Type attributeType, List<Token> tokens);
}
