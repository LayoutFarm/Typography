//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//-----------------------------------

using System.Collections.Generic;
namespace PixelFarm.Drawing.Fonts
{
    public class SvgFontStore
    {
        public const string DEFAULT_SVG_FONTNAME = "svg-LiberationSansFont";
        Dictionary<string, SvgFontFace> fontFaces = new Dictionary<string, SvgFontFace>();
        internal static void SetShapingEngine(SvgFontFace fontFace, string lang, HBDirection hb_direction, int hb_scriptcode)
        {
            ////string lang = "en";
            ////PixelFarm.Font2.NativeMyFontsLib.MyFtSetupShapingEngine(ftFaceHandle,
            ////    lang,
            ////    lang.Length,
            ////    HBDirection.HB_DIRECTION_LTR,
            ////    HBScriptCode.HB_SCRIPT_LATIN); 
            //ExportTypeFaceInfo exportTypeInfo = new ExportTypeFaceInfo();
            //NativeMyFontsLib.MyFtSetupShapingEngine(fontFace.Handle,
            //    lang,
            //    lang.Length,
            //    hb_direction,
            //    hb_scriptcode,
            //    ref exportTypeInfo);
            //fontFace.HBFont = exportTypeInfo.hb_font;
        }
        Dictionary<Font, SvgFont> registerSvgFonts = new Dictionary<Font, SvgFont>();
        public Drawing.Font LoadFont(string facename, int fontPointSize)
        {
            //load font from specific file 
            SvgFontFace fontFace;
            if (!fontFaces.TryGetValue(facename, out fontFace))
            {
                //temp ....
                //all svg font remap to DEFAULT_SVG_FONTNAME
                //TODO: add more svg font
                if (facename != DEFAULT_SVG_FONTNAME)
                {
                    facename = DEFAULT_SVG_FONTNAME;
                }
                //----------------------------------------
                if (facename == DEFAULT_SVG_FONTNAME)
                {
                    fontFaces.Add(facename, fontFace = SvgFontFace_LiberationSans.Instance);
                }
                else
                {
                    //use default?,  svg-liberation san fonts

                }
            }

            if (fontFace == null)
            {
                return null;
            }

            Font font = new Font(facename, fontPointSize);
            SvgFont svgFont = fontFace.GetFontAtSpecificSize(fontPointSize);
            registerSvgFonts.Add(font, svgFont);
            return font;
        }
        public ActualFont GetResolvedFont(Font f)
        {
            SvgFont found;
            registerSvgFonts.TryGetValue(f, out found);
            return found;
        }

        //---------------------------------------------------
        //helper function
        public static int ConvertFromPointUnitToPixelUnit(float point)
        {
            //from FreeType Documenetation
            //pixel_size = (pointsize * (resolution/72);
            return (int)(point * 96 / 72);
        }
    }
}