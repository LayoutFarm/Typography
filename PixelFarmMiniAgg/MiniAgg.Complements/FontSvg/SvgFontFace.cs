//BSD, 2014-2016, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007-2011
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
//
// Class TypeFace.cs
//
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;
namespace PixelFarm.Drawing.Fonts
{
    class SvgFontFace : FontFace
    {
        class Panos_1
        {
            // these are defined in the order in which they are present in the panos-1 attribute.
            enum Family { Any, No_Fit, Latin_Text_and_Display, Latin_Script, Latin_Decorative, Latin_Pictorial };
            enum Serif_Style { Any, No_Fit, Cove, Obtuse_Cove, Square_Cove, Obtuse_Square_Cove, Square, Thin, Bone, Exaggerated, Triangle, Normal_Sans, Obtuse_Sans, Perp_Sans, Flared, Rounded };
            enum Weight { Any, No_Fit, Very_Light_100, Light_200, Thin_300, Book_400_same_as_CSS1_normal, Medium_500, Demi_600, Bold_700_same_as_CSS1_bold, Heavy_800, Black_900, Extra_Black_Nord_900_force_mapping_to_CSS1_100_900_scale };
            enum Proportion { Any, No_Fit, Old_Style, Modern, Even_Width, Expanded, Condensed, Very_Expanded, Very_Condensed, Monospaced };
            enum Contrast { Any, No_Fit, None, Very_Low, Low, Medium_Low, Medium, Medium_High, High, Very_High };
            enum Stroke_Variation { Any, No_Fit, No_Variation, Gradual_Diagonal, Gradual_Transitional, Gradual_Vertical, Gradual_Horizontal, Rapid_Vertical, Rapid_Horizontal, Instant_Horizontal, Instant_Vertical };
            enum Arm_Style { Any, No_Fit, Straight_Arms_Horizontal, Straight_Arms_Wedge, Straight_Arms_Vertical, Straight_Arms_Single_Serif, Straight_Arms_Double_Serif, Non_Straight_Arms_Horizontal, Non_Straight_Arms_Wedge, Non_Straight_Arms_Vertical_90, Non_Straight_Arms_Single_Serif, Non_Straight_Arms_Double_Serif };
            enum Letterform { Any, No_Fit, Normal_Contact, Normal_Weighted, Normal_Boxed, Normal_Flattened, Normal_Rounded, Normal_Off_Center, Normal_Square, Oblique_Contact, Oblique_Weighted, Oblique_Boxed, Oblique_Flattened, Oblique_Rounded, Oblique_Off_Center, Oblique_Square };
            enum Midline { Any, No_Fit, Standard_Trimmed, Standard_Pointed, Standard_Serifed, High_Trimmed, High_Pointed, High_Serifed, Constant_Trimmed, Constant_Pointed, Constant_Serifed, Low_Trimmed, Low_Pointed, Low_Serifed };
            enum XHeight { Any, No_Fit, Constant_Small, Constant_Standard, Constant_Large, Ducking_Small, Ducking_Standard, Ducking_Large };
            Family family;
            Serif_Style serifStyle;
            Weight weight;
            Proportion proportion;
            Contrast contrast;
            Stroke_Variation strokeVariation;
            Arm_Style armStyle;
            Letterform letterform;
            Midline midline;
            XHeight xHeight;
            public Panos_1(String SVGPanos1String)
            {
                int tempInt;
                String[] valuesString = SVGPanos1String.Split(' ');
                if (int.TryParse(valuesString[0], out tempInt))
                    family = (Family)tempInt;
                if (int.TryParse(valuesString[1], out tempInt))
                    serifStyle = (Serif_Style)tempInt;
                if (int.TryParse(valuesString[2], out tempInt))
                    weight = (Weight)tempInt;
                if (int.TryParse(valuesString[3], out tempInt))
                    proportion = (Proportion)tempInt;
                if (int.TryParse(valuesString[4], out tempInt))
                    contrast = (Contrast)tempInt;
                if (int.TryParse(valuesString[5], out tempInt))
                    strokeVariation = (Stroke_Variation)tempInt;
                if (int.TryParse(valuesString[6], out tempInt))
                    armStyle = (Arm_Style)tempInt;
                if (int.TryParse(valuesString[7], out tempInt))
                    letterform = (Letterform)tempInt;
                if (int.TryParse(valuesString[8], out tempInt))
                    midline = (Midline)tempInt;
                if (int.TryParse(valuesString[0], out tempInt))
                    xHeight = (XHeight)tempInt;
            }
        }

