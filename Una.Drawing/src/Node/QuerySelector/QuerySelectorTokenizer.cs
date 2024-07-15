/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Text.RegularExpressions;
using Dalamud.Utility;

namespace Una.Drawing;

internal static partial class QuerySelectorTokenizer
{
    internal static List<QuerySelectorToken> Tokenize(string str)
    {
        str = WhitespaceRemovalRegex().Replace(str, " ");

        var tokens    = new List<QuerySelectorToken>();
        var i         = 0;
        var idBuffer  = "";
        int maxLength = str.Length;

        while (i < maxLength) {
            char c = str[i];

            if (c.ToString().IsNullOrWhitespace()) {
                if (idBuffer.Length > 0) {
                    tokens.Add(new (QuerySelectorTokenType.Identifier, idBuffer));
                    idBuffer = "";
                }
                i++;
                continue;
            }

            // If "c" is any non-alphanumeric character...
            if (idBuffer.Length > 0 && !IdentifierCharRegex().IsMatch(c.ToString())) {
                tokens.Add(new (QuerySelectorTokenType.Identifier, idBuffer));
                idBuffer = "";
            }

            char[] separators = [' ', '.', '#', '[', '>', '+', '~', ':', ','];

            switch (c) {
                case ' ':
                    tokens.Add(new(QuerySelectorTokenType.DeepChild, c.ToString()));
                    i++;
                    continue;
                case '>':
                    tokens.Add(new(QuerySelectorTokenType.Child, c.ToString()));
                    i++;
                    continue;
                case ':': {
                    int start = i;
                    int end   = str.IndexOfAny(separators, start + 1);

                    if (end == -1) {
                        end = str.Length;
                    }

                    tokens.Add(new(QuerySelectorTokenType.TagList, str[(start + 1)..end]));
                    i = end;
                    continue;
                }
                case '.': {
                    int start = i;
                    int end   = str.IndexOfAny(separators, start + 1);

                    if (end == -1) {
                        end = str.Length;
                    }

                    tokens.Add(new(QuerySelectorTokenType.Class, str[(start + 1)..end]));
                    i = end;
                    continue;
                }
                case '#': {
                    int start = i;
                    int end   = str.IndexOfAny(separators, start + 1);

                    if (end == -1) {
                        end = str.Length;
                    }

                    tokens.Add(new(QuerySelectorTokenType.Identifier, str[(start + 1)..end]));
                    i = end;
                    continue;
                }
                case ',':
                    tokens.Add(new(QuerySelectorTokenType.Separator, c.ToString()));
                    if (str[i+1] == ' ') i++;
                    i++;
                    break;

                default:
                    if (!IdentifierCharRegex().IsMatch(c.ToString())) {
                        throw new QuerySelectorParseException(
                            "Unexpected char '{c}' at pos {i} in query selector \"{str}\"."
                        );
                    }
                    idBuffer += c;
                    i++;
                    break;
            }
        }

        if (idBuffer.Length > 0) {
            tokens.Add(new (QuerySelectorTokenType.Identifier, idBuffer));
        }

        return tokens;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRemovalRegex();
    [GeneratedRegex("^[A-Za-z0-9-_]$")]
    private static partial Regex IdentifierCharRegex();
}
