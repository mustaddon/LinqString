using LinqString._internal;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LinqString;

public static class SorterCacheProvider
{
    public static (LambdaExpression lambda, bool finalDesc) GetSorter(this IMemoryCache cache,
        Type sourceType, string path, bool desc, bool nullsafeEnumerables = false,
        Action<ICacheEntry>? options = null)
    {
        var final = SorterBuilder.TrimDirection(path, desc);
        return (GetValue(cache, sourceType, final.path, nullsafeEnumerables, options).Lambda.Value, final.desc);
    }

    public static (Delegate fn, bool finalDesc) GetSorterDelegate(this IMemoryCache cache,
        Type sourceType, string path, bool desc, bool nullsafeEnumerables = false,
        Action<ICacheEntry>? options = null)
    {
        var final = SorterBuilder.TrimDirection(path, desc);
        return (GetValue(cache, sourceType, final.path, nullsafeEnumerables, options).Compiled.Value, final.desc);
    }

    private static CacheValue GetValue(IMemoryCache cache,
        Type sourceType, string path, bool nullsafeEnumerables,
        Action<ICacheEntry>? options)
    {
        return cache.GetOrCreate(new
        {
            Type = sourceType,
            NullsafeEnumerables = nullsafeEnumerables,
            SortProp = path
        }, entry =>
        {
            options?.Invoke(entry);
            return new CacheValue(() => SorterBuilder.BuildLambda(sourceType, path, nullsafeEnumerables));
        })!;
    }
}
