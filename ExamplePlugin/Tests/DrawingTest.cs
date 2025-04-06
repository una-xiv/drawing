using ImGuiNET;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public abstract class DrawingTest
{
    /// <summary>
    /// The name of the test.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// The category of the test.
    /// </summary>
    public abstract string Category { get; }

    /// <summary>
    /// Invoked when the test is activated.
    /// </summary>
    public abstract void OnActivate();

    /// <summary>
    /// Invoked when the test is deactivated.
    /// </summary>
    public abstract void OnDeactivate();

    /// <summary>
    /// Renders ImGUI elements in the configuration window.
    /// </summary>
    public abstract void RenderConfig();

    /// <summary>
    /// Renders the test.
    /// </summary>
    public abstract void RenderTest(ImDrawListPtr dl);
    
    public virtual uint BackgroundColor => 0x80000000;

    /// <summary>
    /// Loads a template file from the "templates" directory relative to the assembly
    /// location.
    /// </summary>
    /// <param name="templateFile"></param>
    /// <returns></returns>
    protected static Node CreateNodeFromFile(string templateFile)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames()
                               .FirstOrDefault(r => r.EndsWith(templateFile, StringComparison.OrdinalIgnoreCase));

        if (resource == null) {
            throw new FileNotFoundException($"Template file '{templateFile}' not found in assembly resources.");
        }

        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream == null) {
            throw new FileNotFoundException($"Could not open template file '{templateFile}'.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        string    code   = reader.ReadToEnd();

        return Node.FromCode(code);
    }
}