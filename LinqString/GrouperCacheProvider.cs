using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class GrouperCacheProvider
{
    public static LambdaExpression GetGrouper(this IMemoryCache cache,
        Type type, IEnumerable<string> props,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, options).Lambda.Value;

    public static Delegate GetGrouperDelegate(this IMemoryCache cache,
        Type type, IEnumerable<string> props,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, options).Compiled.Value;

    private static CacheValue GetValue(IMemoryCache cache,
        Type type, IEnumerable<string> props,
        Action<ICacheEntry>? options)
    {
        var orderedProps = props.Order().Buffer(); // buffer is needed to avoid re-sorting

        var key = new
        {
            Type = type,
            GroupProps = string.Join("|", orderedProps),
        };

        return cache.GetOrCreate(key, entry =>
        {
            options?.Invoke(entry);
            return new CacheValue(() => GrouperBuilder.BuildOrdered(type, orderedProps));
        })!;
    }
}
