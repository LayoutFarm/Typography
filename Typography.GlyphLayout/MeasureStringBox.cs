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

        /// <summary>
        /// unscaled typographic ascending (sTypoAscending)
        /// </summary>
        readonly short _ascending; //2 bytes
        /// <summary>
        /// unscaled typographic descending (sTypoDescending)
        /// </summary>
        readonly short _descending;//2
        /// <summary>
        /// unscaled typographic linegap (sTypoLineGap)
        /// </summary>
        readonly short _lineGap; //2

        /// <summary>
        /// clip Ascending (usWinAscending)
        /// </summary>
        readonly ushort _clipDescending; //2
        /// <summary>
        /// clip Descending (usWinDescending)
        /// </summary>
        readonly ushort _clipAscending;//2

        ushort _stopAt;//2

        public MeasuredStringBox(float width,
            short ascending,
            short descending,
            short lineGap,
            ushort clipAscending,
            ushort clipDescending,
            float pxscale)
        {
            //baseline-to-baseline distance
            this.width = width;
            _stopAt = 0;

            _ascending = ascending;
            _descending = descending;
            _lineGap = lineGap;
            _clipAscending = clipAscending;
            _clipDescending = clipDescending;

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
        /// total clip height 
        /// </summary>
        public float ClipHeightInPx => (_clipAscending + _clipDescending) * _pxscale;
        public float ClipAscendingInPx => _clipAscending * _pxscale;
        public float ClipDescendingInPx => _clipDescending * _pxscale;



        /// <summary>       
        /// recommened linespace (base-line-to-based line distance)
        /// </summary>
        public float LineSpaceInPx => ((_ascending - _descending) + _lineGap) * _pxscale;

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
                                box._clipAscending,
                                box._clipDescending,
                                box._pxscale * scale
                                );
            measureBox._stopAt = box._stopAt;
            return measureBox;
        }
    }
}