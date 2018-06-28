//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           Silverlight procedural Plant
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-02-24 20:36:41 +0100 (Di, 24 Feb 2015) $
//   Changed in:        $Revision: 112951 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapExCurveSample/Plant/Branch.cs $
//   Id:                $Id: Branch.cs 112951 2015-02-24 19:36:41Z unknown $
//
//
//   Copyright © 2010-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//


using System;
using System.Collections.Generic;
using PixelFarm.BitmapBufferEx;
namespace WinFormGdiPlus.PlantDemo
{
  
    /// <summary>
    /// Integer vector.
    /// </summary>
    public struct Vector
    {
        public int X;
        public int Y;


        public static Vector Zero { get { return new Vector(0, 0); } }
        public static Vector One { get { return new Vector(1, 1); } }

        public int Length { get { return (int)System.Math.Sqrt(X * X + Y * Y); } }


        public Vector(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector(PointD point)
           : this((int)point.X, (int)point.Y)
        {
        }


        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator *(Vector p, int s)
        {
            return new Vector(p.X * s, p.Y * s);
        }

        public static Vector operator *(int s, Vector p)
        {
            return new Vector(p.X * s, p.Y * s);
        }

        public static Vector operator *(Vector p, float s)
        {
            return new Vector((int)(p.X * s), (int)(p.Y * s));
        }

        public static Vector operator *(float s, Vector p)
        {
            return new Vector((int)(p.X * s), (int)(p.Y * s));
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }


        public Vector Interpolate(Vector v2, float amount)
        {
            return new Vector((int)(this.X + ((v2.X - this.X) * amount)), (int)(this.Y + ((v2.Y - this.Y) * amount)));
        }

        public int Dot(Vector v2)
        {
            return this.X * v2.X + this.Y * v2.Y;
        }

        public int Angle(Vector v2)
        {
            // Normalize this
            double s1 = 1.0f / this.Length;
            double x1 = this.X * s1;
            double y1 = this.Y * s1;

            // Normalize v2
            double s2 = 1.0f / v2.Length;
            double x2 = v2.X * s2;
            double y2 = v2.Y * s2;

            // The dot product is the cosine between the two vectors
            double dot = x1 * x2 + y1 * y2;
            double rad = Math.Acos(dot);

            // return the angle in degrees
            return (int)(rad * 57.295779513082320876798154814105);
        }


        public override bool Equals(object obj)
        {
            if (obj is Vector)
            {
                return ((Vector)obj) == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y); ;
        }
    }
    /// <summary>
    /// A branching point of a plant.
    /// </summary>
    public struct BranchPoint
    {
        public float Time;
        public int Angle;

        public BranchPoint(float time, int angle)
        {
            this.Time = time;
            this.Angle = angle;
        }
    }
    /// <summary>
    /// A branch of a plant.
    /// </summary>
    public class Branch
    {
        public const float MaxLife = 1;

        public List<Branch> Branches { get; private set; }
        public Vector Start { get; set; }
        public Vector Middle { get; set; }
        public Vector MiddleTarget { get; set; }
        public Vector End { get; set; }
        public Vector EndTarget { get; set; }
        public float Life { get; set; }
        public float GrowthRate { get; private set; }

        public Branch()
        {
            this.Branches = new List<Branch>();
            this.Life = 0;
        }

        public Branch(Vector start, Vector middleTarget, Vector endTarget, float growthRate)
           : this()
        {
            this.Start = start;
            this.MiddleTarget = middleTarget;
            this.Middle = start;
            this.EndTarget = endTarget;
            this.End = start;
            this.GrowthRate = growthRate;
        }

        public void Grow()
        {
            // Slightly overlap
            const float endStart = 0.3f;
            const float endEnd = 1;
            if (Life >= endStart && Life <= endEnd)
            {
                End = Start.Interpolate(EndTarget, (Life - endStart) * (1 / (endEnd - endStart)));
            }
            else if (Life <= 0.5)
            {
                Middle = Start.Interpolate(MiddleTarget, Life * 2);
            }

            // Everyone gets older ya know
            Life += this.GrowthRate;
        }

        public void Clear()
        {
            this.Branches.Clear();
            this.Middle = Start;
            this.End = Start;
            this.Life = 0;
        }
    }

    /// <summary>
    /// A simple plant.
    /// </summary>
    public class Plant
    {
        private Random rand;
        private Dictionary<int, int> branchesPerGen;

        public Branch Root { get; private set; }
        public float Tension { get; set; }
        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }
        public int BranchLenMin { get; set; }
        public int BranchLenMax { get; set; }
        public int BranchAngleVariance { get; set; }
        public float GrowthRate { get; set; }
        public int MaxGenerations { get; set; }
        public ColorInt Color { get; set; }
        public Vector Start { get; private set; }
        public Vector Scale { get; private set; }
        public List<BranchPoint> BranchPoints { get; private set; }
        //public int BranchDegression       { get; set; }
        public float BendingFactor { get; set; }
        public int MaxBranchesPerGeneration { get; set; }

