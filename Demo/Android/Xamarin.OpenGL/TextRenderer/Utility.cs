using System.IO;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.ES30;
using Android.Util;

namespace Xamarin.OpenGL
{
    internal class Utility
    {
        internal static Stream ReadFile(string filePath)
        {
            using (Stream s = MainActivity.AssetManager.Open(filePath))
            using (var ms = new MemoryStream())// This is a simple hack because on Xamarin.Android, a `Stream` created by `AssetManager.Open` is not seekable.
            {
                s.CopyTo(ms);
                return new MemoryStream(ms.ToArray());
            }
        }

        public static void CheckGLESError(
            [CallerFilePath] string fileName = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null)
        {
            var error = GL.GetError();

            if (error != All.NoError)
            {
                string errorStr = error.ToString();
                Log.Debug("GLES Error","{0}({1}): glError: 0x{2:X} ({3}) in {4}",
                    fileName, lineNumber, error, errorStr, memberName);
            }
        }

    }
}