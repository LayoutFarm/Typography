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
namespace PixelFarm.CpuBlit.Rasterization
{
    partial class ScanlineRasterizer
    {
        enum FillingRule
        {
            NonZero,
            EvenOdd
        }

        //-----------------------------------------------------------------cell_aa
        // A pixel cell. There're no constructors defined and it was done ***
        // intentionally in order to avoid extra overhead when allocating an ****
        // array of cells. ***
        struct CellAA
        {
            public readonly int x;
            public readonly int y;
            public readonly int cover;
            public readonly int area;
#if DEBUG
#if !COSMOS
            public int dbugLeft;
            public int dbugRight;
#endif
#endif
            private CellAA(int x, int y, int cover, int area)
            {
                this.x = x;
                this.y = y;
                this.cover = cover;
                this.area = area;
#if DEBUG
#if !COSMOS
                dbugLeft = 0;
                dbugRight = 0;
#endif
#endif
            }

            public static CellAA Create(int x, int y, int cover, int area)
            {
                return new CellAA(x, y, cover, area);
                //CellAA cell = new CellAA();
                //cell.x = x;
                //cell.y = y;
                //cell.cover = cover;
                //cell.area = area;
                //return cell;
            }
#if DEBUG
            public static CellAA dbugCreate(int x, int y, int cover, int area, int left, int right)
            {
                CellAA cell = new CellAA(x, y, cover, area);
                //cell.x = x;
                //cell.y = y;
                //cell.cover = cover;
                //cell.area = area;
#if !COSMOS
                cell.dbugLeft = left;
                cell.dbugRight = right;
#endif
                return cell;
            }
#endif
#if DEBUG
#if !COSMOS
            public override string ToString()
            {
                return "x:" + x + ",y:" + y + ",cover:" + cover + ",area:" + area + ",left:" + dbugLeft + ",right:" + dbugRight;
            }
#endif
#endif

        }


        //-----------------------------------------------------rasterizer_cells_aa
        // An internal class that implements the main rasterization algorithm.
        // Used in the rasterizer. Should not be used directly.
        sealed class CellAARasterizer
        {
            int _num_used_cells;
            ArrayList<CellAA> _cells;
            ArrayList<CellAA> _sorted_cells;
            ArrayList<SortedY> _sorted_y;
            //------------------
            int _cCell_x;
            int _cCell_y;
            int _cCell_cover;
            int _cCell_area;
            //------------------
#if DEBUG
            int _cCell_left;
            int _cCell_right;
#endif
            //------------------
            int _min_x;
            int _min_y;

            int _max_x;
            int _max_y;
            bool _sorted;
            const int BLOCK_SHIFT = 12;
            const int BLOCK_SIZE = 1 << BLOCK_SHIFT;
            const int BLOCK_MASK = BLOCK_SIZE - 1;
            const int BLOCK_POOL = 256;
            const int BLOCK_LIMIT = BLOCK_SIZE * 1024;
            struct SortedY
            {
                internal int start;
                internal int num;
            }

            public CellAARasterizer()
            {
                _sorted_cells = new ArrayList<CellAA>();
                _sorted_y = new ArrayList<SortedY>();
                _min_x = (0x7FFFFFFF);
                _min_y = (0x7FFFFFFF);
                _max_x = (-0x7FFFFFFF);
                _max_y = (-0x7FFFFFFF);
                _sorted = false;
                ResetCurrentCell();
                _cells = new ArrayList<CellAA>(BLOCK_SIZE);
            }
            void ResetCurrentCell()
            {
                _cCell_x = 0x7FFFFFFF;
                _cCell_y = 0x7FFFFFFF;
                _cCell_cover = 0;
                _cCell_area = 0;
#if DEBUG
                _cCell_left = -1;
                _cCell_right = -1;
#endif
            }

            public void Reset()
            {
                _num_used_cells = 0;
                ResetCurrentCell();
                _sorted = false;
                _min_x = 0x7FFFFFFF;
                _min_y = 0x7FFFFFFF;
                _max_x = -0x7FFFFFFF;
                _max_y = -0x7FFFFFFF;
            }


