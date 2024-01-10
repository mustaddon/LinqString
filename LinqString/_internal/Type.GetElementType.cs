namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static Type? GetElementTypeExt(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == Types.IEnumerable1)
            return type.GenericTypeArguments[0];

        var iEnumerable1 = type.GetInterface(Types.IEnumerable1.Name);

        if (iEnumerable1 != null)
            return iEnumerable1.GenericTypeArguments[0];

        if (Types.IEnumerable.IsAssignableFrom(type))
            return typeof(object);

        return null;
    }
}

