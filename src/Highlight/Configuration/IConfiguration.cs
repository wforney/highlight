using Highlight.Patterns;
using System.Collections.Generic;

namespace Highlight.Configuration;

public interface IConfiguration
{
    IDictionary<string, Definition> Definitions { get; }
}
