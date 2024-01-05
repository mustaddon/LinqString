using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    public static Expression PropertyOrField(this Expression expression, string[] props)
    {
        foreach (var prop in props)
            expression = Expression.PropertyOrField(expression, prop);

        return expression;
    }

    public static Expression PropertyOrFieldSafe(this Expression expression, string[] props)
    {
        var nullsafe = expression.NotNull();
        expression = Expression.PropertyOrField(expression, props[0]);

        if (props.Length > 1)
            for (var i = 1; i < props.Length; i++)
            {
                nullsafe = Expression.AndAlso(nullsafe, expression.NotNull());
                expression = Expression.PropertyOrField(expression, props[i]);
            }

        if (expression.Type.TryToNullable(out var nullableType))
            expression = Expression.Convert(expression, nullableType);

        return Expression.Condition(nullsafe, expression, Expression.Constant(null, expression.Type));
    }
}
