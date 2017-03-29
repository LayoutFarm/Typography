using System;
using System.Diagnostics;

using Foundation;
using GLKit;
using OpenGLES;
using OpenTK;
using OpenTK.Graphics.ES20;

//
using DrawingGL;
namespace Xamarin.iOS.GLES2
{
    [Register("GameViewController")]
    public class GameViewController : GLKViewController, IGLKViewDelegate
    {


        EAGLContext context { get; set; }
        [Export("initWithCoder:")]
        public GameViewController(NSCoder coder) : base(coder)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Code to start the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif

            context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);

            if (context == null)
            {
                Debug.WriteLine("Failed to create ES context");
            }

            var view = (GLKView)View;
            view.Context = context;
            view.DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;
            view_width = (int)view.Frame.Width;
            view_height = (int)view.Frame.Height;
            SetupGL();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            TearDownGL();

            if (EAGLContext.CurrentContext == context)
                EAGLContext.SetCurrentContext(null);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            if (IsViewLoaded && View.Window == null)
            {
                View = null;

                TearDownGL();

                if (EAGLContext.CurrentContext == context)
                {
                    EAGLContext.SetCurrentContext(null);
                }
            }

            // Dispose of any resources that can be recreated.
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }


        CustomApp customApp;
        int max;
        int view_width;
        int view_height; 
        void SetupGL()
        {
            
            EAGLContext.SetCurrentContext(context);
            max = Math.Max(view_width, view_height);
            customApp = new CustomApp();
            customApp.Setup(800, 600); 
        }
        public override void Update()
        {
            GL.Viewport(0, 0, max, max);
            customApp.RenderFrame();

        }
        //----------------
        void TearDownGL()
        {
            
        } 
        string LoadResource(string name, string type)
        {
            var path = NSBundle.MainBundle.PathForResource(name, type);
            return System.IO.File.ReadAllText(path);
        } 
    }
}