//MIT, 2016-2017, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Agg;

using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;
using Typography.TextLayout;


namespace Typography.Contours
{


    public struct GlyphControlParameters
    {
        public float avgXOffsetToFit;
        public short minX;
        public short minY;
        public short maxX;
        public short maxY;

    }
    class GlyphMeshStore
    {

        class GlyphMeshData
        {
            public GlyphDynamicOutline dynamicOutline;
            public VertexStore vxsStore;
            public float avgXOffsetToFit;
            public Bounds orgBounds;

            public GlyphControlParameters GetControlPars()
            {
                var pars = new GlyphControlParameters();
                pars.minX = orgBounds.XMin;
                pars.minY = orgBounds.YMin;
                pars.maxX = orgBounds.XMax;
                pars.maxY = orgBounds.YMax;
                pars.avgXOffsetToFit = avgXOffsetToFit;
                return pars;
            }

        }
        /// <summary>
        /// store typeface and its builder
        /// </summary>
        Dictionary<Typeface, GlyphPathBuilder> _cacheGlyphPathBuilders = new Dictionary<Typeface, GlyphPathBuilder>();
        /// <summary>
        /// glyph mesh data for specific condition
        /// </summary>
        GlyphMeshCollection<GlyphMeshData> _hintGlyphCollection = new GlyphMeshCollection<GlyphMeshData>();

        GlyphPathBuilder _currentGlyphBuilder;
        Typeface _currentTypeface;
        float _currentFontSizeInPoints;
        HintTechnique _currentHintTech;

        VertexStorePool _vxsPool = new VertexStorePool(); //TODO: review pool again
        GlyphTranslatorToVxs _tovxs = new GlyphTranslatorToVxs();

        public GlyphMeshStore()
        {

        }
        public void SetHintTechnique(HintTechnique hintTech)
        {
            _currentHintTech = hintTech;

        }

        /// <summary>
        /// set current font
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="fontSizeInPoints"></param>
        public void SetFont(Typeface typeface, float fontSizeInPoints)
        {
            if (_currentGlyphBuilder != null && !_cacheGlyphPathBuilders.ContainsKey(typeface))
            {
                //store current typeface to cache
                _cacheGlyphPathBuilders[_currentTypeface] = _currentGlyphBuilder;
            }
            _currentTypeface = typeface;
            _currentGlyphBuilder = null;
            if (typeface == null) return;

            //----------------------------
            //check if we have this in cache ?
            //if we don't have it, this _currentTypeface will set to null ***                  
            _cacheGlyphPathBuilders.TryGetValue(_currentTypeface, out _currentGlyphBuilder);
            if (_currentGlyphBuilder == null)
            {
                _currentGlyphBuilder = new GlyphPathBuilder(typeface);
            }
            //----------------------------------------------
            this._currentFontSizeInPoints = fontSizeInPoints;
            //------------------------------------------ 
            _hintGlyphCollection.SetCacheInfo(typeface, this._currentFontSizeInPoints, _currentHintTech);
        }
        /// <summary>
        /// get existing or create new one from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        GlyphMeshData InternalGetGlyphMesh(ushort glyphIndex)
        {
            GlyphMeshData glyphMeshData;
            if (!_hintGlyphCollection.TryGetCacheGlyph(glyphIndex, out glyphMeshData))
            {
                //if not found then create new glyph vxs and cache it
                _currentGlyphBuilder.SetHintTechnique(_currentHintTech);
                _currentGlyphBuilder.BuildFromGlyphIndex(glyphIndex, _currentFontSizeInPoints);
                GlyphDynamicOutline dynamicOutline = _currentGlyphBuilder.LatestGlyphFitOutline;
                //-----------------------------------  
                glyphMeshData = new GlyphMeshData();
                glyphMeshData.avgXOffsetToFit = dynamicOutline.AvgXFitOffset;
                glyphMeshData.orgBounds = dynamicOutline.OriginalGlyphControlBounds;
                glyphMeshData.dynamicOutline = dynamicOutline;
                Bounds orgGlyphBounds = dynamicOutline.OriginalGlyphControlBounds;

                _hintGlyphCollection.RegisterCachedGlyph(glyphIndex, glyphMeshData);
                //-----------------------------------    
            }
            return glyphMeshData;
        }
        /// <summary>
        /// get glyph left offset-to-fit value from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        public GlyphControlParameters GetControlPars(ushort glyphIndex)
        {
            return InternalGetGlyphMesh(glyphIndex).GetControlPars();
        }

