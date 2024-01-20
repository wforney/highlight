using System.Text.RegularExpressions;

namespace Highlight.Patterns;

public sealed class BlockPattern(string name, Style style, string beginsWith, string endsWith, string escapesWith) : Pattern(name, style)
{
    public string BeginsWith { get; private set; } = beginsWith;
    public string EndsWith { get; private set; } = endsWith;
    public string EscapesWith { get; private set; } = escapesWith;

    public override string GetRegexPattern()
    {
        if (string.IsNullOrEmpty(EscapesWith))
        {
            return EndsWith.CompareTo(@"\n") == 0
                ? $@"{Escape(BeginsWith)}[^\n\r]*"
                : $@"{Escape(BeginsWith)}[\w\W\s\S]*?{Escape(EndsWith)}";
        }

        return string.Format(
            "{0}(?>{1}.|[^{2}]|.)*?{3}",
            new object[]
            {
                Regex.Escape(BeginsWith),
                Regex.Escape(EscapesWith[..1]),
                Regex.Escape(EndsWith[..1]),
                Regex.Escape(EndsWith)
            });
    }

    public static string Escape(string str)
    {
        if (str.CompareTo(@"\n") != 0)
        {
            str = Regex.Escape(str);
        }

        return str;
    }
}
