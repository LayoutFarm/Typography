//MIT, 2020-present, WinterDev

using System.Drawing;
using System.Windows.Forms;

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

    }
}