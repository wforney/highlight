using Highlight.Patterns;
using SixLabors.Fonts;
using System.Drawing;
using System.Text;

namespace Highlight.Engines;

internal static class HtmlEngineHelper
{
    public static string CreateCssClassName(string definition, string pattern)
    {
        var cssClassName = definition
            .Replace("#", "sharp")
            .Replace("+", "plus")
            .Replace(".", "dot")
            .Replace("-", "");

        return string.Concat(cssClassName, pattern);
    }

    public static string CreatePatternStyle(Style style) => CreatePatternStyle(style.Colors, style.Font);

    public static string CreatePatternStyle(ColorPair colors, Font font)
    {
        var patternStyle = new StringBuilder();

        if (colors != default)
        {
            if (colors.ForeColor != Color.Empty)
            {
                _ = patternStyle.Append("color: ").Append(colors.ForeColor.Name).Append(';');
            }

            if (colors.BackColor != Color.Empty)
            {
                _ = patternStyle.Append("background-color: ").Append(colors.BackColor.Name).Append(';');
            }
        }

        if (font is not null)
        {
            if (font.Name is not null)
            {
                _ = patternStyle.Append("font-family: ").Append(font.Name).Append(';');
            }

            if (font.Size > 0f)
            {
                _ = patternStyle.Append("font-size: ").Append(font.Size).Append("px;");
            }

            if (font.FontMetrics.Description.Style == FontStyle.Regular)
            {
                _ = patternStyle.Append("font-weight: normal;");
            }

            if (font.FontMetrics.Description.Style == FontStyle.Bold)
            {
                _ = patternStyle.Append("font-weight: bold;");
            }
        }

        return patternStyle.ToString();
    }
}
