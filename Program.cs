using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WT_Wiki_Bot_in_CSharp
{
    internal static class Initialization
    {
        private static readonly string[] RemovedGunFiles = 
        {
            "dummy_weapon.blk", "arcadedamagefunction.blk", "lt_proj.blk", "no_gun.blk", "fortest_mgun12mm.blk",
            "railgun_for_pony.blk", "cannon_for_pony.blk", "shotgun_for_pony.blk"
            
        };
        private static readonly HashSet<string> RemovedPlaneFiles = new HashSet<string>
        {
            "ah_1f.blk", "ah_1g.blk", "ah_1z.blk", "ah-64d.blk", "bo_105cb2.blk", "bo_105pah1.blk", "bo_105pah1_a1.blk",
            "ba_65_k14_l.blk", "b_24d_luftwaffe.blk", "d4y1.blk", "d4y2.blk", "d4y3.blk", "fau-1.blk", "g8n2.blk",
            "hp52_hampden_mk1_late.blk", "hp52_hampden_tbmk1.blk", "hp52_hampden_tbmk1_ussr_utk1.blk", "hudson_mk_v_gear_test.blk",
            "ju-52.blk", "ki_32.blk", "maryland_mk1.blk", "mi-35.blk", "mi_24a.blk", "mi_24d.blk", "mi_24p.blk", "mi_24p_german.blk",
            "mi_24v.blk", "mi_35m.blk", "mi_4av.blk", "n1k1_kyuofu.blk", "sa_313b.blk", "sb2c_5.blk", "sb2c_5_france.blk",
            "su-6_am42_23.blk", "uh_1b.blk", "uh_1c.blk", "uh_1c_xm_30.blk", "uh_1d.blk", "dummy_plane.blk", "c-47.blk", "lagg-3-1.blk",
            "lagg-3-29.blk", "lagg_gu-37.blk", "li-2.blk", "vb_10_02.blk", "vb_10c1.blk", "h_34_france.blk", "iar_316b.blk", "sa_313b_france.blk", 
            "sa_316b.blk", "sa_341f.blk", "bf-109z.blk", "sa_342m.blk", "ufo.blk", "ucu_blue_squadron"
        };
        
        private static void Main(string[] args)
        {
            // Terribly optimized piece of trash. Will rewrite... someday.
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
                // Moving weapons folder out.
                Directory.Move(@"..\..\War-Thunder-Files\aces.vromfs.bin_u\gamedata\weapons", @"..\..\War-Thunder-Files\weapons");
                // Moving Flight Model folder out.
                Directory.Move(@"..\..\War-Thunder-Files\aces.vromfs.bin_u\gamedata\flightmodels", @"..\..\War-Thunder-Files\flightmodels");
                // Moving config folder out.
                Directory.Move(@"..\..\War-Thunder-Files\aces.vromfs.bin_u\config", @"..\..\War-Thunder-Files\config");
                // Deleting unused files
                Directory.Delete(@"..\..\War-Thunder-Files\aces.vromfs.bin_u", true);
                // Deleting bombs and tank guns. Those I will do some other day.
                foreach (var subDir in new DirectoryInfo(@"..\..\War-Thunder-Files\weapons").GetDirectories())
                {
                    subDir.Delete(true);
                }
                Console.WriteLine("Finished isolating Weapons folder.");
            }
            // Creating Export Folder
            if (!Directory.Exists(@"..\..\War-Thunder-Files\export"))
            {
                Console.WriteLine("Export folder not found.");
                Directory.CreateDirectory(@"..\..\War-Thunder-Files\export");
                Directory.CreateDirectory(@"..\..\War-Thunder-Files\export\weapons");
                Directory.CreateDirectory(@"..\..\War-Thunder-Files\export\flightmodels");
                Directory.CreateDirectory(@"..\..\War-Thunder-Files\export\flightmodels\fm");
            }
            // Starting stopwatch
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // Reading Weapons Folder
            Parallel.ForEach(new DirectoryInfo(@"..\..\War-Thunder-Files\weapons").GetFiles(), fileInfo => 
            {
                // Skipping trash files
                if (RemovedGunFiles.Contains(fileInfo.Name)) return;
                var parsedFile = Blk.BlkUnpack(fileInfo);
                var completedExport = ExportMain.Main(RawParser.CompletedArr(parsedFile, fileInfo));
                File.WriteAllText($@"..\..\War-Thunder-Files\export\weapons\{Path.GetFileNameWithoutExtension(fileInfo.Name)}.wiki", completedExport);
            });
            
            // Flight Models time. Creating Lookup Dictionary. Holy LINQ.
            var fmLookup = new DirectoryInfo(@"..\..\War-Thunder-Files\flightmodels").GetFiles()
                .Where(fileInfo => !RemovedPlaneFiles.Contains(fileInfo.Name))
                .ToDictionary<FileInfo, string, RawFmParser>(fileInfo => fileInfo.Name, fileInfo => null);
            
            // Reading Arcade modifiers for FM Charts
            // gameparams.blk -> difficulty_settings -> noArcadeBoost -> off
            var arcadeModifiers = (Dictionary<string, object>) ((Dictionary<string, object>) 
                ((Dictionary<string, object>) Blk.BlkUnpack(new FileInfo(@"..\..\War-Thunder-Files\config\gameparams.blk"))
                    ["difficulty_settings"]) ["noArcadeBoost"])["off"];
            
            //Parallel.ForEach(new DirectoryInfo(@"..\..\War-Thunder-Files\flightmodels").GetFiles(), fileInfo =>
            foreach (var fileInfo in new DirectoryInfo(@"..\..\War-Thunder-Files\flightmodels").GetFiles())
            {
                // Skipping Errored files
                if (RemovedPlaneFiles.Contains(fileInfo.Name)) continue;
                var parsedFile = Blk.BlkUnpack(fileInfo);

                // Fleshing dictionary
                fmLookup[fileInfo.Name] = new RawFmParser(parsedFile, fileInfo);
            }//);
            //Parallel.ForEach(fmLookup.Values, plane => 
            foreach (var plane in fmLookup.Values)
            {
                // Skipping Errored files
                if (RemovedPlaneFiles.Contains(plane.FmFileName)) continue;
                // Horsepower graphs
                var fileInfo = new FileInfo(@"..\..\War-Thunder-Files\flightmodels\fm\" + plane.FmFileName);
                // Generating Temporary Copy
                var tempFileName = Path.GetTempPath() + plane.FileName + ".temp";
                fileInfo.CopyTo(tempFileName, true);
                var tempFileInfo = new FileInfo(tempFileName);
                var accessedPlane = fmLookup[plane.FileName + ".blk"];
                // Reading Copy
                accessedPlane.ReadFmDataFile(Blk.BlkUnpack(tempFileInfo));
                // Deleting Copy
                tempFileInfo.Delete();
                var tables = new StringBuilder();
                tables.AppendLine(ExportFmGraphs.SpeedChart(accessedPlane));
                tables.AppendLine(ExportFmGraphs.ClimbRateChart(accessedPlane));
                tables.AppendLine(accessedPlane.ClimbTimeWikiMil.Count != 0
                    ? ExportFmGraphs.ClimbTimeChart(accessedPlane)
                    : "");
                tables.AppendLine(ExportFmGraphs.HorsePowerChart(accessedPlane));
                File.WriteAllText($@"..\..\War-Thunder-Files\export\flightmodels\{plane.FileName}.wiki",
                    tables.ToString());

                // FM charts
                File.WriteAllText($@"..\..\War-Thunder-Files\export\flightmodels\fm\{plane.FileName}.wiki",
                    ExportFmCharts.Main(accessedPlane, arcadeModifiers));
            }//);
            stopWatch.Stop();
            Console.WriteLine("Time Spent: " + stopWatch.ElapsedMilliseconds + "ms");
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