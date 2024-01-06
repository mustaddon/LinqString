using DynamicAnonymousType;
using LinqString._internal;
using System.Linq.Expressions;

namespace LinqString;

public static class GrouperBuilder
{
    public static LambdaExpression Build(Type sourceType, IEnumerable<string> props, bool nullsafeEnumerables = false)
    {
        return BuildOrdered(sourceType, props.Order(), nullsafeEnumerables);
    }

    internal static LambdaExpression BuildOrdered(Type sourceType, IEnumerable<string> orderedProps, bool nullsafeEnumerables)
    {
        var param = Expression.Parameter(sourceType, null);
        var propExpr = orderedProps.Select(x => PropExpr(param, x, nullsafeEnumerables)).Buffer();
        var keyType = DynamicFactory.CreateType(propExpr.Select(x => (x.Name, x.Expr.Type)));

        var initExpr = Expression.MemberInit(
            Expression.New(keyType),
            propExpr.Select(x => Expression.Bind(keyType.GetProperty(x.Name)!, x.Expr)));

        return Expression.Lambda(initExpr, param);
    }

    static (string Name, Expression Expr) PropExpr(Expression expression, string path, bool nullsafeEnumerables)
        => (path.NameFromPath(), expression.PathValue(path, nullsafeEnumerables));

}
