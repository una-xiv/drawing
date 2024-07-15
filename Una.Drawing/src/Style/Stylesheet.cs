/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Linq;

namespace Una.Drawing;

public class Stylesheet
{
    internal Dictionary<Rule, Style> Rules = [];

    public Stylesheet(List<StyleDefinition> rules)
    {
        foreach (var rule in rules) AddRule(rule.Query, rule.Style);
    }

    /// <summary>
    /// Adds a style rule matching the given query.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="style"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddRule(string query, Style style)
    {
        List<QuerySelector> results = QuerySelectorParser.Parse(query);

        foreach (var qs in results) {
            // Don't allow nested selectors in a stylesheet. This is a
            // deliberate design choice to keep performance high.
            if (qs.DirectChild is not null || qs.NestedChild is not null)
                throw new InvalidOperationException("A stylesheet rule declaration cannot have nested selectors.");

            Rules.Add(new(qs), style);
        }
    }

    internal class Rule(QuerySelector querySelector)
    {
        public bool Matches(Node node)
        {
            return (querySelector.Identifier is null || querySelector.Identifier.Equals(node.Id))
                && querySelector.ClassList.All(className => node.ClassList.Contains(className))
                && querySelector.TagList.All(className => node.TagsList.Contains(className));
        }
    }

    public readonly struct StyleDefinition(string query, Style style)
    {
        public string Query => query;
        public Style  Style => style;
    }
}
