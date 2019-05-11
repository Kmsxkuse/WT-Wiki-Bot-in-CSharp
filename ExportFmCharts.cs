using System.Collections.Generic;
using System.Globalization;

namespace WT_Wiki_Bot_in_CSharp
{
    internal static class ExportFmCharts
    {
        public static string Main(RawFmParser planeInfo, Dictionary<string, object> arcadeModifiers)
        {
            //var arcadeSpeedMulti = (decimal) arcadeModifiers["maxSpeedMultiplier"];
            var climbRateWep = planeInfo.ClimbRateWikiWep.Count != 0
                ? planeInfo.ClimbRateWikiWep[0][1].ToString(CultureInfo.InvariantCulture)
                : "N/A";
            var turnTimeWep = planeInfo.TurnTimeWep?[1].ToString(CultureInfo.InvariantCulture) ?? "N/A";

            var output = $@"=== Flight Performance ===
{{| class=""wikitable"" style=""text-align:center"" width=""70%""
! rowspan=""2"" | Game Mode
! colspan=""2"" | {{{{Annotation|Max Speed|Maximum speed in Km/Hr before wing break apart in RB.}}}}
! rowspan=""2"" | {{{{Annotation|Max Altitude|Meters}}}}
! colspan=""2"" | {{{{Annotation|Turn Time|Seconds at {planeInfo.TurnTimeMil[0]}m}}}}
! colspan=""2"" | {{{{Annotation|Rate of Climb|Meters per Second}}}}
! rowspan=""2"" | {{{{Annotation|Takeoff Run|Meters}}}}
! rowspan=""2"" | {{{{Annotation|WEP Duration|Minutes}}}}
|-
! {{{{Annotation|Stock|Without modifications}}}}
! {{{{Annotation|Spaded|With full modifications}}}}
! {{{{Annotation|100%|Full throttle}}}}
! {{{{Annotation|WEP|War Emergency Power Throttle}}}}
! {{{{Annotation|100%|Full throttle}}}}
! {{{{Annotation|WEP|War Emergency Power Throttle}}}}
|-
! Arcade
| N/A || rowspan=""2"" | {planeInfo.Vne} || rowspan=""2"" | {planeInfo.MaxAltitude} || ? || ? || ? || ? || rowspan=""2"" | {planeInfo.TakeoffDistance} || 25s
|-
! {{{{Annotation|Realistic|Simulator uses Realistic FMs}}}}
| ?  || {planeInfo.TurnTimeMil[1]} || {turnTimeWep} || {planeInfo.ClimbRateWikiMil[0][1]} || {climbRateWep} || {planeInfo.NitroTime}
|-
|}}
===Features===
{{| class=""wikitable"" style=""text-align:center"" width=""70%""
! rowspan = ""2"" |
! colspan = ""3"" | Flaps
! colspan = ""2"" | Gears
! colspan = ""3"" | Controls
! colspan = ""2"" | G Forces
! rowspan = ""2"" | Air Breaks
| -
! Combat
! Takeoff
! Landing
! Retractable
! Arrestor
! Ailerons
! Elevator
! Rudder
! +
! -
|-
! Available
| No || No || Yes || Yes || No || Yes || Yes || Yes || N / A || N / A || No
|-
! Speed Limits
| N / A || N / A || 500 || 500 || N / A || 500 || 500 || 500 || ~10 || ~4 || N / A
|-
|}}
===Manual Engine Controls===
{{| class=""wikitable"" style=""text - align:center"" width=""70 % ""
| -
! Water Radiator
! Oil Radiator
! Supercharger
! Propeller
! Mixer
! Magneto
|-
| Automatic || Controllable || {planeInfo.HorsePower.Count} || Featherable || Controllable || No
|-
|}}";
    return output;
        }
    }
}