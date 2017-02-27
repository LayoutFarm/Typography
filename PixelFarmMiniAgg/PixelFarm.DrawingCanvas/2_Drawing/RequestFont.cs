//MIT, 2014-2017, WinterDev

using System;
using PixelFarm.Drawing.Fonts;
namespace PixelFarm.Drawing
{
    /// <summary>
    /// user request for font
    /// </summary>
    public sealed class RequestFont
    {
        //each platform/canvas has its own representation of this Font
        //actual font will be resolved by the platform.
        /// <summary>
        /// font size in points unit
        /// </summary>
        float sizeInPoints;
        FontKey fontKey;
        public RequestFont(string facename, float fontSizeInPts, FontStyle style = FontStyle.Regular)
        {
            WriteDirection = WriteDirection.LTR;
            ScriptCode = ScriptLangs.Latin;//default

            //Lang = "en";//default
            Name = facename;
            SizeInPoints = fontSizeInPts;
            Style = style;
            fontKey = new FontKey(facename, fontSizeInPts, style);

            //TODO: review here ***
            //temp fix 
            //we need font height*** 
            //this.Height = SizeInPixels;
        }
        public FontKey FontKey
        {
            get { return this.fontKey; }
        }

        /// <summary>
        /// font's face name
        /// </summary>
        public string Name { get; private set; }
        public FontStyle Style { get; private set; }

        /// <summary>
        /// emheight in point unit
        /// </summary>
        public float SizeInPoints
        {
            get { return sizeInPoints; }
            private set
            {
                sizeInPoints = value;
            }
        }
        public float DescentInPixels
        {
            get
            {

                if (_actualFont != null)
                {
                    return (float)_actualFont.DescentInPixels;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public float AscentInPixels
        {
            get
            {

                if (_actualFont != null)
                {
                    return (float)_actualFont.AscentInPixels;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        public float LineGapInPixels
        {
            get
            {

                if (_actualFont != null)
                {
                    return (float)_actualFont.LineGapInPixels;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        static int s_POINTS_PER_INCH = 72; //default value
        static int s_PIXELS_PER_INCH = 96; //default value


        public float SizeInPixels
        {
            get
            {
                if (_actualFont != null)
                {
                    return (float)_actualFont.SizeInPixels;
                }
                else
                {
                    return ConvEmSizeInPointsToPixels(SizeInPoints);
                }
            }
        }
        //--------------------------
        //data for shaping engine

        public WriteDirection WriteDirection { get; set; }
        public ScriptLang ScriptCode { get; set; }
        public static float ConvEmSizeInPointsToPixels(float emsizeInPoint)
        {
            return (int)(((float)emsizeInPoint / (float)s_POINTS_PER_INCH) * (float)s_PIXELS_PER_INCH);
        }

        //-------------
        /// <summary>
        /// resolved actual font
        /// </summary>
        ActualFont _actualFont;
        internal static void SetCacheActualFont(RequestFont r, ActualFont f)
        {
            r._actualFont = f;
        }
        internal static ActualFont GetCacheActualFont(RequestFont r)
        {
            return r._actualFont;
        }
    }

}