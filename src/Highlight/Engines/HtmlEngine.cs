using Highlight.Patterns;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Highlight.Engines;

public class HtmlEngine : Engine
{
    private const string StyleSpanFormat = "<span style=\"{0}\">{1}</span>";
    private const string ClassSpanFormat = "<span class=\"{0}\">{1}</span>";

    public bool UseCss { get; set; }

    protected override string PreHighlight(Definition definition, string input) =>
        definition is null
            ? throw new ArgumentNullException(nameof(definition))
            : HttpUtility.HtmlEncode(input);

    protected override string PostHighlight(Definition definition, string input)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (UseCss)
        {
            var cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, null);

            return string.Format(ClassSpanFormat, cssClassName, input);
        }

        var cssStyle = HtmlEngineHelper.CreatePatternStyle(definition.Style);

        return string.Format(StyleSpanFormat, cssStyle, input);
    }

    protected override string ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match)
    {
        if (UseCss)
        {
            var cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, pattern.Name);

            return string.Format(ClassSpanFormat, cssClassName, match.Value);
        }

        var patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.Style);

        return string.Format(StyleSpanFormat, patternStyle, match.Value);
    }

    protected override string ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(match);

        var result = new StringBuilder();
        if (UseCss)
        {
            var cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, $"{pattern.Name}Bracket");
            _ = result.AppendFormat(ClassSpanFormat, cssClassName, match.Groups["openTag"].Value);

            _ = result.Append(match.Groups["ws1"].Value);

            cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, $"{pattern.Name}TagName");
            _ = result.AppendFormat(ClassSpanFormat, cssClassName, match.Groups["tagName"].Value);

            if (pattern.HighlightAttributes)
            {
                var highlightedAttributes = ProcessMarkupPatternAttributeMatches(definition, pattern, match);
                _ = result.Append(highlightedAttributes);
            }

            _ = result.Append(match.Groups["ws5"].Value);

            cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, $"{pattern.Name}Bracket");
            _ = result.AppendFormat(ClassSpanFormat, cssClassName, match.Groups["closeTag"].Value);
        }
        else
        {
            var patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.BracketColors, pattern.Style.Font);
            _ = result.AppendFormat(StyleSpanFormat, patternStyle, match.Groups["openTag"].Value);

            _ = result.Append(match.Groups["ws1"].Value);

            patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.Style);
            _ = result.AppendFormat(StyleSpanFormat, patternStyle, match.Groups["tagName"].Value);

            if (pattern.HighlightAttributes)
            {
                var highlightedAttributes = ProcessMarkupPatternAttributeMatches(definition, pattern, match);
                _ = result.Append(highlightedAttributes);
            }

            _ = result.Append(match.Groups["ws5"].Value);

            patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.BracketColors, pattern.Style.Font);
            _ = result.AppendFormat(StyleSpanFormat, patternStyle, match.Groups["closeTag"].Value);
        }

        return result.ToString();
    }

    protected override string ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match)
    {
        if (UseCss)
        {
            var cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, pattern.Name);

            return string.Format(ClassSpanFormat, cssClassName, match.Value);
        }

        var patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.Style);

        return string.Format(StyleSpanFormat, patternStyle, match.Value);
    }

    private string ProcessMarkupPatternAttributeMatches(Definition definition, MarkupPattern pattern, Match match)
    {
        var result = new StringBuilder();

        for (var i = 0; i < match.Groups["attribute"].Captures.Count; i++)
        {
            _ = result.Append(match.Groups["ws2"].Captures[i].Value);
            if (UseCss)
            {
                var cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, $"{pattern.Name}AttributeName");
                _ = result.AppendFormat(ClassSpanFormat, cssClassName, match.Groups["attribName"].Captures[i].Value);

                if (string.IsNullOrWhiteSpace(match.Groups["attribValue"].Captures[i].Value))
                {
                    continue;
                }

                cssClassName = HtmlEngineHelper.CreateCssClassName(definition.Name, $"{pattern.Name}AttributeValue");
                _ = result.AppendFormat(ClassSpanFormat, cssClassName, match.Groups["attribValue"].Captures[i].Value);
            }
            else
            {
                var patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.AttributeNameColors, pattern.Style.Font);
                _ = result.AppendFormat(StyleSpanFormat, patternStyle, match.Groups["attribName"].Captures[i].Value);

                if (string.IsNullOrWhiteSpace(match.Groups["attribValue"].Captures[i].Value))
                {
                    continue;
                }

                patternStyle = HtmlEngineHelper.CreatePatternStyle(pattern.AttributeValueColors, pattern.Style.Font);
                _ = result.AppendFormat(StyleSpanFormat, patternStyle, match.Groups["attribValue"].Captures[i].Value);
            }
        }

        return result.ToString();
    }
}
