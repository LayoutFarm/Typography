/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////


//Apache2, 2018, WinterDev

using System;
using PixelFarm.Drawing;
namespace PaintFx.Effects
{


    public abstract class EffectConfigToken
    {
        /// <summary>
        /// This should simply call "new myType(this)" ... do not call base class'
        /// implementation of Clone, as this is handled by the constructors.
        /// </summary>
        public abstract object Clone();

        public EffectConfigToken()
        {
        }

        protected EffectConfigToken(EffectConfigToken copyMe)
        {
        }
    }

    public class RotateZoomEffectConfigToken : EffectConfigToken
    {
        internal class RzInfo
        {
            // gradients
            public float startX;
            public float startY;
            public float startZ;
            public float dsxdx;
            public float dsydx;
            public float dszdx;
            public float dsxdy;
            public float dsydy;
            public float dszdy;

            private void Transform(RotateZoomEffectConfigToken token, int x, int y, out float sx, out float sy, out float sz)
            {
                float rb = token._preRotateZ;
                float ra = token._postRotateZ;
                float r = -token._tilt;
                float crb = (float)Math.Cos(rb);
                float cr = (float)Math.Cos(r);
                float cra = (float)Math.Cos(ra);
                float srb = (float)Math.Sin(rb);
                float sr = (float)Math.Sin(r);
                float sra = (float)Math.Sin(ra);
                float ox = x, oy = y, oz = 0;

                sx = (ox * crb + oy * srb) / cr;
                sy = -ox * srb + oy * crb;

                sz = sx * sr;
                sx = sx / cr;
                ox = sx; oy = sy; oz = sz;

                sx = ox * cra + oy * sra;
                sy = -ox * sra + oy * cra;
            }

            public void Update(RotateZoomEffectConfigToken token)
            {
                Transform(token, 0, 0, out startX, out startY, out startZ);
                Transform(token, 1, 0, out dsxdx, out dsydx, out dszdx);
                Transform(token, 0, 1, out dsxdy, out dsydy, out dszdy);

                dsxdx -= startX; dsydx -= startY; dszdx -= startZ;
                dsxdy -= startX; dsydy -= startY; dszdy -= startZ;
            }
        }

        private void UpdateRzInfo()
        {
            lock (this)
            {
                ComputedOnce = new RzInfo();
                ComputedOnce.Update(this);
            }
        }
        public bool HighQuality { get; set; }
        internal RzInfo ComputedOnce { get; private set; }

        float _preRotateZ;
        public float PreRotateZ
        {
            get => _preRotateZ;
            set
            {
                _preRotateZ = value;
                UpdateRzInfo();
            }
        }

        float _postRotateZ;
        public float PostRotateZ
        {
            get => _postRotateZ;
            set
            {
                _postRotateZ = value;
                UpdateRzInfo();
            }
        }

        float _tilt;
        public float Tilt
        {
            get => _tilt;
            set
            {
                _tilt = value;
                UpdateRzInfo();
            }
        }

        float _zoom;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                UpdateRzInfo();
            }
        }

        bool _sourceAsBackground;
        public bool SourceAsBackground
        {
            get => _sourceAsBackground;
            set
            {
                _sourceAsBackground = value;
                UpdateRzInfo();
            }
        }

        bool _tile;
        public bool Tile
        {
            get => _tile;
            set
            {
                _tile = value;
                UpdateRzInfo();
            }
        }

        PointF _offset;
        public PointF Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdateRzInfo();
            }
        }

        public RotateZoomEffectConfigToken(bool highQuality, float preRotateZ, float postRotateZ,
            float tilt, float zoom, PointF offset, bool sourceAsBackground, bool tile)
        {
            this.HighQuality = highQuality;
            _preRotateZ = preRotateZ;
            _postRotateZ = postRotateZ;
            _tilt = tilt;
            _zoom = zoom;
            _offset = offset;
            _sourceAsBackground = sourceAsBackground;
            _tile = tile;
            UpdateRzInfo();
        }

        protected RotateZoomEffectConfigToken(RotateZoomEffectConfigToken copyMe)
        {
            this.HighQuality = copyMe.HighQuality;
            _preRotateZ = copyMe._preRotateZ;
            _postRotateZ = copyMe._postRotateZ;
            _tilt = copyMe._tilt;
            _zoom = copyMe._zoom;
            _offset = copyMe._offset;
            _sourceAsBackground = copyMe._sourceAsBackground;
            _tile = copyMe._tile;
            UpdateRzInfo();
        }

        public override object Clone()
        {
            return new RotateZoomEffectConfigToken(this);
        }
    }
}
