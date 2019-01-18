//MIT, 2018-present, WinterDev
//from http://www.antigrain.com/research/bezier_interpolation/index.html#PAGE_BEZIER_INTERPOLATION

using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit;
using PixelFarm.Drawing;
using PixelFarm.VectorMath;

namespace PixelFarm.PathReconstruction
{
    public class BezierControllerArmPair
    {
        public Vector2 left;
        public Vector2 mid;
        public Vector2 right;

        Vector2 _left_bk; //backup
        Vector2 _mid_bk; //backup
        Vector2 _right_bk; //backup


        float _smooth_coeff;//0-1 coefficiency value
        public BezierControllerArmPair(Vector2 left, Vector2 mid, Vector2 right)
        {
            _smooth_coeff = 0;
            this.left = _left_bk = left;
            this.mid = _mid_bk = mid;
            this.right = _right_bk = right;
        }

        public float UniformSmoothCoefficient
        {
            //we can change this value later ..

            get => _smooth_coeff;
            set
            {
                _smooth_coeff = value;
                if (_smooth_coeff == 0)
                {
                    left = _left_bk;
                    mid = _mid_bk;
                    right = _right_bk;
                }
                else
                {
                    Vector2 newToRight = (_right_bk - _mid_bk) * _smooth_coeff;
                    Vector2 newToLeft = (_left_bk - _mid_bk) * _smooth_coeff;

                    left = _mid_bk + newToLeft;
                    right = _mid_bk + newToRight;
                }
            }
        }
        public void Offset(double dx, double dy)
        {
            Vector2 diff = new Vector2(dx, dy);
            left += diff;
            mid += diff;
            right += diff;

            _left_bk += diff;
            _mid_bk += diff;
            _right_bk += diff;
        }

        static double Len(Vector2 v0, Vector2 v1)
        {
            return System.Math.Sqrt(
                  ((v1.Y - v0.Y) * (v1.Y - v0.Y)) +
                  ((v1.X - v0.X) * (v1.X - v0.x)));
        }

        public static BezierControllerArmPair ReconstructControllerArms(Vector2 left, Vector2 middle, Vector2 right)
        {
            Vector2 a_left = (left + middle) / 2;
            Vector2 a_right = (right + middle) / 2;


            double len_1 = Len(left, middle);
            double len_2 = Len(right, middle);
            //
            //double a_left_right_len = Len(a_left, a_right);

            if ((len_1 + len_2) == 0)
            {
                return null;
            }

            double d1_ratio = (len_1 / (len_1 + len_2));

            Vector2 b = new Vector2(
                a_left.x + (d1_ratio * (a_right.x - a_left.x)),
                a_left.y + (d1_ratio * (a_right.y - a_left.y)));

            var controllerPair = new BezierControllerArmPair(a_left, b, a_right);

            Vector2 diff = b - middle;
            controllerPair.Offset(-diff.x, -diff.y);

            return controllerPair;
        }


    }

    public class ReconstructedFigure
    {
        public List<BezierControllerArmPair> _arms = new List<BezierControllerArmPair>();
        public int Count => _arms.Count;
    }

    public class BezireControllerArmBuilder
    {
        //TODO: optmize this later... 
        LimitedQueue<Vector2> _reusableQueue = new LimitedQueue<Vector2>(3);

        class LimitedQueue<T>
        {
            T[] _que;

            int _readIndex;
            int _writeIndex;
            int _count;

