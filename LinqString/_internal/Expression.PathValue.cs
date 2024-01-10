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
                        nullsafe = nullsafe.And(Expression.IsTrue(Expression.Call(Types.Enumerable, nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression])));
                }

                expression = name switch
                {
                    nameof(Enumerable.Count) => Count(expression),
                    nameof(Enumerable.Any) => Any(expression),
                    nameof(Enumerable.Sum) => Sum(expression, ArgsConvertLambda(expression, args, nullsafeEnumerables)),
                    nameof(Enumerable.Min) => Min(expression, ArgsLambda(expression, args, nullsafeEnumerables)),
                    nameof(Enumerable.Max) => Max(expression, ArgsLambda(expression, args, nullsafeEnumerables)),
                    nameof(Enumerable.Average) => Avg(expression, ArgsConvertLambda(expression, args, nullsafeEnumerables)),
                    nameof(Avg) => Avg(expression, ArgsConvertLambda(expression, args, nullsafeEnumerables)),
                    nameof(Enumerable.First) => First(expression, ArgsLambda(expression, args, nullsafeEnumerables)),
                    nameof(Enumerable.Last) => Last(expression, ArgsLambda(expression, args, nullsafeEnumerables)),
                    _ => throw new KeyNotFoundException(name),
                };
            }
        }

        if (expression.Type.TryGetElementType(out var elementType))
        {
            if (nullsafeEnumerables)
                nullsafe = nullsafe.And(expression.NotNull());

            expression = Expression.Call(Types.Enumerable, nameof(Enumerable.Count), [elementType], [expression]);
        }

        if (nullsafe == null)
            return expression;

        if (expression.Type.TryToNullable(out var nullableType))
            expression = Expression.Convert(expression, nullableType);

        return Expression.Condition(nullsafe, expression, Expression.Constant(null, expression.Type));
    }

    static MethodCallExpression Any(Expression expression)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Count(Expression expression)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Count), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Min(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Min), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Max(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Max), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Sum(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Sum), [lambda.Parameters[0].Type], [expression, lambda]);

    static MethodCallExpression Avg(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Average), [lambda.Parameters[0].Type], [expression, lambda]);

    static MethodCallExpression First(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.FirstOrDefault), [lambda.ReturnType],
            [Expression.Call(Types.Enumerable, nameof(Enumerable.Select), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda])]);

    static MethodCallExpression Last(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.LastOrDefault), [lambda.ReturnType],
            [Expression.Call(Types.Enumerable, nameof(Enumerable.Select), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda])]);

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

    static readonly HashSet<string> _needAny = [
        nameof(Enumerable.Min),
        nameof(Enumerable.Max),
        nameof(Enumerable.Average),
        nameof(Avg)
    ];
}
