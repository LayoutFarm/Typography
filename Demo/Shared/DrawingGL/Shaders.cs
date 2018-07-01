//MIT, 2014-2017, WinterDev  
using System;
using OpenTK.Graphics.ES20;

namespace DrawingGL
{
    /// <summary>
    /// sharing data between canvas and shaders
    /// </summary>
    class CanvasToShaderSharedResource
    {
        /// <summary>
        /// stroke width here is the sum of both side of the line.
        /// </summary>
        internal float _strokeWidth = 1;
        OpenTK.Graphics.ES20.MyMat4 _orthoView;
        internal ShaderBase _currentShader;
        int _orthoViewVersion = 0;
        Color _strokeColor;

        internal OpenTK.Graphics.ES20.MyMat4 OrthoView
        {
            get { return _orthoView; }
            set
            {
                _orthoView = value;
                unchecked { _orthoViewVersion++; }
            }
        }
        public int OrthoViewVersion
        {
            get { return this._orthoViewVersion; }
        }

        internal Color StrokeColor
        {
            get { return _strokeColor; }
            set
            {
                _strokeColor = value;
                _stroke_r = value.R / 255f;
                _stroke_g = value.G / 255f;
                _stroke_b = value.B / 255f;
                _stroke_a = value.A / 255f;
            }
        }

        float _stroke_r;
        float _stroke_g;
        float _stroke_b;
        float _stroke_a;
        internal void AssignStrokeColorToVar(OpenTK.Graphics.ES20.ShaderUniformVar4 color)
        {
            color.SetValue(_stroke_r, _stroke_g, _stroke_b, _stroke_a);
        }
    }

    abstract class ShaderBase
    {
        protected readonly CanvasToShaderSharedResource _canvasShareResource;
        protected readonly MiniShaderProgram shaderProgram = new MiniShaderProgram();
        public ShaderBase(CanvasToShaderSharedResource canvasShareResource)
        {
            _canvasShareResource = canvasShareResource;
        }
        /// <summary>
        /// set as current shader
        /// </summary>
        protected void SetCurrent()
        {
            if (_canvasShareResource._currentShader != this)
            {
                shaderProgram.UseProgram();
                _canvasShareResource._currentShader = this;
                this.OnSwithToThisShader();
            }
        }
        protected virtual void OnSwithToThisShader()
        {
        }
    }

    class GlyphFillShader : ShaderBase
    {
        ShaderVtxAttrib2f a_position;
        ShaderUniformMatrix4 u_matrix;
        ShaderUniformVar4 u_solidColor;
        ShaderUniformVar2 u_2d_offset;
        public GlyphFillShader(CanvasToShaderSharedResource canvasShareResource)
            : base(canvasShareResource)
        {
            //----------------
            //vertex shader source
            string vs = @"        
            attribute vec2 a_position; 
            uniform mat4 u_mvpMatrix;
            uniform vec4 u_solidColor;              
            uniform vec2 u_2d_offset;           
            varying vec4 v_color;
 
            void main()
            {
                gl_Position = u_mvpMatrix* vec4(a_position[0] +u_2d_offset[0],a_position[1]+ u_2d_offset[1],0,1); 
                v_color= u_solidColor;
            }
            ";
            //fragment source
            string fs = @"
                precision mediump float;
                varying vec4 v_color; 
                void main()
                {
                    gl_FragColor = v_color;
                }
            ";
            if (!shaderProgram.Build(vs, fs))
            {
                throw new NotSupportedException();
            }

            a_position = shaderProgram.GetAttrV2f("a_position");
            u_matrix = shaderProgram.GetUniformMat4("u_mvpMatrix");
            u_solidColor = shaderProgram.GetUniform4("u_solidColor");
            u_2d_offset = shaderProgram.GetUniform2("u_2d_offset");

        }
        public void FillTriangleStripWithVertexBuffer(float[] linesBuffer, int nelements, Color color)
        {
            SetCurrent();
            CheckViewMatrix();
            //--------------------------------------------
            u_2d_offset.SetValue(this.OffsetX, this.OffsetY);
            u_solidColor.SetValue((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            a_position.LoadPureV2f(linesBuffer);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, nelements);
        }
        //--------------------------------------------
        int orthoviewVersion = -1;
        void CheckViewMatrix()
        {
            int version = 0;
            if (orthoviewVersion != (version = _canvasShareResource.OrthoViewVersion))
            {
                orthoviewVersion = version;
                u_matrix.SetData(_canvasShareResource.OrthoView.data);
            }
        }

        public void FillTriangles(float[] polygon2dVertices, int nelements, Color color)
        {
            SetCurrent();
            CheckViewMatrix();
            //--------------------------------------------  

            u_2d_offset.SetValue(this.OffsetX, this.OffsetY);
            u_solidColor.SetValue((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            a_position.LoadPureV2f(polygon2dVertices);
            GL.DrawArrays(BeginMode.Triangles, 0, nelements);
        }
        public unsafe void DrawLineLoopWithVertexBuffer(float* polygon2dVertices, int nelements, Color color)
        {
            SetCurrent();
            CheckViewMatrix();
            //--------------------------------------------
            u_2d_offset.SetValue(this.OffsetX, this.OffsetY);
            u_solidColor.SetValue((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            a_position.UnsafeLoadPureV2f(polygon2dVertices);
            GL.DrawArrays(BeginMode.LineLoop, 0, nelements);
        }
        public unsafe void FillTriangleFan(float* polygon2dVertices, int nelements, Color color)
        {
            SetCurrent();
            CheckViewMatrix();
            //--------------------------------------------
            u_2d_offset.SetValue(this.OffsetX, this.OffsetY);
            u_solidColor.SetValue((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            a_position.UnsafeLoadPureV2f(polygon2dVertices);
            GL.DrawArrays(BeginMode.TriangleFan, 0, nelements);
        }
        public void DrawLine(float x1, float y1, float x2, float y2, Color color)
        {
            SetCurrent();
            CheckViewMatrix();
            //--------------------------------------------
            u_2d_offset.SetValue(this.OffsetX, this.OffsetY);
            u_solidColor.SetValue((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            unsafe
            {
                float* vtx = stackalloc float[4];
                vtx[0] = x1; vtx[1] = y1;
                vtx[2] = x2; vtx[3] = y2;
                a_position.UnsafeLoadPureV2f(vtx);
            }
            GL.DrawArrays(BeginMode.Lines, 0, 2);
        }
        //--------------------------------------------
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public void SetOffset(float offsetX, float offsetY)
        {
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
        }
    }

}