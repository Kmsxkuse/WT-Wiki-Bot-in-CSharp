using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportArmorPen {
        private static readonly string[] ColourValues = new [] { 
            "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000", 
            "800000", "008000", "000080", "808000", "800080", "008080", "808080", 
            "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0", 
            "400000", "004000", "000040", "404000", "400040", "004040", "404040", 
            "200000", "002000", "000020", "202000", "200020", "002020", "202020", 
            "600000", "006000", "000060", "606000", "600060", "006060", "606060", 
            "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0", 
            "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0", 
        };
        public static string Main(InfoArray infoList) {
            const decimal tableSize = 40;
            var intSize = Math.Round(tableSize * 17 / 20, 2);
            const int maxLines = 11; // - 1 for actual number of lines
            const string chartEnd = "</div>";
            
            var chartStart = $@"<div style=""position:relative;width:{tableSize}rem;height:{tableSize}rem;background:#f0f0f0;border:solid;margin:1rem auto"">
<div style=""position:absolute;left:{Math.Round(tableSize / 10, 2)}rem;top:{Math.Round(tableSize / 20, 2)}rem;width:{intSize}rem;height:{intSize}rem;background:white;border:solid;"">";
            
            var chartLines = new StringBuilder();
            for (var lineNum = 1; lineNum < maxLines; lineNum++) {
                // Originating from Y Lines
                chartLines.Append($@"<div style=""position:absolute;top:{Math.Round(intSize / maxLines * lineNum, 2)}rem; width:{intSize}rem;border-bottom:dashed 2px lightgray;""></div>");
                // X Lines
                chartLines.Append($@"<div style=""position:absolute;height:{intSize}rem;left:{Math.Round(intSize / maxLines * lineNum, 2)}rem;border-right:dashed 2px lightgray;""></div>");
            }

            var armorPowerArr = new List<List<object>>();
            infoList.UniqueBullets.ForEach(bullet => {
                if (!((Dictionary<string, object>) bullet).ContainsKey("armorpower")) return;
                armorPowerArr.Add(((Dictionary<string, object>) ((Dictionary<string, object>) bullet)["armorpower"]).Values.ToList());
            });

            var maxPen = new float();
            armorPowerArr.ForEach(bullet => {
                bullet.ForEach(coordinate => {
                    if (((float[]) coordinate)[0] < maxPen) return;
                    maxPen = ((float[]) coordinate)[0];
                });
            });
            maxPen = (float)(Math.Ceiling(maxPen / ((float) maxLines / 2)) * ((float) maxLines / 2));
            
            var chartUnits = new StringBuilder();
            for (var lineNum = maxLines; lineNum > 0; lineNum--) {
                // Y Values
                chartUnits.Append($@"<div style=""position:absolute;top:{Math.Round((double) intSize / maxLines * (lineNum + 0.5), 2)}rem;width:{Math.Round(tableSize / 12, 2)}rem"" align=""right"">{Math.Round(maxPen / (maxLines - 1) * (maxLines - lineNum))}</div>");
                
                // X Values
                chartUnits.Append($@"<div style=""position:absolute;top:{Math.Round(intSize + tableSize * 3 / 40 - 0.3M, 2)}rem;left:{Math.Round((double) intSize / maxLines * lineNum + 0.6, 2)}rem;width:{Math.Round(tableSize / lineNum, 2)}"" align=""center"">{(lineNum - 1) * 100}</div>");
            }

            var penBox = $@"<div style=""position:absolute;width:{Math.Round((1 - 1 / (double) (maxLines - 1)) * 100, 2)}%;height:{Math.Round((1 - 1 / (decimal) (maxLines - 1)) * 100, 2)}%;bottom:0;left:0"">";
            
            var penCoords = new StringBuilder();
            var penLines = new StringBuilder();
            var penLegend = new StringBuilder();
            var prevPoint = new[] {0d, 0d};
            for (var bullet = 0; bullet < armorPowerArr.Count; bullet++) {
                for (var coordinate = 0; coordinate < armorPowerArr[bullet].Count; coordinate++) {
                    if (((float[]) armorPowerArr[bullet][coordinate])[1] > (maxLines - 1) * 100) break;
                    const decimal boxSize = 0.25M;
                    /* Vertical (bottom) Offset: (1 - boxSize / 2 / intSize )
                     * Experimentally Determined.
                     * Reduced by half the height (aka length) of the box.
                     * Then converted to % of box.
                     */
                    var boxPointX = Math.Round(((float[]) armorPowerArr[bullet][coordinate])[1] / (maxLines - 1), 2);
                    var boxPointY = Math.Round(((float[]) armorPowerArr[bullet][coordinate])[0] / maxPen * (double)(1 - boxSize / 2 / intSize) * 100, 2);
                    if (coordinate != 0) {
                        var lineLength = Math.Round(Math.Sqrt(Math.Pow(prevPoint[0] - boxPointX, 2) + Math.Pow(prevPoint[1] - boxPointY, 2)));
                        var rotate = Math.Round(Math.Atan((prevPoint[0] - boxPointX) / (prevPoint[1] - boxPointY)), 2);
                        // Lines
                        penLines.Append($@"<div style=""position:absolute;transform:rotate({rotate}rad);left:{(boxPointX + prevPoint[0]) / 2}%;bottom:{(boxPointY + prevPoint[1]) / 2 - lineLength / 2 + 1}%;height:{lineLength}%;border-right:solid #{ColourValues[bullet]}""></div>");
                    }
                    prevPoint[0] = boxPointX;
                    prevPoint[1] = boxPointY;
                    
                    // Points
                    penCoords.Append($@"<div style=""position:absolute;left:{boxPointX}%;bottom:{boxPointY}%;width:{boxSize}rem;height:{boxSize}rem;border:solid #{ColourValues[bullet]};border-radius:50%;background:white;""></div>");
                }
                if (!((Dictionary<string, object>) infoList.UniqueBullets[bullet]).ContainsKey("bulletType")) break;
                
                string Capitalizing(Match m) {
                    return m.Groups[1].Value.ToUpper();
                }
                var cleanedName = ((string)((Dictionary<string, object>) infoList.UniqueBullets[bullet])["bulletType"]).Replace('_', ' ');
                cleanedName = Regex.Replace(cleanedName, @"(\b[a-z])", Capitalizing);
                // Legend
                penLegend.Append($@"<div style=""position:relative;padding:0.2rem;border:solid #{ColourValues[bullet]};width:20%;top:1%;margin:0 0 0 75%;background:white;text-align:center"">{cleanedName}</div>");
            }
            
            // First is X Axis, second is Y Axis.
            var chartWords = $@"<b><div style=""position:absolute;width:100%"" align=""center"">{infoList.GunName} ArmorPower</div>
<div style=""position:absolute;width:100%;top:{tableSize - 1.5M}rem"" align=""center"">Distance in meters</div>
<div style=""transform:rotate(90deg);position:relative;left:-{tableSize - 1.5M}rem;height:100%;"" align=""center"">Penetration in millimeters</div></b>";
            
            // To prevent people from saying my poor chart is bugged.
            const string chartWarning = @"<div style=""position:relative;text-align:center;font-weight:bold;top:0.5rem"">Colors indicated in the legend that do not appear in the graph may be hidden behind another line.<br />Numbers along Y Axis are not precise. They have been rounded to the nearest integer.</div>";

            var exportFile =
                $@"<div class=""mw-customtoggle-armor_{infoList.FileName}"" style=""text-align:center;width:auto;overflow:auto;border:solid orange;border-radius:0.625rem;background:mistyrose"">
<strong style=""font-size:1.2rem;""><i>Armor Penetration Chart</i></strong>
</div>
<div class=""mw-collapsible mw-collapsed"" id=""mw-customcollapsible-armor_{infoList.FileName}"" style=""width:99%;"">
<div class=""mw-collapsible-content"" style=""border:solid lightgray;background:white;margin-left:1%;padding:0 1%;overflow:auto"">
{chartWarning}
{chartStart}
{chartLines}
{penBox}
{penLines}
{penCoords}
{chartEnd}
{penLegend}
{chartEnd}
{chartWords}
{chartUnits}
{chartEnd}
</div>
</div>
";
             return exportFile;
        }
    }
}