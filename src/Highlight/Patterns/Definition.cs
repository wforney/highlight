using System.Collections.Generic;
using System.Text;

namespace Highlight.Patterns;

public class Definition(string name, bool caseSensitive, Style style, IDictionary<string, Pattern> patterns)
{
    public string Name { get; private set; } = name;
    public bool CaseSensitive { get; private set; } = caseSensitive;
    public Style Style { get; private set; } = style;
    public IDictionary<string, Pattern> Patterns { get; private set; } = patterns;

    public string GetRegexPattern()
    {
        var allPatterns = new StringBuilder();
        var blockPatterns = new StringBuilder();
        var markupPatterns = new StringBuilder();
        var wordPatterns = new StringBuilder();

        foreach (var pattern in Patterns.Values)
        {
            if (pattern is BlockPattern)
            {
                if (blockPatterns.Length > 1)
                {
                    _ = blockPatterns.Append('|');
                }

                _ = blockPatterns.AppendFormat("(?'{0}'{1})", pattern.Name, pattern.GetRegexPattern());
            }
            else if (pattern is MarkupPattern)
            {
                if (markupPatterns.Length > 1)
                {
                    _ = markupPatterns.Append('|');
                }

                _ = markupPatterns.AppendFormat("(?'{0}'{1})", pattern.Name, pattern.GetRegexPattern());
            }
            else if (pattern is WordPattern)
            {
                if (wordPatterns.Length > 1)
                {
                    _ = wordPatterns.Append('|');
                }

                _ = wordPatterns.AppendFormat("(?'{0}'{1})", pattern.Name, pattern.GetRegexPattern());
            }
        }

        if (blockPatterns.Length > 0)
        {
            _ = allPatterns.AppendFormat("(?'blocks'{0})+?", blockPatterns);
        }
        if (markupPatterns.Length > 0)
        {
            _ = allPatterns.AppendFormat("|(?'markup'{0})+?", markupPatterns);
        }
        if (wordPatterns.Length > 0)
        {
            _ = allPatterns.AppendFormat("|(?'words'{0})+?", wordPatterns);
        }

        return allPatterns.ToString();
    }

    public override string ToString() => Name;
}