        String fontId;
        int horiz_adv_x;
        String fontFamily;
        int font_weight;
        String font_stretch;
        int unitsPerEm;
        Panos_1 panose_1;
        int ascent;
        public int Ascent { get { return ascent; } }
        int descent;
        public int Descent { get { return descent; } }
        int x_height;
        public int X_height { get { return x_height; } }
        int cap_height;
        public int Cap_height { get { return cap_height; } }
        RectInt boundingBox;
        public RectInt BoundingBox { get { return boundingBox; } }
        int underline_thickness;
        public int Underline_thickness { get { return underline_thickness; } }
        int underline_position;
        public int Underline_position { get { return underline_position; } }
        String unicode_range;
        FontGlyph missingGlyph;
        //-----------------------------------
        Dictionary<int, FontGlyph> originalGlyphs = new Dictionary<int, FontGlyph>(); // a glyph is indexed by the string it represents, usually one character, but sometimes multiple
        Dictionary<Char, Dictionary<Char, int>> hkerns = new Dictionary<char, Dictionary<char, int>>();
        Dictionary<int, SvgFont> stockFonts = new Dictionary<int, SvgFont>();
        //-----------------------------------



        public int UnitsPerEm
        {
            get
            {
                return unitsPerEm;
            }
        }

        protected override void OnDispose()
        {
        }
        public SvgFont GetFontAtSpecificSize(int emsize)
        {
            SvgFont found;
            if (!stockFonts.TryGetValue(emsize, out found))
            {
                found = new SvgFont(this, emsize);
                stockFonts.Add(emsize, found);
            }
            return found;
        }


        static String GetSubString(String source, String start, String end)
        {
            int startIndex = 0;
            return GetSubString(source, start, end, ref startIndex);
        }

        static String GetSubString(String source, String start, String end, ref int startIndex)
        {
            int startPos = source.IndexOf(start, startIndex);
            if (startPos >= 0)
            {
                int endPos = source.IndexOf(end, startPos + start.Length);
                int length = endPos - (startPos + start.Length);
                startIndex = endPos + end.Length; // advance our start position to the last position used
                return source.Substring(startPos + start.Length, length);
            }

            return null;
        }

        static String GetStringValue(String source, String name)
        {
            String element = GetSubString(source, name + "=\"", "\"");
            return element;
        }

        static bool GetIntValue(String source, String name, out int outValue, ref int startIndex)
        {
            String element = GetSubString(source, name + "=\"", "\"", ref startIndex);
            if (int.TryParse(element, NumberStyles.Number, null, out outValue))
            {
                return true;
            }

            return false;
        }

        static bool GetIntValue(String source, String name, out int outValue)
        {
            int startIndex = 0;
            return GetIntValue(source, name, out outValue, ref startIndex);
        }



        static Regex numberRegex = new Regex(@"[-+]?[0-9]*\.?[0-9]+");
        static double GetNextNumber(string source, ref int startIndex)
        {
            Match numberMatch = numberRegex.Match(source, startIndex);
            String returnString = numberMatch.Value;
            startIndex = numberMatch.Index + numberMatch.Length;
            double returnVal;
            double.TryParse(returnString, NumberStyles.Number, CultureInfo.InvariantCulture, out returnVal);
            return returnVal;
        }

