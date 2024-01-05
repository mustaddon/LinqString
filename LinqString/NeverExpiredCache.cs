using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace LinqString;

public class NeverExpiredCache : IMemoryCache
{
    public static readonly NeverExpiredCache Instance = new();


    private readonly ConcurrentDictionary<object, ICacheEntry> _items = new();

    public ICacheEntry CreateEntry(object key) => _items.GetOrAdd(key, k => new Entry(key));

    public bool TryGetValue(object key, out object? value)
    {
        if (_items.TryGetValue(key, out var entry))
        {
            value = entry.Value;
            return true;
        }

        value = null;
        return false;
    }

    public void Dispose() => GC.SuppressFinalize(this);
    public void Remove(object key) => _items.TryRemove(key, out _);
    public void Clear() => _items.Clear();
    public int Count => _items.Count;


    private class Entry(object key) : ICacheEntry
    {
        public object Key { get; } = key;
        public object? Value { get; set; }
        public long? Size { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; }
        public IList<IChangeToken> ExpirationTokens { get => throw new NotSupportedException(); }
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get => throw new NotSupportedException(); }
        public void Dispose() { }
    }
}
