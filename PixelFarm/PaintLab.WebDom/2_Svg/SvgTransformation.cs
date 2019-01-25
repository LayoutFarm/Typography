//
//some parts are from github.com/vvvv/svg 
//license : Microsoft Public License (MS-PL) 
// 

namespace PaintLab.Svg
{

    public enum SvgTransformKind
    {
        Matrix,
        Translation,
        Scale,
        Skew,
        Rotation,
        Shear
    }
    public abstract class SvgTransform
    {
        public abstract SvgTransformKind TransformKind { get; }
        public PixelFarm.CpuBlit.VertexProcessing.ICoordTransformer ResolvedICoordTransformer { get; set; }
    }

    /// <summary>
    /// The class which applies custom transform to this Matrix
    /// </summary>
    public sealed class SvgTransformMatrix : SvgTransform
    {
        float[] _elements;

        public SvgTransformMatrix(
            float sx, float shx,
            float shy, float sy,
            float tx, float ty
            )
        {
            //https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Basic_Transformations
            //... transformations can be expressed by a 2x3 transformation matrix.
            //To combine several transformations,
            //one can set the resulting matrix directly with the
            //matrix
            //(a, b,
            // c, d,
            // e, f) 
            //transformation which maps coordinates from a previous coordinate system into a new coordinate system by ...

            //x1= ax0 + cy0 + e
            //y1= bx0 + dy0 + f 

            //or
            //matrix
            //(v0_sx,  v1_shy,
            // v2_shx, v3_sy,
            // v4_tx,  v5_ty) 

            _elements = new float[] {
                sx, shx,
                shy, sy,
                tx, ty };
        }
        public SvgTransformMatrix(float[] elements)
        {
            _elements = elements;
        }
        public float[] Elements => _elements;
        public override SvgTransformKind TransformKind => SvgTransformKind.Matrix;
    }


    public sealed class SvgTranslate : SvgTransform
    {
        public float X
        {
            get;
            private set;
        }

        public float Y
        {
            get;
            private set;
        }
        public SvgTranslate(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public SvgTranslate(float x)
            : this(x, 0.0f)
        {
        }
        public override SvgTransformKind TransformKind => SvgTransformKind.Translation;
    }
    /// <summary>
    /// The class which applies the specified skew vector to this Matrix.
    /// </summary>
    public sealed class SvgSkew : SvgTransform
    {
        public float AngleX { get; private set; }
        public float AngleY { get; private set; }
        public SvgSkew(float x, float y)
        {
            AngleX = x;
            AngleY = y;
        }
        public override SvgTransformKind TransformKind => SvgTransformKind.Skew;
    }

    /// <summary>
    /// The class which applies the specified shear vector to this Matrix.
    /// </summary>
    public sealed class SvgShear : SvgTransform
    {

        public SvgShear(float x) : this(x, x) { }

        public SvgShear(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public override SvgTransformKind TransformKind => SvgTransformKind.Shear;
    }

    public sealed class SvgScale : SvgTransform
    {
        public SvgScale(float x) : this(x, x) { }
        public SvgScale(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public override SvgTransformKind TransformKind => SvgTransformKind.Scale;
    }

    public sealed class SvgRotate : SvgTransform
    {
        public float Angle { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public SvgRotate(float angle)
        {
            this.Angle = angle;
        }

        public SvgRotate(float angle, float centerX, float centerY)
            : this(angle)
        {
            this.CenterX = centerX;
            this.CenterY = centerY;
            SpecificRotationCenter = true;
        }
        public override SvgTransformKind TransformKind => SvgTransformKind.Rotation;
        public bool SpecificRotationCenter { get; private set; }
    }


}
