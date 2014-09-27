using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace notf
{
    public class OpenTypeReader
    {
        public void Reader(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (BinaryReader input = new ByteOrderSwappingBinaryReader(stream))
            {
                UInt32 version = input.ReadUInt32();
                UInt16 tableCount = input.ReadUInt16();
                UInt16 searchRange = input.ReadUInt16();
                UInt16 entrySelector = input.ReadUInt16();
                UInt16 rangeShift = input.ReadUInt16();

                var tables = new List<TableEntry>(tableCount);
                for (int i = 0; i < tableCount; i++)
                {
                    tables.Add(Table.ReadFrom(input));
                }

                var cmap = tables.Single(t => t.Tag == "glyf");

            }
        }

        private class TableEntry
        {
            private readonly uint _tag;
            private readonly uint _checkSum;
            private readonly uint _offset;
            private readonly uint _length;

            public string Tag { get { return TagToString(_tag); } }
            private TableEntry(uint tag, uint checkSum, uint offset, uint length)
            {
                _tag = tag;
                _checkSum = checkSum;
                _offset = offset;
                _length = length;

            }

            public static TableEntry ReadFrom(BinaryReader input)
            {
                var tag = input.ReadUInt32();
                var checkSum = input.ReadUInt32();
                var offset = input.ReadUInt32();
                var length = input.ReadUInt32();
                return new Table(tag, checkSum, offset, length);
            }
            private String TagToString(uint tag)
            {
                var bytes = BitConverter.GetBytes(tag);
                Array.Reverse(bytes);
                return Encoding.ASCII.GetString(bytes);
            }
        }

    }
}
