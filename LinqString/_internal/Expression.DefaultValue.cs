using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    internal static ConstantExpression DefaultValue(this Expression expression)
    {
        return Expression.Constant(expression.Type.DefaultValue(), expression.Type);
    }

}
