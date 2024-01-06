namespace LinqString._internal;

internal static partial class StringExt
{
    public static IEnumerable<(string Name, string? Args)> SplitPath(this string path)
    {
        var last = 0;

        for (int i = 0; i < path.Length; i++)
        {
            switch (path[i])
            {
                case '.':
                    yield return (path.Substring(last, i - last), null);
                    last = i + 1;
                    break;

                case '(':
                    yield return (path.Substring(last, i - last), path.Substring(i + 1, path.Length - i - 2));
                    last = i = path.Length;
                    break;
            }
        }

        if (last < path.Length)
            yield return (path.Substring(last, path.Length - last), null);
    }
}

