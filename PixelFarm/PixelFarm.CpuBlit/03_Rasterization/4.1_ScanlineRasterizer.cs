//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
//
// The author gratefully acknowleges the support of David Turner, 
// Robert Wilhelm, and Werner Lemberg - the authors of the FreeType 
// libray - in producing this work. See http://www.freetype.org for details.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// Adaptation for 32-bit screen coordinates has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------

using poly_subpix = PixelFarm.CpuBlit.Rasterization.PolySubPix;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;
namespace PixelFarm.CpuBlit.Rasterization
{

    //==================================================rasterizer_scanline_aa
    // Polygon rasterizer that is used to render filled polygons with 
    // high-quality Anti-Aliasing. Internally, by default, the class uses 
    // integer coordinates in format 24.8, i.e. 24 bits for integer part 
    // and 8 bits for fractional - see poly_subpixel_shift. This class can be 
    // used in the following  way:
    //
    // 1. filling_rule(filling_rule_e ft) - optional.
    //
    // 2. gamma() - optional.
    //
    // 3. reset()
    //
    // 4. move_to(x, y) / line_to(x, y) - make the polygon. One can create 
    //    more than one contour, but each contour must consist of at least 3
    //    vertices, i.e. move_to(x1, y1); line_to(x2, y2); line_to(x3, y3);
    //    is the absolute minimum of vertices that define a triangle.
    //    The algorithm does not check either the number of vertices nor
    //    coincidence of their coordinates, but in the worst case it just 
    //    won't draw anything.
    //    The order of the vertices (clockwise or counterclockwise) 
    //    is important when using the non-zero filling rule (fill_non_zero).
    //    In this case the vertex order of all the contours must be the same
    //    if you want your intersecting polygons to be without "holes".
    //    You actually can use different vertices order. If the contours do not 
    //    intersect each other the order is not important anyway. If they do, 
    //    contours with the same vertex order will be rendered without "holes" 
    //    while the intersecting contours with different orders will have "holes".
    //
    // filling_rule() and gamma() can be called anytime before "sweeping".
    //------------------------------------------------------------------------


    public class PrebuiltGammaTable
    {
        internal readonly int[] _gammaLut = new int[ScanlineRasterizer.AA_SCALE];
        public PrebuiltGammaTable(IGammaFunction gamma_function)
        {
            for (int i = ScanlineRasterizer.AA_SCALE - 1; i >= 0; --i)
            {
                _gammaLut[i] = AggMath.uround(
                    gamma_function.GetGamma((float)(i) / ScanlineRasterizer.AA_MASK) * ScanlineRasterizer.AA_MASK);
            }
        }
        private PrebuiltGammaTable() { }

        public static PrebuiltGammaTable CreateSameValuesGammaTable(int value)
        {
            //TODO: review the name,
            //in this case, it should not be call gamma table?
            PrebuiltGammaTable table = new PrebuiltGammaTable();
            for (int i = ScanlineRasterizer.AA_SCALE - 1; i >= 0; --i)
            {
                table._gammaLut[i] = value;
            }
            return table;
        }

    }

    public sealed partial class ScanlineRasterizer
    {
        readonly CellAARasterizer _cellAARas;
        readonly VectorClipper _vectorClipper;

        int[] _gammaLut;//current gamma lut
        bool _useDefaultGammaLut;
        readonly int[] _orgGammaLut = new int[AA_SCALE]; //original(built-in) gamma table


        FillingRule _filling_rule;
        bool _auto_close;
        /// <summary>
        /// multiplied move to start x
        /// </summary>
        int _mul_start_x;
        /// <summary>
        /// multiplied move to starty
        /// </summary>
        int _mul_start_y;
        Status _status;
        int _scan_y;
        //---------------------------
        const int AA_SHIFT = 8;
        internal const int AA_SCALE = 1 << AA_SHIFT; //256
        internal const int AA_MASK = AA_SCALE - 1;   //255, or oxff
        const int AA_SCALE2 = AA_SCALE * 2;
        const int AA_MASK2 = AA_SCALE2 - 1;
        //---------------------------

        Q1Rect _userModeClipBox;
        //---------------

        enum Status
        {
            Initial,
            MoveTo,
            LineTo,
            Closed
        }



