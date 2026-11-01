using System.Threading;

namespace Una.Drawing;

/// <summary>
/// Holds the most recent result of <see cref="ComputedStyleFactory"/> for a
/// single node, along with the state that it was generated from.
/// </summary>
internal sealed class ComputedStyleCache
{
    internal ComputedStyle Result;
    internal int           Hash;

    // The stylesheet rules that match the node, in order of application.
    internal Style[]?    MatchedStyles;
    internal int         MatchedRulesHash;
    internal int         MatchedRulesCachedGeneration;
    internal Stylesheet? Stylesheet;
    internal int         StylesheetVersion;

    // The inputs that were used to generate the result.
    internal bool   HasResult;
    internal Style? Style;
    internal int    StyleVersions;
    internal float  ScaleFactor;
    internal bool   ScaleAffectsBorders;

    /// <summary>
    /// Increases when the matched rules of the node may have changed. This
    /// prevents a style pass on another thread from storing outdated rules.
    /// </summary>
    internal int MatchedRulesGeneration => Volatile.Read(ref _matchedRulesGeneration);

    private int _matchedRulesGeneration;

    internal void InvalidateMatchedRules()
    {
        Interlocked.Increment(ref _matchedRulesGeneration);
    }
}
