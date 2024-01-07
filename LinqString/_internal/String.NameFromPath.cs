using System.Text.RegularExpressions;

namespace LinqString._internal;

internal static partial class StringExt
{
    public static string NameFromPath(this string path)
        => NameFromPathRegex().Replace(path, string.Empty);

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"[\.\(\)]")]
    private static partial Regex NameFromPathRegex();
#else
    static readonly Regex _nameFromPathRegex = new(@"[\.\(\)]");
    private static Regex NameFromPathRegex() => _nameFromPathRegex;
#endif
}

