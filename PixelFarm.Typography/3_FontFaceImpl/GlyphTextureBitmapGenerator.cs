//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;



using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;
using Typography.Rendering;

namespace PixelFarm.Drawing.Fonts
{
    public class GlyphTextureBuildDetail
    {
        public ScriptLang ScriptLang;
        public char[] OnlySelectedGlyphIndices;
        public HintTechnique HintTechnique;
        public bool DoFilter;
    }

    public class GlyphTextureBitmapGenerator
    {

        public delegate void OnEachFinishTotal(int glyphIndex, GlyphImage glyphImage, SimpleFontAtlasBuilder atlasBuilder);


        public GlyphTextureBitmapGenerator()
        {


        }
        public void CreateTextureFontFromScriptLangs(
            Typeface typeface, float sizeInPoint,
            TextureKind textureKind,
            GlyphTextureBuildDetail[] details,
            OnEachFinishTotal onFinishTotal)
        {
            //-------------------------------------------------------------
            var atlasBuilder = new SimpleFontAtlasBuilder();
            atlasBuilder.SetAtlasInfo(textureKind, sizeInPoint);
            //-------------------------------------------------------------  
            int j = details.Length;
            for (int i = 0; i < j; ++i)
            {
                GlyphTextureBuildDetail detail = details[i];
                if (detail.ScriptLang != null)
                {
                    //skip those script lang=null
                    //2. find associated glyph index base on input script langs
                    List<ushort> outputGlyphIndexList = new List<ushort>();
                    typeface.CollectAllAssociateGlyphIndex(outputGlyphIndexList, detail.ScriptLang);
                    CreateTextureFontFromGlyphIndices(typeface,
                        sizeInPoint,
                        detail.HintTechnique,
                        atlasBuilder,
                        detail.DoFilter,
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
                        detail.DoFilter,
                        detail.OnlySelectedGlyphIndices
                        );
                }
            }
            onFinishTotal(0, null, atlasBuilder);
        }

        public void CreateTextureFontFromInputChars(
            Typeface typeface, float sizeInPoint,
            TextureKind textureKind,
            char[] chars,
            OnEachFinishTotal onFinishTotal)
        {

            //convert input chars into glyphIndex
            List<ushort> glyphIndices = new List<ushort>(chars.Length);
            int i = 0;
            foreach (char ch in chars)
            {
                glyphIndices.Add(typeface.LookupIndex(ch));
                i++;
            }
            //-------------------------------------------------------------
            var atlasBuilder = new SimpleFontAtlasBuilder();
            atlasBuilder.SetAtlasInfo(textureKind, sizeInPoint);
            //------------------------------------------------------------- 
            //we can specfic subset with special setting for each set 
            CreateTextureFontFromGlyphIndices(typeface, sizeInPoint,
                HintTechnique.TrueTypeInstruction_VerticalOnly, atlasBuilder, false, GetUniqueGlyphIndexList(glyphIndices));
            onFinishTotal(0, null, atlasBuilder);
        }
        void CreateTextureFontFromGlyphIndices(
              Typeface typeface,
              float sizeInPoint,
              HintTechnique hintTechnique,
              SimpleFontAtlasBuilder atlasBuilder,
              bool applyFilter,
              char[] chars)
        {
            int j = chars.Length;
            ushort[] glyphIndices = new ushort[j];
            for (int i = 0; i < j; ++i)
            {
                glyphIndices[i] = typeface.LookupIndex(chars[i]);
            }

            CreateTextureFontFromGlyphIndices(typeface, sizeInPoint, hintTechnique, atlasBuilder, applyFilter, glyphIndices);
        }
        void CreateTextureFontFromGlyphIndices(
              Typeface typeface,
              float sizeInPoint,
              HintTechnique hintTechnique,
              SimpleFontAtlasBuilder atlasBuilder,
              bool applyFilter,
              ushort[] glyphIndices)
        {

            //sample: create sample msdf texture 
            //-------------------------------------------------------------
            var builder = new GlyphPathBuilder(typeface);
            builder.SetHintTechnique(hintTechnique);
            //
            if (atlasBuilder.TextureKind == TextureKind.Msdf)
            {
                MsdfGenParams msdfGenParams = new MsdfGenParams();
                int j = glyphIndices.Length;
                for (int i = 0; i < j; ++i)
                {
                    ushort gindex = glyphIndices[i];
                    //create picture with unscaled version set scale=-1
                    //(we will create glyph contours and analyze them)
                    builder.BuildFromGlyphIndex(gindex, -1);
                    var glyphToContour = new GlyphContourBuilder();
                    builder.ReadShapes(glyphToContour);
                    //msdfgen with  scale the glyph to specific shapescale
                    msdfGenParams.shapeScale = 1f / 64; //as original
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, msdfGenParams);
                    //
                    atlasBuilder.AddGlyph(gindex, glyphImg);
                }
            }
            else
            {
                AggGlyphTextureGen aggTextureGen = new AggGlyphTextureGen();
                aggTextureGen.TextureKind = atlasBuilder.TextureKind;
                int j = glyphIndices.Length;
                for (int i = 0; i < j; ++i)
                {
                    //build glyph
                    ushort gindex = glyphIndices[i];
                    builder.BuildFromGlyphIndex(gindex, sizeInPoint);

                    GlyphImage glyphImg = aggTextureGen.CreateGlyphImage(builder, 1);
                    if (applyFilter)
                    {

                        glyphImg = Sharpen(glyphImg, 1);
                        //TODO:
                        //the filter make the image shift x and y 1 px
                        //temp fix with this,
                        glyphImg.TextureOffsetX += 1;
                        glyphImg.TextureOffsetY += 1;
                    }
                    //
                    atlasBuilder.AddGlyph(gindex, glyphImg);
                }
            }
        }
        /// <summary>
        /// test only, shapen org image with Paint.net sharpen filter
        /// </summary>
        /// <param name="org"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        static GlyphImage Sharpen(GlyphImage org, int radius)
        {
            GlyphImage newImg = new GlyphImage(org.Width, org.Height);
            CpuBlit.Imaging.ShapenFilterPdn sharpen1 = new CpuBlit.Imaging.ShapenFilterPdn();
            int[] orgBuffer = org.GetImageBuffer();
            unsafe
            {
                fixed (int* orgHeader = &orgBuffer[0])
                {
                    int[] output = sharpen1.Sharpen(orgHeader, org.Width, org.Height, org.Width * 4, radius);
                    newImg.SetImageBuffer(output, org.IsBigEndian);
                }
            }

            return newImg;
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