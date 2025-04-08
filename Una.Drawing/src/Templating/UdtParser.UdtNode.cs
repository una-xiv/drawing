using System.Linq;
using System.Xml;
using Una.Drawing.Templating.StyleParser;

namespace Una.Drawing;

internal sealed partial class UdtParser
{
    /// <summary>
    /// <para>
    /// Parses the UDT node.
    /// </para>
    /// <para>
    /// The order of which the nodes are parsed is important. The CDATA section
    /// is parsed first, from which a Stylesheet is constructed. After this,
    /// all import elements are parsed. Imported files will have the templates
    /// and stylesheets embedded in the current UDT file. This is a recursive
    /// operation.
    /// </para>
    /// <para>
    /// Next up are the template elements. These represent reusable components
    /// that can be used as nodes. Once all template elements have been parsed,
    /// the root node will be located and parsed, if it exists.
    /// </para>
    /// </summary>
    /// <param name="udt"></param>
    /// <exception cref="Exception"></exception>
    private void ParseUdtNode(XmlElement udt)
    {
        // If the UDT element contains a CDATA section, parse it as a style.
        XmlCDataSection? cdata = udt.ChildNodes.OfType<XmlCDataSection>().FirstOrDefault();
        if (cdata != null) {
            _stylesheet = StyleParser.StylesheetFromCode(cdata.Data.Trim());
        }

        var elementNodes = udt.ChildNodes.OfType<XmlElement>().ToList();
        if (elementNodes.Count == 0) return;

        // Find import nodes.
        foreach (var el in elementNodes.Where(el => NormalizeElementName(el.Name) == "import")) {
            ParseImportNode(el);
        }

        // Find template nodes.
        foreach (var el in elementNodes.Where(el => NormalizeElementName(el.Name) == "template")) {
            ParseTemplateNode(el);
        }

        // Find any other node that isn't a template or import node.
        List<XmlElement> rootNodeList =
            elementNodes
               .Where(el => NormalizeElementName(el.Name) is not ("template" or "import"))
               .ToList();

        switch (rootNodeList.Count) {
            case 0:
                return;
            case > 1:
                throw new Exception($"Found multiple root nodes in \"{_filename}\". Only one root node is allowed.");
            default:
                // Parse the root node.
                _rootNode = ParseNode(rootNodeList.First());
                if (_rootNode == null) {
                    throw new Exception($"Failed to construct root node in \"${_filename}\".");
                }
                break;
        }
    }
}