//MIT, 2017, Zou Wei(github/zwcloud)
using Typography.Rendering;

namespace Xamarin.OpenGL
{
    /// <summary>
    /// Text mesh
    /// </summary>
    /// <remarks>
    /// A text mesh contains two parts:
    ///   1. triangles: generated from glyph contours (line segment part)
    ///   2. bezier segments: generated from glyph bezier curves
    /// </remarks>
    class TextMesh : ITextPathBuilder
    {
        List<DrawIndex> indexBuffer = new List<DrawIndex>();
        List<DrawVertex> vertexBuffer = new List<DrawVertex>();

        List<DrawIndex> bezierIndexBuffer = new List<DrawIndex>();
        List<DrawVertex> bezierVertexBuffer = new List<DrawVertex>();

        private int _bezier_vtxWritePosition;
        private int _bezier_idxWritePosition;
        private int _bezier_currentIdx;

        private List<int> _BezierControlPointIndex = new List<int>();

        private void AppendBezierVertex(DrawVertex vertex)
        {
            bezierVertexBuffer[_bezier_vtxWritePosition] = vertex;
            _bezier_vtxWritePosition++;
        }

        private void AppendBezierIndex(int offsetToCurrentIndex)
        {
            bezierIndexBuffer[_bezier_idxWritePosition] = new DrawIndex { Index = _bezier_currentIdx + offsetToCurrentIndex };
            _bezier_idxWritePosition++;
        }

        public void PrimBezierReserve(int idx_count, int vtx_count)
        {
            if (idx_count == 0)
            {
                return;
            }

            int vtx_buffer_size = this.BezierVertexBuffer.Count;
            this._bezier_vtxWritePosition = vtx_buffer_size;
            this.BezierVertexBuffer.Resize(vtx_buffer_size + vtx_count);

            int idx_buffer_size = this.BezierIndexBuffer.Count;
            this._bezier_idxWritePosition = idx_buffer_size;
            this.BezierIndexBuffer.Resize(idx_buffer_size + idx_count);
        }

        public void AddBezier(Point start, Point control, Point end, Color col)
        {
            int idx_count = 3;
            int vtx_count = 3;
            PrimBezierReserve(idx_count, vtx_count);

            var uv0 = new Point(0, 0);
            var uv1 = new Point(0.5, 0);
            var uv2 = new Point(1, 1);

            var p0 = start;
            var p1 = control;
            var p2 = end;

            AppendBezierVertex(new DrawVertex { pos = p0, uv = uv0, color = col });
            AppendBezierVertex(new DrawVertex { pos = p1, uv = uv1, color = col });
            AppendBezierVertex(new DrawVertex { pos = p2, uv = uv2, color = col });

            AppendBezierIndex(0);
            AppendBezierIndex(1);
            AppendBezierIndex(2);

            _bezier_currentIdx += 3;
        }


        /// <summary>
        /// Index buffer. Each command consume DrawCommand.ElemCount of those
        /// </summary>
        public List<DrawIndex> IndexBuffer
        {
            get { return indexBuffer; }
            set { indexBuffer = value; }
        }

        /// <summary>
        /// Vertex buffer
        /// </summary>
        public List<DrawVertex> VertexBuffer
        {
            get { return vertexBuffer; }
            set { vertexBuffer = value; }
        }

        /// <summary>
        /// Index buffer for bezier curves
        /// </summary>
        public List<DrawIndex> BezierIndexBuffer
        {
            get { return bezierIndexBuffer; }
            set { bezierIndexBuffer = value; }
        }

        /// <summary>
        /// Vertex buffer for beziers curves
        /// </summary>
        public List<DrawVertex> BezierVertexBuffer
        {
            get { return bezierVertexBuffer; }
        }

        public void Clear()
        {
            // triangles
            this.IndexBuffer.Clear();
            this.VertexBuffer.Clear();

            _vtxWritePosition = 0;
            _idxWritePosition = 0;
            _currentIdx = 0;

            _Path.Clear();

            // beziers
            this.BezierIndexBuffer.Clear();
            this.BezierVertexBuffer.Clear();

            _bezier_vtxWritePosition = 0;
            _bezier_idxWritePosition = 0;
            _bezier_currentIdx = 0;

            _BezierControlPointIndex.Clear();
        }

        #region buffer writing

        private int _vtxWritePosition;
        private int _idxWritePosition;
        private int _currentIdx;

        private void AppendVertex(DrawVertex vertex)
        {
            vertexBuffer[_vtxWritePosition] = vertex;
            _vtxWritePosition++;
        }

        private void AppendIndex(int offsetToCurrentIndex)
        {
            indexBuffer[_idxWritePosition] = new DrawIndex { Index = _currentIdx + offsetToCurrentIndex };
            _idxWritePosition++;
        }

