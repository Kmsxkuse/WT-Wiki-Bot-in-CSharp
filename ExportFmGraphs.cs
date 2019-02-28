using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp
{
    internal static class ExportFmGraphs
    {
        private static readonly string[] ColourValues =
        {
            "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
            "800000", "008000", "000080", "808000", "800080", "008080", "808080",
            "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
            "400000", "004000", "000040", "404000", "400040", "004040", "404040",
            "200000", "002000", "000020", "202000", "200020", "002020", "202020",
            "600000", "006000", "000060", "606000", "600060", "006060", "606060",
            "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
            "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
        };

        private struct InputInfo
        {
            public string ChartType { get; }
            public string Title { get; }
            public string XAxis { get; }
            public string YAxis { get; }
            public List<float[]> RawDataMil { get; }
            public List<float[]> RawDataWep { get; }
            public double MaxX { get; }
            public double MinX { get; }

            public InputInfo(string chartType, string title, string xAxis, string yAxis, List<float[]> rawDataMil,
                List<float[]> rawDataWep)
            {
                ChartType = chartType;
                Title = title;
                XAxis = xAxis;
                YAxis = yAxis;
                RawDataMil = rawDataMil;
                RawDataWep = rawDataWep;
                switch (chartType)
                {
                    case "Speed":
                        float maxXMil = rawDataMil.Max(point => point[1]),
                            maxXWep = rawDataWep.Count != 0 ? rawDataWep.Max(point => point[1]) : 0;
                        MaxX = Math.Ceiling(maxXMil > maxXWep ? maxXMil / 50: maxXWep / 50) * 50;
                        float minXMil = rawDataMil.Min(point => point[1]),
                            minXWep = rawDataWep.Count != 0 ? rawDataWep.Min(point => point[1]) : 0;
                        MinX = Math.Floor(minXMil > minXWep ? minXMil / 50: minXWep / 50) * 50;
                        break;
                    case "ClimbTime":
                        maxXMil = rawDataMil.Max(point => point[1]);
                        maxXWep = rawDataWep.Count != 0 ? rawDataWep.Max(point => point[1]) : 0;
                        MaxX = Math.Ceiling(maxXMil > maxXWep ? maxXMil / 100: maxXWep / 100) * 100;
                        MinX = 0;
                        // Adding (0,0) point to both mil and wep
                        RawDataMil.Insert(0, new[] {0f,0f});
                        if (rawDataWep.Count != 0) RawDataWep.Insert(0, new[] {0f,0f});
                        break;
                    case "ClimbRate":
                        maxXMil = rawDataMil.Max(point => point[1]);
                        maxXWep = rawDataWep.Count != 0 ? rawDataWep.Max(point => point[1]) : 0;
                        MaxX = Math.Ceiling(maxXMil > maxXWep ? maxXMil / 5: maxXWep / 5) * 5;
                        minXMil = rawDataMil.Min(point => point[1]);
                        minXWep = rawDataWep.Count != 0 ? rawDataWep.Min(point => point[1]) : 0;
                        MinX = Math.Floor(minXMil > minXWep ? minXMil / 5: minXWep / 5) * 5;
                        break;
                    default:
                        throw new Exception("Chart type invalid.");
                }
            }
        }

        public static string SpeedChart(RawFmParser infoList)
        {
            return CreateChart(new InputInfo("Speed", $"{CleanedName(infoList.FileName)} Max Speed", "Speed in km/hr",
                "Altitude in 1000m", infoList.MaxSpeedWikiMil, infoList.MaxSpeedWikiWep));
        }

        public static string ClimbTimeChart(RawFmParser infoList)
        {
            return CreateChart(new InputInfo("ClimbTime", "Minimum Time to Altitude", "Time in Seconds",
                "Altitude in 1000m", infoList.ClimbTimeWikiMil, infoList.ClimbTimeWikiWep));
        }

        public static string ClimbRateChart(RawFmParser infoList)
        {
            return CreateChart(new InputInfo("ClimbRate", "Climb Rate at Altitude", "Rate in m/s",
                "Altitude in 1000m", infoList.ClimbRateWikiMil, infoList.ClimbRateWikiWep));
        }

        private static string CleanedName(string fileName)
        {
            string Capitalizing(Match m)
            {
                return m.Groups[1].Value.ToUpper();
            }

            var cleaning = fileName.Replace('_', ' ').Replace('-', ' ');
            cleaning = Regex.Replace(cleaning, @"(\b[a-z](?!\b))", Capitalizing);
            return cleaning;
        }

        private static string CreateChart(InputInfo inputInfo)
        {
            const decimal tableSize = 40;
            const decimal pointSize = 0.25m;
            var intSize = Math.Round(tableSize * 17 / 20, 2);
            const int maxLines = 11; // - 1 for actual number of lines
            const string chartEnd = "</div>";

            var chartStart =
                $@"<div style=""position:relative;width:{tableSize}rem;height:{tableSize}rem;background:#f0f0f0;border:solid;margin:1rem auto"">
<div style=""position:absolute;left:{Math.Round(tableSize / 10, 2)}rem;top:{Math.Round(tableSize / 20, 2)}rem;width:{intSize}rem;height:{intSize}rem;background:white;border:solid;"">";

            var chartLines = new StringBuilder();
            for (var lineNum = 1; lineNum < maxLines; lineNum++)
            {
                // Originating from Y Lines
                chartLines.Append(
                    $@"<div style=""position:absolute;top:{Math.Round(intSize / maxLines * lineNum, 2)}rem; width:{intSize}rem;border-bottom:dashed 2px lightgray;""></div>");
                // X Lines
                chartLines.Append(
                    $@"<div style=""position:absolute;height:{intSize}rem;left:{Math.Round(intSize / maxLines * lineNum, 2)}rem;border-right:dashed 2px lightgray;""></div>");
            }

            float maxAltMil = inputInfo.RawDataMil[inputInfo.RawDataMil.Count - 1][0] / 1000f,
                maxAltWep = inputInfo.RawDataWep.Count != 0 ? inputInfo.RawDataWep[inputInfo.RawDataWep.Count - 1][0] / 1000f : 0;
            var maxAlt = Math.Ceiling(maxAltMil > maxAltWep ? maxAltMil : maxAltWep);
            maxAlt = maxAlt > 10 ? maxAlt : 10;

            var chartUnits = new StringBuilder();
            for (var lineNum = maxLines; lineNum > 0; lineNum--)
            {
                // Y Values. Top to bottom
                chartUnits.Append(
                    $@"<div style=""position:absolute;top:{Math.Round((double) intSize / maxLines * (lineNum + 0.5), 2)}rem;width:{Math.Round(tableSize / 12, 2)}rem"" align=""right"">{Math.Round(maxAlt / (maxLines - 1) * (maxLines - lineNum), 1)}</div>");

                // X Values. Left to Right
                chartUnits.Append(
                    $@"<div style=""position:absolute;top:{Math.Round(intSize + tableSize * 3 / 40 - 0.3M, 2)}rem;left:{Math.Round((double) intSize / maxLines * lineNum + 0.6, 2)}rem;width:{Math.Round(tableSize / lineNum, 2)}"" align=""center"">{Math.Round(inputInfo.MinX + (inputInfo.MaxX - inputInfo.MinX) / (maxLines - 1) * (lineNum - 1))}</div>");
            }

            var penBox =
                $@"<div style=""position:absolute;width:{Math.Round((1 - 1 / (double) (maxLines - 1)) * 100, 2)}%;height:{Math.Round((1 - 1 / (decimal) (maxLines - 1)) * 100, 2)}%;bottom:0;left:0"">";

            var penCoords = new StringBuilder();
            var penLines = new StringBuilder();
            var penLegend = new StringBuilder();
            var prevPoint = new[] {0d, 0d};

            // Mil list first
            for (var coordinate = 0; coordinate < inputInfo.RawDataMil.Count; coordinate++)
            {
                /* Vertical (bottom) Offset: (1 - boxSize / 2 / intSize )
                 * Experimentally Determined.
                 * Reduced by half the height (aka length) of the box.
                 * Then converted to % of box.
                 */
                var boxPointX = Math.Round((inputInfo.RawDataMil[coordinate][1] - inputInfo.MinX) / (inputInfo.MaxX - inputInfo.MinX) * 100, 2);
                var boxPointY = Math.Round(inputInfo.RawDataMil[coordinate][0] / maxAlt / 1000 * 100, 2);
                if (coordinate != 0)
                {
                    var lineLength =
                        Math.Round(Math.Sqrt(Math.Pow(prevPoint[0] - boxPointX, 2) +
                                             Math.Pow(prevPoint[1] - boxPointY, 2)));
                    var rotate = Math.Round(Math.Atan((prevPoint[0] - boxPointX) / (prevPoint[1] - boxPointY)), 2);
                    // Lines
                    penLines.Append(
                        $@"<div style=""position:absolute;transform:rotate({rotate}rad);left:{(boxPointX + prevPoint[0]) / 2}%;bottom:{(boxPointY + prevPoint[1]) / 2 - lineLength / 2 + 1}%;height:{lineLength}%;border-right:solid #{ColourValues[0]}""></div>");
                }

                prevPoint[0] = boxPointX;
                prevPoint[1] = boxPointY;

                // Points
                penCoords.Append(
                    $@"<div style=""position:absolute;left:{boxPointX - (double) pointSize}%;bottom:{boxPointY}%;width:{pointSize}rem;height:{pointSize}rem;border:solid #{ColourValues[0]};border-radius:50%;background:white;""></div>");
            }
            // Mil Legend
            penLegend.Append(
                $@"<div style=""position:relative;padding:0.2rem;border:solid #{ColourValues[0]};width:20%;top:1%;margin:0 0 0 75%;background:white;text-align:center"">100%</div>");

            if (inputInfo.RawDataWep.Count != 0)
            {
                prevPoint = new[] {0d, 0d};
                // WEP List
                for (var coordinate = 0; coordinate < inputInfo.RawDataWep.Count; coordinate++)
                {
                    var boxPointX = Math.Round((inputInfo.RawDataWep[coordinate][1] - inputInfo.MinX) / (inputInfo.MaxX - inputInfo.MinX) * 100, 2);
                    var boxPointY = Math.Round(inputInfo.RawDataWep[coordinate][0] / maxAlt / 1000 * 100, 2);
                    if (coordinate != 0)
                    {
                        var lineLength =
                            Math.Round(Math.Sqrt(Math.Pow(prevPoint[0] - boxPointX, 2) +
                                                 Math.Pow(prevPoint[1] - boxPointY, 2)));
                        var rotate = Math.Round(Math.Atan((prevPoint[0] - boxPointX) / (prevPoint[1] - boxPointY)), 2);
                        // Lines
                        penLines.Append(
                            $@"<div style=""position:absolute;transform:rotate({rotate}rad);left:{(boxPointX + prevPoint[0]) / 2}%;bottom:{(boxPointY + prevPoint[1]) / 2 - lineLength / 2 + 1}%;height:{lineLength}%;border-right:dashed #{ColourValues[0]}""></div>");
                    }
    
                    prevPoint[0] = boxPointX;
                    prevPoint[1] = boxPointY;
    
                    // Points
                    penCoords.Append(
                        $@"<div style=""position:absolute;left:{boxPointX - (double) pointSize}%;bottom:{boxPointY}%;width:{pointSize}rem;height:{pointSize}rem;border:solid #{ColourValues[0]};border-radius:50%;background:white;""></div>");
                }
                // WEP Legend
                penLegend.Append(
                    $@"<div style=""position:relative;padding:0.2rem;border:dashed #{ColourValues[0]};width:20%;top:1%;margin:0 0 0 75%;background:white;text-align:center"">WEP</div>");
            }
            
            // First is X Axis, second is Y Axis.
            var chartWords = $@"<b><div style=""position:absolute;width:100%"" align=""center"">{inputInfo.Title}</div>
<div style=""position:absolute;width:100%;top:{tableSize - 1.5M}rem"" align=""center"">{inputInfo.XAxis}</div>
<div style=""transform:rotate(90deg);position:relative;left:-{tableSize - 1.5M}rem;height:100%;"" align=""center"">{inputInfo.YAxis}</div></b>";

            // To prevent people from saying my poor chart is bugged.
            const string chartWarning =
                @"<div style=""position:relative;text-align:center;font-weight:bold;top:0.5rem"">Colors indicated in the legend that do not appear in the graph may be hidden behind another line.<br />Numbers along Y Axis are not precise. They have been rounded to the nearest integer.</div>";

            var exportFile =
                $@"<div class=""mw-customtoggle-performance_{inputInfo.ChartType}"" style=""text-align:center;width:auto;overflow:auto;border:solid orange;border-radius:0.625rem;background:mistyrose"">
<span style=""font-size:1.2rem;font-style:italic;font-weight:bold"">{inputInfo.Title} Chart</span>
</div>
<div class=""mw-collapsible"" id=""mw-customcollapsible-performance_{inputInfo.ChartType}"" style=""width:99%;"">
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