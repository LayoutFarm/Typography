//MIT, 2009, 2010, 2013-2016 by the Brotli Authors.
//MIT, 2017, brezza92 (C# port from original code, by hand)

using System.IO;

namespace CSharpBrotli.Decode
{
    public class IntBufferReader
    {
        /// <summary>
        /// Int32 Position
        /// </summary>
        public long Position
        {
            get
            {
                //Divide 4 when get Int32 position from byte position
                return reader.BaseStream.Position / 4;
            }
            set
            {
                //Multiple 4 when set Int32 position to byte position
                reader.BaseStream.Position = value * 4;
            }
        }

        BinaryReader reader;

        public IntBufferReader(Stream input)
        {
            this.reader = new BinaryReader(input);
        }

        public int ReadInt32()
        {
            return reader.ReadInt32();
        }
    }
}