        /// <summary>
        /// get glyph mesh from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        public VertexStore GetGlyphMesh(ushort glyphIndex)
        {
            GlyphMeshData glyphMeshData = InternalGetGlyphMesh(glyphIndex);
            if (glyphMeshData.vxsStore == null)
            {
                //build vxs
                _tovxs.Reset();

                float pxscale = _currentTypeface.CalculateToPixelScaleFromPointSize(_currentFontSizeInPoints);
                GlyphDynamicOutline dynamicOutline = glyphMeshData.dynamicOutline;
                dynamicOutline.GenerateOutput(_tovxs, pxscale);
                glyphMeshData.vxsStore = new VertexStore();
                //----------------
                _tovxs.WriteOutput(glyphMeshData.vxsStore, _vxsPool);

            }
            return glyphMeshData.vxsStore;

        }
    }

    class PixelScaleLayoutEngine : IPixelScaleLayout
    {
        Typeface _typeface;
        GlyphMeshStore _hintedFontStore;
        float _fontSizeInPoints;
        public PixelScaleLayoutEngine()
        {
        }
        public GlyphMeshStore HintedFontStore
        {
            get { return _hintedFontStore; }
            set
            {
                _hintedFontStore = value;
            }
        }

        public bool UseWithLcdSubPixelRenderingTechnique { get; set; }

        public void SetFont(Typeface typeface, float fontSizeInPoints)
        {
            _typeface = typeface;
            _fontSizeInPoints = fontSizeInPoints;
        }


        struct FineABC
        {
            //this struct is used for local calculation (in a method) only
            //not suite for storing data / pass data between methods

            /// <summary>
            /// avg x to fit value, this is calculated value from dynamic layout
            /// </summary>
            public float s_avg_x_ToFit;

            /// <summary>
            /// scaled offsetX
            /// </summary>
            public float s_offsetX;
            /// <summary>
            /// scaled offsetY
            /// </summary>
            public float s_offsetY;
            /// <summary>
            ///  scaled advance width
            /// </summary>
            public float s_advW;
            /// <summary>
            /// scaled x min
            /// </summary>
            public float s_xmin;
            /// <summary>
            /// scaled x max
            /// </summary>
            public float s_xmax;
            /// <summary>
            /// scaled a part
            /// </summary>
            public float s_a;
            /// <summary>
            /// scaled c part
            /// </summary>
            public float s_c;

            /// <summary>
            /// approximate final advance width for this glyph
            /// </summary>
            public int final_advW;


            public float c_diff;
            public float s_xmax_to_final_advance;

