using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class RawParser {
        public static void CompletedArr(Dictionary<string, object> rawBlk, string fileName) {
            /*
            Information Array Layout:
            0.
                0. All Unique Bullets
            1.
                0. Name of Spaded Ammo Belts
                1. ID of Bullets contained in Spaded Ammo Belts
            2.
                0. ID of Bullets contained in Stock/Default Ammo Belt
            3.
                0. Armor Pen of Unique Bullets
                1. Max Armor Pen of Spaded Belts
            4.
                0. New Gun/Stock Gun Dispersion
                1. Cannon Check
                2. ROF
                3. Caliber
            5.
                0. Filename
                1. Gun Name
            */
            var test = GunInfo(rawBlk);
            var test1 = NameCleaning(fileName);
            ConstructBulletArr(GetSpadedBelts(rawBlk), GetStockBelts(rawBlk));
            Console.WriteLine("TESTING");
        }

        private static void ConstructBulletArr(IReadOnlyCollection<KeyValuePair<string, object>> spadedBelts, IEnumerable<KeyValuePair<string, object>> stockBelts) {
            // Spaded Belt Names
            var allKeys = (from beltNames in spadedBelts select beltNames.Key).Distinct().ToList();
            // All Bullets
            var bulletList = new List<object>();
            
            // Spaded Belts
            var sBList = from belt in spadedBelts select belt.Value;
            foreach (var belt in sBList) {
                // TODO: LINQ this addition.
                bulletList.AddRange(((Dictionary<string, object>) belt).Values.ToList());
            }
            
            // Stock Belts
            bulletList.AddRange((from bullet in stockBelts select bullet.Value).ToList());
            
            // Bullet all IDs.
            var bIDs = GetChecksum(bulletList);
            
            // Filtering Uniques.
            var uniqueIDs = bIDs.Distinct().ToList();
            
            // TODO: Unique-ify bulletList then checksum() it.
            
            Console.WriteLine("STOP");
        }

        private static IEnumerable<decimal> GetChecksum(IEnumerable<object> bulletList) {
            return bulletList
                .Select(info =>
                    from values in (Dictionary<string, object>) info where values.Value is decimal select values)
                .Select(numbers => (from num in numbers select (decimal) num.Value).Sum()).ToList();
        }
        
        private static List<KeyValuePair<string, object>> GetSpadedBelts(IReadOnlyDictionary<string, object> rawBlk) {
            var spadedBelts = (from x in rawBlk
                where !x.Key.Contains("bullet") &&
                      x.Value.GetType() == typeof(Dictionary<string, object>) &&
                      ((Dictionary<string, object>) x.Value).ContainsKey("bullet")
                select x).ToList();
            return spadedBelts;
        }
        
        private static IEnumerable<KeyValuePair<string, object>> GetStockBelts(IReadOnlyDictionary<string, object> rawBlk) {
            var stockBullets = from x in rawBlk
                where x.Key.Contains("bullet") && x.Value.GetType() == typeof(Dictionary<string, object>)
                select x;
            return stockBullets;
        }
        
        private static IEnumerable<object> GunInfo(IReadOnlyDictionary<string, object> rawBlk) {
            // TODO: Possible Async with c# Promise Equiv?
            
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
        
        private static IEnumerable<string> NameCleaning(string fileName) {
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