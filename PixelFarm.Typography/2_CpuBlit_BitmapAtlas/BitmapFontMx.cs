//MIT, 2016-present, WinterDev 

using System;
using System.Collections.Generic;
using System.IO;
using PixelFarm.Drawing;
using PixelFarm.Platforms;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Text;

namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public static class GlyphTextureCustomConfigs
    {
        //some font need special treatments...

        static Dictionary<int, GlyphTextureBuildDetail[]> s_registeredDetails;

        static GlyphTextureBuildDetail[] s_default;

        static void SetupDefaults()
        {

            //Your Implementation ...
            s_registeredDetails = new Dictionary<int, GlyphTextureBuildDetail[]>();
            //set default detail
            if (s_default == null)
            {
                //TODO
                SetDefaultDetails(new GlyphTextureBuildDetail[] {
                    new GlyphTextureBuildDetail{
                        ScriptLang= new ScriptLang(ScriptTagDefs.Latin.Tag),
                        UnicodeRanges =new UnicodeRangeInfo[]{
                            Typography.TextBreak.Unicode13RangeInfoList.C0_Controls_and_Basic_Latin,
                            Typography.TextBreak.Unicode13RangeInfoList.C1_Controls_and_Latin_1_Supplement,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_A,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_Additional,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_B,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_C,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_D,
                            Typography.TextBreak.Unicode13RangeInfoList.Latin_Extended_E,
                        },
                        HintTechnique = Typography.OpenFont.Contours.HintTechnique.TrueTypeInstruction_VerticalOnly },

                    new GlyphTextureBuildDetail{
                        ScriptLang= new ScriptLang(ScriptTagDefs.Thai.Tag),
                        UnicodeRanges =new UnicodeRangeInfo[]{
                            Typography.TextBreak.Unicode13RangeInfoList.Thai
                        },
                        HintTechnique = Typography.OpenFont.Contours.HintTechnique.None},
                }); ;
            }
            //


        }
        public static void SetDefaultDetails(GlyphTextureBuildDetail[] defaultDetails)
        {
            s_default = defaultDetails;
        }

        public static GlyphTextureBuildDetail[] TryGetGlyphTextureBuildDetail(ResolvedFont font, bool forAnySize = true, bool forAnyStyle = true)
        {

            if (s_registeredDetails == null)
            {
                SetupDefaults();
            }

            //TODO:
            //bitmap atlas may contains some unicode ranges
            //=> we need to check which atlas is support our request unicode range too

            if (!s_registeredDetails.TryGetValue(font.RuntimeResolvedKey, out GlyphTextureBuildDetail[] found))
            {
                //find general config

                if (forAnySize == false || forAnySize == false)
                {
                    //get general style
                    return TryGetGlyphTextureBuildDetail(font, true, true);
                }

                //not found that font key
                //create default
                //...
                return s_default;
            }
            return found;
        }


    }


    class TypefaceBitmapAtlasCollection
    {
        //a single typeface may contains 
        //many atlases
        readonly List<SimpleBitmapAtlas> _list = new List<SimpleBitmapAtlas>();
        public void AddBitmapAtlas(SimpleBitmapAtlas atlas) => _list.Add(atlas);
        public SimpleBitmapAtlas this[int index] => _list[index];
        public int Count => _list.Count;

        public SimpleBitmapAtlas GetAtlas(float sizeInPoints)
        {
            int count = _list.Count;
            for (int i = 0; i < count; ++i)
            {
                if (_list[i].SizeInPts == sizeInPoints)
                {
                    return _list[i];
                }
            }
            //not found
            return null;
        }
    }

    //special
    public class BitmapFontManager<B> : BitmapAtlasManager<B> where B : IDisposable
    {

        readonly Dictionary<string, TypefaceBitmapAtlasCollection> _bitmapTextures = new Dictionary<string, TypefaceBitmapAtlasCollection>();

        //access by hash
        readonly Dictionary<int, SimpleBitmapAtlas> _resolvedAtlas = new Dictionary<int, SimpleBitmapAtlas>();
        readonly Dictionary<string, SimpleBitmapAtlas> _msdfTextureFonts = new Dictionary<string, SimpleBitmapAtlas>();

        readonly OpenFontTextService _textServices;

        public BitmapFontManager(
            OpenFontTextService textservice,
            LoadNewBmpDelegate<SimpleBitmapAtlas, B> _createNewDel)
            : base(_createNewDel)
        {
            _textServices = textservice;
        }

        public BitmapFontManager(OpenFontTextService textservice)
        {
            _textServices = textservice;
        }

        public TextureKind TextureKindForNewFont { get; set; }

        SvgBmpBuilderFunc _svgBmpBuilderFunc;
        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _svgBmpBuilderFunc = svgBmpBuilderFunc;
        }


