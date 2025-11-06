using System;
using System.Collections.Generic;
using cfEngine.DataStructure;
using cfEngine;

namespace cfEngine.Command
{
    public partial class CommandService
    {
        public static CommandService instance { get; } = new CommandService();

        private readonly MemoryDictionary<char, CommandService> serviceScopeMap = new();
        private readonly MemoryDictionary<char, ICommandHandler> handlerMap = new();

#if UNITY_EDITOR
        public IReadOnlyDictionary<ReadOnlyMemory<char>, CommandService> ServiceScopeMap => serviceScopeMap;
        public IReadOnlyDictionary<ReadOnlyMemory<char>, ICommandHandler> HandlerMap => handlerMap;
#endif

        public void RegisterScope(string scopeName, CommandService service)
        {
            if(string.IsNullOrEmpty(scopeName))
            {
                Log.LogException(new ArgumentNullException(nameof(scopeName)));
                return;
            }
            if (!serviceScopeMap.TryAdd(scopeName.AsMemory(), service))
                Log.LogWarning($"CommandService: Scope '{scopeName}' is already registered. Ignored.");
        }
        
        public void RegisterHandler(string commandName, ICommandHandler handler)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                Log.LogException(new ArgumentNullException(nameof(commandName)));
                return;
            }
            if (!handlerMap.TryAdd(commandName.AsMemory(), handler))
                Log.LogWarning($"CommandService: Command '{commandName}' is already registered. Ignored.");
        }
        
        public void UnregisterScope(string scopeName)
        {
            serviceScopeMap.Remove(scopeName.AsMemory());
        }

        public void UnregisterHandler(string commandName)
        {
            handlerMap.Remove(commandName.AsMemory());
        }

        public bool TryGetScope(string scopeName, out CommandService scope)
        {
            return serviceScopeMap.TryGetValue(scopeName.AsMemory(), out scope);
        }

        public void Execute(string cmdString)
        {
            using var paramters = Parameters.Get();

            var memory = cmdString.AsMemory();

            ReadOnlyMemory<char> token;
            do
            {
                token = TakeToken(ref memory);
                
                if (token.Span.StartsWith("-"))
                {
                    var parameterValue = TakeToken(ref memory);
                    if (parameterValue.IsEmpty)
                    {
                        Log.LogException(new CommandParseException($"Parameter ({token.ToString()}) value is missing."));
                        return;
                    }
                    
                    paramters.Add(token.ToString(), parameterValue.ToString());
                }

                if (paramters.Count > 0 && !token.IsEmpty)
                {
                    Log.LogException(new CommandParseException("Parameter must be at the end of the command."));
                    return;
                }
                
                if (serviceScopeMap.TryGetValue(token, out var subService))
                {
                    subService.Execute(memory.ToString());
                    return;
                }

                if (handlerMap.TryGetValue(token, out var handler))
                {
                    handler.Execute(paramters);
                    return;
                }
                
                Log.LogException(new CommandParseException($"Unknown command or scope: {token.ToString()}"));
            } while (!token.IsEmpty);
        }
        
        private static ReadOnlySpan<char> TakeToken(ref ReadOnlySpan<char> span, char separator = ' ')
        {
            span = span.TrimStart();
            if (span.IsEmpty)
                return ReadOnlySpan<char>.Empty;

            int start = 0;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != separator)
                    continue;

                if (i <= start)
                    return ReadOnlySpan<char>.Empty;
                
                span = span[(i + 1)..];
                return span[start..i];
            }

            return string.Empty;
        }

        private static ReadOnlyMemory<char> TakeToken(ref ReadOnlyMemory<char> memory, char separator = ' ')
        {
            int trimStart = 0;
            for (trimStart = 0; trimStart < memory.Span.Length; trimStart++)
            {
                if(char.IsWhiteSpace(memory.Span[trimStart]))
                    continue;
            }
            memory = memory[trimStart..];
            
            if (memory.IsEmpty)
                return string.Empty.AsMemory();
            
            int start = 0;
            for (int i = 0; i < memory.Length; i++)
            {
                if (memory.Span[i] != separator)
                    continue;

                if (i <= start)
                    return string.Empty.AsMemory();
                
                memory = memory[(i + 1)..];
                return memory[start..i];
            }
            
            return ReadOnlyMemory<char>.Empty;
        }
    }

    public interface ICommandHandler
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class HintAttribute : Attribute
        {
            public readonly string description;

            public HintAttribute(string description)
            {
                this.description = description;
            }
        }
        
        public void Execute(Parameters param);
    }

    public class CommandParseException : ArgumentException
    {
        public CommandParseException(string command) : base($"Failed to parse command: {command}")
        {
        }
    }
}