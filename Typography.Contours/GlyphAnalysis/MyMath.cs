//MIT, 2017, WinterDev
using System.Numerics;
namespace Typography.Contours
{

    public static class MyMath
    {
        static internal void FindMinMax(ref float currentMin, ref float currentMax, float value)
        {
            if (value < currentMin) { currentMin = value; }
            if (value > currentMax) { currentMax = value; }
        }

        /// <summary>
        /// calculate distance to fit interger grid pos, assum grid size=1
        /// </summary>
        /// <param name="value">diff distance to fit grid position</param>
        /// <returns></returns>
        internal static float CalculateDiffToFit(float value)
        {
            //optimized version, assum gridSize = 1
            int floor = (int)value;
            return ((value - floor) >= (1 / 2f)) ?
                (floor + 1) - value : //if true, more than half of 1 
                floor - value; //else
        }
        internal static int FitToHalfGrid(float value, int gridSize)
        {
            //fit to grid 
            //1. lower
            int floor = ((int)(value / gridSize) * gridSize);

            //2. midpoint
            float remaining = value - floor;
            float halfGrid = gridSize / 2f;

            if (remaining >= (2 / 3f) * gridSize)
            {
                return floor + gridSize;
            }
            else if (remaining >= (1 / 3f) * gridSize)
            {
                return (int)(floor + gridSize * (1 / 2f));
            }
            else
            {
                return floor;
            }

        }
        public static double AngleBetween(Vector2 vector1, Vector2 vector2)
        {
            double rad1 = System.Math.Atan2(vector1.Y, vector1.X);
            double rad2 = System.Math.Atan2(vector2.Y, vector2.X);
            //we want to find diff

            if (rad1 < 0)
            {
                rad1 = System.Math.PI + rad1;
            }
            if (rad2 < 0)
            {
                rad2 = System.Math.PI + rad2;
            }

            return rad1 - rad2;
        }
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static double DegreesToRadians(double degrees)
        {
            const double degToRad = System.Math.PI / 180.0f;
            return degrees * degToRad;
        }
        public static bool MinDistanceFirst(Vector2 baseVec, Vector2 compare0, Vector2 compare1)
        {
            return (SquareDistance(baseVec, compare0) < SquareDistance(baseVec, compare1)) ? true : false;
        }

        public static double SquareDistance(Vector2 v0, Vector2 v1)
        {
            double xdiff = v1.X - v0.X;
            double ydiff = v1.Y - v0.Y;
            return (xdiff * xdiff) + (ydiff * ydiff);
        }
        public static int Min(double v0, double v1, double v2)
        {
            //find min of 3
            unsafe
            {
                double* doubleArr = stackalloc double[3];
                doubleArr[0] = v0;
                doubleArr[1] = v1;
                doubleArr[2] = v2;

                double min = double.MaxValue;
                int foundAt = 0;
                for (int i = 0; i < 3; ++i)
                {
                    if (doubleArr[i] < min)
                    {
                        foundAt = i;
                        min = doubleArr[i];
                    }
                }
                return foundAt;
            }

        }
        /// <summary>
        /// find cut point and check if the cut point is on the edge
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="p2"></param>
        /// <param name="cutResult"></param>
        /// <returns></returns>
        public static bool FindPerpendicularCutPoint(EdgeLine edge, Vector2 p2, out Vector2 cutResult)
        {
            cutResult = FindPerpendicularCutPoint(
                new Vector2((float)edge.PX, (float)edge.PY),
                new Vector2((float)edge.QX, (float)edge.QY),
                p2);
            //also check if result cutpoint is on current line segment or not

            Vector2 min, max;
            GetMinMax(edge, out min, out max);
            return (cutResult.X >= min.X && cutResult.X <= max.X && cutResult.Y >= min.Y && cutResult.Y <= max.Y);
        }
        /// <summary>
        /// which one is min,max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        static void GetMinMax(EdgeLine edge, out Vector2 min, out Vector2 max)
        {
            Vector2 a_pos = new Vector2((float)edge.PX, (float)edge.PY);
            Vector2 b_pos = new Vector2((float)edge.QX, (float)edge.QY);
            min = Vector2.Min(a_pos, b_pos);
            max = Vector2.Max(a_pos, b_pos);
        }
        static void GetMinMax(Vector2 a_pos, Vector2 b_pos, out Vector2 min, out Vector2 max)
        {

            min = Vector2.Min(a_pos, b_pos);
            max = Vector2.Max(a_pos, b_pos);
        }
        public static int FindMin(Vector2 a, Vector2 b)
        {
            if (a.X < b.X)
            {
                return 0;
            }
            else if (a.X > b.X)
            {
                return 1;
            }
            else
            {
                if (a.Y < b.Y)
                {
                    return 0;
                }
                else if (a.Y > b.Y)
                {
                    return 1;
                }
                else
                {
                    return -1;//eq
                }
            }
        }



