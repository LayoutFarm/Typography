//BSD, 2014-present, WinterDev

/*
 * Created by SharpDevelop.
 * User: lbrubaker
 * Date: 3/26/2010
 * Time: 4:37 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Tesselate;

namespace DrawingGL
{
    struct TessVertex2d
    {
        public double m_X;
        public double m_Y;
        public TessVertex2d(double x, double y)
        {
            m_X = x;
            m_Y = y;
        }
#if DEBUG
        public override string ToString()
        {
            return this.m_X + "," + this.m_Y;
        }
#endif

    }

    /// <summary>
    /// listen and handle the event from tesslator
    /// </summary>
    class TessListener
    {
        internal List<TessVertex2d> _tempVertexList = new List<TessVertex2d>();
        internal List<ushort> _resultIndexList = new List<ushort>();
        int _inputVertexCount;
        Tesselator.TriangleListType _triangleListType;


        public TessListener()
        {
            //empty not use
            //not use first item in temp
            _tempVertexList.Add(new TessVertex2d(0, 0));
        }

        void OnBegin(Tesselator.TriangleListType type)
        {
            if (type != Tesselator.TriangleListType.Triangles)
            {

            }
            _triangleListType = type;

            //what type of triangle list
            //Console.WriteLine("begin: " + type.ToString());
            //Assert.IsTrue(GetNextOutputAsString() == "B");
            //switch (type)
            //{
            //    case Tesselator.TriangleListType.Triangles:
            //        Assert.IsTrue(GetNextOutputAsString() == "TRI");
            //        break;

            //    case Tesselator.TriangleListType.TriangleFan:
            //        Assert.IsTrue(GetNextOutputAsString() == "FAN");
            //        break;

            //    case Tesselator.TriangleListType.TriangleStrip:
            //        Assert.IsTrue(GetNextOutputAsString() == "STRIP");
            //        break;

            //    default:
            //        throw new Exception("unknown TriangleListType '" + type.ToString() + "'.");
            //}
        }

        void OnEnd()
        {
            //Assert.IsTrue(GetNextOutputAsString() == "E");
            //Console.WriteLine("end");
        }

        void OnVertex(int index)
        {
            //Assert.IsTrue(GetNextOutputAsString() == "V");
            //Assert.AreEqual(GetNextOutputAsInt(), index); 
            if (index < 0)
            {
                //use data from temp store***
                //that will be append to the end of result
                _resultIndexList.Add((ushort)(_inputVertexCount + (-index)));

                //resultVertexList.Add(this.tempVertextList[-index]);
                //Console.WriteLine("temp_v_cb:" + index + ":(" + tempVertextList[-index] + ")");
            }
            else
            {
                _resultIndexList.Add((ushort)index);
                //resultVertexList.Add(this.inputVertextList[index]);
                // Console.WriteLine("v_cb:" + index + ":(" + inputVertextList[index] + ")");
            }
        }

        void OnEdgeFlag(bool IsEdge)
        {
            //Console.WriteLine("edge: " + IsEdge);
            //Assert.IsTrue(GetNextOutputAsString() == "F");
            //Assert.AreEqual(GetNextOutputAsBool(), IsEdge);
        }

        void OnCombine(double v0,
          double v1,
          double v2,
          ref Tesselator.CombineParameters combinePars,
          out int outData)
        {
            //double error = .001;
            //Assert.IsTrue(GetNextOutputAsString() == "C");
            //Assert.AreEqual(GetNextOutputAsDouble(), v0, error);
            //Assert.AreEqual(GetNextOutputAsDouble(), v1, error);
            //Assert.AreEqual(GetNextOutputAsInt(), data4[0]);
            //Assert.AreEqual(GetNextOutputAsInt(), data4[1]);
            //Assert.AreEqual(GetNextOutputAsInt(), data4[2]);
            //Assert.AreEqual(GetNextOutputAsInt(), data4[3]);
            //Assert.AreEqual(GetNextOutputAsDouble(), weight4[0], error);
            //Assert.AreEqual(GetNextOutputAsDouble(), weight4[1], error);
            //Assert.AreEqual(GetNextOutputAsDouble(), weight4[2], error);
            //Assert.AreEqual(GetNextOutputAsDouble(), weight4[3], error); 
            //here , outData = index of newly add vertext 
            //----------------------------------------------------------------------
            //*** new vertext is added into user vertext list ***            
            //use negative to note that this vertext is from temporary source 

            //other implementation:
            // append to end of input list is ok if the input list can grow up ***
            //----------------------------------------------------------------------
            outData = -_tempVertexList.Count;
            //----------------------------------------
            _tempVertexList.Add(new TessVertex2d(v0, v1));
            //----------------------------------------
        }

        /// <summary>
        /// connect to actual Tesselator
        /// </summary>
        /// <param name="tesselator"></param>
        /// <param name="setEdgeFlag"></param>
        public void Connect(Tesselator tesselator, bool setEdgeFlag)
        {
            tesselator.callBegin = OnBegin;
            tesselator.callEnd = OnEnd;
            tesselator.callVertex = OnVertex;
            tesselator.callCombine = OnCombine;
            if (setEdgeFlag)
            {
                tesselator.callEdgeFlag = OnEdgeFlag;
            }
        }
        /// <summary>
        /// clear previous results and load a new input vertex list
        /// </summary>
        /// <param name="inputVertexCount"></param>
        public void ResetAndLoadInputVertexList(int inputVertexCount)
        {
            _inputVertexCount = inputVertexCount;
            //1. reset
            _triangleListType = Tesselator.TriangleListType.LineLoop;//?
            _tempVertexList.Clear();
            _resultIndexList.Clear();
        }
    }


    class TessTool
    {
        readonly Tesselator _tess;
        readonly TessListener _tessListener;

        public TessTool() : this(new Tesselator() { WindingRule = Tesselator.WindingRuleType.Odd }) { }
        public TessTool(Tesselator tess)
        {
            _tess = tess;
            _tessListener = new TessListener();
            _tessListener.Connect(tess, true);
        }
        public List<ushort> TessIndexList => _tessListener._resultIndexList;
        public List<TessVertex2d> TempVertexList => _tessListener._tempVertexList;
        public bool TessPolygon(float[] vertex2dCoords, int[] contourEndPoints)
        {
            int ncoords = vertex2dCoords.Length / 2;
            _tessListener.ResetAndLoadInputVertexList(ncoords);
            if (ncoords == 0) { return false; }
            //this support sub contour in the same array of  vertex2dCoords
            //-----------------------
            _tess.BeginPolygon();

            if (contourEndPoints == null || contourEndPoints.Length == 1)
            {
                //only 1 contour
                int beginAt = 0;
                int thisContourEndAt = vertex2dCoords.Length / 2;
                _tess.BeginContour();
                for (int i = beginAt; i < thisContourEndAt; ++i)
                {
                    _tess.AddVertex(
                        vertex2dCoords[i << 1], //*2
                        vertex2dCoords[(i << 1) + 1], 0, i); //*2+1
                }
                beginAt = thisContourEndAt + 1;
                _tess.EndContour();
            }
            else
            {
                int nContourCount = contourEndPoints.Length;
                int beginAt = 0;
                for (int m = 0; m < nContourCount; ++m)
                {
                    int thisContourEndAt = (contourEndPoints[m] + 1) / 2;
                    _tess.BeginContour();
                    for (int i = beginAt; i < thisContourEndAt; ++i)
                    {
                        _tess.AddVertex(
                            vertex2dCoords[i << 1],
                            vertex2dCoords[(i << 1) + 1],
                            0,
                            i);

                    }
                    beginAt = thisContourEndAt + 1;
                    _tess.EndContour();
                }
            }
            //
            //
            _tess.EndPolygon();
            return true;
        }
    }


    static class TessToolExtensions
    {
        /// <summary>
        /// tess and read result as triangle list vertex array (for GLES draw-array)
        /// </summary>
        /// <param name="tessTool"></param>
        /// <param name="vertex2dCoords"></param>
        /// <param name="contourEndPoints"></param>
        /// <param name="vertexCount"></param>
        /// <returns></returns>
        public static float[] TessAsTriVertexArray(this TessTool tessTool, float[] vertex2dCoords, int[] contourEndPoints, out int vertexCount)
        {
            if (!tessTool.TessPolygon(vertex2dCoords, contourEndPoints))
            {
                vertexCount = 0;
                return null;
            }
            //results
            //1.
            List<ushort> indexList = tessTool.TessIndexList;
            //2.
            List<TessVertex2d> tempVertexList = tessTool.TempVertexList;
            //3.
            vertexCount = indexList.Count;
            //-----------------------------    
            int orgVertexCount = vertex2dCoords.Length / 2;
            float[] vtx = new float[vertexCount * 2];//***
            int n = 0;

            for (int p = 0; p < vertexCount; ++p)
            {
                ushort index = indexList[p];
                if (index >= orgVertexCount)
                {
                    //extra coord (newly created)
                    TessVertex2d extraVertex = tempVertexList[index - orgVertexCount];
                    vtx[n] = (float)extraVertex.m_X;
                    vtx[n + 1] = (float)extraVertex.m_Y;
                }
                else
                {
                    //original corrd
                    vtx[n] = (float)vertex2dCoords[index * 2];
                    vtx[n + 1] = (float)vertex2dCoords[(index * 2) + 1];
                }
                n += 2;
            }
            //triangle list
            return vtx;

        }
        /// <summary>
        /// tess and read result as triangle list index array (for GLES draw element)
        /// </summary>
        /// <param name="tessTool"></param>
        /// <param name="vertex2dCoords"></param>
        /// <param name="contourEndPoints"></param>
        /// <param name="outputCoords"></param>
        /// <param name="vertexCount"></param>
        /// <returns></returns>
        public static ushort[] TessAsTriIndexArray(this TessTool tessTool,
            float[] vertex2dCoords,
            int[] contourEndPoints,
            out float[] outputCoords,
            out int vertexCount)
        {
            if (!tessTool.TessPolygon(vertex2dCoords, contourEndPoints))
            {
                vertexCount = 0;
                outputCoords = null;
                return null; //* early exit
            }
            //results
            //1.
            List<ushort> indexList = tessTool.TessIndexList;
            //2.
            List<TessVertex2d> tempVertexList = tessTool.TempVertexList;
            //3.
            vertexCount = indexList.Count;
            //-----------------------------   

            //create a new array and append with original and new tempVertex list 
            int tempVertListCount = tempVertexList.Count;
            outputCoords = new float[vertex2dCoords.Length + tempVertListCount * 2];
            //1. copy original array
            Array.Copy(vertex2dCoords, outputCoords, vertex2dCoords.Length);
            //2. append with newly create vertex (from tempVertList)
            int endAt = vertex2dCoords.Length + tempVertListCount;
            int p = 0;
            int q = vertex2dCoords.Length; //start adding at
            for (int i = vertex2dCoords.Length; i < endAt; ++i)
            {
                TessVertex2d v = tempVertexList[p];
                outputCoords[q] = (float)v.m_X;
                outputCoords[q + 1] = (float)v.m_Y;
                p++;
                q += 2;
            }

            return indexList.ToArray();
        }

    }
}