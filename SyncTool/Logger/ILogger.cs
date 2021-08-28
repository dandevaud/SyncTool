namespace SyncTool.Logger
{
    public interface ILogger
    {

        public ILogger SecondLogger {get; set;}
        public void Log(string text, LogLevel logLevel);

    }
}