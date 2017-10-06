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

        public override Stream BaseStream { get { return base.BaseStream; } }
        //public override void Close() { base.Close(); }

        public override int PeekChar() { throw new NotImplementedException(); }
        public override int Read() { throw new NotImplementedException(); }
        public override int Read(byte[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override int Read(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override bool ReadBoolean() { throw new NotImplementedException(); }
        public override byte ReadByte() { return base.ReadByte(); }
        public override byte[] ReadBytes(int count) { return base.ReadBytes(count); }
        public override char ReadChar() { throw new NotImplementedException(); }
        public override char[] ReadChars(int count) { throw new NotImplementedException(); }
        //public override decimal ReadDecimal() { throw new NotImplementedException(); }
        public override double ReadDouble() { throw new NotImplementedException(); }
        public override short ReadInt16() { return BitConverter.ToInt16(ReadBytesAndReverse(2), 0); }
        public override int ReadInt32() { throw new NotImplementedException(); }
        public override long ReadInt64() { throw new NotImplementedException(); }
        public override sbyte ReadSByte() { throw new NotImplementedException(); }
        public override float ReadSingle() { throw new NotImplementedException(); }
        public override string ReadString() { throw new NotImplementedException(); }
        public override ushort ReadUInt16() { return BitConverter.ToUInt16(ReadBytesAndReverse(2), 0); }
        public override uint ReadUInt32() { return BitConverter.ToUInt32(ReadBytesAndReverse(4), 0); }
        public override ulong ReadUInt64() { return BitConverter.ToUInt64(ReadBytesAndReverse(8), 0); }

        private byte[] ReadBytesAndReverse(int count) { var b = ReadBytes(count); Array.Reverse(b); return b; }

        protected override void Dispose(bool disposing)
        {
            GC.SuppressFinalize(this);
            base.Dispose(disposing);
        }
    }
}
