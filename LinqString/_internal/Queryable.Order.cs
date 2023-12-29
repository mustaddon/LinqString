#if NET7_0_OR_GREATER == false
using System.Linq.Expressions;
namespace LinqString._internal;

internal static partial class QueryableExt
{
    internal static IOrderedQueryable<T> Order<T>(this IQueryable<T> source)
    {
        return source.OrderBy(QueryableSorter<T>.IdentityExpr);
    }

    internal static IOrderedQueryable<T> OrderDescending<T>(this IQueryable<T> source)
    {
        return source.OrderByDescending(QueryableSorter<T>.IdentityExpr);
    }

    static class QueryableSorter<T>
    {
        public static readonly Expression<Func<T, T>> IdentityExpr = (x) => x;
    }
}
#endif
