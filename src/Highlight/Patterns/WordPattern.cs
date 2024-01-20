using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Highlight.Patterns;

public sealed partial class WordPattern(string name, Style style, IEnumerable<string> words) : Pattern(name, style)
{
    public IEnumerable<string> Words { get; private set; } = words;

    public override string GetRegexPattern()
    {
        var str = string.Empty;
        if (Words.Any())
        {
            var nonWords = GetNonWords();
            str = $@"(?<![\w{nonWords}])(?=[\w{nonWords}])({string.Join("|", Words.ToArray())})(?<=[\w{nonWords}])(?![\w{nonWords}])";
        }

        return str;
    }

    private string GetNonWords()
    {
        var input = string.Join(string.Empty, Words.ToArray());
        var list = new List<string>();
        foreach (var match in WordsRegEx().Matches(input).Cast<Match>().Where(x => !list.Contains(x.Value)))
        {
            list.Add(match.Value);
        }

        return string.Join(string.Empty, [.. list]);
    }

    [GeneratedRegex(@"\W")]
    private static partial Regex WordsRegEx();
}
