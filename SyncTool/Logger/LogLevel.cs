using System;

namespace SyncTool.Logger
{
    [Flags]
    public enum LogLevel : short
    {
        Error = 0,
        Warning = 1,
        Info = 2,
        Debug = 4
    }
}