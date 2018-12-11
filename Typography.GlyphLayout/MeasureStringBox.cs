//MIT, 2016-present, WinterDev 
namespace Typography.TextLayout
{
    public struct MeasuredStringBox
    {
        /// <summary>
        /// pixel scaled size
        /// </summary>
        public readonly float width; //4 
        readonly float _pxscale; //4

        readonly short _ascending; //2
        readonly short _descending;//2
        readonly short _lineGap; //2
        readonly short _btbd;//Baseline-to-Baseline Distance,  2 byte

        public ushort stopAt;

        public MeasuredStringBox(float width,
            short ascending,
            short descending,
            short lineGap,
            short btbd,
            float pxscale)
        {
            this.width = width;
            this.stopAt = 0;
            _ascending = ascending;
            _descending = descending;
            _lineGap = lineGap;
            _btbd = btbd;
            _pxscale = pxscale;

        }
        public float ascending => _ascending * _pxscale;
        public float descending => _descending * _pxscale;
        public float lineGap => _lineGap * _pxscale;
        public float btbd => _btbd * _pxscale;

        public static MeasuredStringBox operator *(MeasuredStringBox box, float scale)
        {
            //scale ***
            var measureBox = new MeasuredStringBox(box.width * scale,
                                box._ascending,
                                box._descending,
                                box._lineGap,
                                box._btbd,
                                box._pxscale * scale
                                );
            measureBox.stopAt = box.stopAt;
            return measureBox;
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