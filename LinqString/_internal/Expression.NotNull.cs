using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    internal static BinaryExpression NotNull(this Expression expression)
    {
        return Expression.NotEqual(expression, _constantNull);
    }

    static readonly ConstantExpression _constantNull = Expression.Constant(null);
}
