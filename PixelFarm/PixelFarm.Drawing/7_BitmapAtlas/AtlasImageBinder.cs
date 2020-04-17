//MIT, 2019-present, WinterDev
//----------------------------------- 

using PixelFarm.CpuBlit.BitmapAtlas;
namespace PixelFarm.Drawing
{
    public class AtlasImageBinder : ImageBinder
    {
        public AtlasImageBinder(string atlasName, string imgName)
        {
#if DEBUG
            if (atlasName != "BitmapAtlas01")
            {

            }
#endif

            AtlasName = atlasName;
            ImageName = imgName;
        }
        public string AtlasName { get; private set; }
        public string ImageName { get; private set; }
        public override bool IsAtlasImage => true;
        public AtlasItem AtlasItem { get; set; }

        public TextureKind TextureKind { get; set; } //set by loadder

        public int LatestPainterId { get; set; } //temp fix***
    }

}