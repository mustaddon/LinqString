using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class SelectorCacheProvider
{
    public static LambdaExpression GetSelector(this IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeObjects, bool nullsafeEnumerables,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, nullsafeObjects, nullsafeEnumerables, options).Lambda.Value;

    public static Delegate GetSelectorDelegate(this IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeObjects, bool nullsafeEnumerables,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, nullsafeObjects, nullsafeEnumerables, options).Compiled.Value;

    private static CacheValue GetValue(IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeObjects, bool nullsafeEnumerables,
        Action<ICacheEntry>? options)
    {
        var orderedProps = props.Order().Buffer(); // buffer is needed to avoid re-sorting

        var key = new
        {
            Type = type,
            NullsafeEnumerables = nullsafeEnumerables,
            NullsafeObjects = nullsafeObjects,
            SelectProps = string.Join("|", orderedProps),
        };

        return cache.GetOrCreate(key, entry =>
        {
            options?.Invoke(entry);
            return new CacheValue(() => SelectorBuilder.BuildOrdered(type, orderedProps, nullsafeObjects, nullsafeEnumerables));
        })!;
    }
}
