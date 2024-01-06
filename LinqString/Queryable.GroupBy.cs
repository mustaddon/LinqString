using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class QueryableGroupExtensions
{
    public static IQueryable<IGrouping<object, T>> GroupBy<T>(this IQueryable<T> source, params string[] props)
        => GroupBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IQueryable<IGrouping<object, T>> GroupBy<T>(this IQueryable<T> source, IEnumerable<string> props)
        => GroupBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IQueryable<IGrouping<object, T>> GroupBy<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => GroupBy(source, cache != null
            ? cache.GetGrouper(source.GetType().GetElementTypeExt()!, props, source.Provider is EnumerableQuery<T>, options)
            : GrouperBuilder.Build(source.GetType().GetElementTypeExt()!, props, source.Provider is EnumerableQuery<T>));


    private static IQueryable<IGrouping<object, T>> GroupBy<T>(IQueryable<T> source, LambdaExpression lambda)
        => (IQueryable<IGrouping<object, T>>)Expression.Call(
            typeof(Queryable),
            nameof(Queryable.GroupBy),
            [lambda.Parameters[0].Type, lambda.Body.Type],
            source.Expression, lambda)
        .Method.Invoke(null, [source, lambda])!;
}
