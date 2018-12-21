using System;
using System.Text;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportArmorPen {
        public static string Main(InfoArray infoList) {
            const decimal tableSize = 40;
            var intSize = Math.Round(tableSize * 17 / 20, 2);
            const int maxLines = 11; // - 1 for actual number of lines
            const string chartEnd = "</div>";
            
            var chartStart = $@"<div style=""position:relative; width:{tableSize}rem; height:{tableSize}rem; background:#f0f0f0; border:solid; margin:1rem auto"">
<div style=""position:absolute; left:{Math.Round(tableSize / 10, 2)}rem; top:{Math.Round(tableSize / 20, 2)}rem; width:{intSize}rem; height:{intSize}rem; background:white; border:solid;"">";
            
            var chartLines = new StringBuilder();
            for (var lineNum = 1; lineNum < maxLines; lineNum++) {
                chartLines.Append($@"<div style=""position:absolute; top:{Math.Round(intSize / maxLines * lineNum, 2)}rem; width:{intSize}rem; border-bottom:dashed lightgray;""></div>");
                chartLines.Append($@"<div style=""position:absolute; height:{intSize}rem; left:{Math.Round(intSize / maxLines * lineNum, 2)}rem; border-right:dashed lightgray;""></div>");
            }
            
            var chartUnits = new StringBuilder();
            for (var lineNum = maxLines; lineNum > 0; lineNum--) {
                chartUnits.Append($@"<div style=""position:absolute; top:{Math.Round((double) intSize / maxLines * (lineNum + 0.5), 2)}rem; width:{Math.Round(tableSize / 12, 2)}rem"" align=""right"">{maxLines - lineNum}</div>");
                chartUnits.Append($@"<div style=""position:absolute; top:{Math.Round(intSize + tableSize * 3 / 40 , 2)}rem; left:{Math.Round((double) intSize / maxLines * (lineNum + 0.25), 2)}rem; width:{Math.Round(tableSize / 10, 2)}"" align=""left"">{lineNum - 1}</div>");
            }

            var chartWords = $@"<div style=""position: absolute; left: 220px; top:540px; width: 150px;"" align=""right"">Horsepower in 1000hp</div>
<div style=""transform: rotate(90deg); position: absolute; left: -140px; top:375px; width: 300px; word-wrap: break-word;"" align=""left"">Altitude in 1000m</div>";

            var exportFile =
                $@"<div class = ""mw-customtoggle-armor_{infoList.FileName}"" style=""text-align:center; width:auto; overflow:auto; border:solid orange; border-radius:0.625rem; background:mistyrose"">
<strong style=""font-size:1.2rem;""><i>Armor Penetration</i></strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-armor_{infoList.FileName}"" style = ""width:99%;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin-left:1%; padding:0 1%; overflow:auto"">
{chartStart}
{chartLines}
{chartEnd}
{chartUnits}
{chartEnd}
</div>
</div>
";
            return exportFile;
        }
    }
}