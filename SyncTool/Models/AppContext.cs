using System;
using System.Collections.Concurrent;
using SyncTool.Implementations;

namespace SyncTool.Models
{
    public class AppContext
    {
        public Options Opt { get; set; }

        public DateTime StartTime { get; set; }

        public ScannedDirectoryInfo SourceDirectoryScanner { get; set; }
        public ScannedDirectoryInfo TargetDirectoryScanner { get; set; }

        public ConcurrentDictionary<string, DifferenceType> Different { get; set; } = new ConcurrentDictionary<string, DifferenceType>();

    }
}