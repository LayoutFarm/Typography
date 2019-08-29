//MIT, from https://github.com/sinbad/UnitySpline2D

using System;
using System.Collections.Generic;


// Utility class for calculating a Cubic multi-segment (Hermite) spline in 2D
// Hermite splines are convenient because they only need 2 positions and 2
// tangents per segment, which can be automatically calculated from the surrounding
// points if desired
// The spline can be extended dynamically over time and must always consist of
// 3 or more points. If the spline is closed, the spline will loop back to the
// first point.
// It can provide positions, derivatives (slope of curve) either at a parametric
// 't' value over the whole curve, or as a function of distance along the curve
// for constant-speed traversal. The distance is calculated approximately via
// sampling (cheap integration), its accuracy is determined by LengthSamplesPerSegment
// which defaults to 5 (a decent trade-off for most cases).
// This object is not a MonoBehaviour to keep it flexible. If you want to
// save/display one in a scene, use the wrapper Spline2DComponent class.

using PixelFarm.VectorMath;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public class Spline2D
    {
        bool _tangentsDirty = true;
        bool _lenSampleDirty = true;
        // Points which the curve passes through.
        List<Vector2> _points = new List<Vector2>();
        // Tangents at each point; automatically calculated
        List<Vector2> _tangents = new List<Vector2>();
        bool _closed;
        /// Whether the spline is closed; if so, the first point is also the last
        public bool IsClosed
        {
            get => _closed;
            set
            {
                _closed = value;
                _tangentsDirty = true;
                _lenSampleDirty = true;
            }
        }
        float _curvature = 0.5f;
        /// The amount of curvature in the spline; 0.5 is Catmull-Rom
        public float Curvature
        {
            get => _curvature;
            set
            {
                _curvature = value;
                _tangentsDirty = true;
                _lenSampleDirty = true;
            }
        }
        int _lengthSamplesPerSegment = 5;
        /// Accuracy of sampling curve to traverse by distance
        public int LengthSamplesPerSegment
        {
            get => _lengthSamplesPerSegment;
            set
            {
                _lengthSamplesPerSegment = value;
                _lenSampleDirty = true;
            }
        }

        struct DistanceToT
        {
            public readonly float distance;
            public readonly float t;
            public DistanceToT(float dist, float tm)
            {
                distance = dist;
                t = tm;
            }
        }

        List<DistanceToT> _distanceToTList = new List<DistanceToT>();

        /// Get point count
        public int Count => _points.Count;

        /// Return the approximate length of the curve, as derived by sampling the
        /// curve at a resolution of LengthSamplesPerSegment
        public float Length
        {
            get
            {
                Recalculate(true);

                if (_distanceToTList.Count == 0)
                    return 0.0f;

                return _distanceToTList[_distanceToTList.Count - 1].distance;
            }
        }



        public Spline2D()
        {
        }

        public Spline2D(List<Vector2> intersectionPoints, bool isClosed = false,
            float curve = 0.5f, int samplesPerSegment = 5)
        {
            _points = intersectionPoints;
            _closed = isClosed;
            _curvature = curve;
            _lengthSamplesPerSegment = samplesPerSegment;
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Add a point to the curve
        public void AddPoint(Vector2 p)
        {
            _points.Add(p);
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Add a point to the curve by dropping the earliest point and scrolling
        /// all other points backwards
        /// This allows you to maintain a fixed-size spline which you extend to new
        /// points at the expense of dropping earliest points. This is efficient for
        /// unbounded paths you need to keep adding to but don't need the old history
        /// Note that when you do this the distances change to being measured from
        /// the new start point so you have to adjust your next interpolation request
        /// to take this into account. Subtract DistanceAtPoint(1) from distances
        /// before calling this method, for example (or for plain `t` interpolation,
        /// reduce `t` by 1f/Count)
        /// This method cannot be used on closed splines
        public void AddPointScroll(Vector2 p)
        {
            //Assert.IsFalse(closed, "Cannot use AddPointScroll on closed splines!");

            if (_points.Count == 0)
            {
                AddPoint(p);
            }
            else
            {
                for (int i = 0; i < _points.Count - 1; ++i)
                {
                    _points[i] = _points[i + 1];
                }
                _points[_points.Count - 1] = p;
            }
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Add a list of points to the end of the spline, in order
        public void AddPoints(IEnumerable<Vector2> plist)
        {
            _points.AddRange(plist);
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Replace all the points in the spline from fromIndex onwards with a new set
        public void ReplacePoints(IEnumerable<Vector2> plist, int fromIndex = 0)
        {
            //Assert.IsTrue(fromIndex < points.Count, "Spline2D: point index out of range");

            _points.RemoveRange(fromIndex, _points.Count - fromIndex);
            _points.AddRange(plist);
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Change a point on the curve
        public void SetPoint(int index, Vector2 p)
        {
            //Assert.IsTrue(index < points.Count, "Spline2D: point index out of range");

            _points[index] = p;
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }
        /// Remove a point on the curve
        public void RemovePoint(int index)
        {
            // Assert.IsTrue(index < points.Count, "Spline2D: point index out of range");

            _points.RemoveAt(index);
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        /// Insert a point on the curve before the given index
        public void InsertPoint(int index, Vector2 p)
        {
            //Assert.IsTrue(index <= points.Count && index >= 0, "Spline2D: point index out of range");
            _points.Insert(index, p);
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }

        // TODO add more efficient 'scrolling' curve of N length where we add one &
        // drop the earliest for effcient non-closed curves that continuously extend

        /// Reset &amp; start again
        public void Clear()
        {
            _points.Clear();
            _tangentsDirty = true;
            _lenSampleDirty = true;
        }
        /// Get a single point
        public Vector2 GetPoint(int index)
        {
            //Assert.IsTrue(index < points.Count, "Spline2D: point index out of range");

            return _points[index];
        }

        /// Interpolate a position on the entire curve. Note that if the control
        /// points are not evenly spaced, this may result in varying speeds.
        public Vector2 Interpolate(float t)
        {
            Recalculate(false);
            ToSegment(t, out int segIdx, out float tSeg);

            return Interpolate(segIdx, tSeg);
        }

        private void ToSegment(float t, out int iSeg, out float tSeg)
        {
            // Work out which segment this is in
            // Closed loops have 1 extra node at t=1.0 ie the first node
            float pointCount = _closed ? _points.Count : _points.Count - 1;
            float fSeg = t * pointCount;
            iSeg = (int)fSeg;
            // Remainder t
            tSeg = fSeg - iSeg;
        }

        static bool Approximately(float a, float b) => Math.Abs(a - b) < float.Epsilon;


        /// Interpolate a position between one point on the curve and the next
        /// Rather than interpolating over the entire curve, this simply interpolates
        /// between the point with fromIndex and the next point
        public Vector2 Interpolate(int fromIndex, float t)
        {
            Recalculate(false);

            int toIndex = fromIndex + 1;
            // At or beyond last index?
            if (toIndex >= _points.Count)
            {
                if (_closed)
                {
                    // Wrap
                    toIndex = toIndex % _points.Count;
                    fromIndex = fromIndex % _points.Count;
                }
                else
                {
                    // Clamp to end
                    return _points[_points.Count - 1];
                }
            }

            // Fast special cases
            if (Approximately(t, 0.0f))
            {
                return _points[fromIndex];
            }
            else if (Approximately(t, 1.0f))
            {
                return _points[toIndex];
            }

            // Now general case
            // Pre-calculate powers
            float t2 = t * t;
            float t3 = t2 * t;
            // Calculate hermite basis parts
            float h1 = 2f * t3 - 3f * t2 + 1f;
            float h2 = -2f * t3 + 3f * t2;
            float h3 = t3 - 2f * t2 + t;
            float h4 = t3 - t2;

            return h1 * _points[fromIndex] +
                   h2 * _points[toIndex] +
                   h3 * _tangents[fromIndex] +
                   h4 * _tangents[toIndex];


        }

        /// Get derivative of the curve at a point. Note that if the control
        /// points are not evenly spaced, this may result in varying speeds.
        /// This is not normalised by default in case you don't need that
        public Vector2 Derivative(float t)
        {
            Recalculate(false);

            ToSegment(t, out int segIdx, out float tSeg);

            return Derivative(segIdx, tSeg);
        }

        /// Get derivative of curve between one point on the curve and the next
        /// Rather than interpolating over the entire curve, this simply interpolates
        /// between the point with fromIndex and the next segment
        /// This is not normalised by default in case you don't need that
        public Vector2 Derivative(int fromIndex, float t)
        {
            Recalculate(false);

            int toIndex = fromIndex + 1;
            // At or beyond last index?
            if (toIndex >= _points.Count)
            {
                if (_closed)
                {
                    // Wrap
                    toIndex = toIndex % _points.Count;
                    fromIndex = fromIndex % _points.Count;
                }
                else
                {
                    // Clamp to end
                    toIndex = fromIndex;
                }
            }

            // Pre-calculate power
            float t2 = t * t;
            // Derivative of hermite basis parts
            float h1 = 6f * t2 - 6f * t;
            float h2 = -6f * t2 + 6f * t;
            float h3 = 3f * t2 - 4f * t + 1;
            float h4 = 3f * t2 - 2f * t;

            return h1 * _points[fromIndex] +
                   h2 * _points[toIndex] +
                   h3 * _tangents[fromIndex] +
                   h4 * _tangents[toIndex];


        }




        //--------------
        static float Lerp(float from, float to, float frac)
        {
            return (from + frac * (to - from));
        }

        static double Lerp(double from, double to, double frac)
        {
            return (from + frac * (to - from));
        }

        static PointF Lerp(PointF from, PointF to, float frac)
        {
            return new PointF(Lerp(from.X, to.X, frac), Lerp(from.Y, to.Y, frac));
        }
        static float InverseLerp(float min, float max, float value)
        {
            if (Math.Abs(max - min) < float.Epsilon) return min;
            return (value - min) / (max - min);
        }

        //--------------


        /// Convert a physical distance to a t position on the curve. This is
        /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
        public float DistanceToLinearT(float dist) => DistanceToLinearT(dist, out int i);


        /// Convert a physical distance to a t position on the curve. This is
        /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
        /// Also returns an out param of the last point index passed
        public float DistanceToLinearT(float dist, out int lastIndex)
        {
            Recalculate(true);

            if (_distanceToTList.Count == 0)
            {
                lastIndex = 0;
                return 0.0f;
            }

            // Check to see if distance > length
            float len = Length;
            if (dist >= len)
            {
                if (_closed)
                {
                    // wrap and continue as usual
                    dist = dist % len;
                }
                else
                {
                    // clamp to end
                    lastIndex = _points.Count - 1;
                    return 1.0f;
                }
            }


            float prevDist = 0.0f;
            float prevT = 0.0f;
            for (int i = 0; i < _distanceToTList.Count; ++i)
            {
                DistanceToT distToT = _distanceToTList[i];
                if (dist < distToT.distance)
                {
                    float distanceT = InverseLerp(prevDist, distToT.distance, dist);
                    lastIndex = i / _lengthSamplesPerSegment; // not i-1 because distanceToTList starts at point index 1
                    return Lerp(prevT, distToT.t, distanceT);
                }
                prevDist = distToT.distance;
                prevT = distToT.t;
            }

            // If we got here then we ran off the end
            lastIndex = _points.Count - 1;
            return 1.0f;
        }

        /// Interpolate a position on the entire curve based on distance. This is
        /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
        public Vector2 InterpolateDistance(float dist)
        {
            float t = DistanceToLinearT(dist);
            return Interpolate(t);
        }

        /// Get derivative of the curve at a point long the curve at a distance. This
        /// is approximate, the accuracy of this can be changed via
        /// LengthSamplesPerSegment
        public Vector2 DerivativeDistance(float dist)
        {
            float t = DistanceToLinearT(dist);
            return Derivative(t);
        }

        /// Get the distance at a point index
        public float DistanceAtPoint(int index)
        {
            //Assert.IsTrue(index < points.Count, "Spline2D: point index out of range");

            // Length samples are from first actual distance, with points at
            // LengthSamplesPerSegment intervals
            if (index == 0)
            {
                return 0.0f;
            }
            Recalculate(true);
            return _distanceToTList[index * _lengthSamplesPerSegment - 1].distance;
        }

        private void Recalculate(bool includingLength)
        {
            if (_tangentsDirty)
            {
                RecalcTangents();
                _tangentsDirty = false;
            }
            // Need to check the length of distanceToTList because for some reason
            // when scripts are reloaded in the editor, tangents survives but
            // distanceToTList does not (and dirty flags remain false). Maybe because
            // it's a custom struct it can't be restored
            if (includingLength &&
                (_lenSampleDirty || _distanceToTList.Count == 0))
            {
                RecalcLength();
                _lenSampleDirty = false;
            }
        }

        void RecalcTangents()
        {
            int numPoints = _points.Count;
            if (numPoints < 2)
            {
                // Nothing to do here
                return;
            }
            _tangents.Clear();
            _tangents.Capacity = numPoints;

            for (int i = 0; i < numPoints; ++i)
            {
                Vector2 tangent;
                if (i == 0)
                {
                    // Special case start
                    if (_closed)
                    {
                        // Wrap around
                        tangent = MakeTangent(_points[numPoints - 1], _points[1]);
                    }
                    else
                    {
                        // starting tangent is just from start to point 1
                        tangent = MakeTangent(_points[i], _points[i + 1]);
                    }
                }
                else if (i == numPoints - 1)
                {
                    // Special case end
                    if (_closed)
                    {
                        // Wrap around
                        tangent = MakeTangent(_points[i - 1], _points[0]);
                    }
                    else
                    {
                        // end tangent just from prev point to end point
                        tangent = MakeTangent(_points[i - 1], _points[i]);
                    }
                }
                else
                {
                    // Mid point is average of previous point and next point
                    tangent = MakeTangent(_points[i - 1], _points[i + 1]);
                }
                _tangents.Add(tangent);
            }
        }

        Vector2 MakeTangent(Vector2 p1, Vector2 p2) => _curvature * (p2 - p1);


        void RecalcLength()
        {
            int numPoints = _points.Count;
            if (numPoints < 2)
            {
                // Nothing to do here
                return;
            }
            // Sample along curve & build distance -> t lookup, can interpolate t
            // linearly between nearest points to approximate distance parametrisation
            // count is segments * lengthSamplesPerSegment
            // We sample from for st t > 0 all the way to t = 1
            // For a closed loop, t = 1 is the first point again, for open its the last point

            int pp_count = _closed ? _points.Count : _points.Count - 1;
            int samples = _lengthSamplesPerSegment * pp_count;

            _distanceToTList.Clear();
            _distanceToTList.Capacity = samples;
            float distanceSoFar = 0.0f;
            float tinc = 1.0f / (float)samples;
            float t = tinc; // we don't start at 0 since that's easy
            Vector2 lastPos = _points[0];
            for (int i = 1; i <= samples; ++i)
            {
                Vector2 pos = Interpolate(t);
                float distInc = (float)(new Vector2(lastPos.x - pos.x, lastPos.y - pos.y)).Length;// Vector2.Distance(lastPos, pos);
                distanceSoFar += distInc;
                _distanceToTList.Add(new DistanceToT(distanceSoFar, t));
                lastPos = pos;
                t += tinc;
            }
        }

    }


}