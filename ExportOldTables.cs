using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportOldTables {
        public static string Main(InfoArray infoList) {
            var multiBelts = new StringBuilder();
            // Stock Belt, default length of only 1, thus hardcoded to access first.
            multiBelts.Append(
                $@"<div class = ""mw-customtoggle-belt_{infoList.FileName}"" style=""text-align:center; width:auto; margin:1% 0 0 0; overflow:auto; border:solid purple; border-radius: 0.625em; background:lavender"">
<strong><i>{infoList.StockNames.First()}</i></strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-belt_{infoList.FileName}"" style = ""width:99%; overflow:auto;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin:0 0 0 1%; padding:0 1%; overflow:auto"">
TEST TEST TEST STOCK
</div>
</div>");
            // Spaded Belts
            for (var belts = 0; belts < (infoList.SpadedNames.Count); belts++) {
                multiBelts.Append($@"<div class = ""mw-customtoggle-belt_{infoList.FileName + belts}"" style=""text-align:center; width:auto; margin:1% 0 0 0; overflow:auto; border:solid purple; border-radius: 0.625em; background:lavender"">
<strong><i>{CleanName(infoList.SpadedNames[belts])}</i></strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-belt_{infoList.FileName + belts}"" style = ""width:99%; overflow:auto;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin:0 0 0 1%; padding:0 1%; overflow:auto"">
TEST TEST TEST SPADED
</div>
</div>");
            }
            
            var exportFile = $@"<div class = ""mw-customtoggle-belts_{infoList.FileName}"" style=""font-size:1.2rem; text-align:center; width:auto; overflow:auto; border:solid purple; border-radius: 0.625rem; background:lavender"">
<strong><i>Old Belt Tables</i></strong>
</div>
<div class = ""mw-collapsible"" id = ""mw-customcollapsible-belts_{infoList.FileName}"" style = ""width:99%; overflow:auto;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin-left:1%; padding:0 1%; overflow:auto"">
{multiBelts}
</div>
</div>";
            return exportFile;
        }

        private static string CleanName(string rawName) {
            string Capitalizing(Match m) {
                return m.Groups[1].Value.ToUpper();
            }
            
            var cleaned = rawName.Substring(rawName.IndexOf('_') + 1)
                .Replace('_', ' ');
            
            cleaned = Regex.Replace(cleaned, @"(\b[a-z](?!\b))", Capitalizing);
            return cleaned;
        }
    }
}