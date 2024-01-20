using Highlight.Patterns;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Highlight.Engines;

// TODO: Refactor this engine to build proper XML using XLinq.
public class XmlEngine : Engine
{
    private const string ElementFormat = "<{0}>{1}</{0}>";

    protected override string PreHighlight(Definition definition, string input) => HttpUtility.HtmlEncode(input);

    protected override string PostHighlight(Definition definition, string input) => $"<highlightedInput>{input}</highlightedInput>";

    protected override string ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match) => ProcessPatternMatch(pattern, match);

    protected override string ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match)
    {
        var builder = new StringBuilder()
            .AppendFormat(ElementFormat, "openTag", match.Groups["openTag"].Value)
            .AppendFormat(ElementFormat, "whitespace", match.Groups["ws1"].Value)
            .AppendFormat(ElementFormat, "tagName", match.Groups["tagName"].Value);

        var builder2 = new StringBuilder();
        for (var i = 0; i < match.Groups["attribute"].Captures.Count; i++)
        {
            _ = builder2
                .AppendFormat(ElementFormat, "whitespace", match.Groups["ws2"].Captures[i].Value)
                .AppendFormat(ElementFormat, "attribName", match.Groups["attribName"].Captures[i].Value);

            if (string.IsNullOrWhiteSpace(match.Groups["attribValue"].Captures[i].Value))
            {
                continue;
            }

            _ = builder2.AppendFormat(ElementFormat, "attribValue", match.Groups["attribValue"].Captures[i].Value);
        }

        _ = builder
            .AppendFormat(ElementFormat, "attribute", builder2)

            .AppendFormat(ElementFormat, "whitespace", match.Groups["ws5"].Value)
            .AppendFormat(ElementFormat, "closeTag", match.Groups["closeTag"].Value);

        return string.Format(ElementFormat, pattern.Name, builder);
    }

    protected override string ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match) => ProcessPatternMatch(pattern, match);

    private static string ProcessPatternMatch(Pattern pattern, Match match) => string.Format(ElementFormat, pattern.Name, match.Value);
}
