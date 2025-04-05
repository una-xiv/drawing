namespace Una.Drawing;

public static class StylesheetRegistry
{
    private static readonly Dictionary<string, string> Scripts = [];

    /// <summary>
    /// <para>
    /// Registers a stylesheet with the given name and source.
    /// </para>
    /// <para>
    /// The script can later be referenced in other scripts via an <c>@import</c>
    /// statement. This is typically used to share common style rules and mixins.
    /// </para>
    /// <example>
    /// When registering a script with the name <c>base</c>, it can be imported
    /// in other scripts like this:
    /// <code>
    /// &lt;style&gt;
    ///     @import "base";
    /// &lt;/style&gt;
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <param name="source">The source code.</param>
    public static void Register(string name, string source)
    {
        Scripts[name.ToLowerInvariant()] = $"<style>\n{source}\n</style>";
    }

    /// <summary>
    /// Removes a stylesheet with the given name from the registry.
    /// </summary>
    /// <param name="name"></param>
    public static void Unregister(string name)
    {
        Scripts.Remove(name.ToLowerInvariant());
    }

    internal static void Dispose()
    {
        Scripts.Clear();
    }

    /// <summary>
    /// Returns true if a script with the given name exists.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    internal static bool Exists(string name)
    {
        return Scripts.ContainsKey(name.ToLowerInvariant());
    }

    /// <summary>
    /// Retrieves a script with the given name.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <returns>The script code.</returns>
    /// <exception cref="Exception"></exception>
    internal static string Get(string name)
    {
        if (Scripts.TryGetValue(name.ToLowerInvariant(), out var code))
            return code;

        throw new Exception($"Script '{name}' not found. Make sure to register it first via StylesheetRegistry.Register()");
    }
}