using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    public static Expression PathValue(this Expression expression, string path, bool nullsafeEnumerables)
    {
        if (path == string.Empty)
            return expression;

        Expression? nullsafe = null;

        foreach (var (name, args) in path.SplitPath())
        {
            if (args == null)
            {
                nullsafe = nullsafe.And(expression.NotNull());
                expression = Expression.PropertyOrField(expression, name);
            }
            else
            {
                if (nullsafeEnumerables)
                {
                    nullsafe = nullsafe.And(expression.NotNull());

                    if (_needAny.Contains(name))
                        nullsafe = nullsafe.And(Expression.IsTrue(
                            PathFunctions.Dictionary.TryGetValue(nameof(Enumerable.Any), out var anyFn) ? anyFn(expression, x => x)
                                : Expression.Call(Types.Enumerable, nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression])));
                }

                expression = PathFunctions.Dictionary.TryGetValue(name, out var builder)
                    ? builder(expression, x => x.PathValue(args, nullsafeEnumerables))
                    : throw new KeyNotFoundException($"Function '{name}' not found");
            }
        }

        if (expression.Type.TryGetElementType(out var elementType))
        {
            if (nullsafeEnumerables)
                nullsafe = nullsafe.And(expression.NotNull());
             
            expression = PathFunctions.Dictionary.TryGetValue(nameof(Enumerable.Count), out var countFn) ? countFn(expression, x => x)
                : Expression.Call(Types.Enumerable, nameof(Enumerable.Count), [elementType], [expression]);
        }

        if (nullsafe == null)
            return expression;

        if (expression.Type.TryToNullable(out var nullableType))
            expression = Expression.Convert(expression, nullableType);

        return Expression.Condition(nullsafe, expression, Expression.Constant(null, expression.Type));
    }

    static readonly HashSet<string> _needAny = [
        nameof(Enumerable.Min),
        nameof(Enumerable.Max),
        nameof(Enumerable.Average),
        nameof(PathFunctions.Avg)
    ];
}
