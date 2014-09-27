using System;
using System.IO;
using System.Text;

namespace notf
{
    public class OpenTypeReader
    {
        public void Reader(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (var input = new ByteOrderSwappingBinaryReader(stream))
            {
                UInt32 version = input.ReadUInt32();
                UInt16 tableCount = input.ReadUInt16();
                UInt16 searchRange = input.ReadUInt16();
                UInt16 entrySelector = input.ReadUInt16();
                UInt16 rangeShift = input.ReadUInt16();

                for (int i = 0; i < tableCount; i++)
                {
                    var tag = input.ReadUInt32();
                    var checkSum = input.ReadUInt32();
                    var offset = input.ReadUInt32();
                    var length = input.ReadUInt32();

                    Console.WriteLine(TagToString(tag));
                }
            }
        }

        private String TagToString(uint tag)
        {
            var bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
