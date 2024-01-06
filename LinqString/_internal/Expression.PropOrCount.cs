using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    public static Expression PropOrCount(this Expression expression, string[] props, bool nullsafeEnumerables)
    {
        var nullsafe = expression.NotNull();
        expression = Expression.PropertyOrField(expression, props[0]);

        if (props.Length > 1)
            for (var i = 1; i < props.Length; i++)
            {
                nullsafe = Expression.AndAlso(nullsafe, expression.NotNull());
                expression = Expression.PropertyOrField(expression, props[i]);
            }

        var elementType = expression.Type == typeof(string) ? null : expression.Type.GetElementTypeExt();

        if (elementType != null)
        {
            if (nullsafeEnumerables) 
                nullsafe = Expression.AndAlso(nullsafe, expression.NotNull());

            expression = Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), [elementType], [expression]);
        }

        if (expression.Type.TryToNullable(out var nullableType))
            expression = Expression.Convert(expression, nullableType);

        return Expression.Condition(nullsafe, expression, Expression.Constant(null, expression.Type));
    }
}
