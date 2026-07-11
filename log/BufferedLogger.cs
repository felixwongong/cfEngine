using System;
using System.Collections.Concurrent;

namespace cfEngine
{
    /// <summary>
    /// Thread-safe logger that buffers log messages so they can be flushed to another
    /// logger on a specific thread (e.g. Godot's main thread). Intended for temporary
    /// use during background work that calls the static <see cref="Log"/> API.
    /// </summary>
    public class BufferedLogger : ILogger
    {
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        private enum EntryType
        {
            Debug,
            Info,
            Asset,
            Warning,
            Exception,
            Error
        }

        private readonly record struct LogEntry(EntryType Type, string Message, object? Context, Exception? Exception);

        public void LogDebug(string message, object? context = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Debug, message, context, null));
        }

        public void LogInfo(string message, object? context = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Info, message, context, null));
        }

        public void Asset(bool condition, object? context = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Asset, condition.ToString(), context, null));
        }

        public void LogWarning(string message, object? context = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Warning, message, context, null));
        }

        public void LogException(Exception ex, object? message = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Exception, message?.ToString() ?? string.Empty, message, ex));
        }

        public void LogError(string message, object? context = null)
        {
            _entries.Enqueue(new LogEntry(EntryType.Error, message, context, null));
        }

        /// <summary>
        /// Flushes all buffered entries to <paramref name="target"/> in enqueue order.
        /// Should be called on the thread that owns <paramref name="target"/>
        /// (e.g. Godot's main thread for <see cref="GodotLogger"/>).
        /// </summary>
        public void Flush(ILogger target)
        {
            while (_entries.TryDequeue(out var entry))
            {
                switch (entry.Type)
                {
                    case EntryType.Debug:
                        target.LogDebug(entry.Message, entry.Context);
                        break;
                    case EntryType.Info:
                        target.LogInfo(entry.Message, entry.Context);
                        break;
                    case EntryType.Asset:
                        if (bool.TryParse(entry.Message, out var condition))
                            target.Asset(condition, entry.Context);
                        else
                            target.Asset(false, entry.Context);
                        break;
                    case EntryType.Warning:
                        target.LogWarning(entry.Message, entry.Context);
                        break;
                    case EntryType.Exception:
                        target.LogException(entry.Exception!, entry.Message);
                        break;
                    case EntryType.Error:
                        target.LogError(entry.Message, entry.Context);
                        break;
                }
            }
        }
    }
}
