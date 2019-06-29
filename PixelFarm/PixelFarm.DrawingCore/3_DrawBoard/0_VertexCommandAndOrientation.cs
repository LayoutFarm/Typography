//BSD, 2014-present, WinterDev


namespace PixelFarm.CpuBlit
{
    /// <summary>
    /// vertex command and flags
    /// </summary>
    public enum VertexCmd : byte
    {
        //---------------------------------
        //the order of these fields are significant!
        //---------------------------------
        //first lower 4 bits compact flags
        /// <summary>
        /// no more command
        /// </summary>
        NoMore = 0x00,
        //-----------------------        
        EndFigure = 0x01, //end current figure,( may not close eg line)
        /// <summary>
        /// close current polygon (but may not complete current figure)
        /// </summary>
        Close = 0x02,
        CloseAndEndFigure = 0x03,//close current polygon + complete end figure
        //----------------------- 
        //start from move to is 
        MoveTo = 0x04,
        LineTo = 0x05,

        /// <summary>
        /// control point for curve3
        /// </summary>
        C3 = 0x06, // 
        /// <summary>
        /// control point for curve4
        /// </summary>
        C4 = 0x07,

        //----------------------- 
        //TODO: add Catmul-Rom point command
        //add other spline point command
        //----------------------- 
    }
    public enum EndVertexOrientation
    {
        Unknown, //0
        CCW,//1
        CW//2
    }


}
