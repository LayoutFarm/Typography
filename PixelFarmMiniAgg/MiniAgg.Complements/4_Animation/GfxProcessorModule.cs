//BSD, 2014-2016, WinterDev
//BSD (Oct,November) 2008, (Jan 2009), 2014 WinterDev

using System;
namespace PixelFarm.Drawing.Animation
{
    public class ArtGfxInstructionInfo
    {
        public const int ROLE_BEGIN = 1;
        public const int ROLE_END = 2;
        public const int ROLE_BEH_COUNT = 3;
        public const int BEH_BEGIN = 4;
        public const int BEH_END = 5;
        public const int STATE_COUNT = 6;
        public const int STATE_INSTRUCTION_BEGIN = 7;
        public const int STATE_INSTRUCTION_END = 8;
        public const int EVENT_ID = 9;
        //---------------------------  
        public const int BEH_STATE_ANIMATION_COUNT = 10;
        //--------------------------- 

        public const int DT_BYTE = 1;
        public const int DT_INT16 = 2;
        public const int DT_INT32 = 3;
        public const int DT_INT64 = 4;
        public const int DT_DATETIME = 5;
        public const int DT_STRING = 6;
        public const int DT_COLOR = 7;
        public const int DT_OBJECT = 8;
        public const int DT_CURSOR = 9;
        public const int DT_BRUSH = 11;
        public const int DT_CSS = 13;
        public readonly int instructionId;
        public readonly int moduleId;
        public readonly GfxProcessorModule module;
        public readonly bool isAnimatable;
        public ArtGfxInstructionInfo(GfxProcessorModule module, int instructionId, bool isAnimatable)
        {
            this.module = module;
            this.moduleId = module.ModuleId;
            this.instructionId = instructionId;
            this.isAnimatable = isAnimatable;
        }


        public static ArtGfxInstructionInfo RegisterGfxInfo(GfxProcessorModule module, int instructionId, bool isAnimatable, ArtGfxInstructionInfo[] table)
        {
            table[instructionId] = new ArtGfxInstructionInfo(module, instructionId, isAnimatable);
            return table[instructionId];
        }
    }


    public abstract class GfxProcessorModule
    {
#if DEBUG
        public string moduleName;
#endif
        public GfxProcessorModule()
        {
        }

        public abstract int ModuleId
        {
            get;
        }


        public abstract ArtGfxInstructionInfo GetGfxInfo(int instructionId);
#if DEBUG
        public override string ToString()
        {
            return moduleName;
        }
#endif

    }

    public static class GfxColor
    {
        public const int COLOR_ALPHA_COMPO = 2;
        public const int COLOR_RED_COMPO = 3;
        public const int COLOR_GREEN_COMPO = 4;
        public const int COLOR_BLUE_COMPO = 5;
        public const int COLOR_SOLID = 6;
        public const int COLOR_GRADIENT_LINEAR = 7;
        public const int COLOR_GRADIENT_CIRCULAR = 8;
    }
}