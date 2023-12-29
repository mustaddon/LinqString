using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqString;

public static class EnumerableSelectExtensions
{
    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, params string[] props)
        => Select(source, SelectorBuilder.Build(source.GetType().GetElementTypeExt()!, props, _nullsafeObjects, _nullsafeEnumerables).Compile());

    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => Select(source, SelectorBuilder.Build(source.GetType().GetElementTypeExt()!, props, _nullsafeObjects, _nullsafeEnumerables).Compile());

    public static IEnumerable<object?> Select<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache cache, Action<ICacheEntry>? options = null)
        => Select(source, cache.GetSelectorDelegate(source.GetType().GetElementTypeExt()!, props, _nullsafeObjects, _nullsafeEnumerables, options));

    
    private static IEnumerable<object?> Select(object source, Delegate fn)
        => (IEnumerable<object?>)_select.MakeGenericMethod(
            fn.Method.GetParameters()[1].ParameterType,
            fn.Method.ReturnType)
        .Invoke(null, [source, fn])!;


    static readonly MethodInfo _select = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(x => x.Name == nameof(Enumerable.Select) && x.IsGenericMethod && x.HasIEnumerableAndFunc());


    const bool _nullsafeObjects = true;
    const bool _nullsafeEnumerables = true;
}
