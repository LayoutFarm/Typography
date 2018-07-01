namespace PixelFarm.CpuBlit.Imaging
{

    /////////////////////////////////////////////////////////////////////////////////
    // Paint.NET                                                                   //
    // Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
    // Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
    // See src/Resources/Files/License.txt for full licensing and attribution      //
    // details.                                                                    //
    // .                                                                           //
    /////////////////////////////////////////////////////////////////////////////////

    static class PixelUtils
    {
        public static byte ClampToByte(int x)
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }
    }
    public class BrightnessAndContrastAdjustment
    {
        int brightness;
        int contrast;
        byte[] rgbTable;

        public void SetParameters(int brightness, int contrast)
        {
            int multiply;
            int divide;
            this.brightness = brightness;
            this.contrast = contrast;
            if (this.contrast < 0)
            {
                multiply = this.contrast + 100;
                divide = 100;
            }
            else if (this.contrast > 0)
            {
                multiply = 100;
                divide = 100 - this.contrast;
            }
            else
            {
                multiply = 1;
                divide = 1;
            }

            if (this.rgbTable == null)
            {
                this.rgbTable = new byte[65536];//256*256
            }
            if (divide == 0)
            {
                unsafe
                {
                    fixed (byte* table_ptr = &this.rgbTable[0])
                    {

                        for (int intensity = 0; intensity < 256; ++intensity)
                        {
                            if (intensity + brightness < 128)
                            {
                                table_ptr[intensity] = 0;
                            }
                            else
                            {
                                table_ptr[intensity] = 255;
                            }
                        }
                    }
                }

            }
            else if (divide == 100)
            {
                unsafe
                {
                    fixed (byte* table_ptr = &this.rgbTable[0])
                    {
                        for (int intensity = 0; intensity < 256; ++intensity)
                        {
                            int shift = (intensity - 127) * multiply / divide + 127 - intensity + brightness;

                            for (int col = 0; col < 256; ++col)
                            {
                                int index = (intensity * 256) + col;
                                table_ptr[index] = PixelUtils.ClampToByte(col + shift);
                            }
                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (byte* table_ptr = &this.rgbTable[0])
                    {
                        for (int intensity = 0; intensity < 256; ++intensity)
                        {
                            int shift = (intensity - 127 + brightness) * multiply / divide + 127 - intensity;

                            for (int col = 0; col < 256; ++col)
                            {
                                int index = (intensity * 256) + col;
                                table_ptr[index] = PixelUtils.ClampToByte(col + shift);
                            }
                        }
                    }
                }
            }
        }


        public void Apply(byte[] srcBuffer, byte[] destBuffer, int stride, int h)
        {
            //this version srcBuffer and destBuffer must have the same size

            int p = 0;
            for (int row = 0; row < h; ++row)
            {
                p = row * stride;
                for (int i = 0; i < stride;)
                {
                    //get color from src
                    //pass to contrast filter
                    //put value to dest**
                    byte b = srcBuffer[p];
                    byte g = srcBuffer[p + 1];
                    byte r = srcBuffer[p + 2];
                    byte a = srcBuffer[i + 3];
                    //------------------------
                    int intensity = GetIntensityByte(b, g, r);
                    int shiftIndex = intensity * 256;

                    byte new_r = this.rgbTable[shiftIndex + r];
                    byte new_g = this.rgbTable[shiftIndex + g];
                    byte new_b = this.rgbTable[shiftIndex + b];

                    //save to dest
                    destBuffer[p] = new_b;
                    destBuffer[p + 1] = new_g;
                    destBuffer[p + 2] = new_r;
                    destBuffer[p + 3] = a;
                    i += 4;
                    p += 4;
                }

            }

        }
        static byte GetIntensityByte(byte b, byte g, byte r)
        {
            return (byte)((7471 * b + 38470 * g + 19595 * r) >> 16);
        }
        //public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        //{
        //    unsafe
        //    {
        //        for (int r = startIndex; r < startIndex + length; ++r)
        //        {
        //            Rectangle rect = rois[r];

        //            for (int y = rect.Top; y < rect.Bottom; ++y)
        //            {
        //                ColorBgra* srcRowPtr = src.GetPointAddressUnchecked(rect.Left, y);
        //                ColorBgra* dstRowPtr = dst.GetPointAddressUnchecked(rect.Left, y);
        //                ColorBgra* dstRowEndPtr = dstRowPtr + rect.Width;

        //                if (divide == 0)
        //                {
        //                    while (dstRowPtr < dstRowEndPtr)
        //                    {
        //                        ColorBgra col = *srcRowPtr;
        //                        int i = col.GetIntensityByte();
        //                        uint c = this.rgbTable[i];
        //                        dstRowPtr->Bgra = (col.Bgra & 0xff000000) | c | (c << 8) | (c << 16);

        //                        ++dstRowPtr;
        //                        ++srcRowPtr;
        //                    }
        //                }
        //                else
        //                {
        //                    while (dstRowPtr < dstRowEndPtr)
        //                    {
        //                        ColorBgra col = *srcRowPtr;
        //                        int i = col.GetIntensityByte();
        //                        int shiftIndex = i * 256;

        //                        col.R = this.rgbTable[shiftIndex + col.R];
        //                        col.G = this.rgbTable[shiftIndex + col.G];
        //                        col.B = this.rgbTable[shiftIndex + col.B];

        //                        *dstRowPtr = col;
        //                        ++dstRowPtr;
        //                        ++srcRowPtr;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}