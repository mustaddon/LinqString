using DynamicAnonymousType;
using LinqString._internal;
using System.Linq.Expressions;

namespace LinqString;

public static class GrouperBuilder
{
    public static LambdaExpression Build(Type sourceType, IEnumerable<string> props)
    {
        return BuildOrdered(sourceType, props.Order());
    }

    internal static LambdaExpression BuildOrdered(Type sourceType, IEnumerable<string> orderedProps)
    {
        var param = Expression.Parameter(sourceType, null);
        var propExpr = orderedProps.Select(x => PropExpr(param, x)).Buffer();
        var keyType = DynamicFactory.CreateType(propExpr.Select(x => (x.Name, x.Expr.Type)));

        var initExpr = Expression.MemberInit(
            Expression.New(keyType),
            propExpr.Select(x => Expression.Bind(keyType.GetProperty(x.Name)!, x.Expr)));

        return Expression.Lambda(initExpr, param);
    }

    static (string Name, Expression Expr) PropExpr(Expression expression, string path)
    {
        var props = path.Split('.');
        var nullsafe = expression.NotNull();
        var result = Expression.PropertyOrField(expression, props[0]) as Expression;

        if (props.Length > 1)
            for (var i = 1; i < props.Length; i++)
            {
                nullsafe = Expression.AndAlso(nullsafe, result.NotNull());
                result = Expression.PropertyOrField(result, props[i]);
            }

        var returnType = result.Type.ToNullableType();

        if (result.Type != returnType)
            result = Expression.Convert(result, returnType);

        result = Expression.Condition(nullsafe, result, Expression.Constant(null, returnType));

        return (string.Join(string.Empty, props), result);
    }

}
