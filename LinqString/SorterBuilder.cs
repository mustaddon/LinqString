using LinqString._internal;
using System.Linq.Expressions;

namespace LinqString;

public static class SorterBuilder
{
    public static (LambdaExpression lambda, bool desc) Build(Type sourceType, string path, bool desc, bool nullsafeEnumerables = false)
    {
        var final = TrimDirection(path, desc);
        return (BuildLambda(sourceType, final.path, nullsafeEnumerables), final.desc);
    }

    internal static LambdaExpression BuildLambda(Type sourceType, string finalPath, bool nullsafeEnumerables)
    {
        var param = Expression.Parameter(sourceType, null);
        return Expression.Lambda(param.PathValue(finalPath, nullsafeEnumerables), param);
    }

    internal static (string path, bool desc) TrimDirection(string path, bool desc)
    {
        return path[0] switch
        {
            '>' => (path.Substring(1), true),
            '<' => (path.Substring(1), false),
            _ => (path, desc)
        };
    }
}
