using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportMain {
        public static void Main(InfoArray infoList) {
            var outputArr = new string[6];
            Parallel.Invoke(() => {
                outputArr[0] = ExportStart.Main(infoList);
            }, () => {
                outputArr[1] = ExportBasic.Main(infoList);
            }, () => {
                outputArr[2] = ExportWeapon.Main(infoList);
            }, () => {
                outputArr[3] = ExportDamage.Main(infoList);
            }, () => {
                outputArr[4] = ExportArmorPen.Main(infoList);
            }, () => {
                outputArr[5] = ExportOldTables.Main(infoList);
            });
            var completedExport = string.Join("", outputArr);
            Console.WriteLine("TEST");
        }
    }
}