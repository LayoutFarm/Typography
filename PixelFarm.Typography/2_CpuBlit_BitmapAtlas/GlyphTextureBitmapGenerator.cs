//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Contours;
using System.IO;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class GlyphTextureBuildDetail
    {
        public ScriptLang ScriptLang;
        public char[] OnlySelectedGlyphIndices;
        public HintTechnique HintTechnique;

    }

    public struct GlyphTextureBitmapGenerator
    {
        public delegate void OnEachGlyph(BitmapAtlasItemSource glyphImage);

        /// <summary>
        /// version of msdf generator
        /// </summary>
        public ushort MsdfGenVersion { get; set; }

        PixelFarm.Drawing.SvgBmpBuilderFunc _svgBmpBuilderFunc;

        public void SetSvgBmpBuilderFunc(PixelFarm.Drawing.SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _svgBmpBuilderFunc = svgBmpBuilderFunc;
        }


        OnEachGlyph _onEachGlyphDel;
        public SimpleBitmapAtlasBuilder CreateTextureFontFromBuildDetail(
            Typeface typeface, float sizeInPoint,
            TextureKind textureKind,
            GlyphTextureBuildDetail[] details,
            OnEachGlyph onEachGlyphDel = null)
        {
            _onEachGlyphDel = onEachGlyphDel;
            //-------------------------------------------------------------
            var atlasBuilder = new SimpleBitmapAtlasBuilder();

            atlasBuilder.SetAtlasInfo(textureKind, sizeInPoint);
            //-------------------------------------------------------------  
            int j = details.Length;
            for (int i = 0; i < j; ++i)
            {
                GlyphTextureBuildDetail detail = details[i];
                if (!detail.ScriptLang.IsEmpty())
                {
                    //skip those script lang=null
                    //2. find associated glyph index base on input script langs
                    List<ushort> outputGlyphIndexList = new List<ushort>();
                    typeface.CollectAllAssociateGlyphIndex(outputGlyphIndexList, detail.ScriptLang);
                    CreateTextureFontFromGlyphIndices(typeface,
                        sizeInPoint,
                        detail.HintTechnique,
                        atlasBuilder,
                        GetUniqueGlyphIndexList(outputGlyphIndexList)
                        );
                }
            }
            for (int i = 0; i < j; ++i)
            {
                GlyphTextureBuildDetail detail = details[i];
                if (detail.OnlySelectedGlyphIndices != null)
                {
                    //skip those script lang=null
                    //2. find associated glyph index base on input script langs

                    CreateTextureFontFromGlyphIndices(typeface,
                        sizeInPoint,
                        detail.HintTechnique,
                        atlasBuilder,
                        detail.OnlySelectedGlyphIndices
                        );
                }
            }

            _onEachGlyphDel = null;//reset
            return atlasBuilder;
        }

        public SimpleBitmapAtlasBuilder CreateTextureFontFromInputChars(
            Typeface typeface, float sizeInPoint,
            TextureKind textureKind,
            char[] chars,
            OnEachGlyph onEachGlyphDel = null)
        {

            _onEachGlyphDel = onEachGlyphDel;
            //convert input chars into glyphIndex
            List<ushort> glyphIndices = new List<ushort>(chars.Length);
            int i = 0;
            foreach (char ch in chars)
            {
                glyphIndices.Add(typeface.GetGlyphIndex(ch));
                i++;
            }
            //-------------------------------------------------------------
            var atlasBuilder = new SimpleBitmapAtlasBuilder();
            atlasBuilder.SetAtlasInfo(textureKind, sizeInPoint);
            //------------------------------------------------------------- 
            //we can specfic subset with special setting for each set 
            CreateTextureFontFromGlyphIndices(typeface, sizeInPoint,
                HintTechnique.TrueTypeInstruction_VerticalOnly, atlasBuilder, GetUniqueGlyphIndexList(glyphIndices));

            _onEachGlyphDel = null;//reset                
            return atlasBuilder;
        }
        void CreateTextureFontFromGlyphIndices(
              Typeface typeface,
              float sizeInPoint,
              HintTechnique hintTechnique,
              SimpleBitmapAtlasBuilder atlasBuilder,
              char[] chars)
        {
            int j = chars.Length;
            ushort[] glyphIndices = new ushort[j];
            for (int i = 0; i < j; ++i)
            {
                glyphIndices[i] = typeface.GetGlyphIndex(chars[i]);
            }

            CreateTextureFontFromGlyphIndices(typeface, sizeInPoint, hintTechnique, atlasBuilder, glyphIndices);
        }

        GlyphBitmap GetGlyphBitmapFromSvg(Typeface typeface, float sizeInPoint, ushort glyphIndex)
        {

            //TODO: use string builder from pool? 
            Glyph glyph = typeface.GetGlyph(glyphIndex);

            var stbuilder = new System.Text.StringBuilder();
            typeface.ReadSvgContent(glyph, stbuilder);

            float bmpScale = typeface.CalculateScaleToPixelFromPointSize(sizeInPoint);
            float target_advW = typeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;

            var req = new PixelFarm.Drawing.SvgBmpBuilderReq
            {
                SvgContent = stbuilder,
                DefaultBgColor = Color.Transparent,
                ExpectedWidth = target_advW
            };

            _svgBmpBuilderFunc.Invoke(req);

            MemBitmap memBmp = req.Output;

            if (memBmp == null)
            {
                //TODO: use blank img?
                return null;
            }

            //TODO... review this again 
            short offset_x = (short)req.BitmapXOffset;
            short offset_y = (short)req.BitmapYOffset;

            return new GlyphBitmap
            {
                Bitmap = memBmp,
                Width = memBmp.Width,
                Height = memBmp.Height,
                ImageStartX = -offset_x,//offset back
                ImageStartY = -offset_y //offset back
            };
        }
        void CreateTextureFontFromGlyphIndices(
              Typeface typeface,
              float sizeInPoint,
              HintTechnique hintTechnique,
              SimpleBitmapAtlasBuilder atlasBuilder,
              ushort[] glyphIndices)
        {

            //sample: create sample msdf texture 
            //-------------------------------------------------------------
            var outlineBuilder = new GlyphOutlineBuilder(typeface);
            outlineBuilder.SetHintTechnique(hintTechnique);
            //
            if (atlasBuilder.TextureKind == TextureKind.Msdf)
            {

                float pxscale = typeface.CalculateScaleToPixelFromPointSize(sizeInPoint);
                var msdfGenParams = new Msdfgen.MsdfGenParams();
                int j = glyphIndices.Length;

                if (MsdfGenVersion == 3)
                {
                    Msdfgen.MsdfGen3 gen3 = new Msdfgen.MsdfGen3();

                    for (int i = 0; i < j; ++i)
                    {
                        ushort gindex = glyphIndices[i];
                        //create picture with unscaled version set scale=-1
                        //(we will create glyph contours and analyze them)

                        var glyphToVxs = new GlyphTranslatorToVxs();
                        outlineBuilder.BuildFromGlyphIndex(gindex, -1, glyphToVxs);

                        using (Tools.BorrowVxs(out var vxs))
                        {
                            glyphToVxs.WriteUnFlattenOutput(vxs, pxscale);
                            BitmapAtlasItemSource glyphImg = gen3.GenerateMsdfTexture(vxs);
                            glyphImg.UniqueInt16Name = gindex;
                            _onEachGlyphDel?.Invoke(glyphImg);
                            //

                            atlasBuilder.AddItemSource(glyphImg);
                        }

                    }
                }
                else
                {

                    Msdfgen.MsdfGen3 gen3 = new Msdfgen.MsdfGen3();
                    for (int i = 0; i < j; ++i)
                    {
                        ushort gindex = glyphIndices[i];
                        //create picture with unscaled version set scale=-1
                        //(we will create glyph contours and analyze them)

                        var glyphToVxs = new GlyphTranslatorToVxs();
                        outlineBuilder.BuildFromGlyphIndex(gindex, -1, glyphToVxs);

                        using (Tools.BorrowVxs(out var vxs))
                        {
                            glyphToVxs.WriteUnFlattenOutput(vxs, pxscale);
                            BitmapAtlasItemSource glyphImg = gen3.GenerateMsdfTexture(vxs);
                            glyphImg.UniqueInt16Name = gindex;
                            _onEachGlyphDel?.Invoke(glyphImg);

                            atlasBuilder.AddItemSource(glyphImg);
                        }
                    }
                }
                return;
            }
            else if (atlasBuilder.TextureKind == TextureKind.Bitmap)
            {
                //generate color bitmap atlas
                int j = glyphIndices.Length;
                if (typeface.HasColorTable())
                {
                    //outline glyph 

                    return;
                }
                else if (typeface.IsBitmapFont)
                {
                    //test this with Noto Color Emoji

                    return;
                }
                else if (typeface.HasSvgTable())
                {
                    //test this with TwitterEmoji
                    //generate membitmap from svg
                    for (int i = 0; i < j; ++i)
                    {
                        ushort gindex = glyphIndices[i];
                        GlyphBitmap glyphBmp = GetGlyphBitmapFromSvg(typeface, sizeInPoint, gindex);
                        if (glyphBmp == null)
                        {
                            //use empty glyph?
                            //temp fix
                            BitmapAtlasItemSource glyphImage = new BitmapAtlasItemSource(16, 16);
                            using (MemBitmap empty = new MemBitmap(16, 16))
                            {
                                empty.Clear(PixelFarm.Drawing.Color.Transparent);
                                glyphImage.SetImageBuffer(MemBitmapExtensions.CopyImgBuffer(empty, empty.Width, empty.Height), false);

                            }

                            glyphImage.UniqueInt16Name = gindex;
                            _onEachGlyphDel?.Invoke(glyphImage);
                            atlasBuilder.AddItemSource(glyphImage);
                        }
                        else
                        {
                            int w = glyphBmp.Width;
                            int h = glyphBmp.Height;

                            BitmapAtlasItemSource glyphImage = new BitmapAtlasItemSource(glyphBmp.Width, glyphBmp.Height);

                            glyphImage.TextureXOffset = (short)glyphBmp.ImageStartX;
                            glyphImage.TextureYOffset = (short)glyphBmp.ImageStartY;

                            //
                            glyphImage.SetImageBuffer(MemBitmapExtensions.CopyImgBuffer(glyphBmp.Bitmap, w, h, true), false);

                            glyphImage.UniqueInt16Name = gindex;
                            _onEachGlyphDel?.Invoke(glyphImage);
                            atlasBuilder.AddItemSource(glyphImage);

                            //clear
                            glyphBmp.Bitmap.Dispose();
                            glyphBmp.Bitmap = null;
                        }

                    }

                    return;
                }
            }



            //---------------------------
            //OTHERS....
            {
                AggGlyphTextureGen aggTextureGen = new AggGlyphTextureGen();
                aggTextureGen.TextureKind = atlasBuilder.TextureKind;
                //create reusable agg painter***

                //assume each glyph size= 2 * line height
                //TODO: review here again...
                int tmpMemBmpHeight = (int)(2 * typeface.CalculateRecommendLineSpacing() * typeface.CalculateScaleToPixelFromPointSize(sizeInPoint));
                //create glyph img    
                using (PixelFarm.CpuBlit.MemBitmap tmpMemBmp = new PixelFarm.CpuBlit.MemBitmap(tmpMemBmpHeight, tmpMemBmpHeight)) //square
                {
                    //draw a glyph into tmpMemBmp and then copy to a GlyphImage                     
                    aggTextureGen.Painter = PixelFarm.CpuBlit.AggPainter.Create(tmpMemBmp);
#if DEBUG
                    tmpMemBmp._dbugNote = "CreateGlyphImage()";
#endif

                    int j = glyphIndices.Length;
                    for (int i = 0; i < j; ++i)
                    {
                        //build glyph
                        ushort gindex = glyphIndices[i];
                        outlineBuilder.BuildFromGlyphIndex(gindex, sizeInPoint);

                        BitmapAtlasItemSource glyphImg = aggTextureGen.CreateAtlasItem(outlineBuilder, 1);

                        glyphImg.UniqueInt16Name = gindex;
                        _onEachGlyphDel?.Invoke(glyphImg);
                        atlasBuilder.AddItemSource(glyphImg);
                    }
                }
            }
        }

        static ushort[] GetUniqueGlyphIndexList(List<ushort> inputGlyphIndexList)
        {
            Dictionary<ushort, bool> uniqueGlyphIndices = new Dictionary<ushort, bool>(inputGlyphIndexList.Count);
            foreach (ushort glyphIndex in inputGlyphIndexList)
            {
                if (!uniqueGlyphIndices.ContainsKey(glyphIndex))
                {
                    uniqueGlyphIndices.Add(glyphIndex, true);
                }
            }
            //
            ushort[] uniqueGlyphIndexArray = new ushort[uniqueGlyphIndices.Count];
            int i = 0;
            foreach (ushort glyphIndex in uniqueGlyphIndices.Keys)
            {
                uniqueGlyphIndexArray[i] = glyphIndex;
                i++;
            }
            return uniqueGlyphIndexArray;
        }
    }
}