        public ScanlineRasterizer()
        {

            _cellAARas = new CellAARasterizer();
            _vectorClipper = new VectorClipper(_cellAARas);
            _filling_rule = FillingRule.NonZero;
            _auto_close = true;
            _mul_start_x = 0;
            _mul_start_y = 0;
            _status = Status.Initial;
            for (int i = AA_SCALE - 1; i >= 0; --i)
            {
                _orgGammaLut[i] = i;
            }
            _gammaLut = _orgGammaLut;
            _useDefaultGammaLut = true;
        }

        //--------------------------------------------------------------------
        /// <summary>
        /// reset scanlineRas cell and status
        /// </summary>
        public void Reset()
        {
            _cellAARas.Reset();
            _status = Status.Initial;
        }

        public Q1Rect GetVectorClipBox() => _userModeClipBox;

        public void SetClipBox(Q1Rect clippingRect)
        {
            SetClipBox(clippingRect.Left, clippingRect.Bottom, clippingRect.Right, clippingRect.Top);
        }
        public void SetClipBox(int left, int bottom, int right, int top)
        {

            left += (int)OffsetOriginX;
            bottom += (int)OffsetOriginY;
            right += (int)OffsetOriginX;
            top += (int)OffsetOriginY;


            _userModeClipBox = new Q1Rect(left, bottom, right, top);
            Reset();
            _vectorClipper.SetClipBox(
                                upscale(left), upscale(bottom),
                                upscale(right), upscale(top));
        }
        //---------------------------------
        //from vector clipper
        static int upscale(double v) => AggMath.iround(v * poly_subpix.SCALE);
        static int upscale(int v) => v << poly_subpix.SHIFT;
        //
        ////from vector clipper
        //static int downscale(int v)
        //{
        //    return v / (int)poly_subpix.SCALE;
        //}
        //---------------------------------
        public FillingRule ScanlineFillingRule
        {
            get => _filling_rule;
            set => _filling_rule = value;
        }

        //--------------------------------------------------------------------
        public void ResetGamma(IGammaFunction gamma_function)
        {
            if (_useDefaultGammaLut)
            {
                //
                for (int i = AA_SCALE - 1; i >= 0; --i)
                {
                    _gammaLut[i] = AggMath.uround(
                        gamma_function.GetGamma((float)(i) / AA_MASK) * AA_MASK);
                }
            }
        }
        public void SetGammaLut(PrebuiltGammaTable prebuiltGammaTable)
        {
            if (prebuiltGammaTable != null)
            {
                _useDefaultGammaLut = false;
                _gammaLut = prebuiltGammaTable._gammaLut;
            }
            else
            {
                _useDefaultGammaLut = true;
                _gammaLut = _orgGammaLut;

            }
        }
        //------------------------------------------------------------------------
        void MoveTo(double x, double y)
        {
            if (_cellAARas.Sorted) { Reset(); }
            if (_auto_close) { ClosePolygon(); }

            _vectorClipper.MoveTo(
                _mul_start_x = upscale(x),
                _mul_start_y = upscale(y));
            _status = Status.MoveTo;
        }
        //------------------------------------------------------------------------
        void LineTo(double x, double y)
        {
            _vectorClipper.LineTo(upscale(x), upscale(y));
            _status = Status.LineTo;
        }

#if DEBUG
        public void dbugDevMoveTo(double x, double y) => MoveTo(x, y);
        public void dbugDevLineTo(double x, double y) => LineTo(x, y);
#endif

        void ClosePolygon()
        {
            if (_status == Status.LineTo)
            {
                _vectorClipper.LineTo(_mul_start_x, _mul_start_y);
                _status = Status.Closed;
            }
        }


