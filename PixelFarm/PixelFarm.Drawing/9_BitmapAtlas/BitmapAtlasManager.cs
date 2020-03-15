//MIT, 2019-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;
using PixelFarm.Platforms;

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class AtlasImageBinder : LayoutFarm.ImageBinder
    {
        public AtlasImageBinder(string atlasName, string imgName)
        {
            AtlasName = atlasName;
            ImageName = imgName;
        }
        public string AtlasName { get; private set; }
        public string ImageName { get; private set; }
        public override bool IsAtlasImage => true;
        public TextureGlyphMapData MapData { get; set; }
    }

    //TODO: review class and method names
    public delegate U LoadNewBmpDelegate<T, U>(T src);

    public class BitmapCache<T, U> : IDisposable
        where U : IDisposable
    {
        Dictionary<T, U> _loadBmps = new Dictionary<T, U>();
        LoadNewBmpDelegate<T, U> _loadNewBmpDel;
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
        protected BitmapCache<SimpleBitmapAtlas, B> _loadAtlases;
        Dictionary<string, SimpleBitmapAtlas> _createdAtlases = new Dictionary<string, SimpleBitmapAtlas>();

        public BitmapAtlasManager() { }
        public BitmapAtlasManager(LoadNewBmpDelegate<SimpleBitmapAtlas, B> _createNewDel)
        {
            //glyph cahce for specific atlas 
            SetLoadNewBmpDel(_createNewDel);
        }
        protected void SetLoadNewBmpDel(LoadNewBmpDelegate<SimpleBitmapAtlas, B> _createNewDel)
        {
            _loadAtlases = new BitmapCache<SimpleBitmapAtlas, B>(_createNewDel);
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


            if (!_createdAtlases.TryGetValue(atlasName, out SimpleBitmapAtlas foundAtlas))
            {
                //check from pre-built cache (if availiable)   
                string textureInfoFile = atlasName + ".info";
                string textureImgFilename = atlasName + ".png";
                //check if the file exist

                if (StorageService.Provider.DataExists(textureInfoFile) &&
                    StorageService.Provider.DataExists(textureImgFilename))
                {
                    SimpleBitmapAtlasBuilder atlasBuilder = new SimpleBitmapAtlasBuilder();
                    using (System.IO.Stream dataStream = StorageService.Provider.ReadDataStream(textureInfoFile))
                    using (System.IO.Stream fontImgStream = StorageService.Provider.ReadDataStream(textureImgFilename))
                    {
                        try
                        {
                            List<SimpleBitmapAtlas> atlasList = atlasBuilder.LoadAtlasInfo(dataStream);
                            foundAtlas = atlasList[0];
                            foundAtlas.MainBitmap = MemBitmap.LoadBitmap(fontImgStream);
                            _createdAtlases.Add(atlasName, foundAtlas);
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
                outputBitmap = _loadAtlases.GetOrCreateNewOne(foundAtlas);
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
            _loadAtlases.Clear();
        }
    }

}