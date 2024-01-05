using Microsoft.Extensions.Caching.Memory;

namespace LinqString;

public static class DefaultCacheSettings
{
    public static IMemoryCache? Instance;
    public static Action<ICacheEntry>? Entry;
}
