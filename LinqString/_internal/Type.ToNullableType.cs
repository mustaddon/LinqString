namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static Type ToNullableType(this Type type)
    {
        if (type.IsValueType && (!type.IsGenericType || type.GetGenericTypeDefinition() != _nullable1))
            return _nullable1.MakeGenericType(type);

        return type;
    }

    static readonly Type _nullable1 = typeof(Nullable<>);

}

