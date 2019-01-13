namespace WT_Wiki_Bot_in_CSharp {
    internal static class ExportBasic {
        public static string Main(InfoArray infoList) {
            // TODO: Automated Pros and Cons based on data in infoList.
            var exportFile = $@"<div style=""margin:1.5rem 0 0;font-size:1rem"">
<strong style=""line-height:2rem;font-size:2.5rem"">{infoList.GunName}</strong>
<hr/>
<br/>
<b>Pros:</b>
* Insert Pros Here!
<b>Cons:</b>
* Insert Cons Here!
<br/>
</div>
";
            return exportFile;
        }
    }
}