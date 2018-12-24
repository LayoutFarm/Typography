//MIT, 2016-present, WinterDev, Sam Hocevar
using System;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.PixelProcessing;
using Typography.OpenFont;
using Typography.Rendering;
using Typography.TextLayout;
using Typography.Contours;

namespace PixelFarm.Drawing.Fonts
{

    public enum AntialiasTechnique
    {
        LcdStencil,
        GreyscaleStencil,
        None,
    }
    public sealed class FontAtlasTextPrinter : TextPrinterBase, ITextPrinter, IDisposable
    {
        PixelBlenderWithMask _maskPixelBlender = new PixelBlenderWithMask();
        PixelBlenderPerColorComponentWithMask _maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

        AggPainter _maskBufferPainter;
        MemBitmap _fontBmp;
        MemBitmap _alphaBmp;


        /// <summary>
        /// target canvas
        /// </summary>
        AggPainter _painter;
        RequestFont _font;
        //-----------------------------------------------------------  
        Typeface _currentTypeface;
        Color _fontColor;


        LayoutFarm.OpenFontTextService _textServices;
        BitmapFontManager<MemBitmap> _bmpFontMx;
        SimpleFontAtlas _fontAtlas;

        public FontAtlasTextPrinter(AggPainter painter)
        {
            StartDrawOnLeftTop = true;
            _painter = painter;

            this.PositionTechnique = PositionTechnique.OpenFont;

            _textServices = new LayoutFarm.OpenFontTextService();
            //2. create manager
            _bmpFontMx = new BitmapFontManager<MemBitmap>(
                TextureKind.StencilLcdEffect,
                _textServices,
                atlas =>
                {
                    GlyphImage totalGlyphImg = atlas.TotalGlyph;
                    return MemBitmap.CreateFromCopy(totalGlyphImg.Width, totalGlyphImg.Height, totalGlyphImg.GetImageBuffer());
                }
            );


            //3.  

            ChangeFont(new RequestFont("tahoma", 10));
            SetupMaskPixelBlender(painter.Width, painter.Height);
        }
        public void Dispose()
        {
            //clear this
            //clear maskbuffer
            //clear alpha buffer
        }

        /// <summary>
        /// start draw on 'left-top' of a given area box
        /// </summary>
        public bool StartDrawOnLeftTop { get; set; }


        public AntialiasTechnique AntialiasTech { get; set; }


        public void ChangeFont(RequestFont font)
        {
            //call to service
            _font = font;
            _textServices.ResolveTypeface(font); //resolve for 'actual' font
            _fontAtlas = _bmpFontMx.GetFontAtlas(_font, out _fontBmp);
            FontSizeInPoints = font.SizeInPoints;

        }

        public RequestFont CurrentFont => _font;

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
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            Typeface typeface = _currentTypeface;
        }

