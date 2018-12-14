using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class RawParser {
        public static void CompletedArr(Dictionary<string, object> rawBlk, string fileName) {
            /*
            Information Array Layout:
            0.
                0. All Unique Bullets
                1. Armor Pen Information of Unique Bullets
            1.
                0. Name of Spaded Ammo Belts
                1. ID of Bullets contained in Spaded Ammo Belts
                2. Max Armor Pen Values of each Belt
            2.
                0. ID of Bullets contained in Stock/Default Ammo Belt
                1. Max Armor Pen of Belt
            3.
                0. New Gun/Stock Gun Dispersion
                1. Cannon Check
                2. ROF
                3. Caliber
            4.
                0. Filename
                1. Gun Name
            */
            var test = GunInfo(rawBlk);
            var test1 = NameCleaning(fileName);
            Console.WriteLine("TESTING");
        }

        private static void UniqueBullet(IReadOnlyDictionary<string, object> rawBlk) {
            
        }
        
        private static void SpadedBelts(IReadOnlyDictionary<string, object> rawBlk) {
            
        }
        
        private static void StockBelts(IReadOnlyDictionary<string, object> rawBlk) {
            
        }
        
        private static IEnumerable<object> GunInfo(IReadOnlyDictionary<string, object> rawBlk) {
            string CannonCheck() {
                if (rawBlk.ContainsKey("cannon") && ((bool) rawBlk["cannon"])) {
                    return "Yes";
                }
                return "No";
            }

            decimal RofCheck() {
                if (rawBlk.ContainsKey("shotFreq")) {
                    return (decimal) rawBlk["shotFreq"] * 60;
                }
                throw new Exception("ROFChecker could not find shotFreq.");
            }

            decimal CaliberCheck() {
                if (rawBlk.ContainsKey("bullet")) {
                    return (decimal) ((Dictionary<string, object>) rawBlk["bullet"])["caliber"] * 1000;
                }
                throw new Exception("CaliberCheck could not find bullet.");
            }

            decimal[] Dispersion() {
                var compiled = new decimal[2];
                var spadedDispersion = from x in rawBlk where x.Key.Contains("new_gun") select x;
                
                var sDList = spadedDispersion.ToList();
                if (!sDList.Any()) {
                    throw new Exception("Dispersion could not find spaded maxDeltaAngle.");
                } else if (sDList.Count() > 1) {
                    throw new Exception("More than one _new_gun found.");
                }
                
                compiled[0] = (decimal) ((Dictionary<string, object>) sDList.First().Value)["maxDeltaAngle"];
                
                if (!rawBlk.ContainsKey("maxDeltaAngle"))
                    throw new Exception("Dispersion could not find stock maxDeltaAngle.");
                
                compiled[1] = (decimal) rawBlk["maxDeltaAngle"];
                return compiled;
            }
            return new object[] {
                Dispersion(),
                CannonCheck(),
                RofCheck(),
                CaliberCheck()
            };
        }
        
        private static string[] NameCleaning(string fileName) {
            string Capitalizing(Match m) {
                return m.Groups[1].Value.ToUpper();
            }

            var cleaning = fileName.Substring(0, fileName.LastIndexOf('.'))
                .Replace('_', ' ')
                .Replace("cannon", "")
                .Replace("gun", "");
            cleaning = Regex.Replace(cleaning, @"(\b[a-z](?!\b))", Capitalizing);
            return new [] {
                fileName,
                cleaning
            };
        }
    }
}