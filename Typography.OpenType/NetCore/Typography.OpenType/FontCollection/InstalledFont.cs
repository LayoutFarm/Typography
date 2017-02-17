//from http://stackoverflow.com/questions/3633000/net-enumerate-winforms-font-styles
// https://www.microsoft.com/Typography/OTSpec/name.htm
//MIT, 2016-2016, WinterDev


namespace Typography.OpenType
{
    public class InstalledFont
    {

        public InstalledFont(string fontName, string fontSubFamily, string fontPath = "")
        {
            FontName = fontName;
            FontSubFamily = fontSubFamily;
            FontPath = fontPath;
        }
         
        public string FontName { get; set; }
        public string FontSubFamily { get; set; }
        public string FontPath { get; set; }

#if DEBUG
        public override string ToString()
        {
            return FontName;
        }
#endif
    }

}