        public override void DrawCaret(float x, float y)
        {
            //TODO: remove draw caret here, this is for debug only 

        }

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
            TextBufferSpan textBuffSpan = new TextBufferSpan(buffer, startAt, len);
            Size s = _textServices.MeasureString(ref textBuffSpan, _painter.CurrentFont);
            w = s.Width;
            h = s.Height;
        }
        /// <summary>
        /// draw specfic glyph with current settings, at specific position
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyph(Glyph glyph, double x, double y)
        {
            //TODO...
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //TODO...
        }
        public override void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanSeq, int startAt, int len, float left, float top)
        {

            Typeface typeface = _textServices.ResolveTypeface(_font);

            float scale = typeface.CalculateScaleToPixelFromPointSize(_font.SizeInPoints);
            int recommendLineSpacing = (int)_font.LineSpacingInPixels;
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

            PixelBlender32 prevPxBlender = _painter.DestBitmapBlender.OutputPixelBlender; //save
            _painter.DestBitmapBlender.OutputPixelBlender = _maskPixelBlenderPerCompo; //change to new blender  

            bool fillGlyphByGlyph = true;

            if (fillGlyphByGlyph)
            {
                //test...
                //fill glyph-by-glyh

                var aaTech = this.AntialiasTech;
                int seqLen = glyphPlanSeq.Count;


                for (int i = 0; i < seqLen; ++i)
                {
                    UnscaledGlyphPlan unscaledGlyphPlan = glyphPlanSeq[i];
                    TextureGlyphMapData glyphData;
                    if (!_fontAtlas.TryGetGlyphMapData(unscaledGlyphPlan.glyphIndex, out glyphData))
                    {
                        //if no glyph data, we should render a missing glyph ***
                        continue;
                    }
                    //--------------------------------------
                    //TODO: review precise height in float
                    //-------------------------------------- 
                    int srcX, srcY, srcW, srcH;
                    glyphData.GetRect(out srcX, out srcY, out srcW, out srcH);

                    float ngx = acc_x + (float)Math.Round(unscaledGlyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(unscaledGlyphPlan.OffsetY * scale);
                    //NOTE:
                    // -glyphData.TextureXOffset => restore to original pos
                    // -glyphData.TextureYOffset => restore to original pos 
                    //--------------------------

                    gx = (float)(left + (ngx - glyphData.TextureXOffset));
                    gy = (float)(top + (ngy + glyphData.TextureYOffset - srcH + lineHeight));

                    acc_x += (float)Math.Round(unscaledGlyphPlan.AdvanceX * scale);
                    gy = (float)Math.Floor(gy);// + lineHeight;

                    //clear with solid black color 
                    //_maskBufferPainter.Clear(Color.Black);
                    //clear mask buffer at specific pos
                    _maskBufferPainter.FillRect(gx - 1, gy - 1, srcW + 2, srcH + 2, Color.Black);
                    //draw 'stencil' glyph on mask-buffer                
                    _maskBufferPainter.DrawImage(_fontBmp, gx, gy, srcX, _fontBmp.Height - (srcY + srcH), srcW, srcH);

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
                            }
                            break;
                        case AntialiasTechnique.GreyscaleStencil:
                            {
                                //fill once
                                //we choose greeh channel (middle)
                                _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                                _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.EnableAll;
                                _painter.FillRect(gx + 1, gy, srcW, srcH);
                            }
                            break;
                    }
                }
            }
            else
            {
                //clear entire line
                _maskBufferPainter.FillRect(gx - 1, gy - 1, _maskBufferPainter.Width - gx + 2, lineHeight + 2, Color.Black);

                bool isFirst = true;
                int startX = 0, startY = 0;
                float lenW = 0;
                float lenH = 0;

                int seqLen = glyphPlanSeq.Count;
                for (int i = 0; i < seqLen; ++i)
                {
                    UnscaledGlyphPlan glyph = glyphPlanSeq[i];

                    TextureGlyphMapData glyphData;
                    if (!_fontAtlas.TryGetGlyphMapData(glyph.glyphIndex, out glyphData))
                    {
                        //if no glyph data, we should render a missing glyph ***
                        continue;
                    }
                    //--------------------------------------
                    //TODO: review precise height in float
                    //-------------------------------------- 
                    int srcX, srcY, srcW, srcH;
                    glyphData.GetRect(out srcX, out srcY, out srcW, out srcH);

                    float ngx = acc_x + (float)Math.Round(glyph.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyph.OffsetY * scale);
                    //NOTE:
                    // -glyphData.TextureXOffset => restore to original pos
                    // -glyphData.TextureYOffset => restore to original pos 
                    //--------------------------
                    gx = (float)(left + (ngx - glyphData.TextureXOffset)); //ideal x
                    gy = (float)(top + (ngy - glyphData.TextureYOffset - srcH + lineHeight));

                    acc_x += (float)Math.Round(glyph.AdvanceX * scale);
                    gy = (float)Math.Floor(gy) + lineHeight;

                    if (isFirst)
                    {
                        startX = (int)gx;
                        startY = (int)gy;
                        isFirst = false;
                    }

                    _maskBufferPainter.DrawImage(_fontBmp, gx + 1, gy, srcX, _fontBmp.Height - (srcY), srcW + 1, srcH);

                    lenW = gx + srcW;
                    if (srcH > lenH)
                    {
                        lenH = srcH;
                    }

                }
                //--------------------------
                //fill color on 'stencil' mask
                {
                    //select component to render this need to render 3 times for lcd technique
                    //1. B
                    _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.B;
                    _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.B;
                    _painter.FillRect(startX + 1, startY, lenW, lenH);
                    //2. G
                    _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                    _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.G;
                    _painter.FillRect(startX + 1, startY, lenW, lenH);
                    //3. R
                    _maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.R;
                    _maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.R;
                    _painter.FillRect(startX + 1, startY, lenW, lenH);
                }
            }

            //
            _painter.DestBitmapBlender.OutputPixelBlender = prevPxBlender;//restore back
        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            InternalDrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InternalDrawString(textBuffer, startAt, len, x, y);
        }

        void InternalDrawString(char[] buffer, int startAt, int len, float x, float y)
        {

            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font      
            DrawFromGlyphPlans(_textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font), startAt, len, x, y);
        }
    }
}
