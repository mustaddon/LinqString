using System.Collections;
using System.Reflection;
namespace LinqString._internal;

internal static partial class MemberExt
{
    internal static bool HasIEnumerableAndFunc(this MethodInfo method)
    {
        if(!method.ContainsGenericParameters)
            return false;

        var parameters = method.GetParameters();

        return parameters.Length == 2
            && parameters[0].ParameterType.IsGenericType
            && parameters[1].ParameterType.IsGenericType
            && typeof(IEnumerable).IsAssignableFrom(parameters[0].ParameterType)
            && parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>);
    }

}