            public void SetData(float pxscale, GlyphControlParameters controlPars, short offsetX, short offsetY, ushort orgAdvW)
            {

                s_avg_x_ToFit = controlPars.avgXOffsetToFit;
                float o_a = controlPars.minX;
                float o_c = (short)(orgAdvW - controlPars.maxX);

                if (o_c < 0)
                {
                    //TODO: review here ...
                    //? 
                    o_c = 0;
                }
                //-----------------
                //calculate...  
                s_offsetX = pxscale * offsetX;
                s_offsetY = pxscale * offsetY;
                s_advW = pxscale * orgAdvW;
                s_xmin = pxscale * controlPars.minX;
                s_xmax = pxscale * controlPars.maxX;
                s_a = pxscale * o_a;
                s_c = pxscale * o_c;
                //--------------------------------------   
                final_advW = ((s_advW - (int)s_advW) > 0.5) ?
                                (int)(s_advW + 1) : //round
                                (int)(s_advW);
                s_xmax_to_final_advance = final_advW - s_xmax;
                c_diff = final_advW - s_advW;
            }

        }
        public void Layout(IGlyphPositions posStream, List<GlyphPlan> outputGlyphPlanList)
        {

            int finalGlyphCount = posStream.Count;
            float pxscale = _typeface.CalculateToPixelScaleFromPointSize(this._fontSizeInPoints);
            float onepx = 1 / pxscale;
            //
            double cx = 0;
            short cy = 0;
            //
            //at this state, we need exact info at this specific pxscale
            //
            _hintedFontStore.SetFont(_typeface, this._fontSizeInPoints);
            FineABC current_ABC = new FineABC();
            FineABC prev_ABC = new FineABC();

            for (int i = 0; i < finalGlyphCount; ++i)
            {
                short offsetX, offsetY, advW; //all from pen-pos
                ushort glyphIndex = posStream.GetGlyph(i, out offsetX, out offsetY, out advW);
                GlyphControlParameters controlPars = _hintedFontStore.GetControlPars(glyphIndex);
                current_ABC.SetData(pxscale, controlPars, offsetX, offsetY, (ushort)advW);
                //-------------------------------------------------------------
                if (i > 0)
                {
                    //ideal interspace
                    //float idealInterGlyphSpace = -prev_ABC.s_avgToFit + prev_ABC.s_c + current_ABC.s_a + current_ABC.s_avgToFit;
                    //float idealInterGlyphSpace = -prev_ABC.s_avgToFit + prev_ABC.s_c + current_ABC.s_a + current_ABC.s_avgToFit;
                    float idealInterGlyphSpace = prev_ABC.s_c + current_ABC.s_a;
                    if (idealInterGlyphSpace > 1 - 0.5f)
                    {
                        //please ensure that we have interspace atleast 1px
                        //if not we just insert 1 px  ***

                        //TODO: review here,
                        //0.66f come from  2/3f of a pixel  
                        if (idealInterGlyphSpace < 1 + 0.66f)
                        {
                            float fine_h = -prev_ABC.s_avg_x_ToFit + prev_ABC.c_diff + current_ABC.s_a + current_ABC.s_avg_x_ToFit;
                            if (fine_h < 0)
                            {
                                //need more space
                                //i-o
                                cx += 1;
                            }
                            else
                            {

                                if (fine_h > 1)
                                {
                                    //o-i
                                    cx -= 1;
                                }
                            }
                        }
                        else
                        {
                            if (-prev_ABC.s_avg_x_ToFit + current_ABC.s_avg_x_ToFit > 0.5f)
                            {
                                cx--;
                            }
                        }
                    }
                    else
                    {
                        float idealInterGlyphSpace2 = -prev_ABC.s_avg_x_ToFit + prev_ABC.s_c + current_ABC.s_a + current_ABC.s_avg_x_ToFit;

                        if (idealInterGlyphSpace2 < 0)
                        {
                            // eg i-j seq
                            cx++;
                        }
                        else
                        {

                            if (prev_ABC.s_xmax_to_final_advance < 0)
                            {
                                //f-f
                                cx++;
                            }
                        }
                    }
                }
                //------------------------------------------------------------- 
                float exact_x = (float)(cx + current_ABC.s_offsetX);
                float exact_y = (float)(cy + current_ABC.s_offsetY);

                //check if the current position can create a sharp glyph
                int exact_x_floor = (int)exact_x;
                float x_offset_to_fit = controlPars.avgXOffsetToFit;
                //offset range that can produce sharp glyph (by observation)
                //is between x_offset_to_fit - 0.3f to x_offset_to_fit + 0.3f 

                float final_x = exact_x_floor + x_offset_to_fit;
                if (UseWithLcdSubPixelRenderingTechnique)
                {
                    final_x += 0.33f;
                }

                outputGlyphPlanList.Add(new GlyphPlan(
                    glyphIndex,
                    final_x,
                    exact_y,
                    current_ABC.final_advW));
                // 
                //
                cx += current_ABC.final_advW;
                //-----------------------------------------------
                prev_ABC = current_ABC;//add to prev

                // Console.WriteLine(exact_x + "+" + (x_offset_to_fit) + "=>" + final_x);
            }
        }
    }
}