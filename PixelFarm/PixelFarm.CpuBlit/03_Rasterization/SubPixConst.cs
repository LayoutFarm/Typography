//----------------------------------------------------poly_subpixel_scale_e
// These constants determine the subpixel accuracy, to be more precise, 
// the number of bits of the fractional part of the coordinates. 
// The possible coordinate capacity in bits can be calculated by formula:
// sizeof(int) * 8 - poly_subpixel_shift, i.e, for 32-bit integers and
// 8-bits fractional part the capacity is 24 bits.

namespace PixelFarm.CpuBlit.Rasterization
{
    static class PolySubPix
    {
        public const int SHIFT = 8;          //----poly_subpixel_shif
        public const int SCALE = 1 << SHIFT; //----poly_subpixel_scale 
        public const int MASK = SCALE - 1;  //----poly_subpixel_mask 
    }
}