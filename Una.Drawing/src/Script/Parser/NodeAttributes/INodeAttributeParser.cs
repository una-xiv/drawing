namespace Una.Drawing.Script.Parser;

internal interface INodeAttributeParser
{
    /// <summary>
    /// Applies the attribute to the given node. Returns true on success, false
    /// otherwise.
    /// </summary>
    internal bool Apply(Node node, string name, dynamic value);
}