        public Plant()
        {
            this.Tension = 1f;
            this.rand = new Random();
            this.BranchLenMin = 150;
            this.BranchLenMax = 200;
            this.GrowthRate = 0.007f;
            this.BranchPoints = new List<BranchPoint>();
            this.BranchAngleVariance = 10;
            this.MaxGenerations = int.MaxValue;
            //this.BranchDegression    = 0;
            this.Color = ColorInt.FromArgb(255, 100, 150, 0);
            this.Start = Vector.Zero;
            this.Scale = Vector.One;
            this.BendingFactor = 0.4f;
            this.MaxBranchesPerGeneration = int.MaxValue;
            this.branchesPerGen = new Dictionary<int, int>();
        }

        public Plant(Vector start, Vector scale, int viewPortWidth, int viewPortHeight)
           : this()
        {
            this.Tension = 0.5f;
            this.Initialize(start, scale, viewPortWidth, viewPortHeight);
        }

        public void Initialize(Vector start, Vector scale, int viewPortWidth, int viewPortHeight)
        {
            this.Start = start;
            this.Scale = scale;
            this.MaxWidth = viewPortWidth;
            this.MaxHeight = viewPortHeight;
            var end = new Vector(Start.X, Start.Y + ((MaxHeight >> 4) * Scale.Y));
            this.Root = new Branch(Start, Start, end, 0.02f);
        }

        public void Clear()
        {
            branchesPerGen.Clear();
            this.Root.Clear();
        }

        public void Grow()
        {
            Grow(this.Root, 0);
        }

        private void Grow(Branch branch, int generation)
        {
            if (generation <= MaxGenerations)
            {
                if (branch.End.Y >= 0 && branch.End.Y <= MaxHeight
                 && branch.End.X >= 0 && branch.End.X <= MaxWidth)
                {
                    // Grow it
                    branch.Grow();

                    // Branch?
                    foreach (var bp in BranchPoints)
                    {
                        if (!branchesPerGen.ContainsKey(generation))
                        {
                            branchesPerGen.Add(generation, 0);
                        }
                        if (branchesPerGen[generation] < MaxBranchesPerGeneration)
                        {
                            if (branch.Life >= bp.Time && branch.Life <= bp.Time + branch.GrowthRate)
                            {
                                // Length and angle of the branch
                                var branchLen = rand.Next(BranchLenMin, BranchLenMax);
                                branchLen -= (int)(branchLen * 0.01f * generation);
                                // In radians
                                var angle = rand.Next(bp.Angle - BranchAngleVariance, bp.Angle + BranchAngleVariance) * 0.017453292519943295769236907684886;

                                // Desired end of new branch
                                var endTarget = new Vector(branch.End.X + ((int)(Math.Sin(angle) * branchLen) * Scale.X),
                                                           branch.End.Y + ((int)(Math.Cos(angle) * branchLen) * Scale.Y));

                                // Desired middle point
                                angle -= Math.Sign(bp.Angle) * BendingFactor;
                                var middleTarget = new Vector(endTarget.X - ((int)(Math.Sin(angle) * (branchLen >> 1)) * Scale.X),
                                                              endTarget.Y - ((int)(Math.Cos(angle) * (branchLen >> 1)) * Scale.Y));

                                // Add new branch
                                branch.Branches.Add(new Branch(branch.End, middleTarget, endTarget, GetRandomGrowthRate()));
                                branchesPerGen[generation]++;
                            }
                        }
                    }
                }

                // Grow the child branches
                foreach (var b in branch.Branches)
                {
                    Grow(b, generation + 1);
                }
            }
        }

        private float GetRandomGrowthRate()
        {
            var r = (float)rand.NextDouble() * GrowthRate - GrowthRate * 0.5f;
            return GrowthRate + r;
        }

        public void Draw(BitmapBuffer writeableBmp)
        {

            // Wrap updates in a GetContext call, to prevent invalidation and nested locking/unlocking during this block
            using (writeableBmp.GetBitmapContext())
            {
                writeableBmp.Clear();
                Draw(writeableBmp, this.Root);
#if SILVERLIGHT
               writeableBmp.Invalidate();
#endif
            }

        }

        private void Draw(BitmapBuffer writeableBmp, Branch branch)
        {
            int[] pts = new int[]
            {
            branch.Start.X,   branch.Start.Y,
            branch.Middle.X,  branch.Middle.Y,
            branch.End.X,     branch.End.Y,
            };

            // Draw with cardinal spline
            writeableBmp.DrawCurve(pts, Tension, this.Color);

            foreach (var b in branch.Branches)
            {
                Draw(writeableBmp, b);
            }
        }
    }


    //--------------------
    /// <summary>
    /// A control point for a spline curve.
    /// </summary>
    public class ControlPoint
    {
        private Vector point;

        public int X { get { return point.X; } set { point.X = value; } }
        public int Y { get { return point.Y; } set { point.Y = value; } }


        public ControlPoint(Vector point)
        {
            this.point = point;
        }

        public ControlPoint()
           : this(Vector.Zero)
        {
        }

        public ControlPoint(int x, int y)
           : this(new Vector(x, y))
        {
        }

        public ControlPoint(PointD point)
           : this((int)point.X, (int)point.Y)
        {
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y); ;
        }
    }
}