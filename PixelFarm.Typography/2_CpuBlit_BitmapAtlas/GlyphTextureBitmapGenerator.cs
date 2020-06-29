//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Contours;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class GlyphTextureBuildDetail
    {
        public ScriptLang ScriptLang;
        public char[] OnlySelectedGlyphIndices;
        public HintTechnique HintTechnique;

        public bool AllGlyphs;
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
                if (detail.AllGlyphs)
                {
#if DEBUG

#endif

                    int count = typeface.GlyphCount;
                    ushort[] glyphIndexList = new ushort[count];
                    for (ushort m = 0; m < count; ++m)
                    {
                        glyphIndexList[m] = m;
                    }
                    CreateTextureFontFromGlyphIndices(typeface,
                       sizeInPoint,
                       detail.HintTechnique,
                       atlasBuilder,
                       glyphIndexList
                       );
                }
                else if (!detail.ScriptLang.IsEmpty())
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
        GlyphBitmap GetGlyphBitmapFromColorOutlineGlyph(Typeface typeface, ushort glyphIndex, GlyphMeshStore glyphMeshStore, ushort colorLayerStart)
        {

            //not found=> create a newone 
            Typography.OpenFont.Tables.COLR _colrTable = typeface.COLRTable;
            Typography.OpenFont.Tables.CPAL _cpalTable = typeface.CPALTable;


            Q1RectD totalBounds = Q1RectD.ZeroIntersection();
            {
                //calculate bounds of this glyph
                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {
                    BoundingRect.GetBoundingRect(glyphMeshStore.GetGlyphMesh(_colrTable.GlyphLayers[c]), ref totalBounds);
                }
            }
            //dbugExportCount++;     
            var memBmp = new MemBitmap((int)Math.Round(totalBounds.Width), (int)Math.Round(totalBounds.Height));//???
            int offset_x = 0;
            int offset_y = 0;
            using (Tools.BorrowAggPainter(memBmp, out AggPainter painter))
            {
                painter.Clear(Color.Transparent);
                painter.SetOrigin(0, 0);

                offset_x = -(int)(totalBounds.Left);
                offset_y = -(int)(totalBounds.Bottom);

                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                int palette = 0; // FIXME: assume palette 0 for now 
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {

                    _cpalTable.GetColor(
                        _cpalTable.Palettes[palette] + _colrTable.GlyphPalettes[c], //index
                         out byte r, out byte g, out byte b, out byte a);

                    ushort gIndex = _colrTable.GlyphLayers[c];
                    VertexStore vxs = glyphMeshStore.GetGlyphMesh(gIndex);
                    using (Tools.BorrowVxs(out var v1))
                    {
                        vxs.TranslateToNewVxs(offset_x, offset_y, v1);
                        painter.FillColor = new Color(r, g, b);//? a component
                        painter.Fill(v1);
                    }
                }
                //find ex
#if DEBUG
                //memBmp.SaveImage("a0x" + (dbugExportCount) + ".png");
#endif
            }

            return new GlyphBitmap
            {
                Bitmap = memBmp,
                Width = memBmp.Width,
                Height = memBmp.Height,
                ImageStartX = -offset_x,//offset back
                ImageStartY = -offset_y //offset back
            };
        }
        GlyphBitmap GetGlyphBitmapFromBitmapFont(Typeface typeface, float sizeInPoint, ushort glyphIndex)
        {
            //not found=> create a new one
            if (typeface.IsBitmapFont)
            {
                //try load
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    //load actual bitmap font
                    Glyph glyph = typeface.GetGlyph(glyphIndex);
                    typeface.ReadBitmapContent(glyph, ms);

                    using (MemBitmap memBitmap = MemBitmapExt.LoadBitmap(ms))
                    {
                        //bitmap that are load may be larger than we need
                        //so we need to scale it to specfic size

                        float bmpScale = typeface.CalculateScaleToPixelFromPointSize(sizeInPoint);
                        float target_advW = typeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;
                        float scaleForBmp = target_advW / memBitmap.Width;

                        MemBitmap scaledMemBmp = memBitmap.ScaleImage(scaleForBmp, scaleForBmp);

                        var glyphBitmap = new GlyphBitmap
                        {
                            Width = scaledMemBmp.Width,
                            Height = scaledMemBmp.Height,
                            Bitmap = scaledMemBmp //**
                        };
                        return glyphBitmap;
                    }
                }
            }
            return null;
        }

        GlyphBitmap GetGlyphBitmapFromSvg(Typeface typeface, float sizeInPoint, ushort glyphIndex)
        {

            //TODO: use string builder from pool?  

            var stbuilder = new System.Text.StringBuilder();
            typeface.ReadSvgContent(glyphIndex, stbuilder);

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
            AggGlyphTextureGen aggTextureGen = new AggGlyphTextureGen();

            //create reusable agg painter*** 
            //assume each glyph size= 2 * line height
            //TODO: review here again...
            int tmpMemBmpHeight = (int)(2 * typeface.CalculateRecommendLineSpacing() * typeface.CalculateScaleToPixelFromPointSize(sizeInPoint));

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
                    //use gen3
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

                GlyphMeshStore glyphMeshStore = new GlyphMeshStore();
                glyphMeshStore.SetFont(typeface, sizeInPoint);
                aggTextureGen.TextureKind = TextureKind.Bitmap;

                using (PixelFarm.CpuBlit.MemBitmap tmpMemBmp = new PixelFarm.CpuBlit.MemBitmap(tmpMemBmpHeight, tmpMemBmpHeight)) //square
                {
                    aggTextureGen.Painter = PixelFarm.CpuBlit.AggPainter.Create(tmpMemBmp);
#if DEBUG
                    tmpMemBmp._dbugNote = "CreateGlyphImage()";
#endif


                    void HandleNotFoundGlyph(ushort glyphIndex, OnEachGlyph onEachDel)
                    {

#if DEBUG
                        System.Diagnostics.Debug.WriteLine("bitmap texture,create fallback glyph:" + glyphIndex);
#endif

                        //NESTED
                        //draw a glyph into tmpMemBmp and then copy to a GlyphImage      
                        //build glyph 
                        outlineBuilder.BuildFromGlyphIndex(glyphIndex, sizeInPoint);
                        BitmapAtlasItemSource glyphImg = aggTextureGen.CreateAtlasItem(outlineBuilder, 1);
                        glyphImg.UniqueInt16Name = glyphIndex;
                        onEachDel?.Invoke(glyphImg);
                        atlasBuilder.AddItemSource(glyphImg);
                    }


                    if (typeface.HasColorTable())
                    {
                        //outline glyph

                        for (int i = 0; i < j; ++i)
                        {
                            ushort gindex = glyphIndices[i];
                            if (!typeface.COLRTable.LayerIndices.TryGetValue(gindex, out ushort colorLayerStart))
                            {
                                //not found, then render as normal
                                //TODO: impl   

                                //create glyph img    
                                HandleNotFoundGlyph(gindex, _onEachGlyphDel);
                            }
                            else
                            {
                                //TODO: review this again
                                GlyphBitmap glyphBmp = GetGlyphBitmapFromColorOutlineGlyph(typeface, gindex, glyphMeshStore, colorLayerStart);
                                if (glyphBmp == null)
                                {
                                    HandleNotFoundGlyph(gindex, _onEachGlyphDel);
                                }
                                else
                                {
                                    int w = glyphBmp.Width;
                                    int h = glyphBmp.Height;

                                    BitmapAtlasItemSource glyphImage = new BitmapAtlasItemSource(glyphBmp.Width, glyphBmp.Height);

                                    glyphImage.TextureXOffset = (short)glyphBmp.ImageStartX;
                                    glyphImage.TextureYOffset = (short)glyphBmp.ImageStartY;

                                    //
                                    glyphImage.SetImageBuffer(MemBitmapExt.CopyImgBuffer(glyphBmp.Bitmap, w, h, true), false);

                                    glyphImage.UniqueInt16Name = gindex;
                                    _onEachGlyphDel?.Invoke(glyphImage);
                                    atlasBuilder.AddItemSource(glyphImage);

                                    //clear
                                    glyphBmp.Bitmap.Dispose();
                                    glyphBmp.Bitmap = null;
                                }
                            }
                        }


                    }
                    else if (typeface.IsBitmapFont)
                    {
                        aggTextureGen.TextureKind = TextureKind.Bitmap;
                        //test this with Noto Color Emoji
                        for (int i = 0; i < j; ++i)
                        {
                            ushort gindex = glyphIndices[i];

                            GlyphBitmap glyphBmp = GetGlyphBitmapFromBitmapFont(typeface, sizeInPoint, gindex);
                            if (glyphBmp == null)
                            {
                                HandleNotFoundGlyph(gindex, _onEachGlyphDel);
                            }
                            else
                            {
                                int w = glyphBmp.Width;
                                int h = glyphBmp.Height;

                                BitmapAtlasItemSource glyphImage = new BitmapAtlasItemSource(glyphBmp.Width, glyphBmp.Height);

                                glyphImage.TextureXOffset = (short)glyphBmp.ImageStartX;
                                glyphImage.TextureYOffset = (short)glyphBmp.ImageStartY;

                                //
                                glyphImage.SetImageBuffer(MemBitmapExt.CopyImgBuffer(glyphBmp.Bitmap, w, h, true), false);

                                glyphImage.UniqueInt16Name = gindex;
                                _onEachGlyphDel?.Invoke(glyphImage);
                                atlasBuilder.AddItemSource(glyphImage);

                                //clear
                                glyphBmp.Bitmap.Dispose();
                                glyphBmp.Bitmap = null;
                            }
                        }

                    }
                    else if (typeface.HasSvgTable())
                    {
                        aggTextureGen.TextureKind = TextureKind.Bitmap;
                        //test this with TwitterEmoji
                        //generate membitmap from svg
                        for (int i = 0; i < j; ++i)
                        {
                            ushort gindex = glyphIndices[i];
                            GlyphBitmap glyphBmp = GetGlyphBitmapFromSvg(typeface, sizeInPoint, gindex);
                            if (glyphBmp == null)
                            {
                                HandleNotFoundGlyph(gindex, _onEachGlyphDel);
                            }
                            else
                            {
                                int w = glyphBmp.Width;
                                int h = glyphBmp.Height;

                                BitmapAtlasItemSource glyphImage = new BitmapAtlasItemSource(glyphBmp.Width, glyphBmp.Height);

                                glyphImage.TextureXOffset = (short)glyphBmp.ImageStartX;
                                glyphImage.TextureYOffset = (short)glyphBmp.ImageStartY;

                                //
                                glyphImage.SetImageBuffer(MemBitmapExt.CopyImgBuffer(glyphBmp.Bitmap, w, h, true), false);

                                glyphImage.UniqueInt16Name = gindex;
                                _onEachGlyphDel?.Invoke(glyphImage);
                                atlasBuilder.AddItemSource(glyphImage);

                                //clear
                                glyphBmp.Bitmap.Dispose();
                                glyphBmp.Bitmap = null;
                            }
                        }

                    }
                    return; //NO go below //***
                } //END using
            }


            //---------------------------
            //OTHERS....
            {
                aggTextureGen.TextureKind = atlasBuilder.TextureKind;
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