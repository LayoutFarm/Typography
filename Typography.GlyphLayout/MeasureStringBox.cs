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

        ushort _stopAt;

        public MeasuredStringBox(float width,
            short ascending,
            short descending,
            short lineGap,
            short btbd,
            float pxscale)
        {
            this.width = width;
            _stopAt = 0;
            _ascending = ascending;
            _descending = descending;
            _lineGap = lineGap;
            _btbd = btbd;
            _pxscale = pxscale;

        }
        /// <summary>
        /// scaled ascending (in pixel)
        /// </summary>
        public float AscendingInPx => _ascending * _pxscale;
        /// <summary>
        /// scaled descending (in pixel)
        /// </summary>
        public float DescendingInPx => _descending * _pxscale;
        /// <summary>
        /// scaled line gap (in pixel)
        /// </summary>
        public float LineGapInPx => _lineGap * _pxscale;
        /// <summary>
        /// base-line-to-based line distance
        /// </summary>
        public float BtbdInPx => _btbd * _pxscale;
        public ushort StopAt
        {
            get => _stopAt;
            internal set => _stopAt = value;
        }

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
            measureBox._stopAt = box._stopAt;
            return measureBox;
        }
    }

    public static class MeasuredStringBoxExtension
    {

        public static float CalculateLineHeight(this MeasuredStringBox box, float scale = 1)
        {
            return box.BtbdInPx;
            //return box.ascending - box.descending + box.lineGap;
        }
    }
}