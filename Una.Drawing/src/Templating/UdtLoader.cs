using System.Linq;
using System.Reflection;
using System.Text;

namespace Una.Drawing;

public static class UdtLoader
{
    private static readonly HashSet<string> CircularReferences = [];

    /// <summary>
    /// Loads a UDT XML file from embedded resources in the given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load the resource from.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="throwOnFailure">Throw an exception on failure. Default is true.</param>
    /// <exception cref="FileNotFoundException">If the resource does not exist.</exception>
    /// <exception cref="InvalidOperationException">When a circular reference is detected.</exception>
    public static UdtDocument LoadFromAssembly(Assembly assembly, string resourceName, bool throwOnFailure = true)
    {
        var resource = assembly
                      .GetManifestResourceNames()
                      .FirstOrDefault(r => r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

        if (resource == null) {
            throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly resources.");
        }

        if (!CircularReferences.Add(resource)) {
            StringBuilder sb = new();
            sb.AppendLine($"Circular reference detected while loading UDT \"{resourceName}\":");

            sb.AppendLine($"  ---> \"{CircularReferences.ElementAt(0)}\":");
            
            for (var i = 1; i < CircularReferences.Count; i++) {
                sb.AppendLine($"  |    \"{CircularReferences.ElementAt(i)}\"");
            }
            
            sb.AppendLine($"  ---> \"{resource}\"");
            
            throw new InvalidOperationException(sb.ToString());
        }

        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream == null) {
            throw new FileNotFoundException($"Failed to load resource stream for '{resourceName}'.");
        }

        using var   reader   = new StreamReader(stream);
        string      code     = reader.ReadToEnd();
        UdtDocument document = Parse(resourceName, code, assembly, throwOnFailure);
        
        CircularReferences.Remove(resource);
        
        return document;
    }

    /// <summary>
    /// Parses the given UDT XML code and returns a <see cref="UdtDocument"/>.
    /// </summary>
    /// <param name="filename">The original name of the file.</param>
    /// <param name="code">The source code of the template file.</param>
    /// <param name="assembly">The assembly from which this template is loaded. Used for importing other resources.</param>
    /// <param name="throwOnFailure">Throw an exception on failure. Default is true.</param>
    public static UdtDocument Parse(string filename, string code, Assembly? assembly = null, bool throwOnFailure = true)
    {
        if (throwOnFailure) {
            using UdtParser parser = new UdtParser(filename, code, assembly);
            return parser.Parse();
        }

        try {
            using UdtParser parser = new UdtParser(filename, code, assembly);
            return parser.Parse();
        } catch (Exception e) {
            DebugLogger.Error(e.Message);
            DebugLogger.Error(e.StackTrace ?? " - No stack trace available - ");
            
            return new(null, null, []);
        }
    }
}