#if DEBUG
        System.Diagnostics.Stopwatch _dbugStopWatch = new System.Diagnostics.Stopwatch();
#endif
        public void LoadBitmapAtlasPreview(Stream textureInfoDataStream)
        {
            SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();
            lock (s_loadDataLock)
            {
                try
                {
                    List<SimpleBitmapAtlas> atlases = atlasBuilder.LoadAtlasInfo(textureInfoDataStream);
                    int j = atlases.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        AddSimpleFontAtlas(atlases[i]);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        void AddSimpleFontAtlas(SimpleBitmapAtlas fontAtlas)
        {
            string fontname = fontAtlas.FontName?.ToUpper() ?? "";
            if (!_bitmapTextures.TryGetValue(fontname, out TypefaceBitmapAtlasCollection collection))
            {
                _bitmapTextures.Add(fontname, collection = new TypefaceBitmapAtlasCollection());
            }
            collection.AddBitmapAtlas(fontAtlas);

            if (fontAtlas.TextureKind == TextureKind.Msdf)
            {
                //if we have msdf texture
                //then we can use this to do autoscale
                _msdfTextureFonts.Add(fontAtlas.FontName, fontAtlas);
            }

        }
        public void AddSimpleFontAtlas(SimpleBitmapAtlas[] simpleFontAtlases, System.IO.Stream totalGlyphImgStream)
        {
            //multiple font atlas that share the same glyphImg

            MemBitmap mainBmp = ReadGlyphImages(totalGlyphImgStream);

            for (int i = 0; i < simpleFontAtlases.Length; ++i)
            {
                SimpleBitmapAtlas simpleFontAtlas = simpleFontAtlases[i];
                simpleFontAtlas.SetMainBitmap(mainBmp, true);
                simpleFontAtlas.UseSharedImage = true;

                AddSimpleFontAtlas(simpleFontAtlas);
            }
        }


        static readonly object s_loadDataLock = new object();
        //
        static int CalculateHashCode(int resolvedFontKey, int startCodepoint, int endCodepoint)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + resolvedFontKey;
                hash = hash * 31 + startCodepoint;
                hash = hash * 31 + endCodepoint;
                return hash;
            }
        }
        /// <summary>
        /// get from cache or create a new one
        /// </summary>
        /// <param name="font"></param>
        /// <param name="brkInfo"></param>
        /// <param name="outputBitmap"></param>
        /// <returns></returns>
        public SimpleBitmapAtlas GetFontAtlas(ResolvedFont font, int startCodepoint, int endCodepoint, out B outputBitmap)
        {

#if DEBUG
            _dbugStopWatch.Reset();
            _dbugStopWatch.Start();
#endif

            //TODO:
            //bitmap atlas may contains some unicode ranges

            int hashCode = CalculateHashCode(font.RuntimeResolvedKey, startCodepoint, endCodepoint);

            if (_resolvedAtlas.TryGetValue(hashCode, out SimpleBitmapAtlas fontAtlas))
            {
                outputBitmap = _bitmapCache.GetOrCreateNewOne(fontAtlas);
                return fontAtlas;
            }


            //check if we have small msdf texture or not

            string font_name = font.Name.ToUpper();

            if (_msdfTextureFonts.TryGetValue(font_name, out SimpleBitmapAtlas msdfTexture))
            {
                //use this
                outputBitmap = _bitmapCache.GetOrCreateNewOne(msdfTexture);
                return msdfTexture;
            }

            if (_bitmapTextures.TryGetValue(font_name, out TypefaceBitmapAtlasCollection bitmapAtlasCollection))
            {
                fontAtlas = bitmapAtlasCollection.GetAtlas(font.SizeInPoints);
                if (fontAtlas != null)
                {
                    if (fontAtlas.MainBitmap == null)
                    {
                        int fontKey = font.RuntimeResolvedKey;
                        Typeface typeface = font.Typeface;
                        string fontTextureFile = typeface.Name + "_" + fontKey;
                        string fontTextureInfoFile = fontTextureFile + ".tx_info";
                        string fontTextureImgFilename = fontTextureInfoFile + ".png";

                        fontAtlas.SetMainBitmap(MemBitmapExt.LoadBitmap(fontTextureImgFilename), true);
                    }
                    outputBitmap = _bitmapCache.GetOrCreateNewOne(fontAtlas);

                    _resolvedAtlas.Add(hashCode, fontAtlas);

                    return fontAtlas;
                }
            }

            //-------------
            //if not found the request font
            //we generate it realtime here, (add add the cache '_createdAtlases') 
            //------------- 

            fontAtlas = CreateBitmapAtlas(font, startCodepoint, endCodepoint, hashCode, out outputBitmap);
            if (bitmapAtlasCollection == null)
            {
                bitmapAtlasCollection = new TypefaceBitmapAtlasCollection();
                _bitmapTextures.Add(font_name, bitmapAtlasCollection);
            }

            bitmapAtlasCollection.AddBitmapAtlas(fontAtlas);
            _resolvedAtlas.Add(hashCode, fontAtlas);
            return fontAtlas;
        }

        SimpleBitmapAtlas CreateBitmapAtlas(ResolvedFont font, int startCodepoint, int endCodepoint, int preCalculatedHash, out B outputBitmap)
        {

            int fontKey = font.RuntimeResolvedKey;
            Typeface typeface = font.Typeface;
            string fontTextureFile = typeface.Name + "_" + fontKey;
            
            string fontTextureInfoFile = fontTextureFile + ".tx_info";
            string fontTextureImgFilename = fontTextureInfoFile + ".png";


            //-------------
            //if not found the request font
            //we generate it realtime here, (add add the cache '_createdAtlases') 
            //------------- 
            //1. create glyph-texture-bitmap generator
            var glyphTextureGen = new GlyphTextureBitmapGenerator();
            glyphTextureGen.SetSvgBmpBuilderFunc(_svgBmpBuilderFunc ?? _textServices.SvgBmpBuilder);

            //2. generate the glyphs
            TextureKind textureForNewFont = TextureKindForNewFont;

            SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();//new blank atlas builder
            if (typeface.HasSvgTable())
            {
                //need special mx
                textureForNewFont = TextureKind.Bitmap;

                glyphTextureGen.CreateTextureFontFromBuildDetail(atlasBuilder,
                    typeface,
                    font.SizeInPoints,
                    textureForNewFont,
                    new GlyphTextureBuildDetail[] { new GlyphTextureBuildDetail() { AllGlyphs = true } }
               );
            }
            else
            {
                GlyphTextureBuildDetail[] buildDetails = GlyphTextureCustomConfigs.TryGetGlyphTextureBuildDetail(font, false, false);

                if (startCodepoint != endCodepoint && startCodepoint > 0)
                {

                }

                glyphTextureGen.CreateTextureFontFromBuildDetail(atlasBuilder,
                     typeface,
                     font.SizeInPoints,
                     textureForNewFont,
                     buildDetails
                );
            }

            //3. set information before write to font-info
            //TODO: review here, check if we need 'filename' or 'fontname'
            atlasBuilder.SetAtlasFontInfo(typeface.Name, font.SizeInPoints);
            atlasBuilder.SpaceCompactOption = SimpleBitmapAtlasBuilder.CompactOption.ArrangeByHeight;

            //4. merge all glyph in the builder into a single image
            MemBitmap totalGlyphsImg = atlasBuilder.BuildSingleImage(true);

            //-------------------------------------------------------------

            //5. create a simple font atlas from information inside this atlas builder.
            SimpleBitmapAtlas fontAtlas = atlasBuilder.CreateSimpleBitmapAtlas();
            fontAtlas.SetMainBitmap(totalGlyphsImg, true);
#if DEBUG
            //save glyph image for debug
            //PixelFarm.Agg.ActualImage.SaveImgBufferToPngFile(
            //    totalGlyphsImg.GetImageBuffer(),
            //    totalGlyphsImg.Width * 4,
            //    totalGlyphsImg.Width, totalGlyphsImg.Height,
            //    "total_" + reqFont.Name + "_" + reqFont.SizeInPoints + ".png");
            ////save image to cache                 

#endif

            //same 
            //TODO: cache the generate bitmap???
            totalGlyphsImg.SaveImage(fontTextureImgFilename);

            //6. cache this in the memory,
            _resolvedAtlas.Add(fontKey, fontAtlas);

#if DEBUG

            _dbugStopWatch.Stop();
            System.Diagnostics.Debug.WriteLine("build font atlas: " + _dbugStopWatch.ElapsedMilliseconds + " ms");
#endif

            //TODO: review here again
            //save font info to local disk cache
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {

                atlasBuilder.SaveAtlasInfo(ms);
                StorageService.Provider.SaveData(fontTextureInfoFile, ms.ToArray());
#if DEBUG
                //write temp debug info
#if !__MOBILE__
                //System.IO.File.WriteAllBytes(fontTextureInfoFile, ms.ToArray());
                //System.IO.File.WriteAllText(fontTextureInfoFile + ".txt", font.Name + ",size" + font.SizeInPoints + "pts");
#endif
#endif

            }



            outputBitmap = _bitmapCache.GetOrCreateNewOne(fontAtlas);

            return fontAtlas;
        }

        static PixelFarm.CpuBlit.MemBitmap ReadGlyphImages(System.IO.Stream stream)
        {
            return PixelFarm.CpuBlit.MemBitmapExt.LoadBitmap(stream);
        }

#if DEBUG
        static void dbugSaveImgBufferToFile(BitmapAtlasItemSource glyphImg, string filename)
        {
            using (PixelFarm.CpuBlit.MemBitmap memBmp = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(
                   glyphImg.Width, glyphImg.Height, glyphImg.GetImageBuffer(), false))
            {
                PixelFarm.CpuBlit.MemBitmapExt.SaveImage(memBmp, filename, PixelFarm.CpuBlit.MemBitmapIO.OutputImageFormat.Png);
            }
        }
        ///// <summary>
        ///// test only, shapen org image with Paint.net sharpen filter
        ///// </summary>
        ///// <param name="org"></param>
        ///// <param name="radius"></param>
        ///// <returns></returns>
        //static BitmapAtlasItemSource Sharpen(BitmapAtlasItemSource org, int radius)
        //{

        //    BitmapAtlasItemSource newImg = new BitmapAtlasItemSource(org.Width, org.Height);

        //    PixelFarm.CpuBlit.Imaging.ShapenFilterPdn sharpen1 = new PixelFarm.CpuBlit.Imaging.ShapenFilterPdn();
        //    int[] orgBuffer = org.GetImageBuffer();
        //    unsafe
        //    {
        //        fixed (int* orgHeader = &orgBuffer[0])
        //        {
        //            int[] output = sharpen1.Sharpen(orgHeader, org.Width, org.Height, org.Width * 4, radius);
        //            newImg.SetImageBuffer(output, org.IsBigEndian);
        //        }
        //    }

        //    return newImg;
        //}
#endif

    }

}