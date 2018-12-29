namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportDamage {
        public static string Main(InfoArray infoList) {
            
            var internalFile = "TEST TEST TEST";
            var exportFile = $@"<div class = ""mw-customtoggle-damage_{infoList.FileName}"" style=""text-align:center; width:auto; overflow:auto; border:solid blue; border-radius: 0.625rem; background:lightskyblue"">
<strong style=""font-size:1.2rem;""><i>Damage Values</i></strong>
</div>
<div class = ""mw-collapsible mw-collapsed"" id = ""mw-customcollapsible-damage_{infoList.FileName}"" style = ""width:99%;"">
<div class = ""mw-collapsible-content"" style = ""border:solid lightgray; background:white; margin-left:1%; padding:0 1%; overflow:auto"">
{internalFile}
</div>
</div>";
            return exportFile;
        }
    }
}