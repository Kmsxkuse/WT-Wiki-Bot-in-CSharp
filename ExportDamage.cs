using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportDamage {
        public static string Main(InfoArray infoList) {
            string PhysicalTable(Dictionary<string, object> bullet) {
                var wikiTable = $@"{{|class=""wikitable"" style=""width:75%;margin:0 auto 0.5rem""
|+ style=""width:auto;font-size:1.2rem;margin:0.5rem 0"" | {NameCleaning((string) bullet["bulletType"])}
! style=""width:25%"" | Mass
! style=""width:25%"" | Velocity
! style=""width:25%"" | Fire Multiplier
! style=""width:25%"" | HitPower Multiplier
|-
| style=""text-align:center"" | {Math.Round((decimal) bullet["mass"] * 1000M)}g
| style=""text-align:center"" | {bullet["speed"]}m/s
| style=""text-align:center"" | {BulletCheck(bullet, "onHitChanceMultFire")}
| style=""text-align:center"" | {BulletCheck(bullet, "hitPowerMult")}
|}}
";
                return wikiTable;
            }

            object BulletCheck(IReadOnlyDictionary<string, object> bullet, string checker) {
                return !bullet.ContainsKey(checker) ? "N/A" : bullet[checker];
            }
            
            string ExplosiveTable(Dictionary<string, object> bullet) {
                var explosives = (from x in bullet
                    where x.Key.Contains("explode") || x.Key.Contains("explosive")
                    select x).ToDictionary(obj => obj.Key, obj => obj.Value);
                
                var tableHeaders = new StringBuilder();
                var tableInfo = new StringBuilder();
                foreach (var keyValuePair in explosives) {
                    tableHeaders.Append($@"! style=""width:{Math.Round(100M / explosives.Count(), 2)}%"" |{HeaderSplitter(keyValuePair.Key)}
");
                    tableInfo.Append($@"| style=""text-align:center"" | {ManualExpUnits(keyValuePair)}
");
                }
                var wikiTable = $@"{{|class=""wikitable"" style=""width:75%;margin:0 auto 0.5rem""
|+ style=""width:auto;font-style:italic"" | Explosive Data
{tableHeaders}
|-
{tableInfo}
|}}
";
                return wikiTable;
            }

            string HeaderSplitter(string header) {
                // Removing the redundant explosive/explode/shutter in header.
                header = NameCleaning(Regex.Replace(header, @"(^[a-z]+(?=[A-Z]))", ""));
                return header;
            }

            string ManualExpUnits(KeyValuePair<string, object> col) {
                switch (col.Key) {
                    case "explosiveMass":
                        return $"{Math.Round((decimal) col.Value * 1000M)}g";
                    case "explosiveType":
                        return NameCleaning((string) col.Value);
                    case "explodeTreshold":
                    case "explodeArmorPower":
                        return $"{col.Value}mm";
                    case "explodeRadius":
                        return $"[{Math.Round(((float[]) col.Value)[0], 2)}, {Math.Round(((float[]) col.Value)[1], 2)}]";
                    default:
                        return col.Value.ToString();
                }
            }
            
            string FragTable(Dictionary<string, object> bullet) {
                var fragmentation = (from x in bullet
                    where x.Key.Contains("shutter") && !x.Key.Equals("shutterDamage")
                    select x).ToDictionary(obj => obj.Key, obj => obj.Value);
                
                var tableHeaders = new StringBuilder();
                var tableInfo = new StringBuilder();
                foreach (var keyValuePair in fragmentation) {
                    tableHeaders.Append($@"! style=""width:{Math.Round(100M / fragmentation.Count(), 2)}%"" |{HeaderSplitter(keyValuePair.Key)}
");
                    tableInfo.Append($@"| style=""text-align:center"" | {ManualFragUnits(keyValuePair)}
");
                }
                var wikiTable = $@"{{|class=""wikitable"" style=""width:75%;margin:0 auto 0.5rem""
|+ style=""width:auto;font-style:italic"" | Fragmentation Data
{tableHeaders}
|-
{tableInfo}
|}}
";
                return wikiTable;
            }

            string ManualFragUnits(KeyValuePair<string, object> col) {
                switch (col.Key) {
                    case "shutterArmorPower":
                        return $"{col.Value}mm";
                    default:
                        return col.Value.ToString();
                }
            }
            
            var internalFile = new StringBuilder();
            infoList.UniqueBullets.ForEach(bullet => {
                internalFile.Append(PhysicalTable((Dictionary<string, object>) bullet));
                if (((Dictionary<string, object>) bullet).ContainsKey("explosiveMass")) {
                    internalFile.Append(ExplosiveTable((Dictionary<string, object>) bullet));
                }
                if (((Dictionary<string, object>) bullet).ContainsKey("shutterDamage")) {
                    internalFile.Append(FragTable((Dictionary<string, object>) bullet));
                }

                internalFile.Append("\n\n<hr/>\n");
            });
            
            var exportFile = $@"<div class=""mw-customtoggle-damage_{infoList.FileName}"" style=""text-align:center;width:auto;overflow:auto;border:solid blue;border-radius:0.625rem;background:lightskyblue"">
<span style=""font-size:1.2rem;font-style:italic;font-weight:bold"">Bullet Information</span>
</div>
<div class=""mw-collapsible"" id=""mw-customcollapsible-damage_{infoList.FileName}"" style=""width:99%;"">
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