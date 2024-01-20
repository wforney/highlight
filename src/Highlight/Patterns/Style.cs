using SixLabors.Fonts;

namespace Highlight.Patterns;

public class Style(ColorPair colors, Font font)
{
    public ColorPair Colors { get; private set; } = colors;

    public Font Font { get; private set; } = font;
}
