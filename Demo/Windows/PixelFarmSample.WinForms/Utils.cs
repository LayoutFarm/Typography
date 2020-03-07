//MIT, 2020-present, WinterDev

using System.Drawing;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using Typography.Rendering;

namespace SampleWinForms
{
    static class SimpleUtils
    {
        public static void DisposeExistingPictureBoxImage(PictureBox pictureBox)
        {
            if (pictureBox.Image is Bitmap currentBmp)
            {
                pictureBox.Image = null;
                currentBmp.Dispose();
                currentBmp = null;
            }
        }
        public static void SaveGlyphImageToPngFile(GlyphImage totalGlyphsImg, string imgFilename)
        {

            //TODO: use helper method 
            using (MemBitmap memBmp = MemBitmap.CreateFromCopy(totalGlyphsImg.Width, totalGlyphsImg.Height, totalGlyphsImg.GetImageBuffer()))
            using (System.Drawing.Bitmap bmp = new Bitmap(memBmp.Width, memBmp.Height))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, memBmp.Width, memBmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var tmpMem = MemBitmap.GetBufferPtr(memBmp);
                unsafe
                {
                    PixelFarm.CpuBlit.NativeMemMx.MemCopy((byte*)bmpdata.Scan0,
                        (byte*)tmpMem.Ptr,
                        tmpMem.LengthInBytes);
                }
                bmp.UnlockBits(bmpdata);

                if (System.IO.File.Exists(imgFilename))
                {
                    System.IO.File.Delete(imgFilename);
                }
                bmp.Save(imgFilename);
            }
        }

    }
}