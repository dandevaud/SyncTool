using System.IO;
using CommandLine;
using SyncTool.Logger;

namespace SyncTool.Models
{
    public class Options
    {
        [Option('s', "source", Required = true, ResourceType = typeof(DirectoryInfo))]
        public DirectoryInfo Source { get; set; }

        [Option('t', "target", Required = true, ResourceType = typeof(DirectoryInfo))]
        public DirectoryInfo Target { get; set; }

        [Option('l', "logLevel", Required = false, ResourceType = typeof(LogLevel), Default = Logger.LogLevel.Error)]
        public LogLevel LogLevel { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }

        [Option('d', "delete", Required = false, Default = false, ResourceType = typeof(bool))]
        public bool Delete { get; set; }

        [Option('c', "concurrentThreads", Required = false, Default = 50, ResourceType = typeof(int))]
        public int Semaphore { get; set; }

         [Option('m', "md5", Required = false, Default = false, ResourceType = typeof(bool))]
        public bool Md5Hash { get; set; }

        [Option('r', "realtime", Required = false)]
        public bool RealTime { get; set;}
    }
}