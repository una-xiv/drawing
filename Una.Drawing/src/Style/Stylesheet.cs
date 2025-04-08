using System.Text;

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
            Rules.Add(new(qs), style);
        }
    }

    public void ImportFrom(Stylesheet other)
    {
        foreach (var rule in other.Rules) {
            Rules.Add(rule.Key, rule.Value);
        }
    }

    internal class Rule(QuerySelector querySelector)
    {
        public override string ToString()
        {
            return querySelector.ToString();
        }

        public bool Matches(Node node)
        {
            return querySelector.Matches(node);
        }
    }

    public readonly struct StyleDefinition(string query, Style style)
    {
        public string Query => query;
        public Style  Style => style;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var rule in Rules) {
            sb.AppendLine(rule.Key.ToString());
            sb.AppendLine("{");
            sb.AppendLine(rule.Value.ToString());
            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}
