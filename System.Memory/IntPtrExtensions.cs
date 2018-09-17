using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    //Polyfills for .NET 2.0
    internal static class IntPtrExtensions
    {
        public static unsafe IntPtr Offset(this IntPtr ptr, int val)
        {
            if (sizeof(IntPtr) == sizeof(int))
            {
                //32-bit path.
                return new IntPtr(ptr.ToInt32() + val);
            }
            else
            {
                //64-bit path.
                return new IntPtr(ptr.ToInt64() + val);
            }
        }
        public static unsafe UIntPtr Offset(this UIntPtr ptr, int val)
        {
            if (sizeof(UIntPtr) == sizeof(uint))
            {
                //32-bit path.
                return new UIntPtr(ptr.ToUInt32() + (uint)val);
            }
            else
            {
                //64-bit path.
                return new UIntPtr(ptr.ToUInt64() + (ulong)val);
            }
        }
    }
}
