//MIT, 2016-present, WinterDev 
//----------------------------------- 
using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using PixelFarm.Platforms;

using Typography.OpenFont;

namespace Typography.Rendering
{
    public delegate U LoadNewBmpDelegate<T, U>(T src);

    public class FontBitmapCache<T, U> : IDisposable
        where U : IDisposable
    {
        Dictionary<T, U> _loadBmps = new Dictionary<T, U>();
        LoadNewBmpDelegate<T, U> _loadNewBmpDel;
        public FontBitmapCache(LoadNewBmpDelegate<T, U> loadNewBmpDel)
        {
            _loadNewBmpDel = loadNewBmpDel;
        }
        public U GetOrCreateNewOne(T key)
        {
            U found;
            if (!_loadBmps.TryGetValue(key, out found))
            {
                return _loadBmps[key] = _loadNewBmpDel(key);
            }
            return found;
        }
        public void Dispose()
        {
            Clear();
        }
        public void Clear()
        {
            foreach (U glbmp in _loadBmps.Values)
            {
                glbmp.Dispose();
            }
            _loadBmps.Clear();
        }
        public void Delete(T key)
        {
            if (_loadBmps.TryGetValue(key, out U found))
            {
                found.Dispose();
                _loadBmps.Remove(key);
            }
        }
    }

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

