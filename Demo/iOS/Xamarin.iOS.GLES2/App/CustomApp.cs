//MIT, 2017, Zou Wei(github/zwcloud)
//MIT, 2017, WinterDev (modified from Xamarin's Android code template)

using System.IO;
using DrawingGL;
using DrawingGL.Text;

namespace Xamarin.iOS.GLES2
{
    class CustomApp
    {

        TypographyTextContext textContext;
        SimpleCanvas simpleCanvas;
        TextRun textRun;

        public void Setup(int canvasW, int canvasH)
        {
            //--------------------------------------
            //TODO: review here again

            DrawingGL.Text.Utility.SetLoadFontDel(
            fontfile =>
                 {
                     return null;
                 });

            //--------------------------------------
            simpleCanvas = new SimpleCanvas(canvasW, canvasH);
            var text = "Typography";
            //optional ....
            //var directory = AndroidOS.Environment.ExternalStorageDirectory;
            //var fullFileName = Path.Combine(directory.ToString(), "TypographyTest.txt");
            //if (File.Exists(fullFileName))
            //{
            //    text = File.ReadAllText(fullFileName);
            //}
            //-------------------------------------------------------------------------- 
            //we want to create a prepared visual object ***
            textContext = new TypographyTextContext()
            {
                FontFamily = "DroidSans.ttf", //corresponding to font file Assets/DroidSans.ttf
                FontSize = 64,//size in Points
                FontStretch = FontStretch.Normal,
                FontStyle = FontStyle.Normal,
                FontWeight = FontWeight.Normal,
                Alignment = DrawingGL.Text.TextAlignment.Leading
            };
            //-------------------------------------------------------------------------- 
            //create blank text run 
            textRun = new TextRun();
            //generate glyph run inside text text run
            textContext.GenerateGlyphRuns(textRun, text);
            //-------------------------------------------------------------------------- 

        }

        public void RenderFrame()
        {
            simpleCanvas.PreRender();
            simpleCanvas.ClearCanvas();

            //-----------
            simpleCanvas.StrokeColor = Color.Black;
            simpleCanvas.DrawLine(0, 0, 700, 700);
            //
            for (int i = 0; i < 10; ++i)
            {
                simpleCanvas.FillTextRun(textRun, i * 100, i * 100);
            }
            //-----------
        }
    }
}