            const int DX_LIMIT = (16384 << PolySubPix.SHIFT);
            const int POLY_SUBPIXEL_SHIFT = PolySubPix.SHIFT;
            const int POLY_SUBPIXEL_MASK = PolySubPix.MASK;
            const int POLY_SUBPIXEL_SCALE = PolySubPix.SCALE;
            public void DrawLine(int x1, int y1, int x2, int y2)
            {
                int dx = x2 - x1;
                if (dx >= DX_LIMIT || dx <= -DX_LIMIT)
                {
                    int cx = (x1 + x2) >> 1;
                    int cy = (y1 + y2) >> 1;
                    DrawLine(x1, y1, cx, cy);
                    DrawLine(cx, cy, x2, y2);
                }

                int dy = y2 - y1;
                int ex1 = x1 >> POLY_SUBPIXEL_SHIFT;
                int ex2 = x2 >> POLY_SUBPIXEL_SHIFT;
                int ey1 = y1 >> POLY_SUBPIXEL_SHIFT;
                int ey2 = y2 >> POLY_SUBPIXEL_SHIFT;
                int fy1 = y1 & POLY_SUBPIXEL_MASK;
                int fy2 = y2 & POLY_SUBPIXEL_MASK;
                int x_from, x_to;
                int p, rem, mod, lift, delta, first, incr;
                if (ex1 < _min_x) _min_x = ex1;
                if (ex1 > _max_x) _max_x = ex1;
                if (ey1 < _min_y) _min_y = ey1;
                if (ey1 > _max_y) _max_y = ey1;
                if (ex2 < _min_x) _min_x = ex2;
                if (ex2 > _max_x) _max_x = ex2;
                if (ey2 < _min_y) _min_y = ey2;
                if (ey2 > _max_y) _max_y = ey2;
                //***
                AddNewCell(ex1, ey1);
                //***
                //everything is on a single horizontal line
                if (ey1 == ey2)
                {
                    RenderHLine(ey1, x1, fy1, x2, fy2);
                    return;
                }

                //Vertical line - we have to calculate start and end cells,
                //and then - the common values of the area and coverage for
                //all cells of the line. We know exactly there's only one 
                //cell, so, we don't have to call render_hline().
                incr = 1;
                if (dx == 0)
                {
                    int ex = x1 >> POLY_SUBPIXEL_SHIFT;
                    int two_fx = (x1 - (ex << POLY_SUBPIXEL_SHIFT)) << 1;
                    int area;
                    first = POLY_SUBPIXEL_SCALE;
                    if (dy < 0)
                    {
                        first = 0;
                        incr = -1;
                    }

                    x_from = x1;
                    delta = first - fy1;
                    _cCell_cover += delta;
                    _cCell_area += two_fx * delta;
                    ey1 += incr;
                    //***
                    AddNewCell(ex, ey1);
                    //***
                    delta = first + first - POLY_SUBPIXEL_SCALE;
                    area = two_fx * delta;
                    while (ey1 != ey2)
                    {
                        _cCell_cover = delta;
                        _cCell_area = area;
                        ey1 += incr;
                        //***
                        AddNewCell(ex, ey1);
                        //***
                    }
                    delta = fy2 - POLY_SUBPIXEL_SCALE + first;
                    _cCell_cover += delta;
                    _cCell_area += two_fx * delta;
                    return;
                }

                //ok, we have to render several hlines
                p = (POLY_SUBPIXEL_SCALE - fy1) * dx;
                first = POLY_SUBPIXEL_SCALE;
                if (dy < 0)
                {
                    p = fy1 * dx;
                    first = 0;
                    incr = -1;
                    dy = -dy;
                }

                delta = p / dy;
                mod = p % dy;
                if (mod < 0)
                {
                    delta--;
                    mod += dy;
                }

                x_from = x1 + delta;
                RenderHLine(ey1, x1, fy1, x_from, first);
                ey1 += incr;
                //***
                AddNewCell(x_from >> POLY_SUBPIXEL_SHIFT, ey1);
                //***
                if (ey1 != ey2)
                {
                    p = POLY_SUBPIXEL_SCALE * dx;
                    lift = p / dy;
                    rem = p % dy;
                    if (rem < 0)
                    {
                        lift--;
                        rem += dy;
                    }
                    mod -= dy;
                    while (ey1 != ey2)
                    {
                        delta = lift;
                        mod += rem;
                        if (mod >= 0)
                        {
                            mod -= dy;
                            delta++;
                        }

                        x_to = x_from + delta;
                        //***
                        RenderHLine(ey1, x_from, POLY_SUBPIXEL_SCALE - first, x_to, first);
                        //***
                        x_from = x_to;
                        ey1 += incr;
                        //***
                        AddNewCell(x_from >> POLY_SUBPIXEL_SHIFT, ey1);
                        //***
                    }
                }
                RenderHLine(ey1, x_from, POLY_SUBPIXEL_SCALE - first, x2, fy2);
            }



