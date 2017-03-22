using System;
using OpenTK;
using OpenTK.Platform.Android;
using Android.Content;
using Android.Util;
using AndroidOS = Android.OS;
using System.IO;

namespace Xamarin.OpenGL
{
    class GLView1 : AndroidGameView
    {
        private readonly OpenGLESRenderer renderer = new OpenGLESRenderer();

        /// <summary> buffer for filled triangles </summary>
        private readonly DrawBuffer TriangleBuffer = new DrawBuffer();

        /// <summary> buffer for filled bezier curves </summary>
        private readonly DrawBuffer BezierBuffer = new DrawBuffer();

        /// <summary> 3D model data of the text </summary>
        private readonly TextMesh textMesh = new TextMesh();

        /// <summary> the text context </summary>
        private readonly TypographyTextContext textContext;

        public GLView1(Context context) : base(context)
        {
            var text = "#12Typography34!";
            var directory = AndroidOS.Environment.ExternalStorageDirectory;
            var fullFileName = Path.Combine(directory.ToString(), "TypographyTest.txt");
            if (File.Exists(fullFileName))
            {
                text = File.ReadAllText(fullFileName);
            }

            textContext = new TypographyTextContext(
                text,
                "DroidSans.ttf", //corresponding to font file Assets/DroidSans.ttf
                36,//font size
                //all following is duumy and not implemented
                FontStretch.Normal, FontStyle.Normal, FontWeight.Normal, 4096, 4096, OpenGL.TextAlignment.Leading
            );
        }

        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Build text mesh
            textContext.Build(0, 0, textMesh);
            textMesh.PathTessPolygon(Typography.Rendering.Color.Black);

            //create vertex and index buffer
            TriangleBuffer.Fill(textMesh.IndexBuffer, textMesh.VertexBuffer);
            BezierBuffer.Fill(textMesh.BezierIndexBuffer, textMesh.BezierVertexBuffer);

            renderer.Init();

            // Run the render loop
            Run();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            renderer.ShutDown();
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        //
        // In this particular case, we demonstrate how to set
        // the graphics mode and fallback in case the device doesn't
        // support the defaults
        protected override void CreateFrameBuffer()
        {
            // using OpenGLES3.0
            this.GLContextVersion = OpenTK.Graphics.GLContextVersion.Gles3_0;
            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try
            {
                Log.Verbose("Xamarin.OpenGL", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("Xamarin.OpenGL", "{0}", ex);
            }

            // this is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try
            {
                Log.Verbose("Xamarin.OpenGL", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("Xamarin.OpenGL", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            renderer.Clear();
            renderer.Render(TriangleBuffer, Material.m, this.Size.Width, this.Size.Height);
            renderer.Render(BezierBuffer, Material.m, this.Size.Width, this.Size.Height);

            SwapBuffers();
        }

    }
}
