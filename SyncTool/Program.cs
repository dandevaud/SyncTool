using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Win32.SafeHandles;
using SyncTool.Implementations;
using SyncTool.Logger;
using SyncTool.Logger.Implementations;
using SyncTool.Models;
using AppContext = SyncTool.Models.AppContext;


namespace SyncTool
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var cmParser = new CommandLine.Parser(x =>
            {
                x.CaseSensitive = false;
                x.AutoHelp = true;
                x.HelpWriter = Console.Error;
                x.IgnoreUnknownArguments = false;
            });
            var opt = new Options();
            Console.WriteLine("args: "+String.Join(" ",args.ToList()));
            cmParser.ParseArguments<Options>(args).WithParsed(SyncJob);           
        }

       

        private static void SyncJob(Options opt)
        {
            if (opt.Verbose)
            {
                opt.LogLevel = LogLevel.Info;
            }
            Logger.Logger.LoggerImpl = new ConsoleLogger(opt.LogLevel){
                SecondLogger = new FileLogger(opt.LogLevel)               
            };

            ((FileLogger) Logger.Logger.LoggerImpl.SecondLogger).PrintLogFileLocation();
            
            var start = DateTime.Now;
            Logger.Logger.Log("======= Sync Tool started =======",LogLevel.Info);
            AppContext ctx = new AppContext()
            {
                Opt = opt,
                StartTime = DateTime.Now
            };
            if (!File.Exists(opt.Source.FullName + Path.DirectorySeparatorChar + "saved.opt"))
            {
                ScanDirectories(ctx);
            } else
            {
                ctx = CompareDirectories.GetSavedFiles(opt.Source);
            }           
            
            CompareMaps(ctx);
            CopyOrDeleteFiles(ctx);
            Logger.Logger.Log($"Total time: {DateTime.Now.Subtract(start):hh\\:mm\\:ss\\.fff}",LogLevel.Info);
            Logger.Logger.Log("======= Sync Tool Ended =======", LogLevel.Info);
            File.Delete(opt.Source.FullName + Path.DirectorySeparatorChar + "saved.opt");
            WriteLogProgress(ctx);

        }

        private static void WriteLogProgress(AppContext ctx)
        {
            using StreamWriter logSource = new($"{ctx.Opt.Source.FullName}{Path.DirectorySeparatorChar}Sync.log",true)
            {
                AutoFlush = true
            };
            using StreamWriter logTarget = new($"{ctx.Opt.Target.FullName}{Path.DirectorySeparatorChar}Sync.log",true)
            {
                AutoFlush = true
            };

            string sourceCheck =
                $"Scanned total of {ctx.SourceDirectoryScanner.PathChecksum.Count} files in the source directory";
            string targetCheck =
                $"Scanned total of {ctx.TargetDirectoryScanner.PathChecksum.Count} files in the target directory";
            string difference =
                $"Found {ctx.Different.Count} differences";
            DateTime now = DateTime.Now;
            WriteToStreamWriters(logSource,logTarget,$"===== Infos for Sync job started {ctx.StartTime}.{ctx.StartTime.Millisecond:000}, ended {now}.{now.Millisecond:000} =======");
            WriteToStreamWriters(logSource,logTarget, sourceCheck);
            WriteToStreamWriters(logSource, logTarget, targetCheck);
            WriteToStreamWriters(logSource, logTarget, difference);
            ctx.Different.ToList().OrderBy(d => d.Value).ToList().ForEach(p => WriteToStreamWriters(logSource,logTarget, $"Different found with difference Type {p.Value.ToString()} in {p.Key}"));
            WriteToStreamWriters(logSource,logTarget,"======================================================================");

            logSource.Flush();
            logTarget.Flush();
            

        }

        private static void WriteToStreamWriters(StreamWriter logSource, StreamWriter logTarget, String logText)
        {
            logSource.WriteLine(logText);
            logTarget.WriteLine(logText);
        }

        private static void CopyOrDeleteFiles(AppContext ctx)
        {
            new CopyProcessor()
            {
                Ctx = ctx
            }.CopyOrDeleteFiles();
        }

        private static void CompareMaps(AppContext opt)
        {
            Logger.Logger.Log("Start comparing scanning results", LogLevel.Info);
            new CompareDirectories(opt).CompareSourceAndTarget();
            Logger.Logger.Log("Comparing scanning results Completed", LogLevel.Info);
        }

        private static void ScanDirectories(AppContext opt)
        {
            Logger.Logger.Log($"Scanning \"{opt.Opt.Source.FullName}\" and \"{opt.Opt.Target.FullName}\"", LogLevel.Info);

            var tasks = new List<Task>();
           opt.SourceDirectoryScanner = new ScannedDirectoryInfo(){
                CurrentDirectory = opt.Opt.Source,
                RootDirectory = opt.Opt.Source
              
            };

            opt.TargetDirectoryScanner = new ScannedDirectoryInfo()
            {
                CurrentDirectory = opt.Opt.Target,
                RootDirectory = opt.Opt.Target
            };


            var sourceScanner = new DirectoryScanner(opt)
            {
                Dir = opt.SourceDirectoryScanner
            };

              var targetScanner = new DirectoryScanner(opt)
            {
                Dir = opt.TargetDirectoryScanner
            };

            var threading = new Threading();
            List<Thread> t1List = null;
            var t1 = new Thread(new ThreadStart(() => t1List = sourceScanner.CheckDirectory()))
            {
                Name = "Check Source Dir",
                Priority = ThreadPriority.Highest
            };
            List<Thread> t2List = null;
            var t2 = new Thread(new ThreadStart(() => t2List = targetScanner.CheckDirectory()))
            {
                Name = "Check Target Dir",
                Priority = ThreadPriority.Highest
            };
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            t1List?.ForEach(th => th.Join());
            t2List?.ForEach(th => th.Join());

            Logger.Logger.Log($"Scanning Completed", LogLevel.Info);
        }
    }
}
