//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
using PixelFarm.Platforms;

namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public delegate U LoadNewBmpDelegate<T, U>(T src);

    public class BitmapCache<T, U> : IDisposable
        where U : IDisposable
    {
        readonly Dictionary<T, U> _loadBmps = new Dictionary<T, U>();
        readonly LoadNewBmpDelegate<T, U> _loadNewBmpDel;
        public BitmapCache(LoadNewBmpDelegate<T, U> loadNewBmpDel)
        {
            _loadNewBmpDel = loadNewBmpDel;
        }
        public U GetOrCreateNewOne(T key)
        {
            if (!_loadBmps.TryGetValue(key, out U found))
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

    public enum TextureKind : byte
    {
        StencilLcdEffect, //default
        StencilGreyScale,
        Msdf,
        Bitmap
    }


    public class BitmapAtlasManager<B> where B : IDisposable
    {
        protected BitmapCache<SimpleBitmapAtlas, B> _bitmapCache;

        readonly Dictionary<string, SimpleBitmapAtlas> _loadedAtlasByNames = new Dictionary<string, SimpleBitmapAtlas>();

        public BitmapAtlasManager() { }
        public BitmapAtlasManager(LoadNewBmpDelegate<SimpleBitmapAtlas, B> createNewDel)
        {
            //glyph cahce for specific atlas 
            SetLoadNewBmpDel(createNewDel);
        }
        protected void SetLoadNewBmpDel(LoadNewBmpDelegate<SimpleBitmapAtlas, B> createNewDel)
        {
            _bitmapCache = new BitmapCache<SimpleBitmapAtlas, B>(createNewDel);
        }

        public void RegisterBitmapAtlas(string atlasName, byte[] atlasInfoBuffer, byte[] totalImgBuffer)
        {
            //direct register atlas
            //instead of loading it from file
            if (!_loadedAtlasByNames.ContainsKey(atlasName))
            {
               
                using (System.IO.Stream fontAtlasTextureInfo = new MemoryStream(atlasInfoBuffer))
                using (System.IO.Stream fontImgStream = new MemoryStream(totalImgBuffer))
                {
                    try
                    {
                        SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();
                        List<SimpleBitmapAtlas> atlasList = atlasBuilder.LoadAtlasInfo(fontAtlasTextureInfo);
                        SimpleBitmapAtlas foundAtlas = atlasList[0];
                        foundAtlas.SetMainBitmap(MemBitmapExt.LoadBitmap(fontImgStream), true);
                        _loadedAtlasByNames.Add(atlasName, foundAtlas);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
#if DEBUG
        System.Diagnostics.Stopwatch _dbugStopWatch = new System.Diagnostics.Stopwatch();
#endif 
        /// <summary>
        /// get from cache or create a new one
        /// </summary>
        /// <param name="reqFont"></param>
        /// <returns></returns>
        public SimpleBitmapAtlas GetBitmapAtlas(string atlasName, out B outputBitmap)
        {

#if DEBUG
            _dbugStopWatch.Reset();
            _dbugStopWatch.Start();
#endif


            if (!_loadedAtlasByNames.TryGetValue(atlasName, out SimpleBitmapAtlas foundAtlas))
            {
                //check from pre-built cache (if availiable)   
                string textureInfoFile = atlasName + ".info";
                string textureImgFilename = atlasName + ".png";
                //check if the file exist
                if (InMemStorage.TryGetBuffer(textureInfoFile, out byte[] texture_info) &&
                    InMemStorage.TryGetBuffer(textureImgFilename, out byte[] img_buffer))
                {
                   
                    using (System.IO.Stream fontAtlasTextureInfo = new MemoryStream(texture_info))
                    using (System.IO.Stream fontImgStream = new MemoryStream(img_buffer))
                    {
                        try
                        {
                            SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();
                            List<SimpleBitmapAtlas> atlasList = atlasBuilder.LoadAtlasInfo(fontAtlasTextureInfo);
                            foundAtlas = atlasList[0];
                            foundAtlas.SetMainBitmap(MemBitmapExt.LoadBitmap(fontImgStream), true);
                            _loadedAtlasByNames.Add(atlasName, foundAtlas);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                else if (StorageService.Provider.DataExists(textureInfoFile) &&
                    StorageService.Provider.DataExists(textureImgFilename))
                {
                    
                    using (System.IO.Stream fontAtlasTextureInfo = StorageService.Provider.ReadDataStream(textureInfoFile))
                    using (System.IO.Stream fontImgStream = StorageService.Provider.ReadDataStream(textureImgFilename))
                    {
                        try
                        {
                            SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();
                            List<SimpleBitmapAtlas> atlasList = atlasBuilder.LoadAtlasInfo(fontAtlasTextureInfo);
                            foundAtlas = atlasList[0];

                            foundAtlas.SetMainBitmap(MemBitmapExt.LoadBitmap(fontImgStream), true);
                            _loadedAtlasByNames.Add(atlasName, foundAtlas);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                }
            }
            if (foundAtlas != null)
            {
                outputBitmap = _bitmapCache.GetOrCreateNewOne(foundAtlas);
                return foundAtlas;
            }
            else
            {
#if DEBUG
                //show warning about this
                System.Diagnostics.Debug.WriteLine("not found atlas:" + atlasName);
#endif

                outputBitmap = default(B);
                return null;
            }
        }

        public void Clear()
        {
            _bitmapCache.Clear();
        }
    }

}