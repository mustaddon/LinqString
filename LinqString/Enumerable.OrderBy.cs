using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace LinqString;

public static class EnumerableOrderExtensions
{
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, params string[] props)
        => OrderBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => OrderBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Order(source, props, false, cache != null ? CacheProvider(cache, options) : Builder) ?? source.Order();


    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, params string[] props)
        => OrderByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => OrderByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Order(source, props, true, cache != null ? CacheProvider(cache, options) : Builder) ?? source.OrderDescending();


    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, params string[] props)
        => ThenBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props)
        => ThenBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Then(source, props, false, cache != null ? CacheProvider(cache, options) : Builder);


    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, params string[] props)
        => ThenByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props)
        => ThenByDescending(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Then(source, props, true, cache != null ? CacheProvider(cache, options) : Builder);


    private static IOrderedEnumerable<T>? Order<T>(IEnumerable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
    {
        var enumerator = props.GetEnumerator();

        if (!enumerator.MoveNext())
            return null;

        var type = source.GetType().GetElementTypeExt()!;
        var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);
        var method = (desc ? _orderByDesc : _orderBy).MakeGenericMethod(type, sorter.Method.ReturnType);

        return Then((IOrderedEnumerable<T>)method.Invoke(null, [source, sorter])!, type, enumerator, defaultDesc, sorterFactory);
    }

    private static IOrderedEnumerable<T> Then<T>(IOrderedEnumerable<T> source, Type type, IEnumerator<string> enumerator, bool defaultDesc, SorterFactory sorterFactory)
    {
        while (enumerator.MoveNext())
        {
            var (sorter, desc) = sorterFactory(type, enumerator.Current, defaultDesc);
            var method = (desc ? _thenByDesc : _thenBy).MakeGenericMethod(type, sorter.Method.ReturnType);

            source = (IOrderedEnumerable<T>)method.Invoke(null, [source, sorter])!;
        }
        return source;
    }

    private static IOrderedEnumerable<T> Then<T>(IOrderedEnumerable<T> source, IEnumerable<string> props, bool defaultDesc, SorterFactory sorterFactory)
        => Then(source, source.GetType().GetElementTypeExt()!, props.GetEnumerator(), defaultDesc, sorterFactory);

    private static (Delegate Fn, bool Desc) Builder(Type type, string path, bool desc)
    {
        var (lambda, finalDesc) = SorterBuilder.Build(type, path, desc, true);
        return (lambda.Compile(), finalDesc);
    }

    private static SorterFactory CacheProvider(IMemoryCache cache, Action<ICacheEntry>? options)
    {
        return (Type type, string path, bool desc) => cache.GetSorterDelegate(type, path, desc, true, options);
    }


    delegate (Delegate, bool) SorterFactory(Type type, string path, bool desc);

    static readonly MethodInfo _orderBy = new Func<IEnumerable<object>, Func<object, object>, IOrderedEnumerable<object>>(Enumerable.OrderBy)
        .Method.GetGenericMethodDefinition();

    static readonly MethodInfo _orderByDesc = new Func<IEnumerable<object>, Func<object, object>, IOrderedEnumerable<object>>(Enumerable.OrderByDescending)
        .Method.GetGenericMethodDefinition();

    static readonly MethodInfo _thenBy = new Func<IOrderedEnumerable<object>, Func<object, object>, IOrderedEnumerable<object>>(Enumerable.ThenBy)
        .Method.GetGenericMethodDefinition();

    static readonly MethodInfo _thenByDesc = new Func<IOrderedEnumerable<object>, Func<object,object>, IOrderedEnumerable<object>>(Enumerable.ThenByDescending)
        .Method.GetGenericMethodDefinition();

}
