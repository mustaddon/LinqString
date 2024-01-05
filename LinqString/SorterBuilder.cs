using LinqString._internal;
using System.Linq.Expressions;

namespace LinqString;

public static class SorterBuilder
{
    public static (LambdaExpression lambda, bool desc) Build(Type sourceType, string path, bool desc)
    {
        var final = TrimDirection(path, desc);
        return (BuildLambda(sourceType, final.path), final.desc);
    }

    internal static LambdaExpression BuildLambda(Type sourceType, string finalPath)
    {
        var param = Expression.Parameter(sourceType, null);
        return Expression.Lambda(param.PropertyOrFieldSafe(finalPath.SplitProps()), param);
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
