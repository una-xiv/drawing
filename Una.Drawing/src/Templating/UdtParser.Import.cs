using System.Reflection;
using System.Xml;

namespace Una.Drawing;

internal sealed partial class UdtParser
{
    private void ParseImportNode(XmlElement node)
    {
        if (!node.HasAttribute("from")) {
            throw new Exception($"Import node \"{node.Name}\" in \"{_filename}\" has no \"from\" attribute.");
        }

        string resourceName = node.GetAttribute("from");
        if (resourceName == "") {
            throw new Exception($"Import node \"{node.Name}\" in \"{_filename}\" has an empty \"from\" attribute.");
        }

        if (_assembly == null) {
            throw new Exception(
                $"UDT \"{_filename}\" has been loaded without an assembly. Cannot import \"{resourceName}\".");
        }

        UdtDocument doc = LoadFromAssembly(_assembly, resourceName);

        if (doc.RootNode != null) {
            _rootNode = doc.RootNode;
        }

        MergeStylesheetFrom(doc);
        MergeTemplatesFrom(doc);
    }

    private UdtDocument LoadFromAssembly(Assembly assembly, string resourceName)
    {
        try {
            return UdtLoader.LoadFromAssembly(assembly, resourceName);
        } catch (Exception err) {
            throw new Exception(
                $"Failed to load UDT \"{resourceName}\" from \"{assembly.GetName().Name}\", imported from \"{_filename}\".\n{err.Message}"
            );
        }
    }

    private void MergeStylesheetFrom(UdtDocument doc)
    {
        if (null == doc.Stylesheet) return;

        _stylesheet ??= new([]);
        _stylesheet.ImportFrom(doc.Stylesheet);
    }

    private void MergeTemplatesFrom(UdtDocument doc)
    {
        if (doc.Templates.Count == 0) return;

        foreach (var template in doc.Templates) {
            if (_templates.ContainsKey(template.Key)) {
                throw new Exception(
                    $"Template \"{template.Key}\" already exists or has already been imported in \"{_filename}\".");
            }

            _templates.Add(template.Key, template.Value);
        }
    }
}