using System.Reflection;
namespace LinqString._internal;

internal static partial class TypeExt
{
    internal static object? DefaultValue(this Type type)
    {
        return _defaultValueMethod.MakeGenericMethod(type).Invoke(null, Array.Empty<object>());
    }

    static T? _defaultValue<T>() => default;

    static readonly MethodInfo _defaultValueMethod = new Func<int>(_defaultValue<int>)
        .Method.GetGenericMethodDefinition();
}

