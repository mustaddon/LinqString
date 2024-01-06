using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class GrouperCacheProvider
{
    public static LambdaExpression GetGrouper(this IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeEnumerables = false,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, nullsafeEnumerables, options).Lambda.Value;

    public static Delegate GetGrouperDelegate(this IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeEnumerables = false,
        Action<ICacheEntry>? options = null)
        => GetValue(cache, type, props, nullsafeEnumerables, options).Compiled.Value;

    private static CacheValue GetValue(IMemoryCache cache,
        Type type, IEnumerable<string> props, bool nullsafeEnumerables,
        Action<ICacheEntry>? options)
    {
        var orderedProps = props.Order().Buffer(); // buffer is needed to avoid re-sorting

        return cache.GetOrCreate(new
        {
            Type = type,
            NullsafeEnumerables = nullsafeEnumerables,
            GroupProps = string.Join("|", orderedProps),
        }, entry =>
        {
            options?.Invoke(entry);
            return new CacheValue(() => GrouperBuilder.BuildOrdered(type, orderedProps, nullsafeEnumerables));
        })!;
    }
}
