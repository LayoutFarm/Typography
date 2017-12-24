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
            //temp fix,            


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

            //@prepare'note, 2017-10-20
            //temp fix, temp disable customfit if we build emoji font
            _currentGlyphBuilder.TemporaryDisableCustomFit = (typeface.COLRTable != null) && (typeface.CPALTable != null);
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

                if (dynamicOutline != null)
                {
                    //has dynamic outline data
                    glyphMeshData.avgXOffsetToFit = dynamicOutline.AvgXFitOffset;
                    glyphMeshData.orgBounds = dynamicOutline.OriginalGlyphControlBounds;
                    glyphMeshData.dynamicOutline = dynamicOutline;
                }
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
                float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_currentFontSizeInPoints);
                GlyphDynamicOutline dynamicOutline = glyphMeshData.dynamicOutline;
                if (dynamicOutline != null)
                {
                    dynamicOutline.GenerateOutput(_tovxs, pxscale);
                    glyphMeshData.vxsStore = new VertexStore();
                    _tovxs.WriteOutput(glyphMeshData.vxsStore, _vxsPool);
                }
                else
                {
                    //no dynamic outline
                    glyphMeshData.vxsStore = new VertexStore();
                    _currentGlyphBuilder.ReadShapes(_tovxs);
                    //TODO: review here,
                    //float pxScale = _glyphPathBuilder.GetPixelScale(); 
                    _tovxs.WriteOutput(glyphMeshData.vxsStore, _vxsPool);
                }


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
            UseWithLcdSubPixelRenderingTechnique = true;//default
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
        /// <summary>
        /// fit the glyph along alignment direction (horizontal, vertical)
        /// </summary>
        public bool UseWritingDirectionFitAligment { get; set; }

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
            /// distance, scaled a part
            /// </summary>
            public float s_a;
            /// <summary>
            /// distance, scaled c part
            /// </summary>
            public float s_c;

            /// <summary>
            /// approximate final advance width for this glyph
            /// </summary>
            public int final_advW;


            public float m_c;
            public float m_a;
            public short m_a_adjust; //-1,0,1
            public short m_c_adjust; //-1,0,1


            public float m_max;
            public void SetData(float pxscale, GlyphControlParameters controlPars, short offsetX, short offsetY, ushort orgAdvW)
            {

#if DEBUG
                dbugIsPrev = false;
#endif
                s_avg_x_ToFit = controlPars.avgXOffsetToFit;


                float o_a = controlPars.minX;
                float o_c = (short)(orgAdvW - controlPars.maxX);
                if (o_c < 0)
                {
                    //TODO: review here ...
                    //? 
                    //o_c = 0;
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

                final_advW = ((s_advW - (int)s_advW) > 0.5) ?
                                (int)(s_advW + 1) : //round
                                (int)(s_advW);

                //
                m_c = final_advW - (s_xmax + s_avg_x_ToFit);
                m_a = s_avg_x_ToFit + s_xmin;

                if (m_a < 0.5f)
                {
                    m_a_adjust = 1;
                }
                else
                {
                    m_a_adjust = 0;
                }

                if (final_advW - m_c > 1f)
                {
                    m_c_adjust = -1;
                }
                else
                {
                    m_c = 0;
                }


                m_max = s_xmax + s_avg_x_ToFit;

            }
#if DEBUG
            public bool dbugIsPrev;
            float dbug_M_C_Diff { get { return m_c - s_c; } }
            float dbug_M_A_Diff { get { return m_a - s_a; } }
            public override string ToString()
            {
                if (dbugIsPrev)
                {
                    return "m_c:" + m_c + ",diff:" + dbug_M_C_Diff;
                }
                else
                {
                    return "m_a" + m_a + ",diff:" + dbug_M_A_Diff;
                }
            }
#endif
        }



        void LayoutWithoutHorizontalFitAlign(IGlyphPositions posStream, GlyphPlanList outputGlyphPlanList)
        {
            //the default OpenFont layout without fit-to-writing alignment
            int finalGlyphCount = posStream.Count;
            float pxscale = _typeface.CalculateScaleToPixelFromPointSize(this._fontSizeInPoints);
            double cx = 0;
            short cy = 0;

            for (int i = 0; i < finalGlyphCount; ++i)
            {
                short offsetX, offsetY, advW; //all from pen-pos
                ushort glyphIndex = posStream.GetGlyph(i, out offsetX, out offsetY, out advW);

                float s_advW = advW * pxscale;
                float exact_x = (float)(cx + offsetX * pxscale);
                float exact_y = (float)(cy + offsetY * pxscale);

                //outputGlyphPlanList.Append(new GlyphPlan(
                //   glyphIndex,
                //    exact_x,
                //    exact_y,
                //    advW)); //old?
                outputGlyphPlanList.Append(new GlyphPlan(
                   glyphIndex,
                    exact_x,
                    exact_y,
                    s_advW));

                cx += s_advW;
            }
        }

        public void Layout(IGlyphPositions posStream, GlyphPlanList outputGlyphPlanList)
        {

            if (!UseWithLcdSubPixelRenderingTechnique)
            {
                //layout without fit to alignment direction
                LayoutWithoutHorizontalFitAlign(posStream, outputGlyphPlanList);
                return; //early exit
            }
            //------------------------------
            int finalGlyphCount = posStream.Count;
            float pxscale = _typeface.CalculateScaleToPixelFromPointSize(this._fontSizeInPoints);
#if DEBUG
            float dbug_onepx = 1 / pxscale;
#endif
            //
            int cx = 0;
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
                    //inter-glyph space
                    //ideal space
                    float ideal_space = prev_ABC.s_c + current_ABC.s_a;
                    //actual space
                    float actual_space = prev_ABC.m_c + current_ABC.m_a;

                    if (ideal_space < 0)
                    {
                        //f-f
                        //f-o 
                        if (prev_ABC.s_c < 0)
                        {
                            ideal_space = 0 + current_ABC.s_a;
                        }
                        if (ideal_space < 0)
                        {
                            ideal_space = 0;
                        }
                    }
                    if (ideal_space >= 0)
                    {
                        //m-a
                        //i-i
                        //o-p 
                        if (actual_space > 1.5 && actual_space - 0.5 > ideal_space)
                        {
                            cx--;
                        }
                        else
                        {
                            if (actual_space < ideal_space)
                            {
                                if (prev_ABC.final_advW + prev_ABC.m_c_adjust < prev_ABC.m_max)
                                {
                                    cx += current_ABC.m_a_adjust;
                                }
                            }
                            else
                            {
                                if (prev_ABC.final_advW - prev_ABC.m_c + prev_ABC.m_c_adjust > prev_ABC.m_max)
                                {
                                    cx += prev_ABC.m_c_adjust;
                                }
                            }
                        }
                    }
                    else
                    {
                        //this should not occur?

                    }
                }
                //------------------------------------------------------------- 
                float exact_x = (float)(cx + current_ABC.s_offsetX);
                float exact_y = (float)(cy + current_ABC.s_offsetY);

                //check if the current position can create a sharp glyph
                int exact_x_floor = (int)exact_x;
                float x_offset_to_fit = current_ABC.s_avg_x_ToFit;

                float final_x = exact_x_floor + x_offset_to_fit;
                if (UseWithLcdSubPixelRenderingTechnique)
                {
                    final_x += 0.33f;
                }

                outputGlyphPlanList.Append(new GlyphPlan(
                    glyphIndex,
                    final_x,
                    exact_y,
                    current_ABC.final_advW));
                // 
                //
                cx += current_ABC.final_advW;
                //-----------------------------------------------
                prev_ABC = current_ABC;//copy current to prev
#if DEBUG
                prev_ABC.dbugIsPrev = true;
#endif
                // Console.WriteLine(exact_x + "+" + (x_offset_to_fit) + "=>" + final_x);
            }
        }


    }
}