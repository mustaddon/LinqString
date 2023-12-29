using System.Reflection;
namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static object? DefaultValue(this Type type)
    {
        return _defaultValueMethod.MakeGenericMethod(type).Invoke(null, Array.Empty<object>());
    }

    static readonly MethodInfo _defaultValueMethod = typeof(TypeExt).GetMethod(nameof(_defaultValue), BindingFlags.Static | BindingFlags.NonPublic)!;
    static T? _defaultValue<T>() => default;
}