        void AddVertex(VertexCmd cmd, double x, double y)
        {
            switch (cmd)
            {
                case VertexCmd.MoveTo:
                    MoveTo(x, y);
                    break;
                case VertexCmd.LineTo:
                case VertexCmd.C3:
                case VertexCmd.C4:
                    LineTo(x, y);
                    break;
                case VertexCmd.Close:
                    ClosePolygon();
                    break;
                default:
                    {
                    }
                    break;
            }
        }
        ////------------------------------------------------------------------------
        //void Edge(double x1, double y1, double x2, double y2)
        //{
        //    if (m_cellAARas.Sorted) { Reset(); }
        //    m_vectorClipper.MoveTo(upscale(x1), upscale(y1));
        //    m_vectorClipper.LineTo(upscale(x2), upscale(y2));
        //    m_status = Status.MoveTo;
        //}
        //-------------------------------------------------------------------
        public float OffsetOriginX { get; internal set; }
        public float OffsetOriginY { get; internal set; }
        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public void AddPath(VertexStore vxs, VertexProcessing.ICoordTransformer tx)
        {

            //-----------------------------------------------------
            //*** we extract vertext command and coord(x,y) from
            //the snap but not store the snap inside rasterizer
            //----------------------------------------------------- 
            double x = 0;
            double y = 0;
            VertexCmd cmd;
            int index = 0;

            if (_cellAARas.Sorted) { Reset(); }
            float offsetOrgX = OffsetOriginX;
            float offsetOrgY = OffsetOriginY;

#if DEBUG
            int dbugVertexCount = 0;
#endif

            if (ExtendWidthX3ForSubPixelLcdEffect)
            {
                while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
                {
#if DEBUG
                    dbugVertexCount++;
#endif
                    //---------------------------------------------
                    //NOTE: we scale horizontal 3 times.
                    //subpixel renderer will shrink it to 1 
                    //--------------------------------------------- 
                    //TODO: review here
                    x *= 3;
                    tx.Transform(ref x, ref y); //***  
                    AddVertex(cmd, x + offsetOrgX, y + offsetOrgY);
                }
            }
            else
            {
                while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
                {
#if DEBUG
                    dbugVertexCount++;
#endif

                    //
                    tx.Transform(ref x, ref y); //***
                    AddVertex(cmd, x + offsetOrgX, y + offsetOrgY);
                }
            }
        }

        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public void AddPath(VertexStore vxs)
        {

            //-----------------------------------------------------
            //*** we extract vertext command and coord(x,y) from
            //the snap but not store the snap inside rasterizer
            //----------------------------------------------------- 
            double x = 0;
            double y = 0;
            VertexCmd cmd;
            int index = 0;

            if (_cellAARas.Sorted) { Reset(); }
            float offsetOrgX = OffsetOriginX;
            float offsetOrgY = OffsetOriginY;

#if DEBUG
            int dbugVertexCount = 0;
#endif

            if (ExtendWidthX3ForSubPixelLcdEffect)
            {
                while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
                {
#if DEBUG
                    dbugVertexCount++;
#endif
                    //---------------------------------------------
                    //NOTE: we scale horizontal 3 times.
                    //subpixel renderer will shrink it to 1 
                    //--------------------------------------------- 
                    AddVertex(cmd, (x + offsetOrgX) * 3, y + offsetOrgY);
                }
            }
            else
            {
                while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
                {
#if DEBUG
                    dbugVertexCount++;
#endif

                    AddVertex(cmd, x + offsetOrgX, y + offsetOrgY);
                }
            }
        }


        bool _extendWidthX3ForSubPixelLcdEffect;
        public bool ExtendWidthX3ForSubPixelLcdEffect
        {
            get => _extendWidthX3ForSubPixelLcdEffect;
            //expand to 3 times
            set => _vectorClipper.SetClipBoxWidthX3ForSubPixelLcdEffect(_extendWidthX3ForSubPixelLcdEffect = value);
        }
        public int MinX => _cellAARas.MinX;
        public int MinY => _cellAARas.MinY;
        public int MaxX => _cellAARas.MaxX;
        public int MaxY => _cellAARas.MaxY;
        //--------------------------------------------------------------------
        void Sort()
        {
            if (_auto_close) { ClosePolygon(); }

            _cellAARas.SortCells();
        }

        //------------------------------------------------------------------------
        internal bool RewindScanlines()
        {
            if (_auto_close) { ClosePolygon(); }

            _cellAARas.SortCells();
            if (_cellAARas.TotalCells == 0) return false;
            _scan_y = _cellAARas.MinY;
            return true;
        }


