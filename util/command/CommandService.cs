using System;
using System.Collections.Generic;
using cfEngine.DataStructure;
using cfEngine;
using cfEngine.Util;

namespace cfEngine.Command
{
    public partial class CommandService
    {
        public static CommandService instance { get; } = new CommandService();

        private readonly MemoryDictionary<char, CommandService> serviceScopeMap = new();
        private readonly MemoryDictionary<char, ICommandHandler> handlerMap = new();

        public event Action<CommandExecutionEvent>? CommandExecuted;

        public IEnumerable<string> GetScopeNames()
        {
            foreach (var key in serviceScopeMap.Keys)
                yield return key.ToString();
        }

        public IEnumerable<string> GetHandlerNames()
        {
            foreach (var key in handlerMap.Keys)
                yield return key.ToString();
        }

        public bool TryGetHandler(string commandName, out ICommandHandler? handler)
        {
            return handlerMap.TryGetValue(commandName.AsMemory(), out handler!);
        }

        public void RegisterScope(string scopeName, CommandService service)
        {
            if (!SanityCheck.RequireNonEmpty(scopeName)) return;
            if (!serviceScopeMap.TryAdd(scopeName.AsMemory(), service))
                Log.LogWarning($"CommandService: Scope '{scopeName}' is already registered. Ignored.");
        }
        
        public void RegisterHandler(string commandName, ICommandHandler handler)
        {
            if (!SanityCheck.RequireNonEmpty(commandName)) return;
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

        public bool TryGetScope(string scopeName, out CommandService? scope)
        {
            return serviceScopeMap.TryGetValue(scopeName.AsMemory(), out scope!);
        }

        public void Execute(string cmdString)
        {
            try
            {
                var success = ExecuteInternal(cmdString);
                CommandExecuted?.Invoke(new CommandExecutionEvent(cmdString, success));
            }
            catch (Exception ex)
            {
                CommandExecuted?.Invoke(new CommandExecutionEvent(cmdString, false, ex.Message, ex));
                throw;
            }
        }

        private bool ExecuteInternal(string cmdString)
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
                        return false;
                    }

                    paramters.Add(token.ToString(), parameterValue.ToString());
                }

                if (paramters.Count > 0 && !token.IsEmpty)
                {
                    Log.LogException(new CommandParseException("Parameter must be at the end of the command."));
                    return false;
                }

                if (serviceScopeMap.TryGetValue(token, out var subService))
                {
                    return subService.ExecuteInternal(memory.ToString());
                }

                if (handlerMap.TryGetValue(token, out var handler))
                {
                    handler.Execute(paramters);
                    return true;
                }

                Log.LogException(new CommandParseException($"Unknown command or scope: {token.ToString()}"));
            } while (!token.IsEmpty);

            return false;
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

                var nextToken = span[start..i];
                span = span[(i + 1)..];
                return nextToken;
            }

            var finalToken = span;
            span = ReadOnlySpan<char>.Empty;
            return finalToken;
        }

        private static ReadOnlyMemory<char> TakeToken(ref ReadOnlyMemory<char> memory, char separator = ' ')
        {
            int trimStart = 0;
            for (trimStart = 0; trimStart < memory.Span.Length; trimStart++)
            {
                if(char.IsWhiteSpace(memory.Span[trimStart]))
                    continue;

                break;
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

                var nextToken = memory[start..i];
                memory = memory[(i + 1)..];
                return nextToken;
            }
            
            var finalToken = memory;
            memory = ReadOnlyMemory<char>.Empty;
            return finalToken;
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
