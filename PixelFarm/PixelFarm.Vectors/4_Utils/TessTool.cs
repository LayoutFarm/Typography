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
namespace PixelFarm.CpuBlit.VertexProcessing
{

    /// <summary>
    /// listen and handle the event from tesslator
    /// </summary>
    class TessListener : Tesselator.ITessListener
    {
        internal List<TessVertex2d> _tempVertexList = new List<TessVertex2d>();
        internal List<ushort> _resultIndexList = new List<ushort>();
        int _inputVertexCount;

        //Tesselator.TriangleListType _triangleListType; 
        public TessListener()
        {
            //empty not use
            //not use first item in temp
            _tempVertexList.Add(new TessVertex2d(0, 0));
        }

        void Tesselator.ITessListener.BeginRead() { }
        void Tesselator.ITessListener.Begin(Tesselator.TriangleListType type)
        {
#if DEBUG

            if (type != Tesselator.TriangleListType.Triangles)
            {

            }
#endif
            //_triangleListType = type;

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

        void Tesselator.ITessListener.End()
        {
            //Assert.IsTrue(GetNextOutputAsString() == "E");
            //Console.WriteLine("end");
        }

        void Tesselator.ITessListener.Vertext(int index)
        {
            //Assert.IsTrue(GetNextOutputAsString() == "V");
            //Assert.AreEqual(GetNextOutputAsInt(), index); 
            if (index < 0)
            {
                //use data from temp store***
                //that will be appended to the end of result
                _resultIndexList.Add((ushort)(_inputVertexCount + (-index)));//** minus,=> make it positive sign.

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


        public bool NeedEdgeFlag { get; set; }
        void Tesselator.ITessListener.EdgeFlag(bool boundaryEdge_isEdge)
        {
            //Console.WriteLine("edge: " + IsEdge);
            //Assert.IsTrue(GetNextOutputAsString() == "F");
            //Assert.AreEqual(GetNextOutputAsBool(), IsEdge);
        }

        void Tesselator.ITessListener.Combine(double v0,
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


        public bool NeedMash { get; set; }
        void Tesselator.ITessListener.Mesh(Mesh mesh)
        {

        }



        /// <summary>
        /// connect to actual Tesselator
        /// </summary>
        /// <param name="tesselator"></param>
        /// <param name="setEdgeFlag"></param>
        public void Connect(Tesselator tesselator, bool setEdgeFlag)
        {

            NeedEdgeFlag = setEdgeFlag;
            tesselator.SetListener(this);

            //tesselator.callBegin = OnBegin;
            //tesselator.callEnd = OnEnd;
            //tesselator.callVertex = OnVertex;
            //tesselator.callCombine = OnCombine;
            //if (setEdgeFlag)
            //{
            //    tesselator.callEdgeFlag = OnEdgeFlag;
            //}
        }
        /// <summary>
        /// clear previous results and load a new input vertex list
        /// </summary>
        /// <param name="inputVertexCount"></param>
        public void ResetAndLoadInputVertexList(int inputVertexCount)
        {
            _inputVertexCount = inputVertexCount;
            //1. reset
            //_triangleListType = Tesselator.TriangleListType.LineLoop;//?
            _tempVertexList.Clear();
            _resultIndexList.Clear();
        }
    }


    public class TessTool
    {
        readonly Tesselator _tess;
        readonly TessListener _tessListener;
        public TessTool() : this(new Tesselator() { WindingRule = Tesselator.WindingRuleType.NonZero }) { }
        public TessTool(Tesselator tess)
        {
            _tess = tess;
            _tessListener = new TessListener();
            _tessListener.Connect(tess, true);
        }
        public Tesselator.WindingRuleType WindingRuleType
        {
            get => _tess.WindingRule;
            set => _tess.WindingRule = value;
        }
        internal List<ushort> TessIndexList => _tessListener._resultIndexList;
        internal List<TessVertex2d> TempVertexList => _tessListener._tempVertexList;
        public bool TessPolygon(float[] vertex2dCoords, int[] contourEndPoints)
        {
            //internal tess the polygon

            int ncoords = vertex2dCoords.Length / 2;
            _tessListener.ResetAndLoadInputVertexList(ncoords);
            if (ncoords == 0) return false;
            //-----------------------
            //this support sub contour in the same array of  vertex2dCoords
            _tess.BeginPolygon();
            if (contourEndPoints == null)
            {
                //only 1 contour
                int beginAt = 0;
                int thisContourEndAt = vertex2dCoords.Length / 2;
                _tess.BeginContour();
                for (int i = beginAt; i < thisContourEndAt; ++i)
                {
                    _tess.AddVertex(
                        vertex2dCoords[i << 1], //*2
                        vertex2dCoords[(i << 1) + 1], i); //*2+1
                }
                beginAt = thisContourEndAt + 1;
                _tess.EndContour();

            }
            else
            {
                //may have more than 1 contour
                int nContourCount = contourEndPoints.Length;
                int beginAt = 0;
                for (int m = 0; m < nContourCount; ++m)
                {
                    int thisContourEndAt = (contourEndPoints[m] + 1) / 2;
                    _tess.BeginContour();
                    for (int i = beginAt; i < thisContourEndAt; ++i)
                    {
                        _tess.AddVertex(
                            vertex2dCoords[i << 1], //*2
                            vertex2dCoords[(i << 1) + 1], //*2+1 
                            i);
                    }
                    beginAt = thisContourEndAt + 1;
                    _tess.EndContour();
                }
            }
            _tess.EndPolygon();
            //-----------------------
            return true;
        }
    }

    public class TessTool2
    {
        //UNDER CONSTRUCTION

        public enum State
        {
            BeginPolygon,
            BeginContour,
            Vertex,
            EndContour,
            EndPolygon,
        }

        public interface IPolygonVerticeReader
        {
            /// <summary>
            /// reset current read position to begin state again
            /// </summary>
            void Reset();
            State ReadNext(out float x, out float y);
            State CurrentState { get; }
        }

        readonly Tesselator _tess;
        public TessTool2()
            : this(new Tesselator() { WindingRule = Tesselator.WindingRuleType.NonZero })
        {
        }
        public TessTool2(Tesselator tess)
        {
            _tess = tess;
        }
        public bool TessPolygon(IPolygonVerticeReader polygonReader, Tesselator.ITessListener listner)
        {
            //internal tess the polygon
            polygonReader.Reset();
            listner.BeginRead();

            //
            State state = polygonReader.CurrentState;
            int current_vertex = 0;
            float cur_x = 0;
            float cur_y = 0;

            for (; ; )
            {
                switch (state)
                {
                    case State.BeginPolygon:
                        _tess.BeginPolygon();
                        break;
                    case State.BeginContour:
                        _tess.BeginContour();
                        break;
                    case State.Vertex:
                        _tess.AddVertex(cur_x, cur_y, current_vertex);
                        current_vertex++;
                        break;
                    case State.EndContour:
                        _tess.EndContour();
                        break;
                    case State.EndPolygon:
                        _tess.EndPolygon();
                        goto EXIT_LOOP;
                }
                state = polygonReader.ReadNext(out cur_x, out cur_y);
            }
            EXIT_LOOP:
            return true;
        }
    }

    public static class TessToolExtensions
    {
        /// <summary>
        /// tess and read result as triangle list vertex array (for GLES draw-array)
        /// </summary>
        /// <param name="tessTool"></param>
        /// <param name="vertex2dCoords"></param>
        /// <param name="contourEndPoints"></param>
        /// <param name="vertexCount"></param>
        /// <returns></returns>
        public static float[] TessAsTriVertexArray(this TessTool tessTool,
            float[] vertex2dCoords,
            int[] contourEndPoints,
            out int vertexCount)
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
                    vtx[n] = (float)extraVertex.x;
                    vtx[n + 1] = (float)extraVertex.y;
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
                outputCoords[q] = (float)v.x;
                outputCoords[q + 1] = (float)v.y;
                p++;
                q += 2;
            }

            return indexList.ToArray();
        }
    }
}