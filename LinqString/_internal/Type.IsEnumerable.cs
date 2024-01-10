using System.Collections;

namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static bool IsEnumerable(this Type type)
        => !type.IsValueType && type != Types.String && (type.IsArray || Types.IEnumerable.IsAssignableFrom(type));
}

