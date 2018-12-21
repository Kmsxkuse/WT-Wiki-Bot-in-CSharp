using System.Globalization;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportWeapon {
        public static string Main(InfoArray infoList) {
            var barOMatic = $@"<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin-left:1%; padding:1%; overflow:auto"">
<div style = ""text-align:center; font-size:1.5rem;"">RoF Bar-o-matic:</div>
<div style = ""background: linear-gradient(to right, red, yellow, green); font-size:3rem; width:auto; height:3rem; padding:1rem 0 0 {PaddingLeft(infoList.Rof)}%; margin:0.3rem 19.5% 0.25rem""> | </div>
";
            var wikiTable = $@"{{|class=""wikitable"" style = ""width:75%; margin:0 auto 1rem; font-size:1.2rem""
|+ style = ""width:auto; font-size:2.0rem; text-align:center; margin:0.5rem 0 1rem"" | Rate Of Fire: {infoList.Rof} rounds per minute
! style = ""width:33%"" | Cannon
! style = ""width:33%"" | Caliber
! style = ""width:33%"" | Effective Distance
|-
| style = ""text-align:center"" | {infoList.Cannon}
| style = ""text-align:center"" | {infoList.Caliber}mm
| style = ""text-align:center"" | {infoList.EffectiveDistance}m
|}}
{{|class=""wikitable"" style = ""width:75%; margin:0 auto 1rem; font-size:1.2rem""
! style = ""width:50%"" | Spaded Disp @ 500m
! style = ""width:50%"" | Stock Disp @ 500m
|-
| style = ""text-align:center"" | {infoList.GunDispersion.SpadedDis}m
| style = ""text-align:center"" | {infoList.GunDispersion.StockDis}m
|}}
</div>
</div>";
            var exportFile = $@"<div class = ""mw-customtoggle-weaponInfo_{infoList.FileName}"" style=""font-size:1.2rem; text-align:center; width:auto; overflow:auto; border:solid green; border-radius: 0.625rem; background:lightgreen;"">
<strong><i>Weapon Information</i></strong>
</div>
<div class = ""mw-collapsible"" id = ""mw-customcollapsible-weaponInfo_{infoList.FileName}"" style = ""width:99%; overflow:auto;"">
{barOMatic}
{wikiTable}
";

            string PaddingLeft(decimal rof) {
                var tempPadLeft = (rof * 30) / 800;
                if (tempPadLeft > 60) {
                    tempPadLeft = 60;
                } else if (tempPadLeft < 0) {
                    tempPadLeft = 0;
                }

                return tempPadLeft.ToString(CultureInfo.InvariantCulture);
            }
            return exportFile;
        }
    }
}