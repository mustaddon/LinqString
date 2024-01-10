using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class QueryableOrderExtensions
{
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, params string[] props)
        => OrderBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<string> props)
        => OrderBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Order(source, props, false, cache != null ? CacheProvider(cache, options) : SorterBuilder.Build) ?? source.Order();


    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, params string[] props)
        => OrderByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IEnumerable<string> props)
        => OrderByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Order(source, props, true, cache != null ? CacheProvider(cache, options) : SorterBuilder.Build) ?? source.OrderDescending();


    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, params string[] props)
        => ThenBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IEnumerable<string> props)
        => ThenBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Then(source, props, false, cache != null ? CacheProvider(cache, options) : SorterBuilder.Build);


    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, params string[] props)
        => ThenByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IEnumerable<string> props)
        => ThenByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Then(source, props, true, cache != null ? CacheProvider(cache, options) : SorterBuilder.Build);


    private static IOrderedQueryable<T>? Order<T>(IQueryable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
    {
        var enumerator = props.GetEnumerator();

        if (!enumerator.MoveNext())
            return null;

        var type = source.GetType().GetElementTypeExt()!;
        var nullsafeEnumerables = source.Provider is EnumerableQuery;
        var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc, nullsafeEnumerables);

        return Then((IOrderedQueryable<T>)source.Provider.CreateQuery<T>(Expression.Call(
            Types.Queryable,
            desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy),
            [type, sorter.Body.Type],
            source.Expression, sorter)), type, enumerator, defaultDesc, nullsafeEnumerables, sorterFactory);
    }

    private static IOrderedQueryable<T> Then<T>(IOrderedQueryable<T> source, Type type, IEnumerator<string> enumerator, bool defaultDesc, bool nullsafeEnumerables, SorterFactory sorterFactory)
    {
        while (enumerator.MoveNext())
        {
            var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc, nullsafeEnumerables);

            source = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(Expression.Call(
                Types.Queryable,
                desc ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy),
                [type, sorter.Body.Type],
                source.Expression, sorter)); // Expression.Quote(sorter)
        }
        return source;
    }

    private static IOrderedQueryable<T> Then<T>(IOrderedQueryable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
        => Then(source, source.GetType().GetElementTypeExt()!, props.GetEnumerator(), defaultDesc, source.Provider is EnumerableQuery, sorterFactory);


    private static SorterFactory CacheProvider(IMemoryCache cache, Action<ICacheEntry>? options)
    {
        return (type, path, desc, nullsafeEnumerables) => cache.GetSorter(type, path, desc, nullsafeEnumerables, options);
    }

    delegate (LambdaExpression lambda, bool finalDesc) SorterFactory(Type type, string path, bool desc, bool nullsafeEnumerables);
}
