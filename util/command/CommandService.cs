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

        /// <summary>
        /// Reads a handler's declared argument metadata. The lookup goes through the
        /// <see cref="ICommandHandler"/> interface (instance dispatch honours explicit
        /// implementations and the default member, unlike reflecting a concrete type). The
        /// result is per-instance and may legitimately be empty; a null implementation result
        /// is defensively coalesced to an empty list.
        /// </summary>
        public static IReadOnlyList<CommandArg> ReadArguments(ICommandHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return handler.Arguments ?? Array.Empty<CommandArg>();
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
                    // Parse any trailing "-name value" pairs into the parameter map
                    // before invoking the handler. The scope/handler walk consumes
                    // one token per iteration, so parameters following the handler
                    // name are collected here rather than in the main loop.
                    while (true)
                    {
                        var paramName = TakeToken(ref memory);
                        if (paramName.IsEmpty)
                            break;

                        if (!paramName.Span.StartsWith("-"))
                        {
                            Log.LogException(new CommandParseException($"Parameter name must start with '-': {paramName.ToString()}"));
                            return false;
                        }

                        var paramValue = TakeToken(ref memory);
                        if (paramValue.IsEmpty)
                        {
                            // Presence-only flag (e.g. "info sync --strict"): register with
                            // an empty value instead of failing. A trailing token with no
                            // successor is a flag. Mid-command bare flags remain unsupported.
                            paramters.Add(paramName.ToString(), string.Empty);
                            break;
                        }

                        paramters.Add(paramName.ToString(), paramValue.ToString());
                    }

                    return handler.Execute(paramters);
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

    /// <summary>
    /// Advisory descriptor for a command argument's wire value. These are documentation/UI
    /// hints only: neither the token walker nor the executor converts or enforces them.
    /// </summary>
    public enum ArgType
    {
        String,
        Int,
        Float,
        Bool,
        Choice,
    }

    /// <summary>
    /// Immutable, descriptive metadata for a single command argument. This is metadata only:
    /// nothing here binds, parses or converts the string wire value, and no field gates
    /// execution. <see cref="Name"/> preserves the supplied name exactly, including any leading
    /// hyphen(s) (e.g. <c>-id</c>, <c>--strict</c>) — metadata authors supply the leading
    /// hyphen. <see cref="Required"/> is likewise descriptive.
    /// </summary>
    public sealed class CommandArg
    {
        private static readonly IReadOnlyList<string> NoChoices = Array.Empty<string>();

        public string Name { get; }
        public ArgType Type { get; }
        public string Description { get; }
        public bool Required { get; }
        public IReadOnlyList<string> Choices { get; }

        public CommandArg(string name, ArgType type, string description = "", bool required = false,
            IReadOnlyList<string>? choices = null)
        {
            Name = name;
            Type = type;
            Description = description;
            Required = required;
            // Snapshot supplied choices into a read-only managed collection so later mutations
            // of the caller's array/list cannot change this spec.
            Choices = choices == null || choices.Count == 0
                ? NoChoices
                : new List<string>(choices).AsReadOnly();
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
        
        public bool Execute(Parameters param);

        /// <summary>
        /// Descriptive argument metadata for this handler instance. This is a default interface
        /// implementation, so existing Execute-only handlers compile and behave unchanged (an
        /// empty, non-null list). Metadata is per-instance rather than per-type: shared handler
        /// types (including delegate wrappers) can override this property on each instance.
        /// </summary>
        public IReadOnlyList<CommandArg> Arguments => Array.Empty<CommandArg>();
    }

    public class CommandParseException : ArgumentException
    {
        public CommandParseException(string command) : base($"Failed to parse command: {command}")
        {
        }
    }
}
