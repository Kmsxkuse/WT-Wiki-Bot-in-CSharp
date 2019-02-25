using System;

namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportStart {
        public static string Main(InfoArray infoList) {
            var infoText = $@"<strong style = ""font-size:2rem;""> Created by Kmsxkuse. </strong>

Bot Version: [1.0 Alpha (C# Edition)]. Bot Last Run: [{DateTime.UtcNow.ToLongDateString()}].

Bot sources: 

    (Typescript Version): https://github.com/Kmsxkuse/War-Thunder-Wiki-Bot
    (C# Version): https://github.com/Kmsxkuse/WT-Wiki-Bot-in-CSharp

Database last updated: [{infoList.LastModified}]. War Thunder Version: [1.85].

The following information should be taken with a block of salt. 

If the numbers don't make sense, assume bugs or missing data. I have no access to server side information that may be modifying the shell data.

If you need to contact the creator, please private message /u/Kmsxkuse on www.reddit.com.";
/*
            var exportFile =
                $@"<div class = ""mw-customtoggle-{infoList.FileName}"" style=""border:mediumturquoise; border-radius: 0.625rem; background:lightcyan; padding:0.5% 1% 0; padding-right:1%; display:inline-block"">
<p><strong style = ""line-height:1rem; font-size:1.5rem"">Click here for {infoList.GunName} Belts!</strong></p>
</div>
<div class = ""mw-collapsible mw-collapsed"" id=""mw-customcollapsible-{infoList.FileName}"" style = ""width:100%"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; padding:1%"">
<div class = ""mw-customtoggle-glossary_{infoList.FileName}"" style=""font-size:1.2rem; text-align:center; width:auto; border:solid crimson; border-radius: 0.625rem; background:lightpink"">
<strong>Information Page</strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-glossary_{infoList.FileName}"" style = ""width:99%;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; margin-left:1%; padding:1%; font-size:1rem; overflow:auto"">
{infoText}
</div>
</div>
";
*/
            var exportFile =
                $@"<div class = ""mw-customtoggle-glossary_{infoList.FileName}"" style=""font-size:1.2rem; text-align:center; width:auto; border:solid crimson; border-radius: 0.625rem; background:lightpink"">
<strong>Information Page</strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-glossary_{infoList.FileName}"" style = ""width:99%;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; margin-left:1%; padding:1%; font-size:1rem; overflow:auto"">
{infoText}
</div>
";
            
            return exportFile;
        }
    }
}