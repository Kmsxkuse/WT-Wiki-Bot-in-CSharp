using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WT_Wiki_Bot_in_CSharp
{
    internal class RawFmParser
    {
        // FM Chart info
        public decimal MaxAltitude { get; }
        public decimal TakeoffDistance { get; }
        public decimal AileronEffectiveSpeed { get; private set; }
        public decimal RudderEffectiveSpeed { get; private set; }
        public decimal ElevatorsEffectiveSpeed { get; private set; }
        public decimal Vne { get; private set; }
        public float[] TurnTimeMil { get; }
        public float[] TurnTimeWep { get; }
        public string NitroTime { get; private set; }
        
        // FM Graph info
        public List<float[]> MaxSpeedWikiWep { get; }
        public List<float[]> MaxSpeedWikiMil { get; }
        public List<float[]> ClimbRateWikiWep { get; }
        public List<float[]> ClimbRateWikiMil { get; }
        public List<float[]> ClimbTimeWikiWep { get; }
        public List<float[]> ClimbTimeWikiMil { get; }
        public List<List<decimal[]>> HorsePower { get; private set; }
        public int NumEngines { get; private set; }
        public decimal afterBoost { get; private set; }
        public string FileName { get; }
        public string FmFileName { get; }

        public RawFmParser(IReadOnlyDictionary<string, object> parsedFile, FileSystemInfo fileInfo)
        {
            FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (!parsedFile.ContainsKey("wiki")) throw new Exception("Wiki section not found! " + fileInfo.Name);
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

            MaxAltitude = (decimal) table["ceiling"];
            TakeoffDistance = Math.Round((decimal) table["takeoffDistance"]);
            TurnTimeMil = (float[]) table["turnTimeMil"];

            if (table.TryGetValue("turnTimeWep", out var tempWepTurnTime))
                TurnTimeWep = (float[]) tempWepTurnTime;
        }

        // From /fm directory
        public void ReadFmDataFile(IReadOnlyDictionary<string, object> parsedFmFile)
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
                    new[] {0m, initialHp * (decimal) Math.Pow(4 / 5d, stage)}, // 0.8 because who knows.
                    altStage,
                    rpmStage,
                    ceilingStage
                });
                // Sorting points ascending
                HorsePower[stage] = HorsePower[stage].OrderBy(point => point[0]).ToList();
                // Removing negative altitudes
                HorsePower[stage].RemoveAll(point => point[0] < 0);
            }

            AileronEffectiveSpeed = (decimal) parsedFmFile["AileronEffectiveSpeed"];
            RudderEffectiveSpeed = (decimal) parsedFmFile["RudderEffectiveSpeed"];
            ElevatorsEffectiveSpeed = (decimal) parsedFmFile["ElevatorsEffectiveSpeed"];
            Vne = (decimal) parsedFmFile["Vne"];
            
            // WEP calculation
            var afterBurner = (Dictionary<string, object>) engine0["Afterburner"];
            if (!(bool) afterBurner["IsControllable"])
            {
                NitroTime = "N/A";
                return;
            }
            var nitroConsumption = (decimal) afterBurner["NitroConsumption"];
            if (nitroConsumption < 0.01m)
            {
                NitroTime = "Infinite";
                return;
            }
            var mass = (Dictionary<string, object>) parsedFmFile["Mass"];
            NitroTime = Math.Round((decimal) mass["MaxNitro"] / nitroConsumption / 60m, 2).ToString(CultureInfo.InvariantCulture);
        }
    }
}