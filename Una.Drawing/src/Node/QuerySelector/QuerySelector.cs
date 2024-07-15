/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Linq;

namespace Una.Drawing;

internal record QuerySelector
{
    /// <summary>
    /// An identifier that must match the current node.
    /// </summary>
    public string? Identifier;

    /// <summary>
    /// Whether the current query selector is the first in the list.
    /// </summary>
    public bool IsFirstSelector = false;

    /// <summary>
    /// A list of class-names that the current node must have.
    /// </summary>
    public List<string> ClassList = [];

    /// <summary>
    /// Represents the tag names that the current node must have.
    /// </summary>
    public List<string> TagList = [];

    /// <summary>
    /// Represents a nested query selector that matches one or more of the
    /// immediate children of the current node.
    /// </summary>
    public QuerySelector? DirectChild;

    /// <summary>
    /// Represents a nested query selector that matches all nested children of
    /// the current node recursively, until it finds one.
    /// </summary>
    public QuerySelector? NestedChild;

    public override string ToString()
    {
        return $"QuerySelector: {Identifier} {ClassList.Aggregate("", (acc, x) => $"{acc}.{x}")}{TagList.Aggregate("", (acc, x) => $"{acc}:{x}")}";
    }
}
