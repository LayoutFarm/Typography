//BSD, 2014-2017, WinterDev 
namespace PixelFarm.Agg
{
    public abstract class AggBuffMx
    {
        //create with default impl 
        static AggBuffMx s_impl = new ManagedAggBufferMx();
        public static void SetNaiveBufferImpl(AggBuffMx impl)
        {
            s_impl = impl;
        }
        protected abstract void InnerMemSet(byte[] dest, int startAt, byte value, int count);
        protected abstract void InnerMemCopy(byte[] dest_buffer, int dest_startAt, byte[] src_buffer, int src_StartAt, int len);

        public static void MemSet(byte[] dest, int startAt, byte value, int count)
        {
            s_impl.InnerMemSet(dest, startAt, value, count);
        }
        public static void MemCopy(byte[] dest_buffer, int dest_startAt, byte[] src_buffer, int src_StartAt, int len)
        {
            s_impl.InnerMemCopy(dest_buffer, dest_startAt, src_buffer, src_StartAt, len);
        }
    }
    class ManagedAggBufferMx : AggBuffMx
    {
        protected override void InnerMemCopy(byte[] dest_buffer, int dest_startAt, byte[] src_buffer, int src_StartAt, int len)
        {

            System.Buffer.BlockCopy(
                src_buffer, src_StartAt, dest_buffer, dest_startAt, len);
        }
        protected override void InnerMemSet(byte[] dest, int startAt, byte value, int count)
        {
            unsafe
            {
                fixed (byte* pos = &dest[startAt])
                {
                    //TODO: review performance here
                    for (int i = count - 1; i >= 0; --i)
                    {
                        *(pos + i) = value;
                    }
                }
            }
        }
    }

}