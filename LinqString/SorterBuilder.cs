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
        var props = finalPath.Split('.');
        var param = Expression.Parameter(sourceType, null);
        var nullsafe = param.NotNull();
        var memberExpr = Expression.PropertyOrField(param, props[0]) as Expression;

        if (props.Length > 1)
            for (var i = 1; i < props.Length; i++)
            {
                nullsafe = Expression.AndAlso(nullsafe, memberExpr.NotNull());
                memberExpr = Expression.PropertyOrField(memberExpr, props[i]);
            }

        var returnType = memberExpr.Type.ToNullableType();

        if (memberExpr.Type != returnType)
            memberExpr = Expression.Convert(memberExpr, returnType);

        return Expression.Lambda(
            Expression.Condition(nullsafe, memberExpr, Expression.Constant(null, returnType)),
            param);
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
