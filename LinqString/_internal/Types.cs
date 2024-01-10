using System.Collections;

namespace LinqString._internal;

internal static class Types
{
    internal static readonly Type String = typeof(string);
    internal static readonly Type Queryable = typeof(Queryable);
    internal static readonly Type Enumerable = typeof(Enumerable);
    internal static readonly Type IEnumerable = typeof(IEnumerable);
    internal static readonly Type IEnumerable1 = typeof(IEnumerable<>);
    internal static readonly Type Nullable1 = typeof(Nullable<>);
}
