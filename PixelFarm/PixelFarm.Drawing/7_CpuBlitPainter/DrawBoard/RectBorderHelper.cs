//MIT, 2018-present, WinterDev
using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{

    public class SimpleRectBorderBuilder
    {

        struct PointF
        {
            public readonly float X, Y;
            public PointF(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        //sum of both inner-outer border***
        public float LeftBorderWidth { get; set; }
        public float TopBorderHeight { get; set; }
        public float RightBorderWidth { get; set; }
        public float BottomBorderHeight { get; set; }

        public void SetBorderWidth(float leftBorderW, float topBorderH, float rightBorderW, float bottomBorderH)
        {
            LeftBorderWidth = leftBorderW;
            TopBorderHeight = topBorderH;
            RightBorderWidth = rightBorderW;
            BottomBorderHeight = bottomBorderH;
        }
        public void SetBorderWidth(float allside)
        {
            LeftBorderWidth =
            TopBorderHeight =
            RightBorderWidth =
            BottomBorderHeight = allside;
        }

        /// <summary>
        /// bound rect border around INNER side of specific reference rect bounds
        /// </summary>
        /// <param name="left">left rect bound</param>
        /// <param name="top">top rect bound</param>
        /// <param name="width">width rect bound</param>
        /// <param name="height">height rect bound</param>
        /// <param name="output16">float[16] output</param>
        public void BuildAroundInnerRefBounds(float left, float top, float width, float height, float[] output16)
        {

            //outer vertices
            var p0 = new PointF(left, top);
            var p1 = new PointF(left, top + height);
            var p2 = new PointF(left + width, top + height);
            var p3 = new PointF(left + width, top);

            //----------
            //inner vertices
            var p4 = new PointF(left + LeftBorderWidth, top + TopBorderHeight);
            var p5 = new PointF(left + width - RightBorderWidth, top + TopBorderHeight);
            var p6 = new PointF(left + width - RightBorderWidth, top + height - BottomBorderHeight);
            var p7 = new PointF(left + LeftBorderWidth, top + height - BottomBorderHeight);

            int index = 0;
            AppendCoord(p0, ref index, output16);
            AppendCoord(p1, ref index, output16);
            AppendCoord(p2, ref index, output16);
            AppendCoord(p3, ref index, output16);

            AppendCoord(p4, ref index, output16);
            AppendCoord(p5, ref index, output16);
            AppendCoord(p6, ref index, output16);
            AppendCoord(p7, ref index, output16);
        }

        /// <summary>
        ///  bound rect border around OUTER side of specific reference rect bounds
        /// </summary>
        /// <param name="left">left rect bound</param>
        /// <param name="top">top rect bound</param>
        /// <param name="width">width rect bound</param>
        /// <param name="height">height rect bound</param>
        /// <param name="output16">float[16] output</param>
        public void BuildAroundOuterRefBounds(float left, float top, float width, float height, float[] output16)
        {

            //outer vertices
            var p0 = new PointF(left - LeftBorderWidth, top - TopBorderHeight);
            var p1 = new PointF(left - LeftBorderWidth, top + height + BottomBorderHeight);
            var p2 = new PointF(left + width + RightBorderWidth, top + height + BottomBorderHeight);
            var p3 = new PointF(left + width + RightBorderWidth, top - TopBorderHeight);
            //----------
            //inner vertices
            var p4 = new PointF(left, top);
            var p5 = new PointF(left + width, top);
            var p6 = new PointF(left + width, top + height);
            var p7 = new PointF(left, top + height);

            int index = 0;
            AppendCoord(p0, ref index, output16);
            AppendCoord(p1, ref index, output16);
            AppendCoord(p2, ref index, output16);
            AppendCoord(p3, ref index, output16);

            AppendCoord(p4, ref index, output16);
            AppendCoord(p5, ref index, output16);
            AppendCoord(p6, ref index, output16);
            AppendCoord(p7, ref index, output16);
        }
        /// <summary>
        /// bound rect border over each side of specific reference rect bounds 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="output16"></param>
        public void BuildOverRefBounds(float left, float top, float width, float height, float[] output16)
        {

            //outer vertices
            var p0 = new PointF(left - LeftBorderWidth / 2, top - TopBorderHeight / 2);
            var p1 = new PointF(left - LeftBorderWidth / 2, top + height + BottomBorderHeight / 2);
            var p2 = new PointF(left + width + RightBorderWidth / 2, top + height + BottomBorderHeight / 2);
            var p3 = new PointF(left + width + RightBorderWidth / 2, top - TopBorderHeight / 2);

            //----------
            //inner vertices
            var p4 = new PointF(left + LeftBorderWidth / 2, top + TopBorderHeight / 2);
            var p5 = new PointF(left + width - RightBorderWidth / 2, top + TopBorderHeight / 2);
            var p6 = new PointF(left + width - RightBorderWidth / 2, top + height - BottomBorderHeight / 2);
            var p7 = new PointF(left + LeftBorderWidth / 2, top + height - BottomBorderHeight / 2);

            int index = 0;
            AppendCoord(p0, ref index, output16);
            AppendCoord(p1, ref index, output16);
            AppendCoord(p2, ref index, output16);
            AppendCoord(p3, ref index, output16);

            AppendCoord(p4, ref index, output16);
            AppendCoord(p5, ref index, output16);
            AppendCoord(p6, ref index, output16);
            AppendCoord(p7, ref index, output16);
        }


        static void AppendCoord(PointF p, ref int index, float[] outputArr)
        {
            outputArr[index] = p.X;
            outputArr[index + 1] = p.Y;
            index += 2;
        }


        //we can use this with output16 for a rect
        //Tess with NONZERO winding rule
        //(see https://github.com/PaintLab/PixelFarm/issues/44#issuecomment-449870315)
        public static ushort[] PrebuiltRectTessIndices = new ushort[]
        {
                1,7,0,
                7,1,2,
                7,2,6,
                6,2,5,
                0,4,3,
                4,0,7,
                3,4,5,
                3,5,2
        };

        public ushort[] GetPrebuiltRectTessIndices() => PrebuiltRectTessIndices;
    }



}