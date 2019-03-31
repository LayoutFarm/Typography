//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;

namespace Typography.OpenFont.IO
{
    class ByteOrderSwappingBinaryReader : BinaryReader
    {
        //All OpenType fonts use Motorola-style byte ordering (Big Endian)
        //
        public ByteOrderSwappingBinaryReader(Stream input)
            : base(input)
        {
        }
        protected override void Dispose(bool disposing)
        {
            GC.SuppressFinalize(this);
            base.Dispose(disposing);
        }
        //
        //as original
        //
        //public override byte ReadByte() { return base.ReadByte(); } 
        // 
        //we override the 6 methods here

        //set1--------------------
        //
        //public override short ReadInt16() => BitConverter.ToInt16(RR(2), 8 - 2);
        //public override ushort ReadUInt16() => BitConverter.ToUInt16(RR(2), 8 - 2);
        //public override uint ReadUInt32() => BitConverter.ToUInt32(RR(4), 8 - 4);
        //public override ulong ReadUInt64() => BitConverter.ToUInt64(RR(8), 8 - 8);
        ////used in CFF font
        //public override double ReadDouble() => BitConverter.ToDouble(RR(8), 8 - 8);
        ////used in CFF font
        //public override int ReadInt32() => BitConverter.ToInt32(RR(4), 8 - 4);
        //--------------------



        ////set2--------------------
        //public override short ReadInt16() => BitConverter.ToInt16(RR2(), 8 - 2);
        //public override ushort ReadUInt16() => BitConverter.ToUInt16(RR2(), 8 - 2);
        //public override uint ReadUInt32() => BitConverter.ToUInt32(RR4(), 8 - 4);
        //public override ulong ReadUInt64() => BitConverter.ToUInt64(RR8(), 8 - 8);
        //////used in CFF font
        //public override double ReadDouble() => BitConverter.ToDouble(RR8(), 8 - 8);
        ////used in CFF font
        //public override int ReadInt32() => BitConverter.ToInt32(RR4(), 8 - 4);
        ////--------------------



        //set3--------------------
        public override short ReadInt16() => RR2AsInt16();
        public override ushort ReadUInt16() => RR2AsUInt16();
        public override uint ReadUInt32() => RR4AsUInt32();
        public override ulong ReadUInt64() => BitConverter.ToUInt64(RR8(), 8 - 8);
        ////used in CFF font
        public override double ReadDouble() => BitConverter.ToDouble(RR8(), 8 - 8);
        //used in CFF font
        public override int ReadInt32() => RR4AsInt32();
        //--------------------


        //
        byte[] _reusable_buffer = new byte[8]; //fix buffer size to 8 bytes


        /// <summary>
        /// read and reverse 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] RR(int count)
        {
            base.Read(_reusable_buffer, 0, count);
            Array.Reverse(_reusable_buffer);
            return _reusable_buffer;
        }

        private short RR2AsInt16()
        {
            base.Read(_reusable_buffer, 0, 2); 
            unsafe
            {
                fixed (byte* b0ptr = &_reusable_buffer[0])
                {
                    //reverse
                    //byte b0 = b0ptr[1];
                    //byte b1 = b0ptr[0];
                    return (short)((b0ptr[0] << 8) | b0ptr[1]);
                }
            }
        }

        private ushort RR2AsUInt16()
        {
            base.Read(_reusable_buffer, 0, 2);

            unsafe
            {
                fixed (byte* b0ptr = &_reusable_buffer[0])
                {
                    //reverse
                    //byte b0 = b0ptr[1];
                    //byte b1 = b0ptr[0];
                    return (ushort)((b0ptr[0] << 8) | b0ptr[1]);
                }
            }
        }

        private uint RR4AsUInt32()
        {
            base.Read(_reusable_buffer, 0, 4);
            unsafe
            {
                fixed (byte* b0ptr = &_reusable_buffer[0])
                {
                    //reverse
                    //byte b0 = b0ptr[3];
                    //byte b1 = b0ptr[2];
                    //byte b2 = b0ptr[1];
                    //byte b3 = b0ptr[0];
                    // return (uint)((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);
                    return (uint)((b0ptr[0] << 24) | (b0ptr[1] << 16) | (b0ptr[2] << 8) | b0ptr[3]);
                }
            }

        }
        private int RR4AsInt32()
        {
            base.Read(_reusable_buffer, 0, 4);
            unsafe
            {
                fixed (byte* b0ptr = &_reusable_buffer[0])
                {
                    //reverse
                    //byte b0 = b0ptr[3];
                    //byte b1 = b0ptr[2];
                    //byte b2 = b0ptr[1];
                    //byte b3 = b0ptr[0];
                    //return ((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);
                    return ((b0ptr[0] << 24) | (b0ptr[1] << 16) | (b0ptr[2] << 8) | b0ptr[3]);
                }
            }
        }

        private byte[] RR8()
        {
            base.Read(_reusable_buffer, 0, 8);
            Array.Reverse(_reusable_buffer);
            return _reusable_buffer;
        }

        //we don't use these methods in our OpenFont, so => throw the exception
        public override int PeekChar() { throw new NotImplementedException(); }
        public override int Read() { throw new NotImplementedException(); }
        public override int Read(byte[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override int Read(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override bool ReadBoolean() { throw new NotImplementedException(); }
        public override char ReadChar() { throw new NotImplementedException(); }
        public override char[] ReadChars(int count) { throw new NotImplementedException(); }
        public override decimal ReadDecimal() { throw new NotImplementedException(); }

        public override long ReadInt64() { throw new NotImplementedException(); }
        public override sbyte ReadSByte() { throw new NotImplementedException(); }
        public override float ReadSingle() { throw new NotImplementedException(); }
        public override string ReadString() { throw new NotImplementedException(); }
        //

    }
}
