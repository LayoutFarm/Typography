//MIT, 2016-present, WinterDev 
namespace Typography.TextLayout
{
    public struct MeasuredStringBox
    {

        public readonly float width;
        public readonly float ascending;
        public readonly float descending;
        public readonly float lineGap;
        public readonly float btbd;//Baseline-to-Baseline Distance

        public MeasuredStringBox(float width, float ascending, float descending, float lineGap, float btbd)
        {
            this.width = width;
            this.ascending = ascending;
            this.descending = descending;
            this.lineGap = lineGap;
            this.btbd = btbd;
        }
        public static MeasuredStringBox operator *(MeasuredStringBox box, float scale)
        {
            //scale ***
            return new MeasuredStringBox(box.width * scale,
                box.ascending * scale,
                box.descending * scale,
                box.lineGap * scale,
                box.btbd * scale);
        }
    }

    public static class MeasuredStringBoxExtension
    {

        public static float CalculateLineHeight(this MeasuredStringBox box, float scale = 1)
        {
            return box.btbd;
            //return box.ascending - box.descending + box.lineGap;
        }
    }
}