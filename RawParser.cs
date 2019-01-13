using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    /// <summary>
    /// Parsed BLK Data Object.
    /// </summary>
    public class InfoArray {
        /// <summary>
        /// All Unique Bullets
        /// </summary>
        public List<object> UniqueBullets { get; }
        /// <summary>
        /// Unique Bullet IDs
        /// </summary>
        public List<decimal> UniqueIDs {get; set;}
        
        /// <summary>
        /// Name of Spaded Bullet Belts
        /// </summary>
        public List<string> SpadedNames { get; }
        /// <summary>
        /// ID of Bullets contained in Spaded Ammo Belts
        /// </summary>
        public List<List<decimal>> SpadedIDs { get; }
        
        /// <summary>
        /// Name of Default Ammo Belt. Default is "Default". Duh.
        /// </summary>
        public List<string> StockNames { get; }
        /// <summary>
        /// ID of Bullets contained in Stock/Default Ammo Belt
        /// </summary>
        public List<decimal> StockIDs { get; }
        
        /// <summary>
        /// New Gun/Stock Gun Dispersion
        /// </summary>
        public GunDis GunDispersion {get; private set;}
        /// <summary>
        /// Gun Dispersion Data Object.
        /// </summary>
        public class GunDis {
            /// <summary>
            /// Stock Dispersion at 500m
            /// </summary>
            public decimal StockDis { get; }
            /// <summary>
            /// Spaded Dispersion at 500m
            /// </summary>
            public decimal SpadedDis { get; }

            /// <summary>
            /// Assigning Stock and Spaded Dispersion Information
            /// </summary>
            /// <param name="disInfoArr"></param>
            public GunDis(IReadOnlyList<decimal> disInfoArr) {
                StockDis = disInfoArr[0];
                SpadedDis = disInfoArr[1];
            }
        }
        /// <summary>
        /// Cannon Check
        /// </summary>
        public string Cannon {get; private set;}
        /// <summary>
        /// Rate of Fire Per Minute
        /// </summary>
        public decimal Rof {get; private set;}
        /// <summary>
        /// Gun Caliber in mm
        /// </summary>
        public decimal Caliber {get; private set;}
        /// <summary>
        /// Effective Distance in m
        /// </summary>
        public decimal EffectiveDistance {get; private set;}


        /// <summary>
        /// Info List Gun Info Setter
        /// </summary>
        /// <param name="gunInfoList"></param>
        internal void SetGunInfo(List<object> gunInfoList) {
            GunDispersion = new GunDis((decimal[]) gunInfoList[0]);
            Cannon = (string) gunInfoList[1];
            Rof = (decimal) gunInfoList[2];
            Caliber = (decimal) gunInfoList[3];
            EffectiveDistance = (decimal) gunInfoList[4];
        }
        
        /// <summary>
        /// Raw File Name
        /// </summary>
        public string FileName {get; private set;}
        /// <summary>
        /// Cleaned File Name
        /// </summary>
        public string GunName {get; private set;}
        /// <summary>
        /// Time Last Modified
        /// </summary>
        public string LastModified {get; private set;}

        /// <summary>
        /// Info List Name Info Setter
        /// </summary>
        /// <param name="nameInfoList"></param>
        internal void SetNameInfo(List<string> nameInfoList) {
            FileName = nameInfoList[0];
            GunName = nameInfoList[1];
            LastModified = nameInfoList[2];
        }

        /// <summary>
        /// Info List Belt Initialization
        /// </summary>
        /// <param name="spadedBelts"></param>
        /// <param name="stockBelts"></param>
        internal InfoArray(IReadOnlyCollection<KeyValuePair<string, object>> spadedBelts, IEnumerable<KeyValuePair<string, object>> stockBelts) {
            // Spaded Belt Names
            SpadedNames = (from beltNames in spadedBelts select beltNames.Key).Distinct().ToList();
            
            // Spaded Belts
            UniqueBullets = new List<object>();
            SpadedIDs = new List<List<decimal>>();
            var sBList = (from belt in spadedBelts select belt.Value).ToList();
            sBList.ForEach(belt => UniqueBullets.AddRange(((Dictionary<string, object>) belt).Values));
            sBList.ForEach(belt => SpadedIDs.Add(GetChecksum(((Dictionary<string, object>) belt).Values)));
            
            // Stock Belts
            var stockBList = (from bullet in stockBelts select bullet.Value).ToList();
            UniqueBullets.AddRange(stockBList);
            StockNames = new List<string> {"Default"};
            StockIDs = GetChecksum(stockBList);
            
            // Unique-ify BulletList by Mass and BulletType
            UniqueBullets = UniqueBullets.GroupBy(x => new {
                massVerifier = ((Dictionary<string, object>) x).First(mass => mass.Key == "mass"),
                nameVerifier = ((Dictionary<string, object>) x).First(bulletType => bulletType.Key == "bulletType")
            }).Select(y => y.First()).ToList();
            UniqueIDs = GetChecksum(UniqueBullets);
        }

        private static List<decimal> GetChecksum(IEnumerable<object> bulletList) {
            return bulletList
                .Select(info =>
                    from values in (Dictionary<string, object>) info where values.Value is decimal select values)
                .Select(numbers => (from num in numbers select (decimal) num.Value).Sum()).ToList();
        }
    }
    internal static class RawParser {
        public static InfoArray CompletedArr(Dictionary<string, object> rawBlk, FileInfo fileInfo) {
            var infoList = new InfoArray(GetSpadedBelts(rawBlk), GetStockBelts(rawBlk));
            infoList.SetGunInfo(GunInfo(rawBlk));
            infoList.SetNameInfo(NameCleaning(fileInfo));

            return infoList;
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
        
        private static List<object> GunInfo(IReadOnlyDictionary<string, object> rawBlk) {
            // TODO: Possible Async with c# Promise Equiv?
            
            string CannonCheck() {
                if (rawBlk.ContainsKey("cannon") && ((bool) rawBlk["cannon"])) {
                    return "Yes";
                }
                return "No";
            }

            decimal RofCheck() {
                if (rawBlk.ContainsKey("shotFreq")) {
                    return Math.Round((decimal) rawBlk["shotFreq"] * 60);
                }
                throw new Exception("ROFChecker could not find shotFreq.");
            }

            decimal CaliberCheck() {
                if (rawBlk.ContainsKey("bullet")) {
                    return Math.Round((decimal) ((Dictionary<string, object>) rawBlk["bullet"])["caliber"] * 1000);
                }
                throw new Exception("CaliberCheck could not find bullet.");
            }

            decimal[] Dispersion() {
                var compiled = new decimal[2];
                var spadedDispersion = from x in rawBlk where x.Key.Contains("new_gun") select x;
                
                var sDList = spadedDispersion.ToList();
                if (!sDList.Any()) {
                    // Stock Dispersion Not Found
                    compiled[0] = -1;
                } else if (sDList.Count() > 1) {
                    throw new Exception("More than one _new_gun found.");
                }

                if (compiled[0] != -1) { // Checking for found stock dispersion
                    compiled[0] =
                        Math.Round(
                            (decimal) Math.Tan(
                                (double) (decimal) ((Dictionary<string, object>) sDList.First().Value)[
                                    "maxDeltaAngle"] * Math.PI / 180) * 500, 2);
                }
                
                if (!rawBlk.ContainsKey("maxDeltaAngle"))
                    throw new Exception("Dispersion could not find stock maxDeltaAngle.");
                
                compiled[1] = Math.Round((decimal) Math.Tan((double) (decimal) rawBlk["maxDeltaAngle"] * Math.PI / 180) * 500, 2);
                return compiled;
            }

            decimal EffectiveDistance() {
                if (rawBlk.ContainsKey("bullet") && ((Dictionary<string, object>) rawBlk["bullet"]).ContainsKey("effectiveDistance")) {
                    return (decimal) ((Dictionary<string, object>) rawBlk["bullet"])["effectiveDistance"];
                }
                // Effective Distance Not Found
                return -1M;
            }
            return new List<object> {
                Dispersion(),
                CannonCheck(),
                RofCheck(),
                CaliberCheck(),
                EffectiveDistance()
            };
        }
        
        private static List<string> NameCleaning(FileSystemInfo fileInfo) {
            string Capitalizing(Match m) {
                return m.Groups[1].Value.ToUpper();
            }

            var cleaning = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'))
                .Replace('_', ' ')
                .Replace("cannon", "")
                .Replace("gun", "");
            cleaning = Regex.Replace(cleaning, @"(\b[a-z](?!\b))", Capitalizing);
            
            return new List<string> {
                fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')),
                cleaning,
                fileInfo.LastWriteTimeUtc.ToLongDateString()
            };
        }
    }
}