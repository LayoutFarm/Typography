//MIT, 2014-2017, WinterDev  

using System.Collections.Generic;
using Tesselate;

namespace DrawingGL
{
    public struct Vertex
    {
        public double m_X;
        public double m_Y;
        public Vertex(double x, double y)
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

    public class TessListener2
    {
        List<Vertex> inputVertextList;
        List<Vertex> tempVertextList = new List<Vertex>();
        public List<Vertex> resultVertexList = new List<Vertex>();
        public TessListener2()
        {
            //empty not use
            //not use first item in temp
            tempVertextList.Add(new Vertex(0, 0));
        }
        public void BeginCallBack(Tesselator.TriangleListType type)
        {
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
                resultVertexList.Add(this.tempVertextList[-index]);
                //Console.WriteLine("temp_v_cb:" + index + ":(" + tempVertextList[-index] + ")");
            }
            else
            {
                resultVertexList.Add(this.inputVertextList[index]);
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
            outData = -this.tempVertextList.Count;
            //----------------------------------------
            tempVertextList.Add(new Vertex(v0, v1));
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
        public void Reset(List<Vertex> vertextList)
        {
            this.tempVertextList.Clear();
            this.resultVertexList.Clear();
            this.inputVertextList = vertextList;
        }
    }



    class TessTool
    {
        internal readonly Tesselate.Tesselator tess;
        internal readonly TessListener2 tessListener;
        List<Vertex> vertexts = new List<Vertex>();
        public TessTool() : this(new Tesselator() { WindingRule = Tesselator.WindingRuleType.Odd }) { }
        public TessTool(Tesselate.Tesselator tess)
        {
            this.tess = tess;
            this.tessListener = new TessListener2();
            tessListener.Connect(tess, true);
        }
        public float[] TessPolygon(float[] vertex2dCoords, int[] contourEndPoints, out int areaCount)
        {
            vertexts.Clear();//reset
            //
            int ncoords = vertex2dCoords.Length / 2;
            if (ncoords == 0) { areaCount = 0; return null; }

            int nn = 0;
            for (int i = 0; i < ncoords; ++i)
            {
                vertexts.Add(new Vertex(vertex2dCoords[nn++], vertex2dCoords[nn++]));
            }
            //-----------------------
            tessListener.Reset(vertexts);
            //-----------------------
            tess.BeginPolygon();

            int nContourCount = contourEndPoints.Length;
            int beginAt = 0;
            for (int m = 0; m < nContourCount; ++m)
            {
                int thisContourEndAt = (contourEndPoints[m] + 1) / 2;
                tess.BeginContour();
                for (int i = beginAt; i < thisContourEndAt; ++i)
                {
                    Vertex v = vertexts[i];
                    tess.AddVertex(v.m_X, v.m_Y, 0, i);
                }
                beginAt = thisContourEndAt + 1;
                tess.EndContour();
            }


            tess.EndPolygon();
            //-----------------------
            List<Vertex> vertextList = tessListener.resultVertexList;
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