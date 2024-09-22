using System;

namespace cfEngine.Logging
{
    [Flags]
    public enum LogType
    {
        Debug = 1 << 0,
        Assert = 1 << 1,
        Info = 1 << 2,
        Warning = 1 << 3,
        Exception = 1 << 5,
        Error = 1 << 5,
    }

    public enum LogLevel
    {
        Error = LogType.Error | LogType.Exception,
        Warn = LogType.Warning | Error,
        Info = LogType.Info | Warn,
        Verbose = LogType.Info,
        Debug = LogType.Assert | LogType.Debug | Verbose,
    }
    
    public interface ILogger
    {
        void LogDebug(string message, object context = null);
        void LogInfo(string message, object context = null);
        void Asset(bool condition, object context = null);
        void LogWarning(string message, object context = null);
        void LogException(Exception ex, object message = null);
        void LogError(string message, object context = null);
    }
}