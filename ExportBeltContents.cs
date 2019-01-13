using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportBeltContents {
        public static string Main(InfoArray infoList) {
            var internalFile = new StringBuilder();
            
            // Default
            internalFile.Append("<b>Default</b>\n\n ");
            infoList.StockIDs.ForEach(bullet => {
                if (!((Dictionary<string, object>) infoList.UniqueBullets[infoList.UniqueIDs.IndexOf(bullet)])
                    .ContainsKey("bulletType")) return;
                internalFile.Append(
                    NameCleaning((string) ((Dictionary<string, object>) infoList.UniqueBullets[infoList.UniqueIDs.IndexOf(bullet)])[
                        "bulletType"]));
                internalFile.Append(", ");
            });
            internalFile.Remove(internalFile.Length - 2, 2);
            
            // Spaded
            for (var belt = 0; belt < infoList.SpadedNames.Count; belt++) {
                internalFile.Append($"\n\n<b>{NameCleaning(infoList.SpadedNames[belt])}</b>\n\n ");
                for (var bullet = 0; bullet < infoList.SpadedIDs[belt].Count; bullet++) {
                    if (!((Dictionary<string, object>) infoList.UniqueBullets[infoList.UniqueIDs.IndexOf(infoList.SpadedIDs[belt][bullet])])
                        .ContainsKey("bulletType")) continue;
                    internalFile.Append(
                        NameCleaning((string) ((Dictionary<string, object>) infoList.UniqueBullets[
                            infoList.UniqueIDs.IndexOf(infoList.SpadedIDs[belt][bullet])])["bulletType"]));
                    internalFile.Append(", ");
                }
                internalFile.Remove(internalFile.Length - 2, 2);
            }
            
            var exportFile = $@"<div class = ""mw-customtoggle-belts_{infoList.FileName}"" style=""text-align:center;width:auto;overflow:auto;border:solid purple;border-radius: 0.625rem;background:lavender"">
<span style=""font-size:1.2rem;font-style:italic;font-weight:bold"">Belt Contents</span>
</div>
<div class=""mw-collapsible mw-collapsed"" id=""mw-customcollapsible-belts_{infoList.FileName}"" style=""width:99%;overflow:auto;"">
<div class=""mw-collapsible-content"" style=""border:solid lightgray;background:white;margin-left:1%;padding:0 1%;overflow:auto"">
{internalFile}
</div>
</div>";
            return exportFile;
        }
        
        private static string NameCleaning(string rawName) {
            string Capitalizing(Match m) {
                return m.Groups[1].Value.ToUpper();
            }

            var cleaning = rawName.Replace('_', ' ');
            cleaning = Regex.Replace(cleaning, @"(\b[a-z])", Capitalizing);
            
            return cleaning;
        }
    }
}