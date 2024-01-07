using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    internal static Expression And(this Expression? left, Expression right)
        => left == null ? right : Expression.AndAlso(left, right);
}
