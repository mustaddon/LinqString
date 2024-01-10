namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static bool TryToNullable(this Type type, out Type nullableType)
    {
        if (type.IsValueType && (!type.IsGenericType || type.GetGenericTypeDefinition() != Types.Nullable1))
        {
            nullableType = Types.Nullable1.MakeGenericType(type);
            return true;
        }

        nullableType = default!;
        return false;
    }
}

