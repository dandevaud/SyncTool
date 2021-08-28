using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyncTool.Models
{
   public class ScannedDirectoryInfo
    {
        
        public ConcurrentDictionary<string,string> PathChecksum { get; set; } = new ConcurrentDictionary<string, string>();

        public DirectoryInfo CurrentDirectory { get; set; }
        public DirectoryInfo RootDirectory { get; set; }

        
    }
}
