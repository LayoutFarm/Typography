//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//----------------------------------- 

using System;
namespace PixelFarm.Drawing.Fonts
{
    public abstract class FontFace : IDisposable
    {
        public bool HasKerning { get; set; }
        protected abstract void OnDispose();
        public void Dispose()
        {
            OnDispose();
        }
        ~FontFace()
        {
            OnDispose();
        }
        public abstract ActualFont GetFontAtPointsSize(float pointSize);
        public abstract string Name { get; }
        public abstract string FontPath { get; }
        public abstract float GetScale(float pointSize);
        public abstract int AscentInDzUnit { get; }
        public abstract int DescentInDzUnit { get; }
        public abstract int LineGapInDzUnit { get; }
    }

    /// <summary>
    /// glyph ABC structure
    /// </summary>
    public struct FontABC
    {
        //see https://msdn.microsoft.com/en-us/library/windows/desktop/dd162454(v=vs.85).aspx
        //The ABC structure contains the width of a character in a TrueType font.
        public int a;
        public uint b;
        public int c;
        public FontABC(int a, uint b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public int Sum
        {
            get
            {
                return a + (int)b + c;
            }
        }
    }
    
}