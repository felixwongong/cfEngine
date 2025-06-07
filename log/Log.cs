using System;
using cfEngine.Extension;

namespace cfEngine.Logging
{
    public static class Log
    {
        private static ILogger _logger;
        private static LogLevel _logLevel = LogLevel.Info;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public static void LogDebug(string message, object context = null)
        {
            if (_logLevel.hasFlag(LogType.Debug))
            {
                _logger.LogDebug(message, context);
            }
        }

        public static void LogInfo(string message, object context = null)
        {
            if (_logLevel.hasFlag(LogType.Info))
            {
                _logger.LogInfo(message, context);
            }
        }

        public static void Asset(bool condition, object context = null)
        {
            if (_logLevel.hasFlag(LogType.Assert))
            {
                _logger.Asset(condition, context);
            }
        }

        public static void LogWarning(string message, object context = null)
        {
            if (_logLevel.hasFlag(LogType.Warning))
            {
                _logger.LogWarning(message, context);
            }
        }

        public static void LogException(Exception ex, string message = null)
        {
            if (_logLevel.hasFlag(LogType.Exception))
            {
                _logger.LogException(ex, message);
            }
        }

        public static void LogError(string message, object context = null)
        {
            if (_logLevel.hasFlag(LogType.Error))
            {
                _logger.LogError(message, context);
            }
        }
    }
}