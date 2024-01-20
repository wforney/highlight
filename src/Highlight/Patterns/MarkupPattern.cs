namespace Highlight.Patterns;

public sealed class MarkupPattern(
    string name,
    Style style,
    bool highlightAttributes,
    ColorPair bracketColors,
    ColorPair attributeNameColors,
    ColorPair attributeValueColors)
    : Pattern(name, style)
{
    public ColorPair AttributeNameColors { get; set; } = attributeNameColors;

    public ColorPair AttributeValueColors { get; set; } = attributeValueColors;
    
    public ColorPair BracketColors { get; set; } = bracketColors;
    
    public bool HighlightAttributes { get; set; } = highlightAttributes;

    public override string GetRegexPattern()
    {
        return @"
                (?'openTag'&lt;\??/?)
                (?'ws1'\s*?)
                (?'tagName'[\w\:]+)
                (?>
                    (?!=[\/\?]?&gt;)
                    (?'ws2'\s*?)
                    (?'attribute'
                        (?'attribName'[\w\:-]+)
                        (?'attribValue'(\s*=\s*(?:&\#39;.*?&\#39;|&quot;.*?&quot;|\w+))?)
                        # (?:(?'ws3'\s*)(?'attribSign'=)(?'ws4'\s*))
                        # (?'attribValue'(?:&\#39;.*?&\#39;|&quot;.*?&quot;|\w+))
                    )
                )*
                (?'ws5'\s*?)
                (?'closeTag'[\/\?]?&gt;)
            ";
    }
}
