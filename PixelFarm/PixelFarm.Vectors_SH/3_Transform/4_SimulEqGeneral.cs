//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// Bilinear 2D transformations
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    //============================================================matrix_pivot
    //template<uint Rows, uint Cols> 
    //===============================================================simul_eq
    //template<uint Size, uint RightCols>
    public static class SimulEqGeneral
    {
        public static bool Solve(double[,] left,
                         double[,] right,
                         double[,] result)
        {
            int rowCountRt = right.GetLength(0);
            int colCountRt = right.GetLength(1);
            double[,] mat = new double[rowCountRt, rowCountRt + colCountRt];
            //--------------------------------------
            //merge left and right matrix to tmp
            for (int i = 0; i < rowCountRt; i++)
            {
                for (int j = 0; j < rowCountRt; j++)
                {
                    mat[i, j] = left[i, j];
                }

                for (int j = 0; j < colCountRt; j++)
                {
                    mat[i, rowCountRt + j] = right[i, j];
                }
            }
            //--------------------------------------
            //try to make identity matrix
            int toalMatWidth = rowCountRt + colCountRt;
            for (int k = 0; k < rowCountRt; k++)
            {
                if (DoMatrixPivot(mat, rowCountRt, k) < 0)
                {
                    return false; // Singularity....
                }

                double a1 = mat[k, k];
                for (int j = k; j < toalMatWidth; j++)
                {
                    mat[k, j] /= a1;
                }

                for (int i = k + 1; i < rowCountRt; i++)
                {
                    a1 = mat[i, k];
                    for (int j = k; j < toalMatWidth; j++)
                    {
                        mat[i, j] -= a1 * mat[k, j];
                    }
                }
            }
            //--------------------------------------

            for (int k = 0; k < colCountRt; k++)
            {
                for (int m = rowCountRt - 1; m >= 0; m--)
                {
                    result[m, k] = mat[m, rowCountRt + k];
                    for (int j = m + 1; j < rowCountRt; j++)
                    {
                        result[m, k] -= mat[m, j] * result[j, k];
                    }
                }
            }
            return true;
        }
        static void SwapRow(double[,] arr, int a1Index0, int a2Index0)
        {
            int cols = arr.GetLength(1);
            for (int i = 0; i < cols; i++)
            {
                double tmp = arr[a1Index0, i];
                arr[a1Index0, i] = arr[a2Index0, i];
                arr[a2Index0, i] = tmp;
            }
        }

        static int DoMatrixPivot(double[,] m, int rowCount, int row)
        {
            int maxAtRow = row;
            double max_val = -1;
            for (int i = row; i < rowCount; i++)
            {
                double tmp;
                if ((tmp = Math.Abs(m[i, row])) > max_val && tmp != 0.0)
                {
                    max_val = tmp;
                    maxAtRow = i;
                }
            }

            if (m[maxAtRow, row] == 0.0)
            {
                return -1;
            }

            if (maxAtRow != row)
            {
                SwapRow(m, maxAtRow, row);
                return maxAtRow;
            }
            return 0;
        }
    }
}