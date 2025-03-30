using System;
using System.Linq;
using System.Reflection;

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
        
        private static readonly BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public static MethodInfo[] GetFlattenMethods(this Type type)
        {
            return type.GetMethods(FLAGS);
        }
        
        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            if (type == typeof(string))
                return string.Empty;
            
            return null;
        }
    }
}