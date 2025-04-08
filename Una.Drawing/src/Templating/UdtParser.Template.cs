using System.Linq;
using System.Xml;

namespace Una.Drawing;

internal sealed partial class UdtParser
{
    private void ParseTemplateNode(XmlElement element)
    {
        if (!element.HasAttribute("name")) {
            throw new Exception($"A template element in \"{_filename}\" has no \"name\" attribute.");
        }

        string name = NormalizeElementName(element.GetAttribute("name"));
        if (name == "") {
            throw new Exception($"A template element in \"{_filename}\" has an empty \"name\" attribute.");
        }

        _templates.Add(name, element);
    }

    /// <summary>
    /// Constructs a new Node from a template.
    /// </summary>
    /// <param name="sourceElement">The element that references the template.</param>
    /// <param name="templateElement">The template element.</param>
    /// <returns></returns>
    private Node ConstructNodeFromTemplate(XmlElement sourceElement, XmlElement templateElement)
    {
        // Extract attributes from the source element.
        Dictionary<string, string> attributes = [];
        foreach (XmlAttribute attr in sourceElement.Attributes) {
            string attrName  = NormalizeElementName(attr.Name);
            string attrValue = attr.Value.Trim();
            attributes.Add(attrName, attrValue);
        }

        UdtTemplateFactory tpl = new(this, templateElement, sourceElement, attributes);

        return tpl.Build();
    }

    private class UdtTemplateFactory
    {
        private readonly Dictionary<string, string>           _attributes;
        private readonly Dictionary<string, TemplateArgument> _arguments;
        private readonly XmlElement                           _element;
        private readonly XmlElement                           _sourceElement;
        private readonly XmlElement                           _rootElement;
        private readonly UdtParser                            _parser;

        internal UdtTemplateFactory(
            UdtParser                  parser,
            XmlElement                 template,
            XmlElement                 sourceElement,
            Dictionary<string, string> attributes
        )
        {
            _parser        = parser;
            _attributes    = attributes;
            _element       = (template.Clone() as XmlElement)!;
            _sourceElement = sourceElement;
            _arguments     = ParseTemplateArguments();
            _rootElement   = ExtractRootElementFromTemplate();
        }

        internal Node Build()
        {
            Node root = ConstructNodeFrom(_rootElement);
            root.Stylesheet = _parser._stylesheet;

            return root;
        }

        private Node ConstructNodeFrom(XmlElement element)
        {
            Dictionary<string, string> attributes = [];

            foreach (var attr in element.Attributes.OfType<XmlAttribute>()) {
                string attrName  = NormalizeElementName(attr.Name);
                string attrValue = ParseTemplateAttribute(attr.Value);

                // IDs are unique and should not be used in templates unless its value
                // is explicitly replaced with a template argument.
                if (attrName == "id" && !attr.Value.Contains("${")) {
                    throw new Exception(
                        $"Template \"{TemplateName}\" in \"{_parser._filename}\" has an \"id\" attribute " +
                        $"with a value that does not contain a template argument.");
                }

                attributes.Add(attrName, attrValue);
            }

            // TODO: Detect self-referencing nodes.
            
            string name = NormalizeElementName(element.Name);
            Node   node = _parser.ConstructNode(name, attributes);

            foreach (XmlNode child in element.ChildNodes) {
                if (child is not XmlElement childElement) {
                    DebugLogger.Log($"WARNING: Child node \"{child.Name}\" is not an element. Skipping.");
                    continue;
                }

                string childName = NormalizeElementName(childElement.Name);
                if (childName == "slot" && childElement.IsEmpty) {
                    string slotName = childElement.GetAttribute("name");
                    List<Node> slottedElements = ExtractSlottedElementsFromSource(slotName);
                    
                    foreach (Node slottedElement in slottedElements) {
                        node.ChildNodes.Add(slottedElement);
                    }

                    continue;
                }

                Node childNode = ConstructNodeFrom(childElement);
                node.ChildNodes.Add(childNode);
            }

            return node;
        }

        private List<Node> ExtractSlottedElementsFromSource(string slotName)
        {
            List<Node> slottedElements = [];
            
            foreach (XmlNode child in _sourceElement.ChildNodes) {
                if (child is not XmlElement childElement) {
                    DebugLogger.Log($"WARNING: Child node \"{child.Name}\" is not an element. Skipping.");
                    continue;
                }

                string slot = childElement.GetAttribute("slot");
                
                if ((slotName == "" && slot == "") || (slotName != "" && slot == slotName)) {
                    slottedElements.Add(_parser.ParseNode(childElement));
                }
            }
            
            return slottedElements;
        }

        private string ParseTemplateAttribute(string attributeValue)
        {
            string result = attributeValue;

            foreach (var (name, arg) in _arguments) {
                string? argValue = _attributes.FirstOrDefault(x => x.Key == name).Value ?? arg.DefaultValue;

                if (argValue == null) {
                    throw new Exception($"Failed to construct element \"{TemplateName}\" in \"{_parser._filename}\": " +
                                        $"Argument \"{name}\" is not defined and has no default value.");
                }

                result = result.Replace($"${{{name}}}", argValue);
            }

            return result;
        }

        /// <summary>
        /// Extracts the root element node from the template.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private XmlElement ExtractRootElementFromTemplate()
        {
            List<XmlElement> rootNodeList =
                _element.ChildNodes
                        .OfType<XmlElement>()
                        .Where(el => NormalizeElementName(el.Name) is not "argument")
                        .ToList();

            return rootNodeList.Count switch {
                0 => throw new Exception(
                    $"Template \"{TemplateName}\" in \"{_parser._filename}\" has no root element."),
                > 1 => throw new Exception(
                    $"Template \"{TemplateName}\" in \"{_parser._filename}\" has multiple root elements."),
                _ => rootNodeList.First()
            };
        }

        /// <summary>
        /// Collects a dictionary of template arguments.
        /// </summary>
        private Dictionary<string, TemplateArgument> ParseTemplateArguments()
        {
            Dictionary<string, TemplateArgument> templateArguments = [];
            foreach (var el in _element.ChildNodes.OfType<XmlElement>()
                                       .Where(t => NormalizeElementName(t.Name) == "argument")
            ) {
                string name = el.GetAttribute("name");
                if (name == "")
                    throw new Exception(
                        $"Template \"{TemplateName}\" has an argument in \"{_parser._filename}\" with no \"name\" attribute.");

                string? defaultValue                 = el.GetAttribute("default");
                if (defaultValue == "") defaultValue = null;

                templateArguments.Add(name, new TemplateArgument(defaultValue));
            }

            return templateArguments;
        }

        // TODO: Add optional description or other metadata?
        private record TemplateArgument(string? DefaultValue);

        private string TemplateName => _element.GetAttribute("name");
    }
}