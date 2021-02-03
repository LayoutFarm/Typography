//MIT, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//---------------------------------------------------------------------------- 


namespace PixelFarm.CpuBlit
{
    public readonly struct ArrayListSegment<T>
    {
        internal readonly ArrayList<T> _arrList;
        public readonly int beginAt;
        public readonly int len;
        public ArrayListSegment(ArrayList<T> arrList, int beginAt, int len)
        {
            this._arrList = arrList;
            this.beginAt = beginAt;
            this.len = len;
        }

        public int Count => len;

        public static void UnsafeGetInternalArr(in ArrayListSegment<T> listSpan, out T[] internalArr)
        {
            internalArr = listSpan._arrList.UnsafeInternalArray;
        }

        public static readonly ArrayListSegment<T> Empty = new ArrayListSegment<T>();
#if DEBUG
        public override string ToString()
        {
            return beginAt + "," + len;
        }
#endif
    }

    public sealed class ArrayList<T>
    {
        static readonly T[] s_empty = new T[0];
        
        int _currentSize;
        T[] _internalArray = s_empty;

        public ArrayList()
        {
        }
        public ArrayList(int cap)
        {
            Allocate(cap, 0);
        }
        public ArrayList(ArrayList<T> srcCopy, int plusSize)
        {
            Allocate(srcCopy.AllocatedSize, srcCopy.AllocatedSize + plusSize);
            if (srcCopy._currentSize != 0)
            {
                srcCopy._internalArray.CopyTo(_internalArray, 0);
            }
        }
        public void RemoveLast()
        {
            if (_currentSize != 0)
            {
                _currentSize--;
            }
        }
        //
        public int Count => _currentSize;
        //
        public int AllocatedSize => _internalArray.Length;
        //
        public void Clear()
        {
            _currentSize = 0;
        }


        // Set new capacity. All data is lost, size is set to zero.
        public void Clear(int newCapacity)
        {
            Clear(newCapacity, 0);
        }
        public void Clear(int newCapacity, int extraTail)
        {
            _currentSize = 0;
            if (newCapacity > AllocatedSize)
            {
                _internalArray = null;
                int sizeToAllocate = newCapacity + extraTail;
                if (sizeToAllocate != 0)
                {
                    _internalArray = new T[sizeToAllocate];
                }
            }
        }
        // Allocate n elements. All data is lost, 
        // but elements can be accessed in range 0...size-1. 
        public void Allocate(int size)
        {
            Allocate(size, 0);
        }

        void Allocate(int size, int extraTail)
        {
            Clear(size, extraTail);
            _currentSize = size;
        }

        /// <summary>
        ///  Resize keeping the content
        /// </summary>
        /// <param name="newSize"></param>
        public void AdjustSize(int newSize)
        {
            if (newSize > _currentSize && newSize > AllocatedSize)
            {
                //create new array and copy data to that 
                var newArray = new T[newSize];
                if (_internalArray != null)
                {
                    System.Array.Copy(_internalArray, newArray, _internalArray.Length);
                    //for (int i = _internalArray.Length - 1; i >= 0; --i)
                    //{
                    //    newArray[i] = _internalArray[i];
                    //}
                }
                _internalArray = newArray;
            }
        }


        public void Zero()
        {
            System.Array.Clear(_internalArray, 0, _internalArray.Length);
        }
        public T[] ToArray()
        {
            T[] output = new T[_currentSize];
            System.Array.Copy(_internalArray, output, _currentSize);
            return output;
        }

        void EnsureSpaceForAppend(int newAppendLen)
        {
            int newSize = _currentSize + newAppendLen;
            if (_internalArray.Length < newSize)
            {
                //copy
                if (newSize < 100000)
                {
                    AdjustSize(newSize + (newSize / 2) + 16);
                }
                else
                {
                    AdjustSize(newSize + newSize / 4);
                }
            }
        }
        /// <summary>
        /// append element to latest index
        /// </summary>
        /// <param name="v"></param>
        public void Append(T v)
        {
            if (_internalArray.Length < (_currentSize + 1))
            {
                if (_currentSize < 100000)
                {
                    AdjustSize(_currentSize + (_currentSize / 2) + 16);
                }
                else
                {
                    AdjustSize(_currentSize + _currentSize / 4);
                }
            }
            _internalArray[_currentSize++] = v;
        }

        public void Append(T[] arr)
        {
            //append arr             
            Append(arr, 0, arr.Length);
        }
        public void Append(T[] arr, int start, int len)
        {
            EnsureSpaceForAppend(len);
            System.Array.Copy(arr, start, _internalArray, _currentSize, len);
            _currentSize = _currentSize + len;
        }
        public void CopyAndAppend(int srcIndex, int len)
        {
            EnsureSpaceForAppend(len);
            System.Array.Copy(_internalArray, srcIndex, _internalArray, _currentSize, len);
            _currentSize = _currentSize + len;
        }


        public T this[int i]
        {
            get => _internalArray[i];
            set => _internalArray[i] = value;
        }
        /// <summary>
        /// access to internal array,
        /// </summary>
        public T[] UnsafeInternalArray => _internalArray;
        public void SetData(int index, T data)
        {
            _internalArray[index] = data;
        }
        //
        public int Length => _currentSize;

        //
        public ArrayListSegment<T> CreateSpan(int beginAt, int len) => new ArrayListSegment<T>(this, beginAt, len);
    }
}
