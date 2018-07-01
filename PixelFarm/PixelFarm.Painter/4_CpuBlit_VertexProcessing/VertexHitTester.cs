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
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

namespace PixelFarm.CpuBlit.VertexProcessing
{
    public static class VertexHitTester
    {
        //======= Crossings Multiply algorithm of InsideTest ======================== 
        //
        // By Eric Haines, 3D/Eye Inc, erich@eye.com
        //
        // This version is usually somewhat faster than the original published in
        // Graphics Gems IV; by turning the division for testing the X axis crossing
        // into a tricky multiplication test this part of the test became faster,
        // which had the additional effect of making the test for "both to left or
        // both to right" a bit slower for triangles than simply computing the
        // intersection each time.  The main increase is in triangle testing speed,
        // which was about 15% faster; all other polygon complexities were pretty much
        // the same as before.  On machines where division is very expensive (not the
        // case on the HP 9000 series on which I tested) this test should be much
        // faster overall than the old code.  Your mileage may (in fact, will) vary,
        // depending on the machine and the test data, but in general I believe this
        // code is both shorter and faster.  This test was inspired by unpublished
        // Graphics Gems submitted by Joseph Samosky and Mark Haigh-Hutchinson.
        // Related work by Samosky is in:
        //
        // Samosky, Joseph, "SectionView: A system for interactively specifying and
        // visualizing sections through three-dimensional medical image data",
        // M.S. Thesis, Department of Electrical Engineering and Computer Science,
        // Massachusetts Institute of Technology, 1993.
        //
        // Shoot a test ray along +X axis.  The strategy is to compare vertex Y values
        // to the testing point's Y and quickly discard edges which are entirely to one
        // side of the test ray.  Note that CONVEX and WINDING code can be added as
        // for the CrossingsTest() code; it is left out here for clarity.
        //
        // Input 2D polygon _pgon_ with _numverts_ number of vertices and test point
        // _point_, returns 1 if inside, 0 if outside.
        public static bool IsPointInVxs(PixelFarm.Drawing.VertexStore vxs, double tx, double ty)
        {
            int m_num_points = vxs.Count;
            if (m_num_points < 3) return false;
            // if (!m_in_polygon_check) return false;

            int j;
            bool yflag0, yflag1, inside_flag;
            double vtx0, vty0, vtx1, vty1;
            vxs.GetVertex(m_num_points -1, out vtx0, out vty0);
            //vtx0 = GetXN(m_num_points - 1);
            //vty0 = GetYN(m_num_points - 1);

            // get test bit for above/below X axis
            yflag0 = (vty0 >= ty);
            //vtx1 = GetXN(0);
            //vty1 = GetYN(0);
            vxs.GetVertex(0, out vtx1, out vty1);
            inside_flag = false;
            for (j = 1; j <= m_num_points; ++j)
            {
                yflag1 = (vty1 >= ty);
                // Check if endpoints straddle (are on opposite sides) of X axis
                // (i.e. the Y's differ); if so, +X ray could intersect this edge.
                // The old test also checked whether the endpoints are both to the
                // right or to the left of the test point.  However, given the faster
                // intersection point computation used below, this test was found to
                // be a break-even proposition for most polygons and a loser for
                // triangles (where 50% or more of the edges which survive this test
                // will cross quadrants and so have to have the X intersection computed
                // anyway).  I credit Joseph Samosky with inspiring me to try dropping
                // the "both left or both right" part of my code.
                if (yflag0 != yflag1)
                {
                    // Check intersection of pgon segment with +X ray.
                    // Note if >= point's X; if so, the ray hits it.
                    // The division operation is avoided for the ">=" test by checking
                    // the sign of the first vertex wrto the test point; idea inspired
                    // by Joseph Samosky's and Mark Haigh-Hutchinson's different
                    // polygon inclusion tests.
                    if (((vty1 - ty) * (vtx0 - vtx1) >=
                          (vtx1 - tx) * (vty0 - vty1)) == yflag1)
                    {
                        inside_flag = !inside_flag;
                    }
                }

                // Move to the next pair of vertices, retaining info as possible.
                yflag0 = yflag1;
                vtx0 = vtx1;
                vty0 = vty1;
                int k = (j >= m_num_points) ? j - m_num_points : j;
                //vtx1 = GetXN(k);
                //vty1 = GetYN(k);
                vxs.GetVertex(k, out vtx1, out vty1);
            }
            return inside_flag;
        }
    }
}