        public void PrimReserve(int idx_count, int vtx_count)
        {
            if (idx_count == 0)
            {
                return;
            }

            int vtx_buffer_size = this.VertexBuffer.Count;
            this._vtxWritePosition = vtx_buffer_size;
            this.VertexBuffer.Resize(vtx_buffer_size + vtx_count);

            int idx_buffer_size = this.IndexBuffer.Count;
            this._idxWritePosition = idx_buffer_size;
            this.IndexBuffer.Resize(idx_buffer_size + idx_count);
        }
        #endregion

        #region primitives

        private static readonly List<Point> _Path = new List<Point>();


        #endregion

        public void PathClear()
        {
            _Path.Clear();
        }

        public void PathMoveTo(Point point)
        {
            _Path.Add(point);
        }

        //inline
        public void PathLineTo(Point pos)
        {
            _Path.Add(pos);
        }

        public void PathClose()
        {
            _Path.Add(_Path[0]);
        }

        #region filled bezier curve

        public void PathAddBezier(Point start, Point control, Point end)
        {
            _Path.Add(start);

            _Path.Add(control);
            _BezierControlPointIndex.Add(_Path.Count - 1);

            _Path.Add(end);
        }

        #endregion

        #region polygon tessellation

        /// <summary>
        /// Append contour
        /// </summary>
        /// <param name="color"></param>
        public void AddContour(Color color)
        {
            // determine the winding of the path
            //var pathIsClockwise = IsClockwise(_Path);//no need

            var contour = new List<LibTessDotNet.ContourVertex>();

            int j = 0;
            for (int i = 0; i < _Path.Count; i++)
            {
                var p = _Path[i];

                //check if p is a control point of a quadratic bezier curve
                bool isControlPoint = false;
                if (j <= _BezierControlPointIndex.Count - 1 && i == _BezierControlPointIndex[j])
                {
                    j++;
                    isControlPoint = true;
                }

                if (isControlPoint)
                {
                    var start = _Path[i - 1];
                    var control = p;
                    var end = _Path[i + 1];

                    var bezierIsClockwise = IsClockwise(start, control, end);

                    if (bezierIsClockwise)//bezier 'triangle' is clockwise
                    {
                        //[picture]
                        contour.Add(new LibTessDotNet.ContourVertex
                        {
                            Position = new LibTessDotNet.Vec3
                            {
                                X = control.X,
                                Y = control.Y,
                                Z = 0.0f
                            }
                        });
                    }

                    // add this bezier to bezier buffer
                    AddBezier(start, control, end, color);
                }
                else//not control point of a bezier
                {
                    contour.Add(new LibTessDotNet.ContourVertex
                    {
                        Position = new LibTessDotNet.Vec3
                        {
                            X = p.X,
                            Y = p.Y,
                            Z = 0.0f
                        }
                    });
                }

            }
            _BezierControlPointIndex.Clear();
            // Add the contour with a specific orientation, use "Original" if you want to keep the input orientation.
            tess.AddContour(contour.ToArray()/* TODO remove this copy here!!  */, LibTessDotNet.ContourOrientation.Original);
        }

        static LibTessDotNet.Tess tess = new LibTessDotNet.Tess();// Create an instance of the tessellator. Can be reused.

        public void PathTessPolygon(Color color)
        {
            tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3, null);
            if (tess.Elements == null || tess.Elements.Length == 0)
            {
                return;
            }
            int numTriangles = tess.ElementCount;
            int idx_count = numTriangles * 3;
            int vtx_count = numTriangles * 3;
            PrimReserve(idx_count, vtx_count);
            for (int i = 0; i < numTriangles; i++)
            {
                var index0 = tess.Elements[i * 3];
                var index1 = tess.Elements[i * 3 + 1];
                var index2 = tess.Elements[i * 3 + 2];
                var v0 = tess.Vertices[index0].Position;
                var v1 = tess.Vertices[index1].Position;
                var v2 = tess.Vertices[index2].Position;

                AppendVertex(new DrawVertex { pos = new Point(v0.X, v0.Y), uv = Point.Zero, color = color });
                AppendVertex(new DrawVertex { pos = new Point(v1.X, v1.Y), uv = Point.Zero, color = color });
                AppendVertex(new DrawVertex { pos = new Point(v2.X, v2.Y), uv = Point.Zero, color = color });
                AppendIndex(0);
                AppendIndex(1);
                AppendIndex(2);
                _currentIdx += 3;
            }
            
        }

        private static bool IsClockwise(Point v0, Point v1, Point v2)
        {
            var vA = v1 - v0; // .normalize()
            var vB = v2 - v1;
            var z = vA.X * vB.Y - vA.Y * vB.X; // z component of cross Production
            var wind = z < 0; // clockwise/anticlock wind
            return wind;
        }

        #endregion

        internal void Build(Point position, Color color)
        {
            //textContext.Build(position, this);
            //this.PathTessPolygon(color);
        }
    }
}
