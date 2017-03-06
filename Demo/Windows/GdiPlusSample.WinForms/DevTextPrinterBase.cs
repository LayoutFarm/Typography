//MIT, 2016-2017, WinterDev

namespace SampleWinForms
{
    /// <summary>
    /// base TextPrinter class for developer only, 
    /// </summary>
    public abstract class DevTextPrinterBase
    {

        protected string _currentSelectedFontFile;
        public DevTextPrinterBase()
        {
            FontSizeInPoints = 14;//
            ScriptLang = Typography.OpenFont.ScriptLangs.Latin;//default?
        }

        public string FontFilename
        {
            get
            {
                return _currentSelectedFontFile;
            }
            set
            {
                if (_currentSelectedFontFile != value)
                {
                    _currentSelectedFontFile = value;
                    //sample only ....
                    //when we change new font, 
                    OnFontFilenameChanged();
                }
               
            }
        }
        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }
        public HintTechnique HintTechnique { get; set; }
        public float FontSizeInPoints { get; set; }
        protected virtual void OnFontFilenameChanged() { }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public Typography.TextLayout.PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }

        public abstract void DrawString(char[] textBuffer, float xpos, float ypos);
    }
}