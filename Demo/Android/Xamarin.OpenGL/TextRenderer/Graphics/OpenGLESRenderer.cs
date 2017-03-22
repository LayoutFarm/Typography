using OpenTK.Graphics.ES30;
using System;
using System.Runtime.InteropServices;
using Android.Opengl;

namespace Xamarin.OpenGL
{
    internal class OpenGLESRenderer
    {
        private static readonly float[] MatArray = new float[16];

        public void Init()
        {
            Material.InitMaterials();

            // Other state
            GL.ClearColor(1, 1, 1, 1);

            Utility.CheckGLESError();
        }

        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }


        private static void DoRender(Material material, List<DrawIndex> indexBuffer, List<DrawVertex> vertexBuffer,
            int width, int height)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled
            GL.Enable(All.Blend);
            GL.BlendEquation(All.FuncAdd);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
            GL.Disable(All.CullFace);
            GL.Disable(All.DepthTest);
            GL.Enable(All.ScissorTest);

            // Setup viewport, orthographic projection matrix
            GL.Viewport(0, 0, width, height);
            Matrix.OrthoM(MatArray, 0, 0.0f, width, height, 0.0f, -5.0f, 5.0f);
            material.program.Bind();
            material.program.SetUniformMatrix4("ProjMtx", MatArray);

            // Send vertex and index data
            GL.BindVertexArray(material.vaoHandle);
            GL.BindBuffer(All.ArrayBuffer, material.positionVboHandle);
            GL.BufferData(All.ArrayBuffer, new IntPtr(vertexBuffer.Count * Marshal.SizeOf<DrawVertex>()), vertexBuffer.Pointer, All.StreamDraw);
            GL.BindBuffer(All.ElementArrayBuffer, material.elementsHandle);
            GL.BufferData(All.ElementArrayBuffer, new IntPtr(indexBuffer.Count * Marshal.SizeOf<DrawIndex>()), indexBuffer.Pointer, All.StreamDraw);

            Utility.CheckGLESError();

            // Draw
            GL.DrawElements(All.Triangles, indexBuffer.Count, All.UnsignedInt, IntPtr.Zero);

            Utility.CheckGLESError();
        }

        public void Render(DrawBuffer DrawBuffer, Material m, int width, int height)
        {
            DoRender(m, DrawBuffer.IndexBuffer, DrawBuffer.VertexBuffer, width, height);
        }

        public void ShutDown()
        {
            Material.DestroyMaterials();
        }
    }
}
