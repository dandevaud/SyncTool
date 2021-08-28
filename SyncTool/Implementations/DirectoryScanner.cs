using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SyncTool.Logger;
using SyncTool.Models;
using AppContext = SyncTool.Models.AppContext;

namespace SyncTool.Implementations
{
    public class DirectoryScanner
    {

        public ScannedDirectoryInfo Dir {get; set;}


        private static Semaphore semaphore;

        [NonSerialized]
       private readonly AppContext ctx;

        public DirectoryScanner(AppContext ctx)
        {
            this.ctx = ctx;
            semaphore ??= new Semaphore(ctx.Opt.Semaphore, ctx.Opt.Semaphore);
            
        }
        
        public List<Thread> CheckDirectory()
        {
            Logger.Logger.Log($"Checking { Dir.CurrentDirectory.FullName} started",LogLevel.Info);
           
            List<Thread> tasks = new List<Thread>();
            foreach (var dirInfo in  Dir.CurrentDirectory.GetDirectories().AsParallel())
            {
                tasks.Add(new Thread(new ThreadStart(() => HandleSubDirectories(dirInfo)))
                {
                    Name = dirInfo.Name,
                    CurrentCulture = CultureInfo.CurrentCulture,
                    IsBackground = true
                });
            }

            tasks.Add(new Thread(new ThreadStart(AddFileChecksum)));
            var threading = new Threading();
            threading.AddAllThreads(tasks);
            threading.StartWork();
            Logger.Logger.Log($"Checking { Dir.CurrentDirectory.FullName} finished",LogLevel.Info);
            return tasks;
        }

        private void HandleSubDirectories(object directoryInfoObj)
        {
            DirectoryInfo directoryInfo = (DirectoryInfo) directoryInfoObj;

                DirectoryScanner dirScanner = new DirectoryScanner(ctx)
                {
                    Dir= new ScannedDirectoryInfo(){
                     
                       CurrentDirectory = directoryInfo,
                     RootDirectory = Dir.RootDirectory
                    }
                };
                dirScanner.CheckDirectory();
                foreach (var (key, value) in dirScanner.Dir.PathChecksum)
                {
                    Dir.PathChecksum.TryAdd(key, value);
                }
         
        }


        private void AddFileChecksum()
        {
            var threading = new Threading();
            var tasks = Dir.CurrentDirectory.GetFiles().Select(i => new Thread(new ThreadStart(() => AddToPathChecksum(i)))
            {
                Name = i.Name,
                IsBackground = true,
                CurrentCulture = CultureInfo.CurrentCulture
            }).ToList();
           threading.AddAllThreads(tasks);
           threading.StartWork();
        }

        private void AddToPathChecksum(FileInfo item)
        {
             Dir.PathChecksum.TryAdd(Path.GetRelativePath(Dir.RootDirectory.FullName, item.FullName),
                GetMD5Hash(item));
        }

        private string GetMD5Hash(FileInfo file)
        {
            if(!ctx.Opt.Md5Hash) return "MD5 Hash not compute";
            try
            {
                semaphore.WaitOne();
               Logger.Logger.Log($"Getting MD5 for {file.FullName}",LogLevel.Debug);
                using var md5 = MD5.Create();
                using (var stream = file.OpenRead())
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            finally
            {
                Logger.Logger.Log($"Successfully got MD5 for {file.FullName}",LogLevel.Debug);
                semaphore.Release();
            }
        }
    }
}