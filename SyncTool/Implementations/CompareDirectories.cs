using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SyncTool.Converter;
using SyncTool.Logger;
using SyncTool.Models;

namespace SyncTool.Implementations
{
    public class CompareDirectories
    {
        public ConcurrentDictionary<string,string> SourceDictionary { get; set; }
        public ConcurrentDictionary<string, string> TargetDictionary { get; set; }
        private const int THREADS = 24;
        private static readonly Semaphore semaphore = new Semaphore(THREADS,THREADS);

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

        public async Task CompareSourceAndTarget()
        {
            var targetKeys = new List<string>(TargetDictionary.Keys);
            var total = SourceDictionary.Count;
            var count = 0;
            var collection = new ConcurrentDictionary<string, string>(SourceDictionary);
            var enumerator = collection.GetEnumerator();
            Task[] task = new Task[total];
            while (enumerator.MoveNext())
            {

                Logger.Logger.Log($" Added Task: {count} of {total}", LogLevel.Info);

                var key = enumerator.Current.Key;
                var value = enumerator.Current.Value;
                task[count] = new Task(() => CheckFile(targetKeys, key, value));
                task[count].Start();
                count++;
                if (count % 100 == 0) SaveFiles();
            }
           await WaitForTasks(task);

            if (!opt.Opt.Delete) return;

            foreach (var key in targetKeys)
            {
                Logger.Logger.Log($" {key} has been removed, adding to difference List", LogLevel.Info);
                opt.Different.TryAdd(key, DifferenceType.Delete);
            }

        }

        private async Task WaitForTasks(ICollection<Task> task)
        {
            var tasks = Task.WhenAll(task);
            while (true)
            {
            var timer = Task.Delay(10000);
            await Task.WhenAny(tasks,timer);
                 Logger.Logger.Log($" Progress {task.Count(t => t.IsCompleted)} of {task.Count}", LogLevel.Info,System.ConsoleColor.Blue);
               
               SaveFiles();
                 if(tasks.IsCompleted) return;
            }
        }


        private void CheckFile(List<string> targetKeys,string key, string value)
        { 
            if(semaphore.WaitOne()){
            try{
               Logger.Logger.Log($" Started: checking {key}", LogLevel.Info);
               if (TargetDictionary.ContainsKey(key))
                {
                    bool modified = opt.Opt.Md5Hash ? !TargetDictionary[key].Equals(value) : !compare.Compare(
                            opt.SourceDirectoryScanner.RootDirectory.FullName + Path.DirectorySeparatorChar + key,
                            opt.TargetDirectoryScanner.RootDirectory.FullName + Path.DirectorySeparatorChar + key
                        );

                    if (modified)
                    {
                        Logger.Logger.Log($"{key} has been modified, adding to difference List", LogLevel.Info);
                        opt.Different.TryAdd(key, DifferenceType.Modified);
                    }

                    targetKeys.Remove(key);

                }
                else
                {
                    Logger.Logger.Log($"{key} is new, adding to difference List", LogLevel.Info);
                    opt.Different.TryAdd(key, DifferenceType.New);
                }


           
                SourceDictionary.Remove(key, out var stringout);
                Logger.Logger.Log($" Ended: checking {key}", LogLevel.Info);          
                } finally
            {
                  Logger.Logger.Log($" Semaphore Free slots count: {semaphore.Release()}", LogLevel.Info); ;
            }
            }
            
            
        }
    }
}