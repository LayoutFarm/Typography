//MIT, 2018-present, WinterDev 
namespace PixelFarm.Drawing
{
    public enum LenUnit : byte
    {
        NotAssigned,
        Empty,
        //
        Pixel, //px
        Point, //pt
        Percent, //%
        Milimeters,//mm
        Centimeters,//cm
        Inches,//in
        Em, //em 
        Ex, //ex
        Picas, //pc 
    }

    public struct Len
    {
        public readonly float Number;
        public readonly LenUnit Unit;
        public Len(float number, LenUnit lengthUnit)
        {
            Number = number;
            Unit = lengthUnit;
        }

        public override string ToString()
        {
            string u = string.Empty;
            switch (Unit)
            {
                default: u = "???"; break;
                case LenUnit.NotAssigned: u = "!"; break;
                case LenUnit.Empty: u = ""; break;
                case LenUnit.Pixel: u = "px"; break;
                case LenUnit.Point: u = "pt"; break;
                case LenUnit.Percent: u = "%"; break;
                case LenUnit.Milimeters: u = "mm"; break;
                case LenUnit.Centimeters: u = "cm"; break;
                case LenUnit.Inches: u = "inch"; break;
                case LenUnit.Em: u = "em"; break;
                case LenUnit.Ex: u = "ex"; break;
                case LenUnit.Picas: u = "pc"; break;
            }
            return string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0}{1}", Number, u);
        }

        /// <summary>
        /// create len in pixel unit
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static Len Px(float number) => new Len(number, LenUnit.Pixel);

        /// <summary>
        /// create len in point unit
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static Len Pt(float number) => new Len(number, LenUnit.Point);


    }

    public static class LenExtensions
    {
        const int POINTS_PER_INCH = 72; //default value
        static int s_PIXELS_PER_INCH = 96; //default value        
        public static float ToPixels(this Len len)
        {
            switch (len.Unit)
            {
                case LenUnit.NotAssigned:
                case LenUnit.Empty:
                    return 0;

                case LenUnit.Pixel:
                    return len.Number;

                case LenUnit.Point:
                    return ((len.Number / POINTS_PER_INCH) * s_PIXELS_PER_INCH);

                default: throw new System.NotSupportedException();//TODO: implement this
            }
        }
        public static float ToPixels(this Len len, int pixelsPerInch)
        {
            switch (len.Unit)
            {
                case LenUnit.NotAssigned:
                case LenUnit.Empty:
                    return 0;

                case LenUnit.Pixel:
                    return len.Number; //same unit no conv

                case LenUnit.Point:
                    return ((len.Number / POINTS_PER_INCH) * pixelsPerInch);

                default: throw new System.NotSupportedException();//TODO: implement this
            }
        }
        public static float ToPoints(this Len len)
        {
            switch (len.Unit)
            {
                case LenUnit.NotAssigned:
                case LenUnit.Empty:
                    return 0;

                case LenUnit.Pixel:
                    return (len.Number / s_PIXELS_PER_INCH) * POINTS_PER_INCH;

                case LenUnit.Point:
                    return len.Number; //same unit no conv

                default: throw new System.NotSupportedException();//TODO: implement this
            }
        }
        public static float ToPoints(this Len len, int pixelsPerInch)
        {
            switch (len.Unit)
            {
                case LenUnit.NotAssigned:
                case LenUnit.Empty:
                    return 0;
                case LenUnit.Pixel:
                    return (len.Number / s_PIXELS_PER_INCH) * POINTS_PER_INCH;

                case LenUnit.Point:
                    return len.Number; //same

                default: throw new System.NotSupportedException();//TODO: implement this
            }
        }
    }



}