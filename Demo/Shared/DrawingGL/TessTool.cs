//MIT, 2014-present, WinterDev  

using System.Collections.Generic;
using Tesselate;

namespace DrawingGL
{
    public struct TessVertex2d
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

    public class TessListener
    {
        List<TessVertex2d> _inputVertextList;

        internal List<TessVertex2d> _tempVertexList = new List<TessVertex2d>();
        internal List<TessVertex2d> _resultVertexList = new List<TessVertex2d>();
        internal List<ushort> _resultIndexList = new List<ushort>();
        int _inputVertexCount;
        Tesselator.TriangleListType _triangleListType;

        public TessListener()
        {
            //empty not use
            //not use first item in temp
            _tempVertexList.Add(new TessVertex2d(0, 0));
        }
        public void BeginCallBack(Tesselator.TriangleListType type)
        {
            _triangleListType = type;

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

        public void EndCallBack()
        {
            //Assert.IsTrue(GetNextOutputAsString() == "E");
            //Console.WriteLine("end");
        }

        public void VertexCallBack(int index)
        {
            //Assert.IsTrue(GetNextOutputAsString() == "V");
            //Assert.AreEqual(GetNextOutputAsInt(), index); 
            if (index < 0)
            {
                //use data from temp store
                _resultVertexList.Add(_tempVertexList[-index]);
                _resultIndexList.Add((ushort)(_inputVertexCount + (-index)));

                //Console.WriteLine("temp_v_cb:" + index + ":(" + tempVertextList[-index] + ")");
            }
            else
            {
                _resultIndexList.Add((ushort)index);
                _resultVertexList.Add(_inputVertextList[index]);
                // Console.WriteLine("v_cb:" + index + ":(" + inputVertextList[index] + ")");
            }
        }

        public void EdgeFlagCallBack(bool IsEdge)
        {
            //Console.WriteLine("edge: " + IsEdge);
            //Assert.IsTrue(GetNextOutputAsString() == "F");
            //Assert.AreEqual(GetNextOutputAsBool(), IsEdge);
        }

        public void CombineCallBack(double v0, double v1, double v2, int[] data4,
            double[] weight4, out int outData)
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
        public void Connect(Tesselate.Tesselator tesselator, bool setEdgeFlag)
        {
            tesselator.callBegin = BeginCallBack;
            tesselator.callEnd = EndCallBack;
            tesselator.callVertex = VertexCallBack;
            tesselator.callCombine = CombineCallBack;
            if (setEdgeFlag)
            {
                tesselator.callEdgeFlag = EdgeFlagCallBack;
            }
        }
        public void Reset(List<TessVertex2d> vertextList)
        {
            _inputVertexCount = vertextList.Count;
            _triangleListType = Tesselator.TriangleListType.LineLoop;//?
            this._tempVertexList.Clear();
            this._resultVertexList.Clear();
            this._inputVertextList = vertextList;
        }
    }



    class TessTool
    {
        readonly Tesselator _tess;
        readonly TessListener _tessListener;
        List<TessVertex2d> _vertexts = new List<TessVertex2d>();
        public TessTool() : this(new Tesselator() { WindingRule = Tesselator.WindingRuleType.Odd }) { }
        public TessTool(Tesselate.Tesselator tess)
        {
            _tess = tess;
            _tessListener = new TessListener();
            _tessListener.Connect(tess, true);
        }
        public List<ushort> TessIndexList => _tessListener._resultIndexList;
        public List<TessVertex2d> TempVertexList => _tessListener._tempVertexList;


        public float[] TessPolygon(float[] vertex2dCoords, int[] contourEndPoints)
        {
            int areaCount = 0;
            _vertexts.Clear();//reset
            //
            int ncoords = vertex2dCoords.Length / 2;
            if (ncoords == 0) { areaCount = 0; return null; }

            int nn = 0;
            for (int i = 0; i < ncoords; ++i)
            {
                _vertexts.Add(new TessVertex2d(vertex2dCoords[nn++], vertex2dCoords[nn++]));
            }
            //-----------------------
            _tessListener.Reset(_vertexts);
            //-----------------------
            _tess.BeginPolygon();

            int nContourCount = contourEndPoints.Length;
            int beginAt = 0;
            for (int m = 0; m < nContourCount; ++m)
            {
                int thisContourEndAt = (contourEndPoints[m] + 1) / 2;
                _tess.BeginContour();
                for (int i = beginAt; i < thisContourEndAt; ++i)
                {
                    TessVertex2d v = _vertexts[i];
                    _tess.AddVertex(v.m_X, v.m_Y, 0, i);
                }
                beginAt = thisContourEndAt + 1;
                _tess.EndContour();
            }


            _tess.EndPolygon();
            //-----------------------
            List<TessVertex2d> vertextList = _tessListener._resultVertexList;
            //-----------------------------   
            //switch how to fill polygon
            int j = vertextList.Count;
            float[] vtx = new float[j * 2];
            int n = 0;
            for (int p = 0; p < j; ++p)
            {
                var v = vertextList[p];
                vtx[n] = (float)v.m_X;
                vtx[n + 1] = (float)v.m_Y;
                n += 2;
            }
            //triangle list
            areaCount = j;
            return vtx;
        }
    }



}