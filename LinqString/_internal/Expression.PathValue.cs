using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class ExpressionExt
{
    public static Expression PathValue(this Expression expression, string path, bool nullsafeEnumerables)
    {
        if (path == string.Empty)
            return expression;

        var nullsafe = expression.NotNull();
        var enumerator = path.SplitPath().GetEnumerator();

        enumerator.MoveNext();

        expression = Expression.PropertyOrField(expression, enumerator.Current.Name);

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Args == null)
            {
                nullsafe = Expression.AndAlso(nullsafe, expression.NotNull());
                expression = Expression.PropertyOrField(expression, enumerator.Current.Name);
            }
            else
            {
                if (nullsafeEnumerables)
                {
                    nullsafe = Expression.AndAlso(nullsafe, expression.NotNull());

                    if (_needAny.Contains(enumerator.Current.Name))
                        nullsafe = Expression.AndAlso(nullsafe, Expression.IsTrue(Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression])));
                }

                expression = enumerator.Current.Name switch
                {
                    nameof(Enumerable.Count) => Count(expression),
                    nameof(Enumerable.Any) => Any(expression),
                    nameof(Enumerable.Sum) => Sum(expression, ArgsConvertLambda(expression, enumerator.Current.Args, nullsafeEnumerables)),
                    nameof(Enumerable.Min) => Min(expression, ArgsLambda(expression, enumerator.Current.Args, nullsafeEnumerables)),
                    nameof(Enumerable.Max) => Max(expression, ArgsLambda(expression, enumerator.Current.Args, nullsafeEnumerables)),
                    nameof(Enumerable.Average) => Avg(expression, ArgsConvertLambda(expression, enumerator.Current.Args, nullsafeEnumerables)),
                    _ => throw new KeyNotFoundException(enumerator.Current.Name),
                };
            }
        }

        var elementType = expression.Type.IsValueType || expression.Type == typeof(string) ? null
            : expression.Type.GetElementTypeExt();

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

    static MethodCallExpression Any(Expression expression)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Count(Expression expression)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Min(Expression expression, LambdaExpression lambda)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Min), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Max(Expression expression, LambdaExpression lambda)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Max), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Sum(Expression expression, LambdaExpression lambda)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Sum), [lambda.Parameters[0].Type], [expression, lambda]);

    static MethodCallExpression Avg(Expression expression, LambdaExpression lambda)
        => Expression.Call(typeof(Enumerable), nameof(Enumerable.Average), [lambda.Parameters[0].Type], [expression, lambda]);

    static LambdaExpression ArgsLambda(Expression expression, string args, bool nullsafeEnumerables)
    {
        var param = Expression.Parameter(expression.Type.GetElementTypeExt()!, null);
        return Expression.Lambda(param.PathValue(args, nullsafeEnumerables), param);
    }

    static LambdaExpression ArgsConvertLambda(Expression expression, string args, bool nullsafeEnumerables)
    {
        var param = Expression.Parameter(expression.Type.GetElementTypeExt()!, null);
        var body = param.PathValue(args, nullsafeEnumerables);

        if (_typeMap.TryGetValue(body.Type, out var convertType))
            body = Expression.Convert(body, convertType);

        return Expression.Lambda(body, param);
    }

    static readonly Dictionary<Type, Type> _typeMap = new()
    {
        { typeof(byte), typeof(int) },
        { typeof(byte?), typeof(int?) },

        { typeof(short), typeof(int) },
        { typeof(short?), typeof(int?) },

        { typeof(ushort), typeof(int) },
        { typeof(ushort?), typeof(int?) },

        { typeof(uint), typeof(long) },
        { typeof(uint?), typeof(long?) },
    };

    static readonly HashSet<string> _needAny = [nameof(Enumerable.Min), nameof(Enumerable.Max), nameof(Enumerable.Average)];
}
