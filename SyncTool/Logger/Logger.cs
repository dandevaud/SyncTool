using System;
using System.ComponentModel.Design;
using SyncTool.Logger.Implementations;

namespace SyncTool.Logger
{
    public class Logger
    {
        public static ILogger LoggerImpl { get; set; }

        public static void Log(string text, LogLevel logLevel)
        {
            
            LoggerImpl.Log($"{GetAddition(logLevel)} {text}",logLevel);
        }

        public static void Log(string text, LogLevel logLevel, ConsoleColor color)
        {
            if (LoggerImpl.GetType() != typeof(ConsoleLogger))
            {
                Log(text, logLevel);
            } else {
                ((ConsoleLogger) LoggerImpl).Log($"{GetAddition(logLevel)} {text}", logLevel,color);
            }
        }

        public static string GetAddition(LogLevel logLevel)
        {
            DateTime now = DateTime.Now;
            return $"{now}.{now.Millisecond:000} - {logLevel} -";
        }
    }
}