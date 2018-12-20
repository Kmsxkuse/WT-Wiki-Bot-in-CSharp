using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportStart {
        public static void Main(InfoArray infoList) {
            var infoText = $@"<strong style = ""font-size:2em;""> Created by Kmsxkuse. </strong>

Bot Version: [0.1 (C# Edition)]. Bot Last Run: [{DateTime.UtcNow.ToLongDateString()}].
            
Database last updated: [{infoList.LastModified}]. War Thunder Version: [1.85].
        
The following information should be taken with a grain of salt. 

If the numbers don't make sense, assume bugs or missing data. I have no access to server side information that may be modifying the shell data.

If you need to contact the creator, please message /u/Kmsxkuse on www.reddit.com.

<strong style = ""font-size:1.5em;""> How numbers are calculated: </strong>

ROF Bar-O-Matic is calculated by multiplying the rate of fire per minute by 30 / 800. 

That value is then the paddingleft css style %.

If the adjusted ROF is > 60 or < 0, then they will be 60 or 0.

Physical Damage is calculated by multiplying the mass of the shell by its (velocity / 100) squared along with hitPowerMult if it exists.

The 1/2 in Ke = 1/2 m v^2 is ignored due to it being a constant and the numbers are for comparison.

High Explosive Damage is calculated by explosiveMass * 100 times the first number in explodeRadius * 100 times explodeHitpower.

Fragmentation Damage is calculated by shutterDamageRadius times shutterAmount times shutterHit.

Belt comparison values for damage are the average of the shells' damage values that compose it.

The rest are values pulled directly from the datamine.";

            var exportFile =
                $@"<div class = ""mw-customtoggle-{infoList.FileName}"" style=""border:mediumturquoise; border-radius: 0.625em; background:lightcyan; padding:0 1%; padding-right:1%; display:inline-block"">
<p><strong style = ""line-height:1em; font-size:1.75em"">Click here for {infoList.GunName} Belts!</strong></p>
</div>
<div class = ""mw-collapsible mw-collapsed"" id=""mw-customcollapsible-{infoList.FileName}"" style = ""width:100%; overflow:auto"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; overflow:auto; padding:1%"">
<div class = ""mw-customtoggle-glossary"" style=""font-size:1.2em; text-align:center; width:auto; overflow:auto; border:solid crimson; border-radius: 0.625em; background:lightpink"">
<strong>Information Page</strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-glossary"" style = ""width:99%; overflow:auto;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin-left:1%; padding:0 1%; overflow:auto"">
{infoText}
</div>
</div>
";
            Console.WriteLine("test");
        }
    }
}