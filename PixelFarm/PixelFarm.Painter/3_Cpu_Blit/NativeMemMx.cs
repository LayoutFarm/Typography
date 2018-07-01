//BSD, 2014-present, WinterDev 
using System.Runtime.InteropServices;
namespace PixelFarm.CpuBlit
{


    [System.Security.SuppressUnmanagedCodeSecurity] //apply this to all native methods in this class
    static class NativeMemMx
    {
        //check this ....
        //for cross platform code

        //TODO: review here again***
        //this is platform specific ***

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void memset(byte* dest, byte c, int byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void memcpy(byte* dest, byte* src, int byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int memcmp(byte* dest, byte* src, int byteCount);
        //----------

        public static void MemSet(byte[] dest, int startAt, byte value, int count)
        {
            unsafe
            {
                fixed (byte* head = &dest[startAt])
                {
                    memset(head, value, count);
                }
            }
        }
        public static void MemCopy(byte[] dest_buffer, int dest_startAt, byte[] src_buffer, int src_StartAt, int len)
        {
            unsafe
            {
                fixed (byte* head_dest = &dest_buffer[dest_startAt])
                fixed (byte* head_src = &src_buffer[src_StartAt])
                {
                    memcpy(head_dest, head_src, len);
                }
            }
        }
        public static unsafe void MemCopy(byte* head_dest, byte* head_src, int len)
        {
            memcpy(head_dest, head_src, len);
        }
    }
}