        //--------------------------------------------------------------------
        int CalculateAlpha(int area)
        {
            //REF: agg_rasterizer_scanline_aa.h
            //AGG_INLINE unsigned calculate_alpha(int area) const
            //{
            //        int cover = area >> (poly_subpixel_shift * 2 + 1 - aa_shift);

            //        if (cover < 0) cover = -cover;
            //        if (m_filling_rule == fill_even_odd)
            //        {
            //            cover &= aa_mask2;
            //            if (cover > aa_scale)
            //            {
            //                cover = aa_scale2 - cover;
            //            }
            //        }
            //        if (cover > aa_mask) cover = aa_mask;
            //        return m_gamma[cover];
            //    }

            int cover = area >> (poly_subpix.SHIFT * 2 + 1 - AA_SHIFT);
            if (cover < 0)
            {
                cover = -cover;
            }

            if (_filling_rule == FillingRule.EvenOdd)
            {
                cover &= AA_SCALE2;
                if (cover > AA_SCALE)
                {
                    cover = AA_SCALE2 - cover;
                }
            }

            if (cover > AA_MASK)
            {
                cover = AA_MASK;
            }
            //look up from gamma
            return _gammaLut[cover];
        }

        //--------------------------------------------------------------------
        internal bool SweepScanline(Scanline scline)
        {
            //see original agg=> agg_rasterizer_scanline_aa.h => sweep_scanline()

            for (; ; )
            {
                if (_scan_y > _cellAARas.MaxY)
                {
                    return false;
                }

                scline.ResetSpans();
                //------------------------- 

                _cellAARas.GetCells(_scan_y, out CellAA[] cells, out int offset, out int num_cells);

                int cover = 0;
                while (num_cells != 0)
                {
                    unsafe
                    {
#if COSMOS
                        int pnt = 0;
                        CellAA cell = cells[pnt + offset];
                        int x = cell.x;
                        int area = cell.area;
                        cover += cell.cover;
                        //accumulate all cells with the same X
                        while (--num_cells != 0)
                        {
                            offset++; //move next
                            cell = cells[pnt + offset];
                            if (cell.x != x)
                            {
                                break;
                            }

                            area += cell.area;
                            cover += cell.cover;
                        }
                        if (area != 0)
                        {
                            //-----------------------------------------------
                            //single cell, for antialias look
                            //-----------------------------------------------
                            //calculate alpha from coverage value
                            int alpha = CalculateAlpha((cover << (poly_subpix.SHIFT + 1)) - area);
                            if (alpha != 0)
                            {
                                scline.AddCell(x, alpha);
                            }

                            x++;
                        }

                        if ((num_cells != 0) && (cell.x > x))
                        {
                            //-----------------------------------------------
                            //this is long span , continuous color, solid look
                            //-----------------------------------------------
                            //calculate alpha from coverage value
                            int alpha = CalculateAlpha(cover << (poly_subpix.SHIFT + 1));
                            if (alpha != 0)
                            {
                                scline.AddSpan(x, (cell.x - x), alpha);
                            }
                        }

#else
                        fixed (CellAA* cur_cell_h = &cells[0])
                        {
                            CellAA* cur_cell_ptr = cur_cell_h + offset;
                            int x = cur_cell_ptr->x;
                            int area = cur_cell_ptr->area;
                            cover += cur_cell_ptr->cover;
                            //accumulate all cells with the same X
                            while (--num_cells != 0)
                            {
                                offset++; //move next
                                cur_cell_ptr++; //move next
                                if (cur_cell_ptr->x != x)
                                {
                                    break;
                                }
                                area += cur_cell_ptr->area;
                                cover += cur_cell_ptr->cover;
                            }

                            if (area != 0)
                            {
                                //-----------------------------------------------
                                //single cell, for antialias look
                                //-----------------------------------------------
                                //calculate alpha from coverage value
                                int alpha = CalculateAlpha((cover << (poly_subpix.SHIFT + 1)) - area);
                                if (alpha != 0)
                                {
                                    scline.AddCell(x, alpha);
                                }

                                x++;
                            }

                            if ((num_cells != 0) && (cur_cell_ptr->x > x))
                            {
                                //-----------------------------------------------
                                //this is long span , continuous color, solid look
                                //-----------------------------------------------
                                //calculate alpha from coverage value
                                int alpha = CalculateAlpha(cover << (poly_subpix.SHIFT + 1));
                                if (alpha != 0)
                                {
                                    scline.AddSpan(x, (cur_cell_ptr->x - x), alpha);
                                }
                            }
                        }
#endif
                    }
                }

                if (scline.SpanCount != 0) { break; }

                ++_scan_y;
            }

            scline.CloseLine(_scan_y);
            ++_scan_y;
            return true;
        }
    }
}

