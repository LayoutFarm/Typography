//----------------------------------------------------poly_subpixel_scale_e
// These constants determine the subpixel accuracy, to be more precise, 
// the number of bits of the fractional part of the coordinates. 
// The possible coordinate capacity in bits can be calculated by formula:
// sizeof(int) * 8 - poly_subpixel_shift, i.e, for 32-bit integers and
// 8-bits fractional part the capacity is 24 bits.
#define ARGB
namespace PixelFarm.CpuBlit.Rasterization
{
    static class PolySubPix
    {
        public const int SHIFT = 8;          //----poly_subpixel_shif
        public const int SCALE = 1 << SHIFT; //----poly_subpixel_scale 
        public const int MASK = SCALE - 1;  //----poly_subpixel_mask 
    } 
     
}

namespace PixelFarm.CpuBlit
{

    /// <summary>
    /// color order
    /// </summary>
    static class Internal_CO
    {

#if ARGB  //  eg. Win32
        //eg. windows
        /// <summary>
        /// order b
        /// </summary>
        public const int B = 0;
        /// <summary>
        /// order g
        /// </summary>
        public const int G = 1;
        /// <summary>
        /// order r
        /// </summary>
        public const int R = 2;
        /// <summary>
        /// order a
        /// </summary>
        public const int A = 3;
#endif

#if ABGR //   eg. Skia on iOS
        /// <summary>
        /// order b
        /// </summary>
        public const int B = 2;
        /// <summary>
        /// order g
        /// </summary>
        public const int G = 1;
        /// <summary>
        /// order r
        /// </summary>
        public const int R = 0;
        /// <summary>
        /// order a
        /// </summary>
        public const int A = 3; 
#endif

        public const int B_SHIFT = B * 8;
        /// <summary>
        /// order g
        /// </summary>
        public const int G_SHIFT = G * 8;
        /// <summary>
        /// order r
        /// </summary>
        public const int R_SHIFT = R * 8;
        /// <summary>
        /// order a
        /// </summary>
        public const int A_SHIFT = A * 8;
    }
}
