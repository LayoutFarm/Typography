//MIT, 2014-2016, WinterDev

using System;
using PixelFarm.Drawing.Fonts;
namespace PixelFarm.Drawing
{

    /// <summary>
    /// font specification
    /// </summary>
    public sealed class Font
    {
        //each platform/canvas has its own representation of this Font

        float emSizeInPixels;
        /// <summary>
        /// emsize in point
        /// </summary>
        float emSize;
        Fonts.FontKey fontKey;
        public Font(string facename, float emSizeInPoints, FontStyle style = FontStyle.Regular)
        {
            HBDirection = Fonts.HBDirection.HB_DIRECTION_LTR;//default
            ScriptCode = HBScriptCode.HB_SCRIPT_LATIN;//default 
            Lang = "en";//default
            Name = facename;
            EmSize = emSizeInPoints;
            Style = style;
            fontKey = new FontKey(facename, emSizeInPoints, style);
        }
        public FontKey FontKey
        {
            get { return this.fontKey; }
        }

        /// <summary>
        /// font's face name
        /// </summary>
        public string Name { get; private set; }
        public float Height { get; set; } //TODO: review here
        public FontStyle Style { get; set; } //TODO: review here

        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float EmSize
        {
            get { return emSize; }
            private set
            {
                emSize = value;
                emSizeInPixels = ConvEmSizeInPointsToPixels(value);
            }
        }
        public float EmSizeInPixels
        {
            get
            {
                return emSizeInPixels;
            }
        }

        static int s_POINTS_PER_INCH = 72; //default value
        static int s_PIXELS_PER_INCH = 96; //default value


        public static int PointsPerInch
        {
            get { return s_POINTS_PER_INCH; }
            set { s_POINTS_PER_INCH = value; }
        }
        public static int PixelsPerInch
        {
            get { return s_PIXELS_PER_INCH; }
            set { s_PIXELS_PER_INCH = value; }
        }


        //--------------------------
        //font shaping info (for native font/shaping engine)
        public HBDirection HBDirection { get; set; }
        public int ScriptCode { get; set; }
        public string Lang { get; set; }


        public static float ConvEmSizeInPointsToPixels(float emsizeInPoint)
        {
            return (int)(((float)emsizeInPoint / (float)s_POINTS_PER_INCH) * (float)s_PIXELS_PER_INCH);
        }

    }

    public interface IFonts
    {

        float MeasureWhitespace(Font f);
        Size MeasureString(char[] str, int startAt, int len, Font font);
        Size MeasureString(char[] str, int startAt, int len, Font font, float maxWidth, out int charFit, out int charFitWidth);
        ActualFont ResolveActualFont(Font f);
        void Dispose();
    }



}