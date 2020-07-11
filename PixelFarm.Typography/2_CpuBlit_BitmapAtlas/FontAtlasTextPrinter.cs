//MIT, 2016-present, WinterDev, Sam Hocevar
using System;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit.PixelProcessing;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Contours;
using Typography.TextLayout;
using Typography.Text;


namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public enum AntialiasTechnique
    {
        LcdStencil,
        GreyscaleStencil,
        None,
    }
    public sealed class FontAtlasTextPrinter : TextPrinterBase, IAggTextPrinter, IDisposable
    {
        PixelBlenderWithMask _maskPixelBlender = new PixelBlenderWithMask();
        PixelBlenderPerColorComponentWithMask _maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

        AggPainter _maskBufferPainter;
        MemBitmap _fontBmp;
        MemBitmap _alphaBmp;

        AggPainter _painter;
        ResolvedFont _font;

        Typeface _currentTypeface;
        Color _fontColor;

        Typography.Text.OpenFontTextService _textServices;
        Typography.Text.TextServiceClient _txtClient;
        BitmapFontManager<MemBitmap> _bmpFontMx;
        SimpleBitmapAtlas _fontAtlas;
        public FontAtlasTextPrinter(AggPainter painter)
        {
            _painter = painter;

            this.PositionTechnique = PositionTechnique.OpenFont;
            _textServices = new Typography.Text.OpenFontTextService();
            _txtClient = _textServices.CreateNewServiceClient();
            //2. 
            _bmpFontMx = new BitmapFontManager<MemBitmap>(
                _textServices,
                atlas => atlas.MainBitmap
            );

            //3.
            ChangeFont(painter.CurrentFont);
            SetupMaskPixelBlender(painter.Width, painter.Height);
        }

        public void Dispose()
        {
            //clear this
            //clear maskbuffer
            //clear alpha buffer
        }


        public AntialiasTechnique AntialiasTech { get; set; }

        public void ChangeFont(RequestFont reqFont)
        {
            //call to service             
            _font = _textServices.ResolveFont(reqFont); //resolve for 'actual' font
            _fontAtlas = _bmpFontMx.GetFontAtlas(_font, out _fontBmp);
            FontSizeInPoints = _font.SizeInPoints;
        }

        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc) => _bmpFontMx.SetSvgBmpBuilderFunc(svgBmpBuilderFunc);

        public ResolvedFont CurrentFont => _font;

        public void ChangeFillColor(Color fontColor)
        {
            //change font color
            _fontColor = fontColor;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: ...
        }
        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            Typeface currentTypeface = _currentTypeface;
            if (currentTypeface != null)
            {
                float pointToPixelScale = currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }

        }
        public override GlyphLayout GlyphLayoutMan => throw new NotSupportedException();

        public override Typeface Typeface
        {
            get => _currentTypeface;
            set
            {
                if (_currentTypeface == value) return;
                //
                _currentTypeface = value;
                OnFontSizeChanged();
            }
        }

        int IAggTextPrinter.CurrentLineSpaceHeight => (int)FontLineSpacingPx;

        void SetupMaskPixelBlender(int width, int height)
        {
            //----------
            //same size
            _alphaBmp = new MemBitmap(width, height);
#if DEBUG
            _alphaBmp._dbugNote = "FontAtlasTextPrinter";
#endif
            _maskBufferPainter = AggPainter.Create(_alphaBmp, new PixelBlenderBGRA());
            _maskBufferPainter.Clear(Color.Black);
            //------------ 
            //draw glyph bmp to _alpha bmp
            //_maskBufferPainter.DrawImage(_glyphBmp, 0, 0);
            _maskPixelBlender.SetMaskBitmap(_alphaBmp);
            _maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBmp);
        }
        public void PrepareStringForRenderVx(AggRenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            //Typeface typeface = _currentTypeface;
        }
        public void PrepareStringForRenderVx(AggRenderVxFormattedString renderVx)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            //Typeface typeface = _currentTypeface;
        }
        //public override void DrawCaret(float x, float y)
        //{
        //    //TODO: remove draw caret here, this is for debug only 

        //}

        public void UpdateGlyphLayoutSettings()
        {
            if (_font == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(_painter.CurrentFont);
            }
        }
        public void MeasureString(char[] buffer, int startAt, int len, out int w, out int h)
        {
            var textBuffSpan = new TextBufferSpan(buffer, startAt, len);
            Size s = _txtClient.MeasureString(textBuffSpan, _painter.CurrentFont);
            w = s.Width;
            h = s.Height;
        }


        public void DrawString(AggRenderVxFormattedString renderVx, double x, double y)
        {

        }
        public override void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanSeq, int startAt, int len, float left, float top)
        {

            Typeface typeface = _font.Typeface;
            float scale = typeface.CalculateScaleToPixelFromPointSize(_font.SizeInPoints);

            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again
            //           

            TextureKind textureKind = _fontAtlas.TextureKind;

            float gx = 0;
            float gy = 0;
            int baseY = (int)Math.Round(top);


            float acc_x = 0;
            float acc_y = 0;

            int lineHeight = (int)_font.LineSpacingInPixels;//temp
            int desc = (int)_font.DescentInPixels;


            PixelBlender32 prevPxBlender = _painter.DestBitmapBlender.OutputPixelBlender; //save
            _painter.DestBitmapBlender.OutputPixelBlender = _maskPixelBlenderPerCompo; //change to new blender  

            //bool fillGlyphByGlyph = true;
            bool painterUseSubPixelLcdEffect = _painter.UseLcdEffectSubPixelRendering;


            //since glyphs may overlap each other
            //then we use fill glyph-by-glyph for best result

            //fill glyph-by-glyh
            AntialiasTechnique aaTech = this.AntialiasTech;
            int seqLen = glyphPlanSeq.Count;

            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan unscaledGlyphPlan = glyphPlanSeq[i];

                if (!_fontAtlas.TryGetItem(unscaledGlyphPlan.glyphIndex,
                    out AtlasItem atlasItem))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 

                atlasItem.GetRect(out int srcX, out int srcY, out int srcW, out int srcH);

                float ngx = acc_x + (float)Math.Round(unscaledGlyphPlan.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(unscaledGlyphPlan.OffsetY * scale);

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------

                gx = (float)(left + (ngx - atlasItem.TextureXOffset));
                gy = (float)(top + (ngy + atlasItem.TextureYOffset - srcH + lineHeight + desc));

                acc_x += (float)Math.Round(unscaledGlyphPlan.AdvanceX * scale);
                gy = (float)Math.Floor(gy);// + lineHeight;

                //clear with solid black color 
                _maskBufferPainter.Clear(Color.Black);
                //clear mask buffer at specific pos
                //_maskBufferPainter.FillRect(gx - 1, gy - 1, srcW + 2, srcH + 2, Color.Black);
                //draw 'stencil' glyph on mask-buffer                
                _maskBufferPainter.DrawImage(_fontBmp, gx, gy, srcX, _fontBmp.Height - (srcY + srcH), srcW, srcH);


#if DEBUG
                //_alphaBmp.SaveImage("alpha_0.png");
#endif

                _painter.UseLcdEffectSubPixelRendering = false; //***

                switch (aaTech)
                {
                    default:
                        {
                            //select component to render this need to render 3 times for lcd technique
                            //1. B
                            _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.B;
                            _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.B;
                            _painter.FillRect(gx + 1, gy, srcW, srcH);
                            //2. G
                            _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                            _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.G;
                            _painter.FillRect(gx + 1, gy, srcW, srcH);
                            //3. R
                            _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.R;
                            _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.R;
                            _painter.FillRect(gx + 1, gy, srcW, srcH);

#if DEBUG
                            //  _painter.RenderSurface.DestBitmap.SaveImage("alpha_2.png");
#endif

                        }
                        break;
                    case AntialiasTechnique.GreyscaleStencil:
                        {
                            //fill once
                            //we choose green channel (middle)
                            _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                            _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.EnableAll;
                            _painter.FillRect(gx + 1, gy, srcW, srcH);
                        }
                        break;
                }
            }


            _painter.UseLcdEffectSubPixelRendering = painterUseSubPixelLcdEffect; //restore
            //
            _painter.DestBitmapBlender.OutputPixelBlender = prevPxBlender;//restore back

        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            DrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            //create temp buffer span that describe the part of a whole char buffer
            var textBufferSpan = new TextBufferSpan(textBuffer, startAt, len);
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font      
            DrawFromGlyphPlans(_txtClient.CreateGlyphPlanSeq(textBufferSpan, _font), startAt, len, x, y);
        }


    }
}
