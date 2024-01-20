using LinqString._internal;
using System.Linq.Expressions;

namespace LinqString;

public static class PathFunctions
{
    public delegate Expression ExpressionBuilder(Expression expression, Func<Expression, Expression> arg);

    public static readonly Dictionary<string, ExpressionBuilder> Dictionary = new() {
        { nameof(Enumerable.Count), (expr, arg) => Count(expr) },
        { nameof(Enumerable.Any), (expr, arg) => Any(expr) },
        { nameof(Enumerable.Min), (expr, arg) => Min(expr, ArgsLambda(expr, arg)) },
        { nameof(Enumerable.Max), (expr, arg) => Max(expr, ArgsLambda(expr, arg)) },
        { nameof(Enumerable.Sum), (expr, arg) => Sum(expr, ArgsConvertLambda(expr, arg)) },
        { nameof(Avg), (expr, arg) => Avg(expr, ArgsConvertLambda(expr, arg)) },
        { nameof(Enumerable.Average), (expr, arg) => Avg(expr, ArgsConvertLambda(expr, arg)) },
        { nameof(Enumerable.First), (expr, arg) => First(expr, ArgsLambda(expr, arg)) },
        { nameof(Enumerable.Last), (expr, arg) => Last(expr, ArgsLambda(expr, arg)) },
    };


    static MethodCallExpression Count(Expression expression)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Count), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Any(Expression expression)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Any), [expression.Type.GetElementTypeExt()!], [expression]);

    static MethodCallExpression Min(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Min), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Max(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Max), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda]);

    static MethodCallExpression Sum(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Sum), [lambda.Parameters[0].Type], [expression, lambda]);

    internal static MethodCallExpression Avg(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.Average), [lambda.Parameters[0].Type], [expression, lambda]);

    static MethodCallExpression First(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.FirstOrDefault), [lambda.ReturnType],
            [Expression.Call(Types.Enumerable, nameof(Enumerable.Select), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda])]);

    static MethodCallExpression Last(Expression expression, LambdaExpression lambda)
        => Expression.Call(Types.Enumerable, nameof(Enumerable.LastOrDefault), [lambda.ReturnType],
            [Expression.Call(Types.Enumerable, nameof(Enumerable.Select), [lambda.Parameters[0].Type, lambda.ReturnType], [expression, lambda])]);

    static LambdaExpression ArgsLambda(Expression expression, Func<Expression, Expression> arg)
    {
        var param = Expression.Parameter(expression.Type.GetElementTypeExt()!, null);
        return Expression.Lambda(arg(param), param);
    }

    static LambdaExpression ArgsConvertLambda(Expression expression, Func<Expression, Expression> arg)
    {
        var param = Expression.Parameter(expression.Type.GetElementTypeExt()!, null);
        var body = arg(param);

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
}
