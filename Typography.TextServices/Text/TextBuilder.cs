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


namespace Typography.Text
{

    public sealed class TextBuilder<T>
    {
        static readonly T[] s_empty = new T[0];
        int _count;
        T[] _internalArray = s_empty;

        public TextBuilder()
        {
        }
        public TextBuilder(int cap)
        {
            Allocate(cap, 0);
        }
      
        public int AllocatedSize => _internalArray.Length;

        public void Clear()
        {
            _count = 0;
        }

        // Set new capacity. All data is lost, size is set to zero.
        public void Clear(int newCapacity)
        {
            Clear(newCapacity, 0);
        }
        public void Clear(int newCapacity, int extraTail)
        {
            _count = 0;
            if (newCapacity > AllocatedSize)
            {
                _internalArray = null;
                int sizeToAllocate = newCapacity + extraTail;
                if (sizeToAllocate != 0)
                {
                    //array is replace
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
            _count = size;
        }

        /// <summary>
        ///  Resize keeping the content
        /// </summary>
        /// <param name="newSize"></param>
        void AdjustSize(int newSize)
        {
            if (newSize > _count && newSize > AllocatedSize)
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

        void EnsureSpaceForAppend(int newAppendLen)
        {
            int newSize = _count + newAppendLen;
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
            if (_internalArray.Length < (_count + 1))
            {
                if (_count < 100000)
                {
                    AdjustSize(_count + (_count / 2) + 16);
                }
                else
                {
                    AdjustSize(_count + _count / 4);
                }
            }
            _internalArray[_count++] = v;
        }

        public void Append(T[] arr)
        {
            //append arr             
            Append(arr, 0, arr.Length);
        }
        public void Append(T[] arr, int start, int len)
        {
            EnsureSpaceForAppend(len);
            System.Array.Copy(arr, start, _internalArray, _count, len);
            _count += len;
        }
        public void Append(TextBuilder<T> buffer, int start, int len)
        {
            Append(buffer.UnsafeInternalArray, start, len);
        }
        public T this[int i]
        {
            get
            {
#if DEBUG
                if (i >= _count)
                {
                    throw new System.IndexOutOfRangeException();
                }
#endif
                return _internalArray[i];
            }
        }

        //
        public int Count => _count;

        public void Insert(int index, T value)
        {

            //split to left-right
            if (index < 0 || index > _count)
            {
                throw new System.NotSupportedException();
            }
            EnsureSpaceForAppend(_count + 1);
            //
            //move data to right side
            //TODO: review here
            for (int i = Count - 1; i >= index; --i)
            {
                _internalArray[i + 1] = _internalArray[i];
            }

            _internalArray[index] = value;
            _count++;
        }
        public void Insert(int index, T[] values) => Insert(index, values, 0, values.Length);
        public void Insert(int index, T[] values, int srcIndex, int len)
        {
            if (index < 0 || index > _count)
            {
                throw new System.NotSupportedException();
            }

            EnsureSpaceForAppend(_count + len);
            //TODO: review here
            for (int i = Count - 1; i >= index; --i)
            {
                _internalArray[i + len] = _internalArray[i];
            }

            for (int i = 0; i < len; ++i)
            {
                _internalArray[index + i] = values[i];
            }

            System.Array.Copy(values, srcIndex, _internalArray, index, len);
            _count += len;
        }
        public void Insert(int index, TextBuilder<T> src, int srcIndex, int len)
        {
            Insert(index, src.UnsafeInternalArray, srcIndex, len);
        }
        public void Remove(int index) => Remove(index, 1);
        public void Remove(int index, int len)
        {
            if (len < 1 || index < 0 || index > _count)
            {
                throw new System.NotSupportedException();
            }

            int pos = index;
            int copy_count = _count - (index + len);
            //TODO: review here, 
            for (int i = 0; i < copy_count; ++i)
            {
                _internalArray[pos] = _internalArray[pos + len];
                pos++;
            }
            _count -= len;
        }
        public void RemoveLast()
        {
            if (_count > 0)
            {
                _count--;
            }
            else
            {

            }
        }
        /// <summary>
        /// access to internal array,
        /// </summary>
        internal T[] UnsafeInternalArray => _internalArray;

        public static T[] UnsafeGetInternalArray(TextBuilder<T> t) => t._internalArray;
         
        //public void CopyAndAppend(int srcIndex, int len)
        //{
        //    EnsureSpaceForAppend(len);
        //    System.Array.Copy(_internalArray, srcIndex, _internalArray, _currentSize, len);
        //    _currentSize += len;
        //}
        //public T[] ToArray()
        //{
        //    T[] output = new T[_count];
        //    System.Array.Copy(_internalArray, output, _count);
        //    return output;
        //}
    }
}
