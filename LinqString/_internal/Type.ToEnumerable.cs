namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static Type ToEnumerable(this Type type)
        => Types.IEnumerable1.MakeGenericType(type);
}

