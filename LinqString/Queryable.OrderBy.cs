using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class QueryableOrderExtensions
{
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, params string[] props)
        => OrderBy(source, props.AsEnumerable());

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<string> props)
        => Order(source, props, false, SorterBuilder.Build) ?? source.Order();

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Order(source, props, false, CacheProvider(cache, options)) ?? source.Order();


    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, params string[] props)
        => OrderByDescending(source, props.AsEnumerable());

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IEnumerable<string> props)
        => Order(source, props, true, SorterBuilder.Build) ?? source.OrderDescending();

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Order(source, props, true, CacheProvider(cache, options)) ?? source.OrderDescending();


    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, params string[] props)
        => ThenBy(source, props.AsEnumerable());

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IEnumerable<string> props)
        => Then(source, props.GetEnumerator(), false, SorterBuilder.Build);

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Then(source, props.GetEnumerator(), false, CacheProvider(cache, options));


    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, params string[] props)
        => ThenByDescending(source, props.AsEnumerable());

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IEnumerable<string> props)
        => Then(source, props.GetEnumerator(), false, SorterBuilder.Build);

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Then(source, props.GetEnumerator(), false, CacheProvider(cache, options));


    private static IOrderedQueryable<T>? Order<T>(IQueryable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
    {
        var enumerator = props.GetEnumerator();

        if (!enumerator.MoveNext())
            return null;

        var type = typeof(T);
        var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);

        return Then((IOrderedQueryable<T>)source.Provider.CreateQuery<T>(Expression.Call(
            _queryableType,
            desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy),
            [typeof(T), sorter.Body.Type],
            source.Expression, sorter)), enumerator, defaultDesc, sorterFactory);
    }

    private static IOrderedQueryable<T> Then<T>(IOrderedQueryable<T> source, IEnumerator<string> enumerator, bool defaultDesc, SorterFactory sorterFactory)
    {
        var type = typeof(T);
        while (enumerator.MoveNext())
        {
            var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);

            source = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(Expression.Call(
                _queryableType,
                desc ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy),
                [type, sorter.Body.Type],
                source.Expression, sorter)); // Expression.Quote(sorter)
        }
        return source;
    }

    private static SorterFactory CacheProvider(IMemoryCache cache, Action<ICacheEntry>? options)
    {
        return (type, path, desc) => cache.GetSorter(type, path, desc, options);
    }

    delegate (LambdaExpression lambda, bool finalDesc) SorterFactory(Type type, string path, bool desc);

    static readonly Type _queryableType = typeof(Queryable);
}
