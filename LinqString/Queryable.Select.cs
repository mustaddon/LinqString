using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class QueryableSelectExtensions
{

    public static IQueryable<object?> Select<T>(this IQueryable<T> source, params string[] props)
       => Select(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IQueryable<object?> Select<T>(this IQueryable<T> source, IEnumerable<string> props)
       => Select(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IQueryable<object?> Select<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Select(source, cache != null
            ? cache.GetSelector(source.GetType().GetElementTypeExt()!, props, true, source.Provider is EnumerableQuery<T>, options)
            : SelectorBuilder.Build(source.GetType().GetElementTypeExt()!, props, true, source.Provider is EnumerableQuery<T>));


    private static IQueryable<object?> Select(IQueryable source, LambdaExpression lambda)
        => (IQueryable<object?>)Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Select),
            [lambda.Parameters[0].Type, lambda.Body.Type],
            source.Expression, lambda)
        .Method.Invoke(null, [source, lambda])!;
}
