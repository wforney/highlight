using Highlight.Patterns;
using SixLabors.Fonts;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Highlight.Engines;

// TODO: Clean up and refactor big methods into smaller, more manageable chunks.
public class RtfEngine : Engine
{
    private const string RtfFormat = "{0} {1}";
    private readonly ArrayList colors = [];
    private readonly ArrayList fonts = [];

    protected override string PreHighlight(Definition definition, string input) => HttpUtility.HtmlEncode(input);

    protected override string PostHighlight(Definition definition, string input)
    {
        var result = input
            .Replace("{", @"\{").Replace("}", @"\}").Replace("\t", @"\tab ")
            .Replace("\r\n", @"\par ");
        var fontList = BuildFontList();
        var colorList = BuildColorList();

        return $@"{{\rtf1\ansi{{\fonttbl{{{fontList}}}}}{{\colortbl;{colorList}}}{result}}}";
    }

    protected override string ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match)
    {
        var style = CreateRtfPatternStyle(pattern.Style.Colors.ForeColor, pattern.Style.Colors.BackColor, pattern.Style.Font);

        return $"{{{string.Format(RtfFormat, style, match.Value)}}}";
    }

    protected override string ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match)
    {
        var builder = new StringBuilder();
        var style = CreateRtfPatternStyle(pattern.Style.Colors.ForeColor, pattern.Style.Colors.BackColor, pattern.Style.Font);
        var bracketStyle = CreateRtfPatternStyle(pattern.BracketColors.ForeColor, pattern.BracketColors.BackColor, pattern.Style.Font);
        string attributeNameStyle = null;
        string attributeValueStyle = null;
        if (pattern.HighlightAttributes)
        {
            attributeNameStyle = CreateRtfPatternStyle(pattern.AttributeNameColors.ForeColor, pattern.AttributeNameColors.BackColor, pattern.Style.Font);
            attributeValueStyle = CreateRtfPatternStyle(pattern.AttributeValueColors.ForeColor, pattern.AttributeValueColors.BackColor, pattern.Style.Font);
        }

        _ = builder.AppendFormat(RtfFormat, bracketStyle, match.Groups["openTag"].Value);
        _ = builder.Append(match.Groups["ws1"].Value);
        _ = builder.AppendFormat(RtfFormat, style, match.Groups["tagName"].Value);
        if (attributeNameStyle is not null)
        {
            for (var i = 0; i < match.Groups["attribute"].Captures.Count; i++)
            {
                _ = builder.Append(match.Groups["ws2"].Captures[i].Value);
                _ = builder.AppendFormat(RtfFormat, attributeNameStyle, match.Groups["attribName"].Captures[i].Value);

                if (string.IsNullOrWhiteSpace(match.Groups["attribValue"].Captures[i].Value))
                {
                    continue;
                }

                _ = builder.AppendFormat(RtfFormat, attributeValueStyle, match.Groups["attribValue"].Captures[i].Value);
            }
        }
        _ = builder.Append(match.Groups["ws5"].Value);
        _ = builder.AppendFormat(RtfFormat, bracketStyle, match.Groups["closeTag"].Value);

        return $"{{{builder}}}";
    }

    protected override string ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match)
    {
        var style = CreateRtfPatternStyle(pattern.Style.Colors.ForeColor, pattern.Style.Colors.BackColor, pattern.Style.Font);

        return $"{{{string.Format(RtfFormat, style, match.Value)}}}";
    }

    private string CreateRtfPatternStyle(Color foreColor, Color backColor, Font font) =>
        string.Concat(
            new object[]
            {
                @"\cf",
                GetIndexOfColor(foreColor),
                @"\highlight",
                GetIndexOfColor(backColor),
                @"\f",
                GetIndexOfFont(font.Name),
                @"\fs",
                font.Size * 2f
            });

    private int GetIndexOfColor(Color color)
    {
        var red = color.Name.IndexOf('#') > -1
            ? int.Parse(color.Name.Substring(1, 2), NumberStyles.AllowHexSpecifier)
            : color.R;

        var green = color.Name.IndexOf('#') > -1
            ? int.Parse(color.Name.Substring(3, 2), NumberStyles.AllowHexSpecifier)
            : color.G;

        var blue = color.Name.IndexOf('#') > -1
            ? int.Parse(color.Name.Substring(5, 2), NumberStyles.AllowHexSpecifier)
            : color.B;

        var color2 = new HexColor(red, green, blue);

        var index = colors.IndexOf(color2);
        if (index > -1)
        {
            return index + 1;
        }
        
        _ = colors.Add(color2);

        return colors.Count;
    }

    private int GetIndexOfFont(string font)
    {
        var index = fonts.IndexOf(font);
        if (index > -1)
        {
            return index + 1;
        }

        _ = fonts.Add(font);

        return fonts.Count;
    }

    private string BuildColorList()
    {
        var builder = new StringBuilder();
        foreach (var hexColor in colors.Cast<HexColor>())
        {
            _ = builder.AppendFormat(@"\red{0}\green{1}\blue{2};", hexColor.Red, hexColor.Green, hexColor.Blue);
        }
        
        return builder.ToString();
    }

    private string BuildFontList()
    {
        var builder = new StringBuilder();
        for (var i = 0; i < fonts.Count; i++)
        {
            _ = builder.AppendFormat(@"\f{0} {1};", i, fonts[i]);
        }
        
        return builder.ToString();
    }

    [StructLayout(LayoutKind.Sequential)]
    private record struct HexColor(int Red, int Green, int Blue)
    {
    }
}
