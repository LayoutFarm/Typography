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

using System;
namespace PixelFarm.CpuBlit
{

    public struct TempMemPtr : IDisposable
    {
        readonly int _lenInBytes; //in bytes         
        readonly bool _isOwner;
        IntPtr _nativeBuffer;

        public TempMemPtr(IntPtr nativeBuffer32, int lenInBytes, bool isOwner = false)
        {
            _lenInBytes = lenInBytes;
            _nativeBuffer = nativeBuffer32;
            _isOwner = isOwner;
        }
        //
        public int LengthInBytes => _lenInBytes;
        //
        public IntPtr Ptr => _nativeBuffer;
        //
        public void Dispose()
        {
            if (_isOwner && _nativeBuffer != IntPtr.Zero)
            {
                //destroy in
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_nativeBuffer);
                _nativeBuffer = IntPtr.Zero;
            }
        }
        public unsafe static TempMemPtr FromBmp(IBitmapSrc actualBmp, out int* headPtr)
        {
            TempMemPtr ptr = actualBmp.GetBufferPtr();
            headPtr = (int*)ptr.Ptr;
            return ptr;
        }


       
    }
}