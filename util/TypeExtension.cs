using System;
using System.Linq;

namespace cfEngine.Util
{
    public static class TypeExtension
    {
        public static string GetTypeName(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                var genericArgumentsString = string.Join(", ", genericArguments.Select(GetTypeName));
                return $"{genericType.Name}<{genericArgumentsString}>";
            }

            return type.Name;
        }
    }
}