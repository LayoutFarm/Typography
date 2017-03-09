//MIT, 2016-2017, WinterDev

namespace Typography.Rendering
{
    /// <summary>
    /// base TextPrinter class for developer only, 
    /// </summary>
    public abstract class DevTextPrinterBase
    {
        HintTechnique _hintTech;
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
        public bool UseTrueTypeInstructions { get; private set; }
        public bool UseVerticalHint { get; private set; }
        public HintTechnique HintTechnique
        {
            get { return _hintTech; }
            set
            {
                this._hintTech = value;
                this.UseTrueTypeInstructions = false; //reset
                this.UseVerticalHint = false; //reset
                switch (value)
                {
                    case HintTechnique.TrueTypeInstruction:
                        this.UseTrueTypeInstructions = true;
                        break;
                    case HintTechnique.TrueTypeInstruction_VerticalOnly:
                        this.UseTrueTypeInstructions = true;
                        this.UseVerticalHint = true;
                        break;
                    case HintTechnique.CustomAutoFit:
                        UseVerticalHint = true;
                        break;
                }
            }
        }
        //
        public float FontSizeInPoints { get; set; }
        protected virtual void OnFontFilenameChanged() { }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public Typography.TextLayout.PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos);



        public void DrawString(char[] textBuffer, float xpos, float ypos)
        {
            this.DrawString(textBuffer, 0, textBuffer.Length, xpos, ypos);
        }

    }
}