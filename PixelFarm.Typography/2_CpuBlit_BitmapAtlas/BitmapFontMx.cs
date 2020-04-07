//MIT, 2016-present, WinterDev 
//----------------------------------- 
using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.Platforms;
using Typography.OpenFont;

namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public static class GlyphTextureCustomConfigs
    {
        //some font need special treatments...

        static Dictionary<int, GlyphTextureBuildDetail[]> s_registerDetails;

        public static void Register(RequestFont reqFont, GlyphTextureBuildDetail[] details, bool forAnySize = true, bool forAnyStyle = true)
        {
            if (s_registerDetails == null)
            {
                SetupDefaults();
            }
            //
            FontStyle fontStyle = reqFont.Style;
            float sizeInPt = reqFont.SizeInPoints;
            if (forAnySize)
            {
                sizeInPt = 0;
            }
            if (forAnyStyle)
            {
                fontStyle = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic;
            }
            int fontKey = RequestFont.CalculateFontKey(reqFont.Name.ToLower(), sizeInPt, fontStyle);
            s_registerDetails[fontKey] = details;
        }

        static GlyphTextureBuildDetail[] s_default;


        static void SetupDefaults()
        {

            //Your Implementation ...
            s_registerDetails = new Dictionary<int, GlyphTextureBuildDetail[]>();
            //set default detail
            if (s_default == null)
            {
                SetDefaultDetails(new GlyphTextureBuildDetail[] {
                    new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Latin, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.TrueTypeInstruction_VerticalOnly },
                    new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Thai, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.None},
                });
            }
            //
            //Your Implementation ...
            //eg. Tahoma             
            Register(new RequestFont("tahoma", 10), new GlyphTextureBuildDetail[]
            {
                new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Latin, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.TrueTypeInstruction_VerticalOnly },
                new GlyphTextureBuildDetail{ OnlySelectedGlyphIndices=new char[]{ 'x', 'X', '7','k','K','Z','z','R','Y','%' },
                    DoFilter = false ,  HintTechnique = Typography.Contours.HintTechnique.None},
                new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Thai, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.None},
            });

        }
        public static void SetDefaultDetails(GlyphTextureBuildDetail[] defaultDetails)
        {
            s_default = defaultDetails;
        }

        public static GlyphTextureBuildDetail[] TryGetGlyphTextureBuildDetail(RequestFont reqFont, bool forAnySize = true, bool forAnyStyle = true)
        {

            if (s_registerDetails == null)
            {
                SetupDefaults();
            }
            //
            FontStyle fontStyle = reqFont.Style;
            float sizeInPt = reqFont.SizeInPoints;
            if (forAnySize)
            {
                sizeInPt = 0;
            }
            if (forAnyStyle)
            {
                fontStyle = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic;
            }
            int fontKey = RequestFont.CalculateFontKey(reqFont.Name.ToLower(), sizeInPt, fontStyle);
            if (!s_registerDetails.TryGetValue(fontKey, out var found))
            {
                //find general config

                if (forAnySize == false || forAnySize == false)
                {
                    //get general style
                    return TryGetGlyphTextureBuildDetail(reqFont, true, true);
                }

                //not found that font key
                //create default
                //...
                return s_default;
            }
            return found;
        }
    }



    public class BitmapFontManager<B> : BitmapAtlasManager<B> where B : IDisposable
    {
        OpenFontTextService _textServices;
        Dictionary<int, SimpleBitmapAtlas> _createdAtlases = new Dictionary<int, SimpleBitmapAtlas>();
        Dictionary<string, SimpleBitmapAtlas> _msdfTextureFonts = new Dictionary<string, SimpleBitmapAtlas>();

        public BitmapFontManager(OpenFontTextService textServices, LoadNewBmpDelegate<SimpleBitmapAtlas, B> _createNewDel)
            : base(_createNewDel)
        {
            _textServices = textServices;
        }
        public BitmapFontManager(OpenFontTextService textServices)
        {
            _textServices = textServices;
        }
        public TextureKind TextureKindForNewFont { get; set; }


#if DEBUG
        System.Diagnostics.Stopwatch _dbugStopWatch = new System.Diagnostics.Stopwatch();
#endif


        public void AddSimpleFontAtlas(SimpleBitmapAtlas[] simpleFontAtlases, System.IO.Stream totalGlyphImgStream)
        {
            //multiple font atlas that share the same glyphImg

            MemBitmap mainBmp = ReadGlyphImages(totalGlyphImgStream);
            for (int i = 0; i < simpleFontAtlases.Length; ++i)
            {
                SimpleBitmapAtlas simpleFontAtlas = simpleFontAtlases[i];
                simpleFontAtlas.SetMainBitmap(mainBmp, true);
                simpleFontAtlas.UseSharedImage = true;
                _createdAtlases.Add(simpleFontAtlas.FontKey, simpleFontAtlas);

                if (simpleFontAtlas.TextureKind == TextureKind.Msdf)
                {
                    //if we have msdf texture
                    //then we can use this to do autoscale
                    _msdfTextureFonts.Add(simpleFontAtlas.FontFilename, simpleFontAtlas);
                }
            }
        }
        static object s_loadDataLock = new object();

        /// <summary>
        /// get from cache or create a new one
        /// </summary>
        /// <param name="reqFont"></param>
        /// <returns></returns>
        public SimpleBitmapAtlas GetFontAtlas(RequestFont reqFont, out B outputBitmap)
        {

#if DEBUG
            _dbugStopWatch.Reset();
            _dbugStopWatch.Start();
#endif

            int fontKey = reqFont.FontKey;

            if (_createdAtlases.TryGetValue(fontKey, out SimpleBitmapAtlas fontAtlas))
            {
                outputBitmap = _loadAtlases.GetOrCreateNewOne(fontAtlas);
                return fontAtlas;
            }

            //check if we have small msdf texture or not
            if (_msdfTextureFonts.TryGetValue(reqFont.Name, out SimpleBitmapAtlas msdfTexture))
            {
                //use this
                outputBitmap = _loadAtlases.GetOrCreateNewOne(msdfTexture);
                return msdfTexture;
            }


            //--------------------------------
            //check from pre-built cache (if availiable)     
            Typeface resolvedTypeface = _textServices.ResolveTypeface(reqFont);
            string fontTextureFile = reqFont.Name + "_" + fontKey;
            string resolveFontFile = fontTextureFile + ".info";
            string fontTextureInfoFile = resolveFontFile;
            string fontTextureImgFilename = fontTextureInfoFile + ".png";


            if (StorageService.Provider.DataExists(fontTextureInfoFile) &&
                StorageService.Provider.DataExists(fontTextureImgFilename))
            {
                //check local caching, if found then load-> create it

                SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();


                lock (s_loadDataLock)
                {
                    using (System.IO.Stream textureInfoFileStream = StorageService.Provider.ReadDataStream(fontTextureInfoFile))
                    using (System.IO.Stream fontAtlasImgStream = StorageService.Provider.ReadDataStream(fontTextureImgFilename))
                    {
                        try
                        {
                            //TODO: review here
                            fontAtlas = atlasBuilder.LoadAtlasInfo(textureInfoFileStream)[0];
                            fontAtlas.SetMainBitmap(ReadGlyphImages(fontAtlasImgStream), true);
                            fontAtlas.OriginalFontSizePts = reqFont.SizeInPoints;
                            _createdAtlases.Add(fontKey, fontAtlas);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

            }
            else
            {
                //-------------
                //if not found the request font
                //we generate it realtime here, (add add the cache '_createdAtlases') 
                //-------------


                //1. create glyph-texture-bitmap generator
                var glyphTextureGen = new GlyphTextureBitmapGenerator();

                //2. generate the glyphs
                SimpleBitmapAtlasBuilder atlasBuilder = glyphTextureGen.CreateTextureFontFromBuildDetail(
                    resolvedTypeface,
                    reqFont.SizeInPoints,
                    TextureKindForNewFont,
                    GlyphTextureCustomConfigs.TryGetGlyphTextureBuildDetail(reqFont, false, false)
                );

                //3. set information before write to font-info
                atlasBuilder.FontFilename = reqFont.Name;//TODO: review here, check if we need 'filename' or 'fontname'
                atlasBuilder.FontKey = reqFont.FontKey;
                atlasBuilder.SpaceCompactOption = SimpleBitmapAtlasBuilder.CompactOption.ArrangeByHeight;

                //4. merge all glyph in the builder into a single image
                PixelFarm.CpuBlit.MemBitmap totalGlyphsImg = atlasBuilder.BuildSingleImage(true);

                //-------------------------------------------------------------

                //5. create a simple font atlas from information inside this atlas builder.
                fontAtlas = atlasBuilder.CreateSimpleBitmapAtlas();
                fontAtlas.SetMainBitmap(totalGlyphsImg, true);
#if DEBUG
                //save glyph image for debug
                //PixelFarm.Agg.ActualImage.SaveImgBufferToPngFile(
                //    totalGlyphsImg.GetImageBuffer(),
                //    totalGlyphsImg.Width * 4,
                //    totalGlyphsImg.Width, totalGlyphsImg.Height,
                //    "total_" + reqFont.Name + "_" + reqFont.SizeInPoints + ".png");
                ////save image to cache                 
                totalGlyphsImg.SaveImage(fontTextureImgFilename);
#endif


                //6. cache this in the memory,
                _createdAtlases.Add(fontKey, fontAtlas);

                //
                ////calculate some commonly used values
                //fontAtlas.SetTextureScaleInfo(
                //    resolvedTypeface.CalculateScaleToPixelFromPointSize(fontAtlas.OriginalFontSizePts),
                //    resolvedTypeface.CalculateScaleToPixelFromPointSize(reqFont.SizeInPoints));
                ////TODO: review here, use scaled or unscaled values
                //fontAtlas.SetCommonFontMetricValues(
                //    resolvedTypeface.Ascender,
                //    resolvedTypeface.Descender,
                //    resolvedTypeface.LineGap,
                //    resolvedTypeface.CalculateRecommendLineSpacing());

                ///
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
                    System.IO.File.WriteAllBytes(fontTextureInfoFile, ms.ToArray());
                    System.IO.File.WriteAllText(fontTextureInfoFile + ".txt", reqFont.Name + ",size" + reqFont.SizeInPoints + "pts");
#endif
#endif

                }
            }


            outputBitmap = _loadAtlases.GetOrCreateNewOne(fontAtlas);
            return fontAtlas;
        }

        static PixelFarm.CpuBlit.MemBitmap ReadGlyphImages(System.IO.Stream stream)
        {
            return PixelFarm.CpuBlit.MemBitmap.LoadBitmap(stream);
        }
        static void SaveImgBufferToFile(BitmapAtlasItemSource glyphImg, string filename)
        {
            using (PixelFarm.CpuBlit.MemBitmap memBmp = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(
                   glyphImg.Width, glyphImg.Height, glyphImg.GetImageBuffer(), false))
            {
                PixelFarm.CpuBlit.MemBitmapExtensions.SaveImage(memBmp, filename, PixelFarm.CpuBlit.MemBitmapIO.OutputImageFormat.Png);
            }
        }
#if DEBUG
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