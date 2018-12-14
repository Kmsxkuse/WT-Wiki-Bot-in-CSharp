using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WT_Wiki_Bot_in_CSharp
{
    internal static class Initialization
    {
        private static void Main(string[] args)
        {
            // Checking for Pre-initialized Weapons Folder.
            if (!Directory.Exists(@"..\..\War-Thunder-Files\weapons"))
            {
                Console.WriteLine("Weapons folder not found.");
                if (Directory.Exists(@"..\..\War-Thunder-Files\aces.vromfs.bin_u"))
                {
                    Console.WriteLine("Removing previous Aces.vromfs.bin_u.");
                    Directory.Delete(@"..\..\War-Thunder-Files\aces.vromfs.bin_u", true);
                }
                if (!(File.Exists(@"..\..\wt-tools\blk_unpack.exe") && File.Exists(@"..\..\wt-tools\vromfs_unpacker.exe")))
                {
                    Console.WriteLine("WT-Tools not found in main directory.");
                    Console.ReadKey();
                    System.Environment.Exit(1);
                }
                if (!File.Exists(@"..\..\War-Thunder-Files\aces.vromfs.bin"))
                {
                    Console.WriteLine("Aces.vromfs.bin not found in War-Thunder-Files.");
                    Console.ReadKey();
                    System.Environment.Exit(2);
                }
                Console.WriteLine("Running Vromfs Depacker on Aces.");
                RunFile(@"..\..\wt-tools\vromfs_unpacker.exe", @"..\..\War-Thunder-Files\aces.vromfs.bin");
                Directory.Move(@"..\..\War-Thunder-Files\aces.vromfs.bin_u\gamedata\weapons", @"..\..\War-Thunder-Files\weapons");
                Directory.Delete(@"..\..\War-Thunder-Files\aces.vromfs.bin_u", true);
                foreach (var subDir in new DirectoryInfo(@"..\..\War-Thunder-Files\weapons").GetDirectories())
                {
                    subDir.Delete(true);
                }
                /*
                Console.WriteLine("Unpacking BLK files and deleting them.");
                Parallel.ForEach(new DirectoryInfo(@".\War-Thunder-Files\weapons").GetFiles(), (currentFile) =>
                {
                    RunFile(@".\wt-tools\blk_unpack.exe", Path.Combine(@".\War-Thunder-Files\weapons\", currentFile.ToString()));
                    currentFile.Delete();
                });
                */
                Console.WriteLine("Finished isolating Weapons folder.");
            }
            // Reading Weapons Folder
            //Parallel.ForEach(new DirectoryInfo(@".\War-Thunder-Files\weapons").GetFiles(), Blk.BlkUnpack);
            foreach (var fileInfo in new DirectoryInfo(@"..\..\War-Thunder-Files\weapons").GetFiles())
            {
                var parsedFile = Blk.BlkUnpack(fileInfo);
                RawParser.CompletedArr(parsedFile, fileInfo.Name);
            }
            Console.ReadKey();
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