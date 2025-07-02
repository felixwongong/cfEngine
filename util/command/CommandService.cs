using System;
using System.Collections.Generic;
using System.Reflection;
using cfEngine.Logging;

namespace cfEngine.Command
{
    public static class CommandService
    {
        public class RegisterOnInitializedAttribute: Attribute 
        {
            public readonly string registerMethodName;
            public RegisterOnInitializedAttribute(string registerMethodName)
            {
                this.registerMethodName = registerMethodName;
            }
        }
        
        private class CommandEqualityComparer : IEqualityComparer<string[]>
        {
            public bool Equals(string[] x, string[] y)
            {
                if (x == null || y == null) return false;
                
                if (x.Length != y.Length)
                    return false;

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                        return false;
                }

                return true;
            }

            public int GetHashCode(string[] obj)
            {
                int hash = 17;
                foreach (var str in obj)
                {
                    hash = hash * 31 + (str?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
        public static Dictionary<string[], ICommand> commandMap = new(new CommandEqualityComparer());

        static CommandService()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    var attribute = type.GetCustomAttribute<RegisterOnInitializedAttribute>();
                    if (attribute == null)
                        continue;
                    
                    var methodInfo = type.GetMethod(attribute.registerMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                    if (methodInfo == null || !methodInfo.IsStatic)
                    {
                        Log.LogError($"[CommandService] Command register method not found, methodName: {attribute.registerMethodName}");
                        continue;
                    }

                    methodInfo.Invoke(null, Array.Empty<object>());
                }
            }
        }
        
        public static void RegisterCommand(ICommand resolver, params string[] commandKeys)
        {
            commandMap[commandKeys] = resolver;
        }
        
        public static void UnregisterCommand(params string[] command)
        {
            commandMap.Remove(command);
        }
    }
}