using System;

namespace Highlight.Extensions;

internal static class Enum<T> where T : struct
{
    public static T Parse(string value, T defaultValue) =>
        Parse(value, defaultValue, false);

    public static T Parse(string value, T defaultValue, bool ignoreCase) =>
        Enum.TryParse(value, ignoreCase, out T result)
            ? result
            : defaultValue;
}
