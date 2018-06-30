/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
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


    public abstract class EffectConfigToken : ICloneable
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
                float rb = token.preRotateZ;
                float ra = token.postRotateZ;
                float r = -token.tilt;
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
                computedOnce = new RzInfo();
                computedOnce.Update(this);
            }
        }

        private bool highQuality;
        public bool HighQuality
        {
            get
            {
                return highQuality;
            }

            set
            {
                this.highQuality = value;
            }
        }

        private RzInfo computedOnce;
        internal RzInfo ComputedOnce
        {
            get
            {
                return computedOnce;
            }
        }

        private float preRotateZ;
        public float PreRotateZ
        {
            get
            {
                return preRotateZ;
            }

            set
            {
                preRotateZ = value;
                UpdateRzInfo();
            }
        }

        private float postRotateZ;
        public float PostRotateZ
        {
            get
            {
                return postRotateZ;
            }

            set
            {
                postRotateZ = value;
                UpdateRzInfo();
            }
        }

        private float tilt;
        public float Tilt
        {
            get
            {
                return tilt;
            }

            set
            {
                tilt = value;
                UpdateRzInfo();
            }
        }

        private float zoom;
        public float Zoom
        {
            get
            {
                return zoom;
            }

            set
            {
                zoom = value;
                UpdateRzInfo();
            }
        }

        private bool sourceAsBackground;
        public bool SourceAsBackground
        {
            get
            {
                return sourceAsBackground;
            }

            set
            {
                sourceAsBackground = value;
                UpdateRzInfo();
            }
        }

        private bool tile;
        public bool Tile
        {
            get
            {
                return tile;
            }

            set
            {
                tile = value;
                UpdateRzInfo();
            }
        }

        private PointF offset;
        public PointF Offset
        {
            get
            {
                return offset;
            }

            set
            {
                offset = value;
                UpdateRzInfo();
            }
        }

        public RotateZoomEffectConfigToken(bool highQuality, float preRotateZ, float postRotateZ,
            float tilt, float zoom, PointF offset, bool sourceAsBackground, bool tile)
        {
            this.highQuality = highQuality;
            this.preRotateZ = preRotateZ;
            this.postRotateZ = postRotateZ;
            this.tilt = tilt;
            this.zoom = zoom;
            this.offset = offset;
            this.sourceAsBackground = sourceAsBackground;
            this.tile = tile;
            UpdateRzInfo();
        }

        protected RotateZoomEffectConfigToken(RotateZoomEffectConfigToken copyMe)
        {
            this.highQuality = copyMe.highQuality;
            this.preRotateZ = copyMe.preRotateZ;
            this.postRotateZ = copyMe.postRotateZ;
            this.tilt = copyMe.tilt;
            this.zoom = copyMe.zoom;
            this.offset = copyMe.offset;
            this.sourceAsBackground = copyMe.sourceAsBackground;
            this.tile = copyMe.tile;
            UpdateRzInfo();
        }

        public override object Clone()
        {
            return new RotateZoomEffectConfigToken(this);
        }
    }
}
