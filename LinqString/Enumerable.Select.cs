using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace LinqString;

public static class EnumerableSelectExtensions
{
    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, params string[] props)
        => Select(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => Select(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => Select(source, cache != null
            ? cache.GetSelectorDelegate(source.GetType().GetElementTypeExt()!, props, true, true, options)
            : SelectorBuilder.Build(source.GetType().GetElementTypeExt()!, props, true, true).Compile());


    private static IEnumerable<object?> Select(object source, Delegate fn)
        => (IEnumerable<object?>)_select.MakeGenericMethod(
            fn.Method.GetParameters()[1].ParameterType,
            fn.Method.ReturnType)
        .Invoke(null, [source, fn])!;


    static readonly MethodInfo _select = new Func<IEnumerable<object>, Func<object, object>, IEnumerable<object>>(Enumerable.Select)
        .Method.GetGenericMethodDefinition();
}
