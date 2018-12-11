//MIT, 2014-2017, WinterDev  

using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;

//
using DrawingGL.Text;

namespace DrawingGL
{
    public class SimpleCanvas
    {
        //--------------------------------
        //very simple GLES2 canvas
        //provide data and tool for simple rendering
        //-------------------------------- 

        int _max;
        MyMat4 _orthoView;
        MyMat4 _flipVerticalView;
        MyMat4 _orthoAndFlip;

        TessTool _tessTool;
        SimpleCurveFlattener _curveFlattener;
        //---------------------------------
        CanvasToShaderSharedResource _shaderRes;
        GlyphFillShader _fillShader;
        //--------------------------------
        TextPrinter _textPrinter = new TextPrinter();
        //--------------------------------
        int _view_width;
        int _view_height;
        public SimpleCanvas(int view_width, int view_height)
        {

            FillColor = Color.Black;
            StrokeColor = Color.Black;

            //dimension
            _view_width = view_width;
            _view_height = view_height;
            _max = Math.Max(view_width, view_height);
            //------------
            //matrix
            ////square viewport 
            _orthoView = MyMat4.ortho(0, _max, 0, _max, 0, 1);
            _flipVerticalView = MyMat4.scale(1, -1) * MyMat4.translate(new OpenTK.Vector3(0, -_max, 0));
            _orthoAndFlip = _orthoView * _flipVerticalView;
            //----------------------------------------------------------------------- 
            //shader
            _shaderRes = new CanvasToShaderSharedResource();
            _shaderRes.OrthoView = _orthoView;
            //
            _fillShader = new GlyphFillShader(_shaderRes);
            //------------
            //tools
            Tesselate.Tesselator tt = new Tesselate.Tesselator();
            _tessTool = new TessTool(tt);
            _curveFlattener = new SimpleCurveFlattener();
            ClearColor = Color.White;
            //--------
            //set blend mode
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
        public Color ClearColor
        {
            get;
            set;
        }
        public Color FillColor
        {
            get;
            set;
        }
        public Color StrokeColor
        {
            get;
            set;
        }
        public void PreRender()
        {
            //prepare canvas before actual render
            GL.Viewport(0, 0, _max, _max);

        }
        public void ClearCanvas()
        {
            //set clear color to white
            Color c_color = this.ClearColor;
            //set clear color
            GL.ClearColor(c_color.R / 255f, c_color.G / 255f, c_color.B / 255f, c_color.A / 255f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }
        public void DrawLine(float x0, float y0, float x1, float y1)
        {
            _fillShader.DrawLine(x0, y0, x1, y1, this.StrokeColor);
        }
        internal TextPrinter TextPrinter
        {
            get
            {
                return _textPrinter;
            }
        }
        public void FillTextRun(TextRun textRun, float x, float y)
        {
            //fill text run at spefic pos

            List<GlyphRun> glyphs = textRun._glyphs;
            int j = glyphs.Count;
            float accX = 0;
            float accY = 0;
            float nx = x;
            float ny = y;

            float pxscale = _textPrinter.Typeface.CalculateScaleToPixelFromPointSize(_textPrinter.FontSizeInPoints);


            for (int i = 0; i < j; ++i)
            {
                //render each glyph
                GlyphRun run = glyphs[i];

                Typography.TextLayout.UnscaledGlyphPlan plan = run.GlyphPlan;

                nx = x + accX + plan.OffsetX * pxscale;
                ny = y + accY + plan.OffsetY * pxscale;

                _fillShader.SetOffset(nx, ny);
                accX += (plan.AdvanceX * pxscale);


                _fillShader.FillTriangles(
                    run.tessData,
                    run.nTessElements,
                    this.FillColor
                    );
            }
            _fillShader.SetOffset(0, 0);
        }
    }


}