        static void GetMinMax(GlyphBone bone, out Vector2 min, out Vector2 max)
        {
            if (bone.JointB != null)
            {
                var a_pos = bone.JointA.OriginalJointPos;
                var b_pos = bone.JointB.OriginalJointPos;

                min = Vector2.Min(a_pos, b_pos);
                max = Vector2.Max(a_pos, b_pos);

            }
            else if (bone.TipEdge != null)
            {
                var a_pos = bone.JointA.OriginalJointPos;
                var tip_pos = bone.TipEdge.GetMidPoint();
                min = Vector2.Min(a_pos, tip_pos);
                max = Vector2.Max(a_pos, tip_pos);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }
        /// <summary>
        /// find a perpendicular cut-point from p to bone
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="p"></param>
        /// <param name="cutPoint"></param>
        /// <returns></returns>
        public static bool FindPerpendicularCutPoint(GlyphBone bone, Vector2 p, out Vector2 cutPoint)
        {
            if (bone.JointB != null)
            {
                cutPoint = FindPerpendicularCutPoint(
                  bone.JointA.OriginalJointPos,
                  bone.JointB.OriginalJointPos,
                  p);
                //find min /max
                Vector2 min, max;
                GetMinMax(bone, out min, out max);
                return cutPoint.X >= min.X && cutPoint.X <= max.X && cutPoint.Y >= min.Y && cutPoint.Y <= max.Y;
            }
            else
            {
                //to tip
                if (bone.TipEdge != null)
                {
                    cutPoint = FindPerpendicularCutPoint(
                        bone.JointA.OriginalJointPos,
                        bone.TipEdge.GetMidPoint(),
                        p);
                    Vector2 min, max;
                    GetMinMax(bone, out min, out max);
                    return cutPoint.X >= min.X && cutPoint.X <= max.X && cutPoint.Y >= min.Y && cutPoint.Y <= max.Y;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        public static Vector2 FindPerpendicularCutPoint(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            //a line from p0 to p1
            //p2 is any point
            //return p3 -> cutpoint on p0,p1 


            double xdiff = p1.X - p0.X;
            double ydiff = p1.Y - p0.Y;
            if (xdiff == 0)
            {
                return new Vector2(p1.X, p2.Y);
            }
            if (ydiff == 0)
            {
                return new Vector2(p2.X, p1.Y);
            }

            double m1 = ydiff / xdiff;
            double b1 = FindB(p0, p1);

            double m2 = -1 / m1;
            double b2 = p2.Y - (m2) * p2.X;
            //find cut point
            double cutx = (b2 - b1) / (m1 - m2);
            double cuty = (m2 * cutx) + b2;
            return new Vector2((float)cutx, (float)cuty);
        }
        /// <summary>
        /// find parameter A,B,C from Ax + By = C, with given 2 points
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        static void FindABC(Vector2 p0, Vector2 p1, out double a, out double b, out double c)
        {
            //line is in the form
            //Ax + By = C 
            //from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
            //and https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/
            a = p1.Y - p0.Y;
            b = p0.X - p1.X;
            c = a * p0.X + b * p0.Y;
        }
        public static bool FindCutPoint(
              Vector2 p0, Vector2 p1,
              Vector2 p2, Vector2 p3, out Vector2 result)
        {
            //TODO: review here
            //from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
            //and https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/

            //------------------------------------------
            //use matrix style ***
            //------------------------------------------
            //line is in the form
            //Ax + By = C
            //so   A1x +B1y= C1 ... line1
            //     A2x +B2y=C2  ... line2
            //------------------------------------------
            //
            //from Ax+By=C ... (1)
            //By = C- Ax;

            double a1, b1, c1;
            FindABC(p0, p1, out a1, out b1, out c1);

            double a2, b2, c2;
            FindABC(p2, p3, out a2, out b2, out c2);

            double delta = a1 * b2 - a2 * b1; //delta is the determinant in math parlance
            if (delta == 0)
            {
                //"Lines are parallel"
                result = Vector2.Zero;
                return false; //
                throw new System.ArgumentException("Lines are parallel");
            }
            double x = (b2 * c1 - b1 * c2) / delta;
            double y = (a1 * c2 - a2 * c1) / delta;
            result = new Vector2((float)x, (float)y);
            return true; //has cutpoint
        }

        static Vector2 FindCutPoint_Algebra(
            Vector2 p0, Vector2 p1,
            Vector2 p2, Vector2 p3)
        {
            //prefer matrix style (upper)

            //TODO: refactor here... 
            //find cut point of 2 line 
            //y = mx + b
            //from line equation
            //y = mx + b ... (1)
            //from (1)
            //b = y- mx ... (2) 
            //----------------------------------
            //line1:
            //y1 = (m1 * x1) + b1 ...(3)            
            //line2:
            //y2 = (m2 * x2) + b2 ...(4)
            //----------------------------------
            //from (3),
            //b1 = y1 - (m1 * x1) ...(5)
            //b2 = y2 - (m2 * x2) ...(6)
            //----------------------------------
            //at cutpoint of line1 and line2 => (x1,y1)== (x2,y2)
            //or find (x,y) where (3)==(4)
            //---------------------------------- 
            //at cutpoint, find x
            // (m1 * x1) + b1 = (m2 * x1) + b2  ...(11), replace x2 with x1
            // (m1 * x1) - (m2 * x1) = b2 - b1  ...(12)
            //  x1 * (m1-m2) = b2 - b1          ...(13)
            //  x1 = (b2-b1)/(m1-m2)            ...(14), now we know x1
            //---------------------------------- 
            //at cutpoint, find y
            //  y1 = (m1 * x1) + b1 ... (15), replace x1 with value from (14)
            //Ans: (x1,y1)
            //----------------------------------

            double y1diff = p1.Y - p0.Y;
            double x1diff = p1.X - p0.X;
            //find slope 
            double m1 = y1diff / x1diff;
            double b1 = p0.Y - (m1 * p0.X);
            //from (2) b = y-mx, and (5)
            //so ...  
            //------------------------------
            double y2diff = p3.Y - p2.Y;
            double x2diff = p3.X - p2.X;

            if (y1diff == 0)
            {
                //p0p1 -> same y, p0p1 is horizontal
                //we known y value
                float ky = p0.Y;
                //so find X
                //from y1=m1x1 +b1;
                //x1= (y1-b1)/m1 
                if (x2diff == 0)
                {   //p2p3 -> same x, p2p3 is vertical
                    return new Vector2((float)p3.X, ky);
                }
                else
                {
                    double m2 = y2diff / x2diff;
                    double b2 = p2.Y - (m2) * p2.X;
                    //from (6)             
                    //find cut point
                    //from y2=m2x2 +b2;
                    //
                    //
                    //x2= (y2-b2)/m2
                    //replace y2 with ky
                    //x2 = (ky - b2)/m2
                    double findingX = (ky - b2) / m2;
                    return new Vector2((float)findingX, ky);
                }

            }
            else if (x1diff == 0)
            {
                //p0p1 -> same x, p0p1 is vertical
                if (y2diff == 0)
                {
                    //p2p3 -> same y, p2p3 is horizontal
                    return new Vector2((float)p1.X, (float)p2.Y);
                }
                else
                {
                    //we know cutX
                    //so find cutY
                    double cutx_p2p3 = p1.X;

                    {
                        double m2 = y2diff / x2diff;
                        double b2 = p2.Y - (m2) * p2.X;
                        //from (6)             
                        //find cut point

                        //check if (m1-m2 !=0)
                        double cutx = cutx_p2p3; //from  (14) 
                        double cuty = (m2 * cutx) + b2;  //from (15)
                        return new Vector2((float)cutx, (float)cuty);
                    }
                }
            }
            else if (x2diff == 0)
            {
                //p2p3 -> same x, p2p3 is vertical
                //for p0p1
                //cutX = p2.x
                //cutY =  (m1 * cutx) + b1
                double cutx_p2p3 = p2.X;
                double cuty_p2p3 = (m1 * cutx_p2p3) + b1;  //from (15)
                return new Vector2((float)cutx_p2p3, (float)cuty_p2p3);
            }
            else if (y2diff == 0)
            {
                //p2p3 -> same y, p2p3 is horizontal
                //we know cutY
                double cuty_p2p3 = p2.Y;
                //from y1=m1x1 +b1;
                //x1= (y1-b1)/m1
                double cutx_p2p3 = (cuty_p2p3 - b1) / m1;
                return new Vector2((float)cutx_p2p3, (float)cuty_p2p3);
            }

            {
                double m2 = y2diff / x2diff;
                double b2 = p2.Y - (m2) * p2.X;
                //from (6)             
                //find cut point

                //check if (m1-m2 !=0)
                double cutx = (b2 - b1) / (m1 - m2); //from  (14) 
                double cuty = (m1 * cutx) + b1;  //from (15)
                return new Vector2((float)cutx, (float)cuty);
            }

        }
        static Vector2 FindCutPoint(Vector2 p0, Vector2 p1, Vector2 p2, float cutAngle)
        {
            //a line from p0 to p1
            //p2 is any point
            //return p3 -> cutpoint on p0,p1

            //from line equation
            //y = mx + b ... (1)
            //from (1)
            //b = y- mx ... (2) 
            //----------------------------------
            //line1:
            //y1 = (m1 * x1) + b1 ...(3)            
            //line2:
            //y2 = (m2 * x2) + b2 ...(4)
            //----------------------------------
            //from (3),
            //b1 = y1 - (m1 * x1) ...(5)
            //b2 = y2 - (m2 * x2) ...(6)
            //----------------------------------
            //y1diff = p1.Y-p0.Y  ...(7)
            //x1diff = p1.X-p0.X  ...(8)
            //
            //m1 = (y1diff/x1diff) ...(9)
            //m2 = cutAngle of m1 ...(10)
            //
            //replace value (x1,y1) and (x2,y2)
            //we know b1 and b2         
            //----------------------------------              
            //at cutpoint of line1 and line2 => (x1,y1)== (x2,y2)
            //or find (x,y) where (3)==(4)
            //---------------------------------- 
            //at cutpoint, find x
            // (m1 * x1) + b1 = (m2 * x1) + b2  ...(11), replace x2 with x1
            // (m1 * x1) - (m2 * x1) = b2 - b1  ...(12)
            //  x1 * (m1-m2) = b2 - b1          ...(13)
            //  x1 = (b2-b1)/(m1-m2)            ...(14), now we know x1
            //---------------------------------- 
            //at cutpoint, find y
            //  y1 = (m1 * x1) + b1 ... (15), replace x1 with value from (14)
            //Ans: (x1,y1)
            //---------------------------------- 

            double y1diff = p1.Y - p0.Y;
            double x1diff = p1.X - p0.X;

            if (x1diff == 0)
            {
                //90 or 180 degree
                return new Vector2(p1.X, p2.Y);
            }
            if (y1diff == 0)
            {
                return new Vector2(p2.X, p1.Y);
            }
            //------------------------------
            //
            //find slope 
            double m1 = y1diff / x1diff;
            //from (2) b = y-mx, and (5)
            //so ...
            double b1 = p0.Y - (m1 * p0.X);
            // 
            //from (10)
            //double invert_m = -(1 / slope_m);
            //double m2 = -1 / m1;   //rotate m1
            //---------------------
            double angle = System.Math.Atan2(y1diff, x1diff); //rad in degree 
                                                              //double m2 = -1 / m1;

            double m2 = cutAngle == 90 ?
                //short cut
                (-1 / m1) :
                //or 
                System.Math.Tan(
                //radial_angle of original line + radial of cutAngle
                //return new line slope
                System.Math.Atan2(y1diff, x1diff) +
                MyMath.DegreesToRadians(cutAngle)); //new m 
            //--------------------- 
            //from (6)
            double b2 = p2.Y - (m2) * p2.X;
            //find cut point

            //check if (m1-m2 !=0)
            double cutx = (b2 - b1) / (m1 - m2); //from  (14)
            double cuty = (m1 * cutx) + b1;  //from (15)
            return new Vector2((float)cutx, (float)cuty);

        }

        public static bool FindPerpendicularCutPoint2(Vector2 p0, Vector2 p1, Vector2 p2, out Vector2 cutPoint)
        {
            //a line from p0 to p1
            //p2 is any point
            //return p3 -> cutpoint on p0,p1 


            double xdiff = p1.X - p0.X;
            double ydiff = p1.Y - p0.Y;
            if (xdiff == 0)
            {
                //90 or 180 degree
                cutPoint = new Vector2(p1.X, p2.Y);
                Vector2 min, max;
                GetMinMax(p0, p1, out min, out max);
                return cutPoint.X >= min.X && cutPoint.X <= max.X && cutPoint.Y >= min.Y && cutPoint.Y <= max.Y;

            }
            if (ydiff == 0)
            {
                cutPoint = new Vector2(p2.X, p1.Y);
                Vector2 min, max;
                GetMinMax(p0, p1, out min, out max);
                return cutPoint.X >= min.X && cutPoint.X <= max.X && cutPoint.Y >= min.Y && cutPoint.Y <= max.Y;
            }

            double m1 = ydiff / xdiff;
            double b1 = FindB(p0, p1);

            double m2 = -1 / m1;
            double b2 = p2.Y - (m2) * p2.X;
            //find cut point
            double cutx = (b2 - b1) / (m1 - m2);
            double cuty = (m2 * cutx) + b2;
            cutPoint = new Vector2((float)cutx, (float)cuty);
            //
            {
                Vector2 min, max;
                GetMinMax(p0, p1, out min, out max);
                return cutPoint.X >= min.X && cutPoint.X <= max.X && cutPoint.Y >= min.Y && cutPoint.Y <= max.Y;
            }
        }
        static double FindB(Vector2 p0, Vector2 p1)
        {

            double m1 = (p1.Y - p0.Y) / (p1.X - p0.X);
            //y = mx + b ...(1)
            //b = y- mx

            //substitue with known value to gett b 
            //double b0 = p0.Y - (slope_m) * p0.X;
            //double b1 = p1.Y - (slope_m) * p1.X;
            //return b0;

            return p0.Y - (m1) * p0.X;
        }


        internal static readonly double _85degreeToRad = MyMath.DegreesToRadians(85);
        internal static readonly double _03degreeToRad = MyMath.DegreesToRadians(3);
        /// <summary>
        /// compare d0, d1, d2 return min value by index 0 or 1 or 2
        /// </summary>
        /// <param name="d0"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        static int FindMinByIndex(double d0, double d1, double d2)
        {
            unsafe
            {
                double* tmpArr = stackalloc double[3];
                tmpArr[0] = d0;
                tmpArr[1] = d1;
                tmpArr[2] = d2;

                int minAt = -1;
                double currentMin = double.MaxValue;
                for (int i = 0; i < 3; ++i)
                {
                    double d = tmpArr[i];
                    if (d < currentMin)
                    {
                        currentMin = d;
                        minAt = i;
                    }
                }
                return minAt;
            }
        }

    }
}