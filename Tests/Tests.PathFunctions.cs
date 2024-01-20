using NUnit.Framework.Internal;
using System.Collections;
using System.Linq.Expressions;
using System.Text.Json;

namespace Tests;

public class TestPathFunctions
{
    [Test]
    public void Override()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Prop1 = x,
            Prop2 = x == 1 ? null : Enumerable.Range(0, x).Select(xx => xx == 0 ? null : new
            {
                Prop21 = $"text-{x}-{xx}",
                IsDeleted = xx % 2 == 0,
            }).ToList()
        }).ToList();

        var baseCount = PathFunctions.Dictionary[nameof(Enumerable.Count)];
        try
        {
            PathFunctions.Dictionary[nameof(Enumerable.Count)] = (expr, arg) => WithIsDeleted(expr, arg, baseCount);

            var ordered = items.AsQueryable().OrderBy(">Prop2").ToList();
            var orderedTest = items.OrderByDescending(x => x?.Prop2?.Where(xx => xx?.IsDeleted == false).Count()).ToList();

            Assert.That(ordered, Is.EqualTo(orderedTest).AsCollection);

            var selected = items.AsQueryable().Select("Prop1", "Prop2.Count()").ToList();
            var selectedTest = items.Select(x => x == null ? null : new { 
                x.Prop1,
                Prop2Count = x.Prop2?.Where(xx => xx?.IsDeleted == false).Count(),
            }).ToList();

            Assert.That(JsonSerializer.Serialize(selected), Is.EqualTo(JsonSerializer.Serialize(selectedTest)));
        }
        finally
        {
            PathFunctions.Dictionary[nameof(Enumerable.Count)] = baseCount;
        }
    }

    [Test]
    public void Custom()
    {
        var items = Enumerable.Range(0, 10).Select(x => x == 0 ? null : new
        {
            Prop1 = x,
            Prop2 = x == 1 ? null : Enumerable.Range(0, x).Select(xx => xx == 0 ? null : new
            {
                Prop21 = $"text-{x}-{xx}",
                IsDeleted = xx % 2 == 0,
            }).ToList()
        }).ToList();

        var baseCount = PathFunctions.Dictionary[nameof(Enumerable.Count)];

        PathFunctions.Dictionary.Add("CustomCount", (expr, arg) => WithIsDeleted(expr, arg, baseCount));

        var ordered = items.AsQueryable().OrderBy(">Prop2.CustomCount()").ToList();
        var orderedTest = items.OrderByDescending(x => x?.Prop2?.Where(xx => xx?.IsDeleted == false).Count()).ToList();

        Assert.That(ordered, Is.EqualTo(orderedTest).AsCollection);

        var selected = items.AsQueryable().Select("Prop1", "Prop2.CustomCount()").ToList();
        var selectedTest = items.Select(x => x == null ? null : new
        {
            x.Prop1,
            Prop2CustomCount = x.Prop2?.Where(xx => xx?.IsDeleted == false).Count(),
        }).ToList();

        Assert.That(JsonSerializer.Serialize(selected), Is.EqualTo(JsonSerializer.Serialize(selectedTest)));
    }


    Expression WithIsDeleted(Expression expr, Func<Expression, Expression> arg, PathFunctions.ExpressionBuilder baseFn)
    {
        var elementType = GetElementType(expr.Type)!;

        var whereParam = Expression.Parameter(elementType);
        var whereLambda = Expression.Lambda(Expression.AndAlso(
            Expression.NotEqual(whereParam, Expression.Constant(null)),
            Expression.IsFalse(Expression.PropertyOrField(whereParam, "IsDeleted"))),
            whereParam);

        var where = Expression.Call(typeof(Enumerable), nameof(Enumerable.Where), [elementType], [expr, whereLambda]);

        return baseFn(where, arg);
    }


    static Type? GetElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GenericTypeArguments[0];

        var iEnumerable1 = type.GetInterface(typeof(IEnumerable<>).Name);

        if (iEnumerable1 != null)
            return iEnumerable1.GenericTypeArguments[0];

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return typeof(object);

        return null;
    }
}