        FontGlyph CreateGlyphFromSVGGlyphData(string svgGlyphData)
        {
            FontGlyph newGlyph = new FontGlyph();
            if (!GetIntValue(svgGlyphData, "horiz-adv-x", out newGlyph.horiz_adv_x))
            {
                newGlyph.horiz_adv_x = horiz_adv_x;
            }

            newGlyph.glyphName = GetStringValue(svgGlyphData, "glyph-name");
            String unicodeString = GetStringValue(svgGlyphData, "unicode");
            if (unicodeString != null)
            {
                if (unicodeString.Length == 1)
                {
                    newGlyph.unicode = (int)unicodeString[0];
                }
                else
                {
                    if (unicodeString.Split(';').Length > 1 && unicodeString.Split(';')[1].Length > 0)
                    {
                        throw new NotImplementedException("We do not currently support glyphs longer than one character.  You need to wirite the seach so that it will find them if you want to support this");
                    }

                    if (int.TryParse(unicodeString, NumberStyles.Number, null, out newGlyph.unicode) == false)
                    {
                        // see if it is a unicode 
                        String hexNumber = GetSubString(unicodeString, "&#x", ";");
                        int.TryParse(hexNumber, NumberStyles.HexNumber, null, out newGlyph.unicode);
                    }
                }
            }

            String dString = GetStringValue(svgGlyphData, "d");
            //if (newGlyph.glyphName == "a")
            //{
            //}
            int parseIndex = 0;
            int polyStartVertexSourceIndex = 0;
            Vector2 lastXY = new Vector2(0, 0);
            double px = 0;
            double py = 0;
            PathWriter gyphPath = new PathWriter();
            newGlyph.originalVxs = gyphPath.Vxs;
            if (dString == null || dString.Length == 0)
            {
                return newGlyph;
            }




            while (parseIndex < dString.Length)
            {
                char command = dString[parseIndex];
                switch (command)
                {
                    case 'M':
                        {
                            parseIndex++;
                            // svg fonts are stored cw and agg expects its shapes to be ccw.  cw shapes are holes.
                            // so we store the position of the start of this polygon so we can flip it when we colse it.
                            polyStartVertexSourceIndex = gyphPath.Count;
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.MoveTo(px, py);
                        }
                        break;
                    case 'v':
                        {
                            parseIndex++;
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.VerticalLineToRel(py);
                        }
                        break;
                    case 'V':
                        {
                            parseIndex++;
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.VerticalLineTo(py);
                        }
                        break;
                    case 'h':
                        {
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            gyphPath.HorizontalLineToRel(px);
                        }
                        break;
                    case 'H':
                        {
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            gyphPath.HorizontalLineTo(px);
                        }
                        break;
                    case 'l':
                        {
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.LineToRel(px, py);
                        }
                        break;
                    case 'L':
                        {
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.LineTo(px, py);
                        }
                        break;
                    case 'q':
                        {
                            //Curve3 
                            parseIndex++;
                            double p2x = GetNextNumber(dString, ref parseIndex);
                            double p2y = GetNextNumber(dString, ref parseIndex);
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.Curve3Rel(p2x, p2y, px, py);
                        }
                        break;
                    case 'Q':
                        {   //Curve3 
                            parseIndex++;
                            double p2x = GetNextNumber(dString, ref parseIndex);
                            double p2y = GetNextNumber(dString, ref parseIndex);
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.Curve3(p2x, p2y, px, py);
                        }
                        break;
                    case 't':
                        {
                            //svg smooth curve3
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.SmoothCurve3Rel(px, py);
                        }
                        break;
                    case 'T':
                        {
                            parseIndex++;
                            px = GetNextNumber(dString, ref parseIndex);
                            py = GetNextNumber(dString, ref parseIndex);
                            gyphPath.SmoothCurve3(px, py);
                        }
                        break;
                    case 'z':
                    case 'Z':
                        {
                            parseIndex++;
                            //curXY = lastXY; // value not used this is to remove an error.
                            //newGlyph.glyphData.ClosePathStorage();
                            gyphPath.CloseFigure();
                            // svg fonts are stored cw and agg expects its shapes to be ccw.  cw shapes are holes.
                            // We stored the position of the start of this polygon, no we flip it as we close it.
                            //newGlyph.glyphData.InvertPolygon(polyStartVertexSourceIndex);
                            // VertexHelper.InvertPolygon(gyphPath.Vxs, polyStartVertexSourceIndex);
                        }
                        break;
                    case ' ':
                    case '\n': // some white space we need to skip
                    case '\r':
                        {
                            parseIndex++;
                        }
                        break;
                    default:
                        throw new NotImplementedException("unrecognized d command '" + command + "'.");
                }
            }

            return newGlyph;
        }

