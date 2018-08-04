//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;

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


            internal GlyphMeshData _synthOblique;

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


        GlyphTranslatorToVxs _tovxs = new GlyphTranslatorToVxs();

        public GlyphMeshStore()
        {

        }
        public void SetHintTechnique(HintTechnique hintTech)
        {
            _currentHintTech = hintTech;

        }

        /// <summary>
        /// simulate italic glyph
        /// </summary>
        public bool SimulateOblique { get; set; }

        public bool FlipGlyphUpward { get; set; }


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


        static readonly PixelFarm.CpuBlit.VertexProcessing.Affine _invertY = PixelFarm.CpuBlit.VertexProcessing.Affine.NewScaling(1, -1);

        //shearing horizontal axis to right side, 20 degree, TODO: user can configure this value
        static PixelFarm.CpuBlit.VertexProcessing.Affine _slantHorizontal = PixelFarm.CpuBlit.VertexProcessing.Affine.NewSkewing(PixelFarm.CpuBlit.AggMath.deg2rad(-15), 0);


        VertexStore _temp1 = new VertexStore();
        VertexStore _temp2 = new VertexStore();

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

                    //version 3 
                    if (FlipGlyphUpward)
                    {

                        _temp1.Clear();
                        _temp2.Clear();
                        _tovxs.WriteOutput(_temp1);//write to temp buffer first

                        PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(_invertY, _temp1, _temp2);
                        //then
                        glyphMeshData.vxsStore = _temp2.CreateTrim();


                    }
                    else
                    {
                        _temp1.Clear();
                        _tovxs.WriteOutput(_temp1);
                        glyphMeshData.vxsStore = _temp1.CreateTrim();
                    }
                }
                else
                {

                    if (FlipGlyphUpward)
                    {
                        _temp1.Clear();
                        _temp2.Clear();
                        _currentGlyphBuilder.ReadShapes(_tovxs);
                        _tovxs.WriteOutput(_temp1); //write to temp buffer first 
                        PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(_invertY, _temp1, _temp2);
                        //then
                        glyphMeshData.vxsStore = _temp2.CreateTrim();
                    }
                    else
                    {
                        //no dynamic outline
                        _temp1.Clear();
                        _currentGlyphBuilder.ReadShapes(_tovxs);
                        //TODO: review here,
                        //float pxScale = _glyphPathBuilder.GetPixelScale(); 

                        _tovxs.WriteOutput(_temp1);
                        glyphMeshData.vxsStore = _temp1.CreateTrim();

                    }
                }
            }


            if (SimulateOblique)
            {
                if (glyphMeshData._synthOblique == null)
                {
                    //create italic version
                    SimulateQbliqueGlyph(glyphMeshData);
                }
                return glyphMeshData._synthOblique.vxsStore;
            }
            else
            {
                return glyphMeshData.vxsStore;
            }
        }
        void SimulateQbliqueGlyph(GlyphMeshData orgGlyphMashData)
        {
            _temp1.Clear();
            PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(_slantHorizontal, orgGlyphMashData.vxsStore, _temp1);
            //italic mesh data

            GlyphMeshData obliqueVersion = new GlyphMeshData();
            obliqueVersion.vxsStore = _temp1.CreateTrim();

            orgGlyphMashData._synthOblique = obliqueVersion;

        }
    }


}