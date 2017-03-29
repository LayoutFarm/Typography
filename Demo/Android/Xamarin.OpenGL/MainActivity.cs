using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Content.Res;
namespace Test_Android_Glyph
{
    [Activity(Label = "Test_Android_Glyph",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
		,HardwareAccelerated=false
#endif
        )]
    public class MainActivity : Activity
    {
        GLView1 view;
        private static AssetManager assetManager;
        public static AssetManager AssetManager { get { return assetManager; } }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            assetManager = this.Assets;
            // Create our OpenGL view, and display it
            view = new GLView1(this);
            SetContentView(view);
        }

        protected override void OnPause()
        {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            view.Resume();
        }
    }
}

