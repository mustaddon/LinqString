#if NET7_0_OR_GREATER == false
namespace LinqString._internal;

internal static partial class EnumerableExt
{
    internal static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(EnumerableSorter<T>.IdentityFunc);
    }

    internal static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> source)
    { 
        return source.OrderByDescending(EnumerableSorter<T>.IdentityFunc);
    }

    static class EnumerableSorter<T>
    {
        public static readonly Func<T, T> IdentityFunc = (x) => x;
    }
}
#endif
