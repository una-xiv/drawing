using System.Xml;

namespace Una.Drawing;

public record UdtDocument(Node? RootNode, Stylesheet? Stylesheet, Dictionary<string, XmlElement> Templates);