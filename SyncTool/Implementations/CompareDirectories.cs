using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SyncTool.Converter;
using SyncTool.Logger;
using SyncTool.Models;

namespace SyncTool.Implementations
{
    public class CompareDirectories
    {
        public ConcurrentDictionary<string,string> SourceDictionary { get; set; }
        public ConcurrentDictionary<string, string> TargetDictionary { get; set; }

        private AppContext opt;
        private readonly FileCompare compare = new FileCompare();
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            Converters =
            {
                new DirectoryInfoConverter()
            },
             WriteIndented = true,
                MaxDepth = 20
        };
 

        public CompareDirectories(AppContext opt)
        {
            this.opt = opt;
            SourceDictionary = opt.SourceDirectoryScanner.PathChecksum;
            TargetDictionary = new ConcurrentDictionary<string, string>(opt.TargetDirectoryScanner.PathChecksum);
        }

        private void SaveFiles()
        {
            lock("File"){
            var optString = JsonSerializer.Serialize(opt,jsonSerializerOptions);
                File.WriteAllText(opt.SourceDirectoryScanner.RootDirectory.FullName+Path.DirectorySeparatorChar+"saved.opt",string.Empty);
            using FileStream outstream = new FileStream(opt.SourceDirectoryScanner.RootDirectory.FullName+Path.DirectorySeparatorChar+"saved.opt",FileMode.OpenOrCreate,FileAccess.Write);
            using StreamWriter writer = new StreamWriter(outstream);
            writer.Write(optString);
            writer.Flush();
                Logger.Logger.Log("File Saved",LogLevel.Info);
            }
        }

          public static AppContext GetSavedFiles(DirectoryInfo sourceDir)
        {
            using FileStream stream = new FileStream(sourceDir.FullName+Path.DirectorySeparatorChar+"saved.opt",FileMode.Open,FileAccess.Read);
            using StreamReader read = new StreamReader(stream);
            var optString = read.ReadToEnd();
            return JsonSerializer.Deserialize<AppContext>(optString,jsonSerializerOptions);
               
         }

        public void CompareSourceAndTarget()
        {
            var targetKeys = new List<string>(TargetDictionary.Keys);
            var total = SourceDictionary.Count;
            var count = 0;
            var collection = new ConcurrentDictionary<string,string>(SourceDictionary);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Current.Key;
                var value = enumerator.Current.Value;
                Logger.Logger.Log($" Progress: {count} of {total}", LogLevel.Info);
                count++;
                if (TargetDictionary.ContainsKey(key))
                {
                    bool modified = opt.Opt.Md5Hash ? !TargetDictionary[key].Equals(value) : !compare.Compare(
                            opt.SourceDirectoryScanner.RootDirectory.FullName+Path.DirectorySeparatorChar+key,
                            opt.TargetDirectoryScanner.RootDirectory.FullName+Path.DirectorySeparatorChar+key
                        );

                    if (modified)
                    {
                        Logger.Logger.Log($"{key} has been modified, adding to difference List",LogLevel.Info);
                        opt.Different.TryAdd(key, DifferenceType.Modified);
                    }

                    targetKeys.Remove(key);

                }
                else
                {
                    Logger.Logger.Log($"{key} is new, adding to difference List", LogLevel.Info);
                    opt.Different.TryAdd(key, DifferenceType.New);
                }


                if(count%100==0) SaveFiles();
                SourceDictionary.Remove(key, out var stringout);

            }

            if (!opt.Opt.Delete) return;
           
                foreach (var key in targetKeys)
                {
                    Logger.Logger.Log($"{key} has been removed, adding to difference List", LogLevel.Info);
                    opt.Different.TryAdd(key, DifferenceType.Delete);
                }
            
        }
    }
}