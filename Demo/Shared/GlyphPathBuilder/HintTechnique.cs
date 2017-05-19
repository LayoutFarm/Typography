//MIT, 2016-2017, WinterDev
namespace Typography.Rendering
{

    public enum HintTechnique : byte
    {
        /// <summary>
        /// no hinting
        /// </summary>
        None,
        /// <summary>
        /// truetype instruction
        /// </summary>
        TrueTypeInstruction,
        /// <summary>
        /// truetype instruction vertical only
        /// </summary>
        TrueTypeInstruction_VerticalOnly,
        /// <summary>
        /// custom hint
        /// </summary>
        CustomAutoFit
    }
}