            public int MinX => _min_x;
            public int MinY => _min_y;
            //
            public int MaxX => _max_x;
            public int MaxY => _max_y;

            public void SortCells()
            {
                if (_sorted) return; //Perform sort only the first time.
                WriteCurrentCell();
                //----------------------------------
                //reset current cell 
                _cCell_x = 0x7FFFFFFF;
                _cCell_y = 0x7FFFFFFF;
                _cCell_cover = 0;
                _cCell_area = 0;
                //----------------------------------

                if (_num_used_cells == 0) return;
                // Allocate the array of cell pointers 
                _sorted_cells.Allocate(_num_used_cells);
                // Allocate and zero the Y array
                _sorted_y.Allocate((int)(_max_y - _min_y + 1));
                _sorted_y.Zero();
                CellAA[] cells = _cells.UnsafeInternalArray;
                SortedY[] sortedYData = _sorted_y.UnsafeInternalArray;
                CellAA[] sortedCellsData = _sorted_cells.UnsafeInternalArray;
                // Create the Y-histogram (count the numbers of cells for each Y)
                for (int i = 0; i < _num_used_cells; ++i)
                {
                    int index = cells[i].y - _min_y;
                    sortedYData[index].start++;
                }

                // Convert the Y-histogram into the array of starting indexes
                int start = 0;
                int sortedYSize = _sorted_y.Count;
                for (int i = 0; i < sortedYSize; i++)
                {
                    int v = sortedYData[i].start;
                    sortedYData[i].start = start;
                    start += v;
                }

                // Fill the cell pointer array sorted by Y
                for (int i = 0; i < _num_used_cells; ++i)
                {
                    int sortedIndex = cells[i].y - _min_y;
                    int curr_y_start = sortedYData[sortedIndex].start;
                    int curr_y_num = sortedYData[sortedIndex].num;
                    sortedCellsData[curr_y_start + curr_y_num] = cells[i];
                    sortedYData[sortedIndex].num++;
                }

                // Finally arrange the X-arrays
                for (int i = 0; i < sortedYSize; i++)
                {
                    var yData = sortedYData[i];
                    if (yData.num != 0)
                    {
                        QuickSort.Sort(sortedCellsData,
                            yData.start,
                            yData.start + yData.num - 1);
                    }
                }
                _sorted = true;
            }

            public int TotalCells => _num_used_cells;


            public void GetCells(int y, out CellAA[] cellData, out int offset, out int num)
            {
                cellData = _sorted_cells.UnsafeInternalArray;
                SortedY d = _sorted_y[y - _min_y];
                offset = d.start;
                num = d.num;
            }

            //
            public bool Sorted => _sorted;
            //
            void AddNewCell(int x, int y)
            {
                WriteCurrentCell();
                _cCell_x = x;
                _cCell_y = y;
                //reset area and coverage after add new cell
                _cCell_cover = 0;
                _cCell_area = 0;
            }

            void WriteCurrentCell()
            {
                if ((_cCell_area | _cCell_cover) != 0)
                {
                    //check cell limit
                    if (_num_used_cells >= BLOCK_LIMIT)
                    {
                        return;
                    }
                    //------------------------------------------
                    //alloc if required
                    if ((_num_used_cells + 1) >= _cells.AllocatedSize)
                    {
                        _cells = new ArrayList<CellAA>(_cells, BLOCK_SIZE);
                    }
#if DEBUG
                    //m_cells.SetData(m_num_used_cells, CellAA.dbugCreate(
                    //    cCell_x, cCell_y,
                    //    cCell_cover, cCell_area,
                    //    cCell_left,
                    //    cCell_right));
                    _cells.SetData(_num_used_cells, CellAA.Create(
                     _cCell_x, _cCell_y,
                     _cCell_cover, _cCell_area));
#else
                     _cells.SetData(_num_used_cells, CellAA.Create(
                     _cCell_x, _cCell_y,
                     _cCell_cover, _cCell_area));
#endif
                    _num_used_cells++;
                }
            }

