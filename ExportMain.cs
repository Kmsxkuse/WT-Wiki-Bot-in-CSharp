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
            // TODO: Async export creation here.
            var outputArr = new string[5];
            Parallel.Invoke(() => {
                outputArr[0] = ExportStart.Main(infoList);
            }, () => {
                outputArr[1] = ExportBasic.Main(infoList);
            }, () => {
                outputArr[2] = ExportWeapon.Main(infoList);
            });
            var completedExport = string.Join("", outputArr);
            Console.WriteLine("TEST");
        }
    }
}