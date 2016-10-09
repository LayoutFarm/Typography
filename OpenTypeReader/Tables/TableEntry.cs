//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{
    class TableEntry
    {
        readonly BinaryReader _input;
        readonly uint _tag;
        readonly uint _checkSum;
        readonly uint _offset;
        readonly uint _length;

        public string Tag { get { return TagToString(_tag); } }

        private TableEntry(BinaryReader input)
        {
            _input = input;
            _tag = _input.ReadUInt32();
            _checkSum = _input.ReadUInt32();
            _offset = _input.ReadUInt32();
            _length = _input.ReadUInt32();
        }

        public static TableEntry ReadFrom(BinaryReader input)
        {
            return new TableEntry(input);
        }

        // TODO: Take offset parameter as commonly two seeks are made in a row
        public BinaryReader GetDataReader()
        {
            _input.BaseStream.Seek(_offset, SeekOrigin.Begin);
            // TODO: Limit reading to _length by wrapping BinaryReader (or Stream)?
            return _input;
        }

        private String TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return Encoding.ASCII.GetString(bytes);
        }

        public override string ToString()
        {
            return "{" + Tag + "}";
        }
    }
}