    public class BitmapFontManager<B>
        where B : IDisposable
    {
        FontBitmapCache<SimpleFontAtlas, B> _loadedGlyphs;
        Dictionary<int, SimpleFontAtlas> _createdAtlases = new Dictionary<int, SimpleFontAtlas>();
        Dictionary<string, SimpleFontAtlas> _msdfTextureFonts = new Dictionary<string, SimpleFontAtlas>();

        LayoutFarm.OpenFontTextService _textServices;

        public BitmapFontManager(
            LayoutFarm.OpenFontTextService textServices,
            LoadNewBmpDelegate<SimpleFontAtlas, B> _createNewDel)
            : this(textServices)
        {
            //glyph cahce for specific atlas 
            SetLoadNewBmpDel(_createNewDel);
            TextureKindForNewFont = PixelFarm.Drawing.BitmapAtlas.TextureKind.StencilLcdEffect;

        }
        public BitmapFontManager(LayoutFarm.OpenFontTextService textServices)
        {
            _textServices = textServices;

        }
        protected void SetLoadNewBmpDel(LoadNewBmpDelegate<SimpleFontAtlas, B> _createNewDel)
        {
            _loadedGlyphs = new FontBitmapCache<SimpleFontAtlas, B>(_createNewDel);
        }

        public PixelFarm.Drawing.BitmapAtlas.TextureKind TextureKindForNewFont { get; set; }



#if DEBUG
        System.Diagnostics.Stopwatch _dbugStopWatch = new System.Diagnostics.Stopwatch();
#endif


        public void AddSimpleFontAtlas(SimpleFontAtlas[] simpleFontAtlases, System.IO.Stream totalGlyphImgStream)
        {
            //multiple font atlas that share the same glyphImg

            GlyphImage glyphImg = ReadGlyphImages(totalGlyphImgStream);
            for (int i = 0; i < simpleFontAtlases.Length; ++i)
            {
                SimpleFontAtlas simpleFontAtlas = simpleFontAtlases[i];
                simpleFontAtlas.TotalGlyph = glyphImg;
                simpleFontAtlas.UseSharedGlyphImage = true;
                _createdAtlases.Add(simpleFontAtlas.FontKey, simpleFontAtlas);

                if (simpleFontAtlas.TextureKind == PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf)
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
        public SimpleFontAtlas GetFontAtlas(RequestFont reqFont, out B outputBitmap)
        {

#if DEBUG
            _dbugStopWatch.Reset();
            _dbugStopWatch.Start();
#endif

            int fontKey = reqFont.FontKey;

            if (!_createdAtlases.TryGetValue(fontKey, out SimpleFontAtlas fontAtlas))
            {

                //-------------
                //check if we have small msdf texture or not
                if (_msdfTextureFonts.TryGetValue(reqFont.Name, out SimpleFontAtlas msdfTexture))
                {
                    //use this
                    outputBitmap = _loadedGlyphs.GetOrCreateNewOne(msdfTexture);
                    return msdfTexture;
                }

                //-------------
                //check from pre-built cache (if availiable)     
                Typeface resolvedTypeface = _textServices.ResolveTypeface(reqFont);
                string fontTextureFile = reqFont.Name + "_" + fontKey;
                string resolveFontFile = fontTextureFile + ".info";
                string fontTextureInfoFile = resolveFontFile;
                string fontTextureImgFilename = fontTextureInfoFile + ".png";

                //TODO: review here
                //
                if (StorageService.Provider.DataExists(fontTextureInfoFile) &&
                    StorageService.Provider.DataExists(fontTextureImgFilename))
                {
                    SimpleFontAtlasBuilder atlasBuilder = new SimpleFontAtlasBuilder();
                    lock (s_loadDataLock)
                    {
                        using (System.IO.Stream textureInfoFileStream = StorageService.Provider.ReadDataStream(fontTextureInfoFile))
                        using (System.IO.Stream fontAtlasImgStream = StorageService.Provider.ReadDataStream(fontTextureImgFilename))
                        {
                            try
                            {
                                //TODO: review here
                                fontAtlas = atlasBuilder.LoadFontAtlasInfo(textureInfoFileStream)[0];
                                fontAtlas.TotalGlyph = ReadGlyphImages(fontAtlasImgStream);
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
                    GlyphImage totalGlyphsImg = null;

                    var glyphTextureGen = new GlyphTextureBitmapGenerator();
                    SimpleFontAtlasBuilder atlasBuilder = glyphTextureGen.CreateTextureFontFromBuildDetail(
                        resolvedTypeface,
                        reqFont.SizeInPoints,
                        TextureKindForNewFont,
                        GlyphTextureCustomConfigs.TryGetGlyphTextureBuildDetail(reqFont, false, false)
                    );

                    atlasBuilder.FontFilename = reqFont.Name;//TODO: review here, check if we need 'filename' or 'fontname'
                    atlasBuilder.FontKey = reqFont.FontKey;
                    atlasBuilder.SpaceCompactOption = SimpleFontAtlasBuilder.CompactOption.ArrangeByHeight;
                    totalGlyphsImg = atlasBuilder.BuildSingleImage();
                    //if (reqFont.SizeInPoints == 14 && cacheImg != null)
                    //{
                    //    totalGlyphsImg = cacheImg;
                    //}
                    //totalGlyphsImg = Sharpen(totalGlyphsImg, 1); //test shapen primary image
                    //-               
                    //
                    //create atlas
                    fontAtlas = atlasBuilder.CreateSimpleFontAtlas();
                    fontAtlas.TotalGlyph = totalGlyphsImg;
#if DEBUG
                    //save glyph image for debug
                    //PixelFarm.Agg.ActualImage.SaveImgBufferToPngFile(
                    //    totalGlyphsImg.GetImageBuffer(),
                    //    totalGlyphsImg.Width * 4,
                    //    totalGlyphsImg.Width, totalGlyphsImg.Height,
                    //    "total_" + reqFont.Name + "_" + reqFont.SizeInPoints + ".png");
                    ////save image to cache
                    SaveImgBufferToFile(totalGlyphsImg, fontTextureImgFilename);
#endif

                    ////save image to cache
                    //SaveImgBufferToFile(totalGlyphsImg, fontTextureImgFilename);
                    //cache the atlas
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
                    //save font info to cache
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        atlasBuilder.SaveAtlasInfo(ms);
                        //System.IO.File.WriteAllBytes(fontTextureInfoFile, ms.ToArray());
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
            }

            outputBitmap = _loadedGlyphs.GetOrCreateNewOne(fontAtlas);
            return fontAtlas;
        }

        static GlyphImage ReadGlyphImages(System.IO.Stream stream)
        {
            using (PixelFarm.CpuBlit.MemBitmap bmp = PixelFarm.CpuBlit.MemBitmap.LoadBitmap(stream))
            {
                GlyphImage img = new GlyphImage(bmp.Width, bmp.Height);
                int[] buffer = new int[bmp.Width * bmp.Height];
                unsafe
                {
                    PixelFarm.CpuBlit.Imaging.TempMemPtr tmp = PixelFarm.CpuBlit.MemBitmap.GetBufferPtr(bmp);
                    System.Runtime.InteropServices.Marshal.Copy(tmp.Ptr, buffer, 0, bmp.Width * bmp.Height);
                    img.SetImageBuffer(buffer, true);
                }
                return img;
            }
        }
        static void SaveImgBufferToFile(GlyphImage glyphImg, string filename)
        {
            using (PixelFarm.CpuBlit.MemBitmap memBmp = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(
                   glyphImg.Width, glyphImg.Height, glyphImg.GetImageBuffer(), false))
            {
                PixelFarm.CpuBlit.MemBitmapExtensions.SaveImage(memBmp, filename, PixelFarm.CpuBlit.MemBitmapIO.OutputImageFormat.Png);
            }
        }
#if DEBUG
        /// <summary>
        /// test only, shapen org image with Paint.net sharpen filter
        /// </summary>
        /// <param name="org"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        static GlyphImage Sharpen(GlyphImage org, int radius)
        {

            GlyphImage newImg = new GlyphImage(org.Width, org.Height);

            PixelFarm.CpuBlit.Imaging.ShapenFilterPdn sharpen1 = new PixelFarm.CpuBlit.Imaging.ShapenFilterPdn();
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
#endif
        public void Clear()
        {
            _loadedGlyphs.Clear();
        }
    }

}