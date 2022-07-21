using System.IO;
using CommandLine;
using SyncTool.Logger;
using System;

namespace SyncTool.Models
{
    public class Options
    {
        [Option('s', "source", Required = false, ResourceType = typeof(DirectoryInfo), HelpText ="Source Directory")]
        public DirectoryInfo Source { get; set; } = new DirectoryInfo(Environment.CurrentDirectory);

        [Option('t', "target", Required = false, ResourceType = typeof(DirectoryInfo), HelpText = "Target Directory")]
        public DirectoryInfo Target { get; set; } = new DirectoryInfo(Environment.CurrentDirectory);

        [Option('l', "logLevel", Required = false, ResourceType = typeof(LogLevel), Default = Logger.LogLevel.Error)]
        public LogLevel LogLevel { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }

        [Option('d', "delete", Required = false, Default = false, ResourceType = typeof(bool), HelpText ="Should files in the target directory be deleted if noneexistent in the source (Default: false)")]
        public bool Delete { get; set; }

        [Option('c', "concurrentThreads", Required = false, Default = 50, ResourceType = typeof(int), HelpText ="Number of concurrent Threads running (default: 50)") ]
        public int Semaphore { get; set; }

        [Option('m', "md5", Required = false, Default = false, ResourceType = typeof(bool), HelpText ="Use MD5 Hash to compare (might impact the performance)")]
        public bool Md5Hash { get; set; }

        [Option('w', "watch", Required = false, HelpText ="Will watch the source directory and directly sync it to the target directory")]
        public bool Watch { get; set;}
    }
}