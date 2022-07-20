using System;
using SyncTool.Models;
using System.IO;
using static SyncTool.Logger.Logger;

namespace SyncTool.Implementations
{
    public class FileWatcher
    {
        private readonly Options Options;

        public FileWatcher(Options opt)
        {
            Options = opt;
        }


        public void Watch()
        {
            using var watcher = new FileSystemWatcher(Options.Source.FullName);
            

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
           Logger.Logger.Log($"Change detected in {e.Name}",Logger.LogLevel.Info);
            CopyFile(e.FullPath);
        }

        private void CopyFile(string fullPath)
        {
            var fileInfo = GetFileInTargetFolder(fullPath);
            if(fileInfo == null)
            {
                return ;
            }
             Logger.Logger.Log($"Copying {fullPath} to {fileInfo.FullName}",Logger.LogLevel.Info);
            try { 
                File.Copy(fullPath, fileInfo.FullName, true);
            } catch (UnauthorizedAccessException ex)
            {
                Logger.Logger.Log($"Could not Sync {fullPath} as Access is denied",Logger.LogLevel.Warning);
            }
        }

        private FileInfo? GetFileInTargetFolder(string FullPath)
        {
            var relativePath = Path.GetRelativePath(Options.Source.FullName,FullPath);
            if(relativePath.StartsWith(".")) return null;
            var fileInfo = new FileInfo(Options.Target.FullName +Path.DirectorySeparatorChar +relativePath);
            if (!fileInfo.Directory?.Exists ?? false) fileInfo.Directory.Create();
            return fileInfo;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
             Logger.Logger.Log($"Created: {e.FullPath}",Logger.LogLevel.Info);
            CopyFile(e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
             Logger.Logger.Log($"Deleted: {e.FullPath}", Logger.LogLevel.Info);
            FileDelete(e.FullPath);

        }

        private void FileDelete(string fullPath)
        {
            var file = GetFileInTargetFolder(fullPath);
            if (file == null)
            {
                return;
            }
            if (file.Exists)
            {
                 Logger.Logger.Log($"Deleting {file.FullName}", Logger.LogLevel.Info);
                try
                {
                    file.Delete();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Logger.Log($"Could not Sync {fullPath} as Access is denied", Logger.LogLevel.Warning);
                }
            }
        }


        private void OnRenamed(object sender, RenamedEventArgs e)
        {
             Logger.Logger.Log($"Renamed:" , Logger.LogLevel.Info);
             Logger.Logger.Log($"    Old: {e.OldFullPath}", Logger.LogLevel.Info);
             Logger.Logger.Log($"    New: {e.FullPath}", Logger.LogLevel.Info);
            FileDelete(e.OldFullPath);
            CopyFile(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Logger.Logger.Log($"Message: {ex.Message} \n Stacktrace: \n {ex.StackTrace} ", Logger.LogLevel.Error);
                PrintException(ex.InnerException);
            }
        }
    }
}
