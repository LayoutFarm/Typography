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
// Class StringPrinter.cs
// 
// Class to output the vertex source of a string as a run of glyphs.
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;
using PixelFarm.Agg;
namespace PixelFarm.Drawing.Fonts
{
    class MyTypeFacePrinter
    {
        Vector2 totalSizeCach;
        string textToPrint;
        Font currentFont;
        IFonts ifonts;
        public MyTypeFacePrinter(GraphicsPlatform gx)
        {
            this.ifonts = gx.Fonts;
            this.Baseline = Baseline.Text;
            this.Justification = Justification.Left;
        }
        public PixelFarm.Drawing.Font CurrentFont
        {
            get { return this.currentFont; }
            set
            {
                this.currentFont = value;
            }
        }
        public Justification Justification { get; set; }
        public Baseline Baseline { get; set; }
        public bool DrawFromHintedCache { get; set; }


        public VertexStore MakeVxs()
        {
            return VertexStoreBuilder.CreateVxs(this.GetVertexIter(textToPrint));
        }
        public VertexStoreSnap MakeVertexSnap()
        {
            return new VertexStoreSnap(this.MakeVxs());
        }

        public void LoadText(string textToPrint)
        {
            this.textToPrint = textToPrint;
        }
        IEnumerable<VertexData> GetVertexIter(string text)
        {
            if (text != null && text.Length > 0)
            {

                Vector2 currentOffset = new Vector2(0, 0);
                ActualFont font = ifonts.ResolveActualFont(currentFont);
                currentOffset = GetBaseline(currentOffset);
                string[] lines = text.Split('\n');
                foreach (string line in lines)
                {
                    currentOffset = GetXPositionForLineBasedOnJustification(currentOffset, line);
                    for (int currentChar = 0; currentChar < line.Length; currentChar++)
                    {
                        var currentGlyph = font.GetGlyph(line[currentChar]);
                        if (currentGlyph != null)
                        {
                            //use flatten ?
                            var glyphVxs = currentGlyph.flattenVxs;
                            int j = glyphVxs.Count;
                            for (int i = 0; i < j; ++i)
                            {
                                double x, y;
                                var cmd = glyphVxs.GetVertex(i, out x, out y);
                                if (cmd != VertexCmd.Stop)
                                {
                                    yield return new VertexData(cmd,
                                        (x + currentOffset.x),
                                        (y + currentOffset.y));
                                }
                            }
                        }

                        // get the advance for the next character
                        if (currentChar < line.Length - 1)
                        {
                            // pass the next char so the typeFaceStyle can do kerning if it needs to.
                            currentOffset.x += font.GetAdvanceForCharacter(line[currentChar], line[currentChar + 1]);
                        }
                        else
                        {
                            currentOffset.x += font.GetAdvanceForCharacter(line[currentChar]);
                        }
                    }

                    // before we go onto the next line we need to move down a line
                    currentOffset.x = 0;
                    currentOffset.y -= font.EmSizeInPixels;
                }
            }
            yield return new VertexData(VertexCmd.Stop);
        }

        private Vector2 GetXPositionForLineBasedOnJustification(Vector2 currentOffset, string line)
        {
            Vector2 size = GetSize(line);
            switch (Justification)
            {
                case Justification.Left:
                    currentOffset.x = 0;
                    break;
                case Justification.Center:
                    currentOffset.x = -size.x / 2;
                    break;
                case Justification.Right:
                    currentOffset.x = -size.x;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return currentOffset;
        }

        Vector2 GetBaseline(Vector2 currentOffset)
        {
            switch (Baseline)
            {
                case Baseline.Text:
                    currentOffset.y = 0;
                    break;
                case Baseline.BoundsTop:
                    {

                        ActualFont font = ifonts.ResolveActualFont(currentFont);
                        currentOffset.y = -font.AscentInPixels;
                    }
                    break;
                case Baseline.BoundsCenter:
                    {
                        ActualFont font = ifonts.ResolveActualFont(currentFont);
                        currentOffset.y = -font.AscentInPixels / 2;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
            return currentOffset;
        }

        public Vector2 GetSize(string text)
        {
            if (text == null)
            {
                text = this.textToPrint;
            }

            if (text != this.textToPrint)
            {
                Vector2 calculatedSize;
                GetSize(0, Math.Max(0, text.Length - 1), out calculatedSize, text);
                return calculatedSize;
            }

            if (totalSizeCach.x == 0)
            {
                Vector2 calculatedSize;
                GetSize(0, Math.Max(0, text.Length - 1), out calculatedSize, text);
                totalSizeCach = calculatedSize;
            }

            return totalSizeCach;
        }

        public void GetSize(int characterToMeasureStartIndexInclusive,
            int characterToMeasureEndIndexInclusive,
            out Vector2 offset,
            string text)
        {
            if (text == null)
            {
                text = this.textToPrint;
            }

            ActualFont implFont = ifonts.ResolveActualFont(currentFont);

            offset.x = 0;
            offset.y = implFont.EmSizeInPixels;
            double currentLineX = 0;
            for (int i = characterToMeasureStartIndexInclusive; i < characterToMeasureEndIndexInclusive; i++)
            {
                if (text[i] == '\n')
                {
                    if (i + 1 < characterToMeasureEndIndexInclusive && (text[i + 1] == '\n') && text[i] != text[i + 1])
                    {
                        i++;
                    }
                    currentLineX = 0;
                    offset.y += implFont.EmSizeInPixels;
                }
                else
                {
                    if (i + 1 < text.Length)
                    {
                        //some font has kerning ...
                        currentLineX += implFont.GetAdvanceForCharacter(text[i], text[i + 1]);
                    }
                    else
                    {
                        currentLineX += implFont.GetAdvanceForCharacter(text[i]);
                    }
                    if (currentLineX > offset.x)
                    {
                        offset.x = currentLineX;
                    }
                }
            }

            if (text.Length > characterToMeasureEndIndexInclusive)
            {
                if (text[characterToMeasureEndIndexInclusive] == '\n')
                {
                    currentLineX = 0;
                    offset.y += implFont.EmSizeInPixels;
                }
                else
                {
                    offset.x += implFont.GetAdvanceForCharacter(text[characterToMeasureEndIndexInclusive]);
                }
            }
        }
    }
}
