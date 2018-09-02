//MIT, 2016-present, WinterDev
//----------------------------------- 
using System;
using System.Collections.Generic;
using PixelFarm;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;

using PixelFarm.Platforms;

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
            U found;
            if (_loadBmps.TryGetValue(key, out found))
            {
                found.Dispose();
                _loadBmps.Remove(key);
            }
        }
    }

    public class BitmapFontManager<B>
        where B : IDisposable
    {
        static FontBitmapCache<SimpleFontAtlas, B> _loadedGlyphs;
        static Dictionary<int, SimpleFontAtlas> _createdAtlases = new Dictionary<int, SimpleFontAtlas>();

        LayoutFarm.OpenFontTextService textServices;
        ScriptLang[] _currentScriptLangs;
        TextureKind _textureKind;
        public BitmapFontManager(TextureKind textureKind,
            LayoutFarm.OpenFontTextService textServices,
            LoadNewBmpDelegate<SimpleFontAtlas, B> _createNewDel)
            : this(textureKind, textServices)
        {
            //glyph cahce for specific atlas 
            SetLoadNewBmpDel(_createNewDel);
        }
        public BitmapFontManager(TextureKind textureKind, LayoutFarm.OpenFontTextService textServices)
        {
            this.textServices = textServices;
            _textureKind = textureKind;
        }


        protected void SetLoadNewBmpDel(LoadNewBmpDelegate<SimpleFontAtlas, B> _createNewDel)
        {
            _loadedGlyphs = new FontBitmapCache<SimpleFontAtlas, B>(_createNewDel);
        }
        GlyphTextureBuildDetail[] _textureBuildDetails;
        public void SetCurrentScriptLangs(ScriptLang[] currentScriptLangs)
        {
            this._currentScriptLangs = currentScriptLangs;

            //TODO: review here again,
            //this is a fixed version for tahoma font
            //temp fix here ...
            _textureBuildDetails = new GlyphTextureBuildDetail[]
            {
                new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Latin, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.TrueTypeInstruction_VerticalOnly },
                new GlyphTextureBuildDetail{ OnlySelectedGlyphIndices=new char[]{ 'x', 'X', '7','k','K','Z','z','R','Y','%' },
                    DoFilter = false ,  HintTechnique = Typography.Contours.HintTechnique.None},
                new GlyphTextureBuildDetail{ ScriptLang= ScriptLangs.Thai, DoFilter= false, HintTechnique = Typography.Contours.HintTechnique.None},
            };
        }

#if DEBUG
        System.Diagnostics.Stopwatch _dbugStopWatch = new System.Diagnostics.Stopwatch();
#endif 
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
            SimpleFontAtlas fontAtlas;
            if (!_createdAtlases.TryGetValue(fontKey, out fontAtlas))
            {
                //check from pre-built cache (if availiable) 
                Typeface resolvedTypeface = textServices.ResolveTypeface(reqFont);

                string fontTextureFile = reqFont.Name + " " + fontKey;
                string resolveFontFile = fontTextureFile + ".info";
                string fontTextureInfoFile = resolveFontFile;
                string fontTextureImg = fontTextureInfoFile + ".png";

                //check if the file exist

                if (StorageService.Provider.DataExists(fontTextureInfoFile))
                {
                    SimpleFontAtlasBuilder atlasBuilder2 = new SimpleFontAtlasBuilder();

                    using (System.IO.Stream dataStream = StorageService.Provider.ReadDataStream(fontTextureInfoFile))
                    {
                        try
                        {
                            fontAtlas = atlasBuilder2.LoadFontInfo(dataStream);
                            fontAtlas.TotalGlyph = ReadGlyphImages(fontTextureImg);
                            fontAtlas.OriginalFontSizePts = reqFont.SizeInPoints;
                            _createdAtlases.Add(fontKey, fontAtlas);
                            ////
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
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                }
                else
                {

                    GlyphImage totalGlyphsImg = null;
                    SimpleFontAtlasBuilder atlasBuilder = null;
                    var textureGen = new GlyphTextureBitmapGenerator();
                    textureGen.CreateTextureFontFromScriptLangs(
                        resolvedTypeface,
                        reqFont.SizeInPoints,
                       _textureKind,
                       _textureBuildDetails,
                        (glyphIndex, glyphImage, outputAtlasBuilder) =>
                        {
                            if (outputAtlasBuilder != null)
                            {
                                //finish
                                atlasBuilder = outputAtlasBuilder;
                            }
                        }
                    );

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
                    //    "d:\\WImageTest\\total_" + reqFont.Name + "_" + reqFont.SizeInPoints + ".png");
                    ////save image to cache
                    SaveImgBufferToFile(totalGlyphsImg, fontTextureImg);
#endif

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

                    //save font info to cache
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        atlasBuilder.SaveFontInfo(ms);
                        System.IO.File.WriteAllBytes(fontTextureInfoFile, ms.ToArray());

                        StorageService.Provider.SaveData(fontTextureInfoFile, ms.ToArray());
                    }
                }
            }

            outputBitmap = _loadedGlyphs.GetOrCreateNewOne(fontAtlas);
            return fontAtlas;
        }

        static GlyphImage ReadGlyphImages(string filename)
        {
            PixelFarm.CpuBlit.ActualBitmap bmp = StorageService.Provider.ReadPngBitmap(filename);
            GlyphImage img = new GlyphImage(bmp.Width, bmp.Height);
            int[] buffer = new int[bmp.Width * bmp.Height];
            unsafe
            {
                PixelFarm.CpuBlit.Imaging.TempMemPtr tmp = PixelFarm.CpuBlit.ActualBitmap.GetBufferPtr(bmp);
                System.Runtime.InteropServices.Marshal.Copy(tmp.Ptr, buffer, 0, bmp.Width * bmp.Height);
                img.SetImageBuffer(buffer, true);
            }
            return img;

        }
        static void SaveImgBufferToFile(GlyphImage glyphImg, string filename)
        {

            var bmp = new PixelFarm.CpuBlit.ActualBitmap(glyphImg.Width, glyphImg.Height, glyphImg.GetImageBuffer());
            StorageService.Provider.SavePngBitmap(bmp, filename);

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