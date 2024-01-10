using System.Linq.Expressions;

namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static bool TryGetElementType(this Type type, out Type elementType)
    {
        if (!type.IsValueType && type != Types.String)
        {
            elementType = type.GetElementTypeExt()!;
            return elementType != null;
        }

        elementType = default!;
        return false;
    }
}

