using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SyncTool.Logger;
using SyncTool.Models;

namespace SyncTool.Implementations
{
    public class CopyProcessor
    {
       public AppContext Ctx { get; set; }

        public string SourceFullName { get; set; }

        public string TargetFullName { get; set; }

        private readonly List<string> deleted = new List<string>();
        private readonly List<string> copied = new List<string>();
        public void CopyOrDeleteFiles()
        {
            SourceFullName = Ctx.Opt.Source.FullName+Path.DirectorySeparatorChar;
            TargetFullName = Ctx.Opt.Target.FullName + Path.DirectorySeparatorChar;

            Logger.Logger.Log($"Started Copying Files from {SourceFullName} to {TargetFullName}",LogLevel.Info);
            var tasks =  Ctx.Different.Select(i => Task.Factory.StartNew(() => HandleDifference(i)));
           tasks.ToList().ForEach(i => i.Wait());
           Logger.Logger.Log($"Copying Completed",LogLevel.Info);
           LogChangedFiles();
        }

        private void LogChangedFiles()
        {
            if (Ctx.Opt.LogLevel >= LogLevel.Info)
            {
                Logger.Logger.Log("Following values have been copied:", LogLevel.Info);
                copied.ForEach(i => Logger.Logger.Log(i, LogLevel.Info));
                Logger.Logger.Log("Following values have been Deleted:", LogLevel.Info);
                deleted.ForEach(i => Logger.Logger.Log(i, LogLevel.Info));
            }
        }

        private void HandleDifference(KeyValuePair<string, DifferenceType> keyValue)
        {
            switch (keyValue.Value)
            {
                case DifferenceType.New:
                case DifferenceType.Modified:

                    CopyFile(keyValue);
                    break;
                case DifferenceType.Delete:
                    DeleteFile(keyValue);
                    break;
                default:
                    Logger.Logger.Log($"Difference Type not recognized {keyValue.Value}", LogLevel.Error);
                    break;
            }
        }

        private void DeleteFile(KeyValuePair<string, DifferenceType> keyValue)
        {
            FileInfo targetFileToDelete = new FileInfo(TargetFullName + keyValue.Key);
            Logger.Logger.Log($"File is {keyValue.Value} --> Deleting of {targetFileToDelete.FullName}",
                LogLevel.Info);
            deleted.Add(targetFileToDelete.FullName);
            targetFileToDelete.Delete();
        }

        private void CopyFile(KeyValuePair<string, DifferenceType> keyValue)
        {
            string sourceFile = SourceFullName + keyValue.Key;
            FileInfo targetFile = new FileInfo(TargetFullName + keyValue.Key);
            if (!targetFile.Directory?.Exists ?? false) targetFile.Directory.Create();
            Logger.Logger.Log($"File is {keyValue.Value} --> Copying from {sourceFile} to {targetFile.FullName}",
                LogLevel.Info);
            copied.Add(sourceFile);
            File.Copy(SourceFullName + keyValue.Key, targetFile.FullName, true);
        }
    }
}