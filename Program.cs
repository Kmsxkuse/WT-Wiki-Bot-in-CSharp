using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WT_Wiki_Bot_in_CSharp
{
    class Initialization
    {
        private static void Main(string[] args)
        {
            // Checking for Pre-initialized Weapons Folder.
            if (!Directory.Exists(@".\War-Thunder-Files\weapons"))
            {
                Console.WriteLine("Weapons folder not found.");
                if (Directory.Exists(@".\War-Thunder-Files\aces.vromfs.bin_u"))
                {
                    Console.WriteLine("Removing previous Aces.vromfs.bin_u.");
                    Directory.Delete(@".\War-Thunder-Files\aces.vromfs.bin_u", true);
                }
                if (!(File.Exists(@".\wt-tools\blk_unpack.exe") && File.Exists(@".\wt-tools\vromfs_unpacker.exe")))
                {
                    Console.WriteLine("WT-Tools not found in main directory.");
                    Console.ReadKey();
                    System.Environment.Exit(1);
                }
                if (!File.Exists(@".\War-Thunder-Files\aces.vromfs.bin"))
                {
                    Console.WriteLine("Aces.vromfs.bin not found in War-Thunder-Files.");
                    Console.ReadKey();
                    System.Environment.Exit(2);
                }
                Console.WriteLine("Running Vromfs Depacker on Aces.");
                RunFile(@".\wt-tools\vromfs_unpacker.exe", @".\War-Thunder-Files\aces.vromfs.bin");
                Directory.Move(@".\War-Thunder-Files\aces.vromfs.bin_u\gamedata\weapons", @".\War-Thunder-Files\weapons");
                Directory.Delete(@".\War-Thunder-Files\aces.vromfs.bin_u", true);
                Parallel.ForEach(new DirectoryInfo(@".\War-Thunder-Files\weapons").GetDirectories(), (subDir) =>
                {
                    subDir.Delete(true);
                });
                Console.WriteLine("Unpacking BLK files.");
                Parallel.ForEach(new DirectoryInfo(@".\War-Thunder-Files\weapons").GetFiles(), (currentFile) =>
                {
                    RunFile(@".\wt-tools\blk_unpack.exe", Path.Combine(@".\War-Thunder-Files\weapons\", currentFile.ToString()));
                });
                Console.WriteLine("Deleting BLK files.");
                Parallel.ForEach(new DirectoryInfo(@".\War-Thunder-Files\weapons").GetFiles("*.blk").Where(f => f.Extension == ".blk"),
                    (currentFile) =>
                    {
                        currentFile.Delete();
                    });
                Console.WriteLine("Finished.");
                Console.ReadKey();
            }
        }

        private static void RunFile(string fileName, string target)
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = target
            };
            try
            {
                using (var exeProcess = Process.Start(startInfo))
                {
                    // The ?. is a null checker.
                    exeProcess?.WaitForExit();
                }
            }
            catch (Exception exeException)
            {
                Console.WriteLine(exeException);
                Console.ReadKey();
                System.Environment.Exit(3);
            }
        }
    }
}
