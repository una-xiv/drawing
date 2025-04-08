using System.Reflection;
using System.Xml;

namespace Una.Drawing;

internal sealed partial class UdtParser : IDisposable
{
    private string?                        _filename;
    private string?                        _code;
    private Assembly?                      _assembly;
    private Node?                          _rootNode;
    private Stylesheet?                    _stylesheet;
    private Dictionary<string, XmlElement> _templates = [];

    internal UdtParser(string filename, string code, Assembly? assembly = null)
    {
        _filename = filename;
        _code     = code;
        _assembly = assembly;
    }

    internal UdtDocument Parse()
    {
        using StringReader stringReader = new StringReader(_code!);
        using XmlReader xmlReader = XmlReader.Create(
            stringReader, new() { IgnoreWhitespace = true, IgnoreComments = true, }
        );

        XmlDocument doc = new();

        try {
            doc.Load(xmlReader);
        } catch (XmlException ex) {
            throw new InvalidOperationException(
                $"Failed to parse UDT file \"{_filename}\" at {ex.LineNumber}:{ex.LinePosition}. {ex.Message}", ex
            );
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to parse UDT file \"{_filename}\". {ex.Message}", ex);
        }

        // Check for the root element.
        if (doc.DocumentElement == null) {
            throw new InvalidOperationException($"UDT file \"{_filename}\" has no root element.");
        }

        if (NormalizeElementName(doc.DocumentElement.Name) != "udt") {
            throw new InvalidOperationException(
                $"UDT file \"{_filename}\" has an invalid root element \"{doc.DocumentElement.Name}\". Expected \"udt\"."
            );
        }

        ParseUdtNode(doc.DocumentElement);

        return new(_rootNode, _stylesheet, _templates);
    }

    public void Dispose()
    {
        _filename   = null;
        _code       = null;
        _rootNode   = null;
        _stylesheet = null;
        _assembly   = null;
    }

    /// <summary>
    /// Normalizes the element name by converting it to lowercase and removing
    /// hyphens to allow for kebab-case names.
    /// </summary>
    private static string NormalizeElementName(in string name)
    {
        return name.ToLowerInvariant().Replace("-", "").Trim();
    }
}