        public void ReadSVG(string svgContent)
        {
            int startIndex = 0;
            String fontElementString = GetSubString(svgContent, "<font", ">", ref startIndex);
            fontId = GetStringValue(fontElementString, "id");
            GetIntValue(fontElementString, "horiz-adv-x", out horiz_adv_x);
            String fontFaceString = GetSubString(svgContent, "<font-face", "/>", ref startIndex);
            fontFamily = GetStringValue(fontFaceString, "font-family");
            GetIntValue(fontFaceString, "font-weight", out font_weight);
            font_stretch = GetStringValue(fontFaceString, "font-stretch");
            GetIntValue(fontFaceString, "units-per-em", out unitsPerEm);
            panose_1 = new Panos_1(GetStringValue(fontFaceString, "panose-1"));
            GetIntValue(fontFaceString, "ascent", out ascent);
            GetIntValue(fontFaceString, "descent", out descent);
            GetIntValue(fontFaceString, "x-height", out x_height);
            GetIntValue(fontFaceString, "cap-height", out cap_height);
            String bboxString = GetStringValue(fontFaceString, "bbox");
            String[] valuesString = bboxString.Split(' ');
            int.TryParse(valuesString[0], out boundingBox.Left);
            int.TryParse(valuesString[1], out boundingBox.Bottom);
            int.TryParse(valuesString[2], out boundingBox.Right);
            int.TryParse(valuesString[3], out boundingBox.Top);
            GetIntValue(fontFaceString, "underline-thickness", out underline_thickness);
            GetIntValue(fontFaceString, "underline-position", out underline_position);
            unicode_range = GetStringValue(fontFaceString, "unicode-range");
            String missingGlyphString = GetSubString(svgContent, "<missing-glyph", "/>", ref startIndex);
            missingGlyph = CreateGlyphFromSVGGlyphData(missingGlyphString);
            String nextGlyphString = GetSubString(svgContent, "<glyph", "/>", ref startIndex);
            while (nextGlyphString != null)
            {
                // get the data and put it in the glyph dictionary

                FontGlyph newGlyph = CreateGlyphFromSVGGlyphData(nextGlyphString);
                if (newGlyph.unicode > 0)
                {
                    originalGlyphs.Add(newGlyph.unicode, newGlyph);
                }

                nextGlyphString = GetSubString(svgContent, "<glyph", "/>", ref startIndex);
            }
        }

        internal FontGlyph GetGlyphForCharacter(char character)
        {
            // TODO: check for multi character glyphs (we don't currently support them in the reader).
            FontGlyph glyph;
            if (!originalGlyphs.TryGetValue(character, out glyph))
            {
                return missingGlyph;
            }
            return glyph;
        }
        internal FontGlyph GetGlyphByIndex(int index)
        {
            // TODO: check for multi character glyphs (we don't currently support them in the reader).
            FontGlyph glyph;
            if (!originalGlyphs.TryGetValue(index, out glyph))
            {
                return missingGlyph;
            }
            return glyph;
        }
        internal int GetAdvanceForCharacter(char character, char nextCharacterToKernWith)
        {
            // TODO: check for kerning and adjust
            FontGlyph glyph;
            if (!originalGlyphs.TryGetValue(character, out glyph))
            {
                return 0;
            }
            return glyph.horiz_adv_x;
        }

        internal int GetAdvanceForCharacter(char character)
        {
            FontGlyph glyph;
            if (originalGlyphs.TryGetValue(character, out glyph))
            {
                return glyph.horiz_adv_x;
            }

            return 0;
        }
#if DEBUG
        //public void dbugShowDebugInfo(Graphics2D graphics2D)
        //{
        //    //StyledTypeFace typeFaceNameStyle = new StyledTypeFace(this, 30);

