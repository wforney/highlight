using Highlight.Extensions;
using Highlight.Patterns;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Highlight.Configuration;

public class XmlConfiguration : IConfiguration
{
    private IDictionary<string, Definition> definitions;
    public IDictionary<string, Definition> Definitions => GetDefinitions();

    public XDocument XmlDocument { get; protected set; }

    public XmlConfiguration(XDocument xmlDocument) => XmlDocument = xmlDocument ?? throw new ArgumentNullException(nameof(xmlDocument));

    protected XmlConfiguration()
    {
    }

    private IDictionary<string, Definition> GetDefinitions()
    {
        definitions ??= XmlDocument
                .Descendants("definition")
                .Select(GetDefinition)
                .ToDictionary(x => x.Name);

        return definitions;
    }

    private static Definition GetDefinition(XElement definitionElement)
    {
        var name = definitionElement.GetAttributeValue("name");
        var patterns = GetPatterns(definitionElement);
        var caseSensitive = bool.Parse(definitionElement.GetAttributeValue("caseSensitive"));
        var style = GetDefinitionStyle(definitionElement);

        return new Definition(name, caseSensitive, style, patterns);
    }

    private static Dictionary<string, Pattern> GetPatterns(XContainer definitionElement)
    {
        var patterns = definitionElement
            .Descendants("pattern")
            .Select(GetPattern)
            .ToDictionary(x => x.Name);

        return patterns;
    }

    private static Pattern GetPattern(XElement patternElement)
    {
        const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;
        var patternType = patternElement.GetAttributeValue("type");
        if (patternType.Equals("block", stringComparison))
        {
            return GetBlockPattern(patternElement);
        }
        if (patternType.Equals("markup", stringComparison))
        {
            return GetMarkupPattern(patternElement);
        }
        return patternType.Equals("word", stringComparison)
            ? (Pattern)GetWordPattern(patternElement)
            : throw new InvalidOperationException(string.Format("Unknown pattern type: {0}", patternType));
    }

    private static BlockPattern GetBlockPattern(XElement patternElement)
    {
        var name = patternElement.GetAttributeValue("name");
        var style = GetPatternStyle(patternElement);
        var beginsWith = patternElement.GetAttributeValue("beginsWith");
        var endsWith = patternElement.GetAttributeValue("endsWith");
        var escapesWith = patternElement.GetAttributeValue("escapesWith");

        return new BlockPattern(name, style, beginsWith, endsWith, escapesWith);
    }

    private static MarkupPattern GetMarkupPattern(XElement patternElement)
    {
        var name = patternElement.GetAttributeValue("name");
        var style = GetPatternStyle(patternElement);
        var highlightAttributes = bool.Parse(patternElement.GetAttributeValue("highlightAttributes"));
        var bracketColors = GetMarkupPatternBracketColors(patternElement);
        var attributeNameColors = GetMarkupPatternAttributeNameColors(patternElement);
        var attributeValueColors = GetMarkupPatternAttributeValueColors(patternElement);

        return new MarkupPattern(name, style, highlightAttributes, bracketColors, attributeNameColors, attributeValueColors);
    }

    private static WordPattern GetWordPattern(XElement patternElement)
    {
        var name = patternElement.GetAttributeValue("name");
        var style = GetPatternStyle(patternElement);
        var words = GetPatternWords(patternElement);

        return new WordPattern(name, style, words);
    }

    private static List<string> GetPatternWords(XContainer patternElement)
    {
        var words = new List<string>();
        var wordElements = patternElement.Descendants("word");
        if (wordElements is not null)
        {
            words.AddRange(from wordElement in wordElements select Regex.Escape(wordElement.Value));
        }

        return words;
    }

    private static Style GetPatternStyle(XContainer patternElement)
    {
        var fontElement = patternElement.Descendants("font").Single();
        var colors = GetPatternColors(fontElement);
        var font = GetPatternFont(fontElement);

        return new Style(colors, font);
    }

    private static ColorPair GetPatternColors(XElement fontElement)
    {
        var foreColor = Color.FromName(fontElement.GetAttributeValue("foreColor"));
        var backColor = Color.FromName(fontElement.GetAttributeValue("backColor"));

        return new ColorPair(foreColor, backColor);
    }

    private static Font GetPatternFont(XElement fontElement, Font defaultFont = null)
    {
        var fontFamily = fontElement.GetAttributeValue("name");
        if (fontFamily is not null)
        {
            var emSize = fontElement.GetAttributeValue("size").ToSingle(11f);
            var style = Enum<FontStyle>.Parse(fontElement.GetAttributeValue("style"), FontStyle.Regular, true);

            return SystemFonts.CreateFont(fontFamily, emSize, style);
        }

        return defaultFont;
    }

    private static ColorPair GetMarkupPatternBracketColors(XContainer patternElement)
    {
        const string descendantName = "bracketStyle";
        return GetMarkupPatternColors(patternElement, descendantName);
    }

    private static ColorPair GetMarkupPatternAttributeNameColors(XContainer patternElement)
    {
        const string descendantName = "attributeNameStyle";
        return GetMarkupPatternColors(patternElement, descendantName);
    }

    private static ColorPair GetMarkupPatternAttributeValueColors(XContainer patternElement)
    {
        const string descendantName = "attributeValueStyle";
        return GetMarkupPatternColors(patternElement, descendantName);
    }

    private static ColorPair GetMarkupPatternColors(XContainer patternElement, XName descendantName)
    {
        var fontElement = patternElement.Descendants("font").Single();
        var element = fontElement.Descendants(descendantName).SingleOrDefault();
        if (element is not null)
        {
            var colors = GetPatternColors(element);

            return colors;
        }

        return default;
    }

    private static Style GetDefinitionStyle(XNode definitionElement)
    {
        const string xpath = "default/font";
        var fontElement = definitionElement.XPathSelectElement(xpath);
        var colors = GetDefinitionColors(fontElement);
        var font = GetDefinitionFont(fontElement);

        return new Style(colors, font);
    }

    private static ColorPair GetDefinitionColors(XElement fontElement)
    {
        var foreColor = Color.FromName(fontElement.GetAttributeValue("foreColor"));
        var backColor = Color.FromName(fontElement.GetAttributeValue("backColor"));

        return new ColorPair(foreColor, backColor);
    }

    private static Font GetDefinitionFont(XElement fontElement)
    {
        var fontName = fontElement.GetAttributeValue("name");
        var fontSize = Convert.ToSingle(fontElement.GetAttributeValue("size"));
        var fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), fontElement.GetAttributeValue("style"), true);

        return SystemFonts.CreateFont(fontName, fontSize, fontStyle);
    }
}
