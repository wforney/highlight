using Highlight.Configuration;
using Highlight.Engines;
using System;

namespace Highlight;

public class Highlighter(IEngine engine, IConfiguration configuration)
{
    public IEngine Engine { get; set; } = engine;
    public IConfiguration Configuration { get; set; } = configuration;

    public Highlighter(IEngine engine) : this(engine, new DefaultConfiguration())
    {
    }

    public string Highlight(string definitionName, string input)
    {
        ArgumentNullException.ThrowIfNull(definitionName);

        return Configuration.Definitions.TryGetValue(definitionName, out var value)
            ? Engine.Highlight(value, input)
            : input;
    }
}