        //    //TypeFacePrinter fontNamePrinter = new TypeFacePrinter(this.fontFamily + " - 30 point", typeFaceNameStyle);
        //    //TextPrinter printer = new TextPrinter(graphics2D);
        //    //var svgFont = SvgFontStore.LoadFont(SvgFontStore.DEFAULT_SVG_FONTNAME, 30);

        //    //RectD bounds = typeFaceNameStyle.BoundingBoxInPixels;
        //    //double origX = 10 - bounds.Left;
        //    //double x = origX;
        //    //double y = 10 - typeFaceNameStyle.DescentInPixels;
        //    //int width = 50;
        //    //ColorRGBA boundingBoxColor = new ColorRGBA(0, 0, 0);
        //    //ColorRGBA originColor = new ColorRGBA(0, 0, 0);
        //    //ColorRGBA ascentColor = new ColorRGBA(255, 0, 0);
        //    //ColorRGBA descentColor = new ColorRGBA(255, 0, 0);
        //    //ColorRGBA xHeightColor = new ColorRGBA(12, 25, 200);
        //    //ColorRGBA capHeightColor = new ColorRGBA(12, 25, 200);
        //    //ColorRGBA underlineColor = new ColorRGBA(0, 150, 55);

        //    //// the origin
        //    //graphics2D.dbugLine(x, y, x + width, y, originColor);

        //    //graphics2D.Rectangle(x + bounds.Left, y + bounds.Bottom, x + bounds.Right, y + bounds.Top, boundingBoxColor);

        //    //x += typeFaceNameStyle.BoundingBoxInPixels.Width * 1.5;

        //    //width = width * 3;

        //    //double temp = typeFaceNameStyle.AscentInPixels;
        //    //graphics2D.dbugLine(x, y + temp, x + width, y + temp, ascentColor);

        //    //temp = typeFaceNameStyle.DescentInPixels;
        //    //graphics2D.dbugLine(x, y + temp, x + width, y + temp, descentColor);

        //    //temp = typeFaceNameStyle.XHeightInPixels;
        //    //graphics2D.dbugLine(x, y + temp, x + width, y + temp, xHeightColor);

        //    //temp = typeFaceNameStyle.CapHeightInPixels;
        //    //graphics2D.dbugLine(x, y + temp, x + width, y + temp, capHeightColor);

        //    //temp = typeFaceNameStyle.UnderlinePositionInPixels;
        //    //graphics2D.dbugLine(x, y + temp, x + width, y + temp, underlineColor);

        //    //Affine textTransform = Affine.NewMatix(AffinePlan.Translate(10, origX));
        //    ////textTransform = Affine.NewIdentity();
        //    ////textTransform *= Affine.NewTranslation(10, origX);

        //    ////VertexSourceApplyTransform transformedText = new VertexSourceApplyTransform(textTransform);
        //    ////fontNamePrinter.Render(graphics2D, ColorRGBA.Black, transformedText);
        //    ////graphics2D.Render(transformedText, ColorRGBA.Black);

        //    //// render the legend
        //    //StyledTypeFace legendFont = new StyledTypeFace(this, 12);
        //    //double newx = x + width / 2;
        //    //double newy = y + typeFaceNameStyle.EmSizeInPixels * 1.5;

        //    //graphics2D.Render(new TypeFacePrinter("Descent").MakeVertexSnap(), newx, newy, descentColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("Underline").MakeVertexSnap(), newx, newy, underlineColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("X Height").MakeVertexSnap(), newx, newy, xHeightColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("CapHeight").MakeVertexSnap(), newx, newy, capHeightColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("Ascent").MakeVertexSnap(), newx, newy, ascentColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("Origin").MakeVertexSnap(), newx, newy, originColor); newy += legendFont.EmSizeInPixels;
        //    //graphics2D.Render(new TypeFacePrinter("Bounding Box").MakeVertexSnap(), newx, newy, boundingBoxColor);
        //}
#endif
    }
}