            public LimitedQueue(int limitCapacity)
            {
                _que = new T[limitCapacity];
            }
            public int Count => _count;
            public void Enqueue(T input)
            {
                if (_count == _que.Length)
                {
                    throw new Exception("queue is fulled");
                }

                //
                _que[_writeIndex] = input;
                _writeIndex++;
                _count++;

                if (_writeIndex == _que.Length)
                {
                    //we are at the end of the arr
                    //turn back to 0
                    _writeIndex = 0;
                }
            }
            public T Dequeue()
            {
                if (_count == 0)
                {
                    //no obj in queue
                    throw new Exception("no obj in queue");
                }

                //-----------
                T obj = _que[_readIndex];
                _count--;
                _readIndex++;

                if (_readIndex == _que.Length)
                {
                    //we are on the end of the array
                    //turn to 0
                    _readIndex = 0;
                }
                return obj;
            }
            public T NextQueue(int offset)
            {
                if (offset > _count - 1)
                {
                    throw new Exception("no obj in queue");
                }
                if (_readIndex + offset >= _que.Length)
                {
                    return _que[(_readIndex + offset) - _que.Length];
                }
                else
                {
                    return _que[_readIndex + offset];
                }
            }
            public void Clear()
            {
                _writeIndex = _readIndex = _count = 0;
                //clear arr? 
            }
        }


        public float SmoothCoefficiency { get; set; }
        /// <summary>
        /// create an arm-pair from collected data in reuseable queue
        /// </summary>
        /// <returns></returns>
        BezierControllerArmPair CreateArmPair()
        {
            return BezierControllerArmPair.ReconstructControllerArms(
                   _reusableQueue.Dequeue(),
                   _reusableQueue.NextQueue(0),
                   _reusableQueue.NextQueue(1));
        }
        public void ReconstructionControllerArms(VertexStore inputVxs, List<ReconstructedFigure> figures)
        {
            _reusableQueue.Clear();
            int count = inputVxs.Count;
            if (count < 2)
            {
                return;
            }
            if (count == 2)
            {
                //simulate right node?

            }


            ReconstructedFigure currentFig = new ReconstructedFigure();
            // 
            double lastest_moveToX = 0, latest_moveToY = 0;
            for (int i = 0; i < count; ++i)
            {

                if (_reusableQueue.Count == 3)
                {
                    BezierControllerArmPair arm = CreateArmPair();
                    if (arm != null)
                    {
                        arm.UniformSmoothCoefficient = this.SmoothCoefficiency;
                        currentFig._arms.Add(arm);
                    }
                }

                switch (inputVxs.GetVertex(i, out double x, out double y))
                {
                    case VertexCmd.MoveTo:
                        _reusableQueue.Enqueue(new Vector2(lastest_moveToX = x, latest_moveToY = y));
                        break;
                    case VertexCmd.P2c:
                    case VertexCmd.P3c:
                    case VertexCmd.LineTo:
                        _reusableQueue.Enqueue(new Vector2(x, y));
                        break;
                    case VertexCmd.Close:
                        _reusableQueue.Enqueue(new Vector2(lastest_moveToX, latest_moveToY));

                        if (_reusableQueue.Count == 3)
                        {
                            BezierControllerArmPair arm = CreateArmPair();
                            if (arm != null)
                            {
                                arm.UniformSmoothCoefficient = this.SmoothCoefficiency;
                                currentFig._arms.Add(arm);
                            }
                        }
                        if (_reusableQueue.Count == 2)
                        {
                            //close the 1st point
                            inputVxs.GetVertex(1, out x, out y);
                            _reusableQueue.Enqueue(new Vector2(x, y));

                            BezierControllerArmPair arm = CreateArmPair();
                            if (arm != null)
                            {
                                arm.UniformSmoothCoefficient = this.SmoothCoefficiency;
                                currentFig._arms.Add(arm);
                            }
                        }
                        // 
                        if (currentFig.Count > 0)
                        {
                            figures.Add(currentFig);
                        }

                        currentFig = new ReconstructedFigure();
                        _reusableQueue.Clear();
                        break;
                    case VertexCmd.NoMore:
                        goto EXIT;
                }
            }

            //
            EXIT:
            switch (_reusableQueue.Count)
            {
                default:
                    throw new NotSupportedException();//should not occur
                case 0:
                    //ok
                    break;
                case 3:
                    {
                        BezierControllerArmPair arm = CreateArmPair();
                        if (arm != null)
                        {
                            arm.UniformSmoothCoefficient = this.SmoothCoefficiency;
                            currentFig._arms.Add(arm);
                        }
                        if (currentFig.Count > 0)
                        {
                            figures.Add(currentFig);
                        }
                    }
                    break;
            }
        }
    }

}