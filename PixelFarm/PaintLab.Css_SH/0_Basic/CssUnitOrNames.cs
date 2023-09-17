//BSD, 2014-present, WinterDev
//ArthurHub, Jose Manuel Menendez Poo

namespace LayoutFarm.Css
{
    /// <summary>
    /// Represents the possible units of the CSS lengths
    /// </summary>
    /// <remarks>
    /// http://www.w3.org/TR/CSS21/syndata.html#length-units
    /// </remarks>
    public enum CssUnitOrNames : byte
    {
        //empty value must be 0, and auto must be 1 ****
        //(range usage)
        //------------------------------
        EmptyValue,//extension flags 
        AutoLength,//extension flags
        //------------------------------ 
        //W3C Unit
        Ems,
        Pixels,
        Ex,
        Inches,
        Centimeters,
        Milimeters,
        Points,
        Picas,
        //------------------------------ 

        Percent,//extension flags
        //------------------------------ 
        //names , 
        NormalLength,//extension flags 
        BorderThick,//extension flags
        BorderThin,//extension flags
        BorderMedium,//extension flags
        //------------------------------  
        MainSize,//css3 flex mainsize property
        //------------------------------ 
        //font size name
        FONTSIZE_MEDIUM,
        FONTSIZE_XX_SMALL,
        FONTSIZE_X_SMALL,
        FONTSIZE_SMALL,
        FONTSIZE_LARGE,
        FONTSIZE_X_LARGE,
        FONTSIZE_XX_LARGE,
        FONTSIZE_SMALLER,
        FONTSIZE_LARGER,
        //------------------------------  
        //background position
        LEFT,
        TOP,
        RIGHT,
        BOTTOM,
        CENTER,
        //------------------------------  
        Unknown,//extension flags 
    }
}