            void RenderHLine(int ey, int x1, int y1, int x2, int y2)
            {

                //trivial case. Happens often
                if (y1 == y2)
                {
                    //***
                    AddNewCell(x2 >> poly_subpix.SHIFT, ey);
                    //***
                    return;
                }
                int ex1 = x1 >> poly_subpix.SHIFT;
                int ex2 = x2 >> poly_subpix.SHIFT;

                int fx1 = x1 & (int)poly_subpix.MASK;
                int fx2 = x2 & (int)poly_subpix.MASK;
                int delta;
                //everything is located in a single cell.  That is easy!
                if (ex1 == ex2)
                {
                    delta = y2 - y1;
                    _cCell_cover += delta;
                    _cCell_area += (fx1 + fx2) * delta;
                    return;
                }
                //----------------------------
                int p, first, dx;
                int incr, lift, mod, rem;
                //----------------------------


                //ok, we'll have to render a run of adjacent cells on the same hline...
                p = ((int)poly_subpix.SCALE - fx1) * (y2 - y1);
                first = (int)poly_subpix.SCALE;
                incr = 1;
                dx = x2 - x1;
                if (dx < 0)
                {
                    p = fx1 * (y2 - y1);
                    first = 0;
                    incr = -1;
                    dx = -dx;
                }

                delta = p / dx;
                mod = p % dx;
                if (mod < 0)
                {
                    delta--;
                    mod += dx;
                }

                _cCell_cover += delta;
                _cCell_area += (fx1 + first) * delta;
                ex1 += incr;
                //***
                AddNewCell(ex1, ey);
                //***
                y1 += delta;
                if (ex1 != ex2)
                {
                    p = (int)poly_subpix.SCALE * (y2 - y1 + delta);
                    lift = p / dx;
                    rem = p % dx;
                    if (rem < 0)
                    {
                        lift--;
                        rem += dx;
                    }

                    mod -= dx;
                    while (ex1 != ex2)
                    {
                        delta = lift;
                        mod += rem;
                        if (mod >= 0)
                        {
                            mod -= dx;
                            delta++;
                        }

                        _cCell_cover += delta;
                        _cCell_area += (int)poly_subpix.SCALE * delta;
                        y1 += delta;
                        ex1 += incr;
                        //***
                        AddNewCell(ex1, ey);
                        //***
                    }
                }
                delta = y2 - y1;
                _cCell_cover += delta;
                _cCell_area += (fx2 + (int)poly_subpix.SCALE - first) * delta;
            }

            //------------
            static class QuickSort
            {
                public static void Sort(CellAA[] dataToSort)
                {
                    Sort(dataToSort, 0, dataToSort.Length - 1);
                }

                public static void Sort(CellAA[] dataToSort, int beg, int end)
                {
                    if (end == beg)
                    {
                        return;
                    }
                    else
                    {
                        int pivot = GetPivotPoint(dataToSort, beg, end);
                        if (pivot > beg)
                        {
                            Sort(dataToSort, beg, pivot - 1);
                        }

                        if (pivot < end)
                        {
                            Sort(dataToSort, pivot + 1, end);
                        }
                    }
                }

                static int GetPivotPoint(CellAA[] dataToSort, int begPoint, int endPoint)
                {
                    int pivot = begPoint;
                    int m = begPoint + 1;
                    int n = endPoint;
                    var x_at_PivotPoint = dataToSort[pivot].x;
                    while ((m < endPoint)
                        && x_at_PivotPoint >= dataToSort[m].x)
                    {
                        m++;
                    }

                    while ((n > begPoint) && (x_at_PivotPoint <= dataToSort[n].x))
                    {
                        n--;
                    }

                    while (m < n)
                    {
                        //swap data between m and n
                        CellAA temp = dataToSort[m];
                        dataToSort[m] = dataToSort[n];
                        dataToSort[n] = temp;
                        while ((m < endPoint) && (x_at_PivotPoint >= dataToSort[m].x))
                        {
                            m++;
                        }

                        while ((n > begPoint) && (x_at_PivotPoint <= dataToSort[n].x))
                        {
                            n--;
                        }
                    }

                    if (pivot != n)
                    {
                        CellAA temp2 = dataToSort[n];
                        dataToSort[n] = dataToSort[pivot];
                        dataToSort[pivot] = temp2;
                    }
                    return n;
                }
            }
        }
    }
}