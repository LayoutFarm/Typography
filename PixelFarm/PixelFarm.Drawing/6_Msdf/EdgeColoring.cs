//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port)
using System;
using System.Collections.Generic;

namespace Msdfgen
{

    /// <summary>
    /// Assigns colors to edges of the shape in accordance to the multi-channel distance field technique.
    /// May split some edges if necessary. 
    /// angleThreshold specifies the maximum angle(in radians) to be considered a corner, for example 3 (~172 degrees).
    /// Values below 1/2 PI will be treated as the external angle.  
    /// </summary>
    public static class EdgeColoring
    {

        static bool isCorner(Vector2 aDir, Vector2 bDir, double crossThreshold)
        {
            return Vector2.dotProduct(aDir, bDir) <= 0 || Math.Abs(Vector2.crossProduct(aDir, bDir)) > crossThreshold;
        }

        static void switchColor(ref EdgeColor color, ref ulong seed, EdgeColor banned = EdgeColor.BLACK)
        {
            EdgeColor combined = color & banned;

            if (combined == EdgeColor.RED || combined == EdgeColor.GREEN || combined == EdgeColor.BLUE)
            {
                color = combined ^ EdgeColor.WHITE;
                return;
            }
            if (color == EdgeColor.BLACK || color == EdgeColor.WHITE)
            {
                //original code use array
                //EdgeColor[] start = new EdgeColor[] { EdgeColor.CYAN, EdgeColor.MAGENTA, EdgeColor.YELLOW };
                //color = start[seed % 3];

                //I don't want to alloc new array=>  I use switch
                switch (seed % 3)
                {
                    default: throw new NotSupportedException();
                    case 0: color = EdgeColor.CYAN; break;
                    case 1: color = EdgeColor.MAGENTA; break;
                    case 2: color = EdgeColor.YELLOW; break;
                }

                seed /= 3;
                return;
            }

            int shifted = (int)color << (1 + ((int)seed & 1));
            color = (EdgeColor)((shifted | shifted >> 3) & (int)EdgeColor.WHITE);
            seed >>= 1;
        }


        public static void edgeColoringSimple(Shape shape, double angleThreshold, ulong seed = 0)
        {
            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>(); //TODO: review reusable list


            // for (std::vector<Contour>::iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
            foreach (Contour contour in shape.contours)
            {
                // Identify corners 
                corners.Clear();
                List<EdgeSegment> edges = contour.edges;
                int edgeCount = edges.Count;
                if (edgeCount != 0)
                {

                    //original
                    Vector2 prevDirection = edges[edgeCount - 1].direction(1);// (*(contour->edges.end() - 1))->direction(1); 
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        EdgeSegment edge = edges[i];
                        if (isCorner(prevDirection.normalize(),
                            edge.direction(0).normalize(), crossThreshold))
                        {
                            corners.Add(i);
                        }
                        prevDirection = edge.direction(1);
                    }


                }

                // Smooth contour
                if (corners.Count == 0) //is empty
                {
                    for (int i = edgeCount - 1; i >= 0; --i)
                    {
                        edges[i].color = EdgeColor.WHITE;
                    }

                }
                else if (corners.Count == 1)
                {
                    // "Teardrop" case
                    EdgeColor[] colors = { EdgeColor.WHITE, EdgeColor.WHITE, EdgeColor.BLACK };
                    switchColor(ref colors[0], ref seed);
                    colors[2] = colors[0];
                    switchColor(ref colors[2], ref seed);

                    int corner = corners[0];
                    if (edgeCount >= 3)
                    {
                        int m = edgeCount;
                        for (int i = 0; i < m; ++i)
                        {
                            //TODO: review here 
                            contour.edges[(corner + i) % m].color = colors[((int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3) + 1];
                            //(colors + 1)[int(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
                        }
                    }
                    else if (edgeCount >= 1)
                    {
                        // Less than three edge segments for three colors => edges must be split
                        EdgeSegment[] parts = new EdgeSegment[7]; //empty array, TODO: review array alloc here
                        edges[0].splitInThirds(
                            out parts[0 + 3 * corner],
                            out parts[1 + 3 * corner],
                            out parts[2 + 3 * corner]);

                        if (edgeCount >= 2)
                        {
                            edges[1].splitInThirds(
                                out parts[3 - 3 * corner],
                                out parts[4 - 3 * corner],
                                out parts[5 - 3 * corner]
                                );
                            parts[0].color = parts[1].color = colors[0];
                            parts[2].color = parts[3].color = colors[1];
                            parts[4].color = parts[5].color = colors[2];
                        }
                        else
                        {
                            parts[0].color = colors[0];
                            parts[1].color = colors[1];
                            parts[2].color = colors[2];
                        }
                        contour.edges.Clear();
                        for (int i = 0; i < 7; ++i)
                        {
                            edges.Add(parts[i]);
                        }
                    }
                }
                // Multiple corners
                else
                {

                    //original
                    int cornerCount = corners.Count;
                    int spline = 0;
                    int start = corners[0];

                    EdgeColor color = EdgeColor.WHITE;
                    switchColor(ref color, ref seed);
                    EdgeColor initialColor = color;
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        int index = (start + i) % edgeCount;
                        if (spline + 1 < cornerCount && corners[spline + 1] == index)
                        {
                            ++spline;
                            switchColor(ref color, ref seed, (EdgeColor)(((spline == cornerCount - 1) ? 1 : 0) * (int)initialColor));
                        }
                        edges[index].color = color;
                    }


                }
            }
        }

    }
}
