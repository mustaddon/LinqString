using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace LinqString;

public static class EnumerableGroupExtensions
{
    public static IEnumerable<IGrouping<object, T>> GroupBy<T>(this IEnumerable<T> source, params string[] props)
        => GroupBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IEnumerable<IGrouping<object, T>> GroupBy<T>(this IEnumerable<T> source, IEnumerable<string> props)
        => GroupBy(source, props, DefaultCacheSettings.Instance, DefaultCacheSettings.Entry);

    public static IEnumerable<IGrouping<object, T>> GroupBy<T>(this IEnumerable<T> source, IEnumerable<string> props, IMemoryCache? cache, Action<ICacheEntry>? options = null)
        => GroupBy(source, cache != null 
            ? cache.GetGrouperDelegate(source.GetType().GetElementTypeExt()!, props, options) 
            : GrouperBuilder.Build(source.GetType().GetElementTypeExt()!, props).Compile());


    private static IEnumerable<IGrouping<object, T>> GroupBy<T>(IEnumerable<T> source, Delegate fn)
        => (IEnumerable<IGrouping<object, T>>)_groupBy.MakeGenericMethod(
            fn.Method.GetParameters()[1].ParameterType,
            fn.Method.ReturnType)
        .Invoke(null, [source, fn])!;


    static readonly MethodInfo _groupBy = new Func<IEnumerable<object>, Func<object, object>, IEnumerable<IGrouping<object, object>>>(Enumerable.GroupBy)
        .Method.GetGenericMethodDefinition();
}
