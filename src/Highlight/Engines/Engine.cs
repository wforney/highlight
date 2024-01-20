using Highlight.Patterns;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Highlight.Engines;

public abstract class Engine : IEngine
{
    private const RegexOptions DefaultRegexOptions = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;

    public string Highlight(Definition definition, string input)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var output = PreHighlight(definition, input);
        output = HighlightUsingRegex(definition, output);
        output = PostHighlight(definition, output);

        return output;
    }

    protected virtual string PreHighlight(Definition definition, string input) => input;

    protected virtual string PostHighlight(Definition definition, string input) => input;

    private string HighlightUsingRegex(Definition definition, string input)
    {
        var regexOptions = GetRegexOptions(definition);
        var evaluator = GetMatchEvaluator(definition);
        var regexPattern = definition.GetRegexPattern();
        var output = Regex.Replace(input, regexPattern, evaluator, regexOptions);

        return output;
    }

    private static RegexOptions GetRegexOptions(Definition definition) => definition.CaseSensitive ? DefaultRegexOptions : DefaultRegexOptions | RegexOptions.IgnoreCase;

    private string ElementMatchHandler(Definition definition, Match match)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(match);

        var pattern = definition.Patterns.First(x => match.Groups[x.Key].Success).Value;
        return pattern switch
        {
            BlockPattern blockPattern => ProcessBlockPatternMatch(definition, blockPattern, match),
            MarkupPattern markupPattern => ProcessMarkupPatternMatch(definition, markupPattern, match),
            WordPattern wordPattern => ProcessWordPatternMatch(definition, wordPattern, match),
            _ => match.Value,
        };
    }

    private MatchEvaluator GetMatchEvaluator(Definition definition) => match => ElementMatchHandler(definition, match);

    protected abstract string ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match);
    protected abstract string ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match);
    protected abstract string ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match);
}
