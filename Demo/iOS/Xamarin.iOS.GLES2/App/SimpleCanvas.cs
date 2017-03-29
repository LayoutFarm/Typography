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

        int max;
        MyMat4 orthoView;
        MyMat4 flipVerticalView;
        MyMat4 orthoAndFlip;

        TessTool tessTool;
        SimpleCurveFlattener curveFlattener;
        //---------------------------------
        CanvasToShaderSharedResource shaderRes;
        GlyphFillShader fillShader;
        //--------------------------------
        TextPrinter _textPrinter = new TextPrinter();
        //--------------------------------
        int view_width;
        int view_height;
        public SimpleCanvas(int view_width, int view_height)
        {

            FillColor = Color.Black;
            StrokeColor = Color.Black;

            //dimension
            this.view_width = view_width;
            this.view_height = view_height;
            this.max = Math.Max(view_width, view_height);
            //------------
            //matrix
            ////square viewport 
            orthoView = MyMat4.ortho(0, max, 0, max, 0, 1);
            flipVerticalView = MyMat4.scale(1, -1) * MyMat4.translate(new OpenTK.Vector3(0, -max, 0));
            orthoAndFlip = orthoView * flipVerticalView;
            //----------------------------------------------------------------------- 
            //shader
            shaderRes = new CanvasToShaderSharedResource();
            shaderRes.OrthoView = orthoView;
            //
            fillShader = new GlyphFillShader(shaderRes);
            //------------
            //tools
            Tesselate.Tesselator tt = new Tesselate.Tesselator();
            tessTool = new TessTool(tt);
            curveFlattener = new SimpleCurveFlattener();
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
            GL.Viewport(0, 0, max, max);

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
            fillShader.DrawLine(x0, y0, x1, y1, this.StrokeColor);
        }
        public void FillTextRun(TextRun textRun, float x, float y)
        {
            //fill text run at spefic pos

            List<GlyphRun> glyphs = textRun._glyphs;
            int j = glyphs.Count;

            float scale = textRun.CalculateToPixelScaleFromPointSize(textRun.sizeInPoints);

            for (int i = 0; i < j; ++i)
            {
                //render each glyph
                GlyphRun run = glyphs[i];
                //
                fillShader.SetOffset(
                   x + run.OffsetX * scale,
                   y + run.OffsetY * scale);
                //
                fillShader.FillTriangles(
                    run.tessData,
                    run.nTessElements,
                    this.FillColor
                    );
            }
            fillShader.SetOffset(0, 0);
        }
    }
}