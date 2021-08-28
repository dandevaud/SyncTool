using System;

namespace SyncTool.Logger.Implementations
{
    public class ConsoleLogger : ILogger
    {
        public static LogLevel LogLevel = LogLevel.Error;
        public ILogger SecondLogger {get;set;}

        public ConsoleLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;

        }

        public void Log(String text, LogLevel logLevel)
        {
            if (LogLevel >= logLevel)
            {
                SetColor(LogLevel);
                Console.WriteLine(text);
            }
            SecondLogger?.Log(text,logLevel);
        }

        public void Log(String text, LogLevel logLevel, ConsoleColor color)
        {
            if (LogLevel >= logLevel)
            {
                Console.BackgroundColor = color;
                SetColor(logLevel);
                Console.WriteLine(text);
                Console.BackgroundColor = ConsoleColor.Black;
            }
            SecondLogger?.Log(text,logLevel);
        }

        private void SetColor(LogLevel logLevel)
        {
            
            switch (logLevel)
            {
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
            if (Console.BackgroundColor == Console.ForegroundColor) Console.BackgroundColor = ConsoleColor.Black;




        }
    }
}