using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WT_Wiki_Bot_in_CSharp
{
    internal class RawFmParser
    {
        //public List<decimal> MaxSpeedAltSpec { get; }
        public List<float[]> MaxSpeedWikiWep { get; }
        public List<float[]> MaxSpeedWikiMil { get; }
        public List<float[]> ClimbRateWikiWep { get; }
        public List<float[]> ClimbRateWikiMil { get; }
        public List<float[]> ClimbTimeWikiWep { get; }
        public List<float[]> ClimbTimeWikiMil { get; }
        public List<List<decimal[]>> HorsePower { get; set; }
        public int NumEngines { get; set; }
        public decimal afterBoost { get; set; }
        public string FileName { get; }
        public string FmFileName { get; }

        public RawFmParser(IReadOnlyDictionary<string, object> parsedFile, FileInfo fileInfo)
        {
            FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (!parsedFile.ContainsKey("wiki")) throw new Exception("Wiki section not found!");
            var wikiInfo = (Dictionary<string, object>) parsedFile["wiki"];
            if (!wikiInfo.ContainsKey("performance")) throw new Exception("Performance subsection of wiki not found!");
            var performanceInfo = (Dictionary<string, object>) wikiInfo["performance"];
            var table = (Dictionary<string, object>) performanceInfo["table"];
            var plot = (Dictionary<string, object>) performanceInfo["plot"];
            // FM file name
            FmFileName = parsedFile.ContainsKey("fmFile") ? ((string) parsedFile["fmFile"]).Substring(3) : fileInfo.Name;
            // Grabbing airSpeedWep# from plot
            MaxSpeedWikiWep = (from airSpeedWikiWep in plot
                where airSpeedWikiWep.Key.Contains("airSpeedWep")
                select (float[]) airSpeedWikiWep.Value).ToList();
            // Grabbing airSpeedMil# from plot
            MaxSpeedWikiMil = (from airSpeedWikiMil in plot
                where airSpeedWikiMil.Key.Contains("airSpeedMil")
                select (float[]) airSpeedWikiMil.Value).ToList();
            // Grabbing climbRateWep# from plot
            ClimbRateWikiWep = (from climbRateWikiWep in plot
                where climbRateWikiWep.Key.Contains("climbRateWep")
                select (float[]) climbRateWikiWep.Value).ToList();
            // Grabbing climbRateMil# from plot
            ClimbRateWikiMil = (from climbRateWikiMil in plot
                where climbRateWikiMil.Key.Contains("climbRateMil")
                select (float[]) climbRateWikiMil.Value).ToList();
            // Grabbing climbTimeWep# from table
            ClimbTimeWikiWep = (from climbTimeWikiWep in table
                where climbTimeWikiWep.Key.Contains("climbTimeWep")
                select (float[]) climbTimeWikiWep.Value).ToList();
            // Grabbing climbTimeMil# from table
            ClimbTimeWikiMil = (from climbTimeWikiMil in table
                where climbTimeWikiMil.Key.Contains("climbTimeMil")
                select (float[]) climbTimeWikiMil.Value).ToList();
        }

        // From /fm directory
        public void AddHorsePower(IReadOnlyDictionary<string, object> parsedFmFile)
        {
            NumEngines = (from engCounter in parsedFmFile
                where engCounter.Key.Contains("Engine")
                select engCounter.Value).Count();
            HorsePower = new List<List<decimal[]>>();
            var engine0 = (Dictionary<string, object>) parsedFmFile["Engine0"];
            var compressor = (Dictionary<string, object>) engine0["Compressor"];
            var main = ((Dictionary<string, object>) engine0["Main"]);
            var numSteps = (int) compressor["NumSteps"];
            // Alt 0 Horsepower
            var initialHp = main.ContainsKey("Power") ? (decimal) main["Power"] : (decimal) main["HorsePowers"];
            afterBoost = (decimal) main["AfterburnerBoost"];
            for (var stage = 0; stage < numSteps; stage++)
            {
                var ihpMulti = stage == 0 ? 1 / 0.8m : stage;
                var altStage = compressor.ContainsKey($"Altitude{stage}") 
                    ? new[] {(decimal) compressor[$"Altitude{stage}"], (decimal) compressor[$"Power{stage}"]} 
                    : new []{-1m, -1m};
                var rpmStage = compressor.ContainsKey($"AltitudeConstRPM{stage}") 
                    ? new[] {(decimal) compressor[$"AltitudeConstRPM{stage}"], (decimal) compressor[$"PowerConstRPM{stage}"]} 
                    : new []{-1m, -1m};
                var ceilingStage = compressor.ContainsKey($"Ceiling{stage}") 
                    ? new[] {(decimal) compressor[$"Ceiling{stage}"], (decimal) compressor[$"PowerAtCeiling{stage}"]} 
                    : new []{-1m, -1m};
                HorsePower.Add(new List<decimal[]>
                {
                    // { Alt, HP }
                    new[] {0m, initialHp * 0.8m * ihpMulti}, // 0.8 because who knows.
                    altStage,
                    rpmStage,
                    ceilingStage
                });
                // Sorting points ascending
                HorsePower[stage] = HorsePower[stage].OrderBy(point => point[0]).ToList();
                // Removing negative altitudes
                HorsePower[stage].RemoveAll(point => point[0] < 0);
            }
        }
    }
}