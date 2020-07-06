//BSD, 2014-present, WinterDev


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
//#define USE_UNSAFE // no real code for this yet

using System;


namespace PixelFarm.Drawing.Internal
{
    public static class MemMx
    {

        public unsafe delegate void _memset(byte* dest, byte c, int byteCount);
        public unsafe delegate void _memcpy(byte* dest, byte* src, int byteCount);

        public static void memcpy(byte[] dest,
            int destIndex, byte[] source,
            int sourceIndex, int count)
        {
            unsafe
            {
                fixed (byte* head_dest = &dest[destIndex])
                fixed (byte* head_src = &source[sourceIndex])
                {
                    s_memCopyImpl(head_dest, head_src, count);
                }
            }
        }
        public static unsafe void memcpy(byte* dest, byte* src, int len)
        {
            s_memCopyImpl(dest, src, len);
        }
        public static void memmove(byte[] dest, int destIndex, byte[] source, int sourceIndex, int count)
        {
            if (source != dest
                || destIndex < sourceIndex)
            {
                memcpy(dest, destIndex, source, sourceIndex, count);
            }
            else
            {
                throw new Exception("this code needs to be tested");
            }
        }
        public static unsafe void memmove(byte* dest, int destIndex, byte* source, int sourceIndex, int count)
        {
            if (source != dest
                || destIndex < sourceIndex)
            {
                s_memCopyImpl(dest + destIndex, source + sourceIndex, count);
                // memcpy(dest, destIndex, source, sourceIndex, Count);
            }
            else
            {
                throw new Exception("this code needs to be tested");
            }
        }
        public static void memset(byte[] dest, int destIndex, byte byteValue, int count)
        {
            unsafe
            {
                fixed (byte* d = &dest[destIndex])
                {
                    s_memSetImpl(d, byteValue, count);
                }

            }
        }
        public static void memset_unsafe(IntPtr dest, byte byteValue, int count)
        {
            unsafe
            {
                s_memSetImpl((byte*)dest, byteValue, count);
            }
        }

        static _memcpy s_memCopyImpl;
        static _memset s_memSetImpl;
        public static void SetMemImpl(_memcpy memcopyImpl, _memset memsetImpl)
        {
            s_memCopyImpl = memcopyImpl;
            s_memSetImpl = memsetImpl;
        }
         
        static MemMx()
        {
            unsafe
            {
                //set default implementation
                SetMemImpl(NativeMemMx.memcpy, NativeMemMx.memset);
            }
        }
    }
}