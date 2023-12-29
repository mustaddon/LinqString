namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static Type? GetElementTypeExt(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == _iEnumerable1)
            return type.GenericTypeArguments[0];

        return type.GetInterface(_iEnumerable1.Name)?.GenericTypeArguments[0];
    }

    static readonly Type _iEnumerable1 = typeof(IEnumerable<>);

}

