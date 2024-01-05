namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static bool TryToNullable(this Type type, out Type nullableType)
    {
        if (type.IsValueType && (!type.IsGenericType || type.GetGenericTypeDefinition() != _nullable1))
        {
            nullableType = _nullable1.MakeGenericType(type);
            return true;
        }

        nullableType = default!;
        return false;
    }

    static readonly Type _nullable1 = typeof(Nullable<>);

}

