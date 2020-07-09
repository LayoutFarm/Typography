//MIT, 2020-present, WinterDev 
using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.BitmapAtlas;

using Typography.Contours;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Tables;
using Typography.TextLayout;
using Typography.TextBreak;
using Typography.FontManagement;
using Typography.TextServices;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.Drawing
{

#if DEBUG
    //experiment
    public interface IMultiLayerGlyphTranslator : IGlyphTranslator
    {
        void HasColorInfo(int nsubLayer);//if 0 => no color info
        void BeginSubGlyph(ushort glyphIndex);
        void EndSubGlyph(ushort glyphIndex);
        void SetFillColor(byte r, byte g, byte b, byte a);
    }



    static class MultiLayeGlyphTx
    {
        //experiment

        /// <summary>
        /// build a multi-layer glyph (eg. Emoji)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="glyphIndex"></param>
        /// <param name="sizeInPoints"></param>
        /// <param name="tx"></param>
        public static void BuildFromGlyphIndex(this GlyphOutlineBuilderBase builder, ushort glyphIndex, float sizeInPoints, IMultiLayerGlyphTranslator tx)
        {
            //1. current typeface support multilayer or not
            if (builder.HasColorInfo)
            {
                Typeface typeface = builder.Typeface;
                COLR colrTable = typeface.COLRTable;
                CPAL cpalTable = typeface.CPALTable;

                if (colrTable.LayerIndices.TryGetValue(glyphIndex, out ushort colorLayerStart))
                {
                    //has color information on this glyphIndex

                    ushort colorLayerCount = colrTable.LayerCounts[glyphIndex];
                    tx.HasColorInfo(colorLayerCount);


                    for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                    {
                        ushort gIndex = colrTable.GlyphLayers[c];
                        tx.BeginSubGlyph(gIndex);//BEGIN SUB GLYPH

                        int palette = 0; // FIXME: assume palette 0 for now 
                        cpalTable.GetColor(
                            cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                            out byte r, out byte g, out byte b, out byte a);

                        tx.SetFillColor(r, g, b, a); //SET COLOR

                        builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                        builder.ReadShapes(tx);

                        tx.EndSubGlyph(gIndex);//END SUB GLYPH
                    }

                }
                else
                {
                    //build as normal glyph
                    builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                    tx.HasColorInfo(0);
                    builder.ReadShapes(tx);
                }
            }
            else
            {
                //build as normal glyph
                builder.BuildFromGlyphIndex(glyphIndex, sizeInPoints);

                tx.HasColorInfo(0);
                builder.ReadShapes(tx);
            }

        }
    }
#endif
}