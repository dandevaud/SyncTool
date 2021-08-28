using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyncTool.Logger.Implementations
{
    class FileLogger : ILogger
    {
        private static readonly DateTime now= DateTime.Now;
        public string FilePath {get;set;} = $"./Logs/Log-Sync-{now.ToShortDateString()}-{now.ToShortTimeString()}.log";
        public LogLevel LogLevel {get; set;}

        public ILogger SecondLogger {get;set;}

        private FileStream ostrm;
        private StreamWriter writer;


        public FileLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;
           Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            ostrm = new FileStream(FilePath,FileMode.OpenOrCreate,FileAccess.Write);
            writer = new StreamWriter(ostrm);
           
        }
        public void Log(string text, LogLevel logLevel)
        {
            if(LogLevel>=logLevel){
                writer.WriteLine(text);
                writer.Flush();
            }
             SecondLogger?.Log(text,logLevel);
        }

        public void PrintLogFileLocation()
        {
             Logger.LoggerImpl.Log($"Log File located: {ostrm.Name}",LogLevel.Info);
        }
    }
}
