//MIT, 2015-2016, Michael Popoloski, WinterDev

using System.IO;
namespace Typography.OpenFont.Tables
{

    class CvtTable : TableEntry
    {
        public const string Name = "cvt ";//need 4 chars//***

        //

        /// <summary>
        /// control value in font unit
        /// </summary>
        internal int[] _controlValues;
        internal CvtTable(TableHeader header, BinaryReader reader) : base(header, reader)
        {
            int nelems = (int)(this.TableLength / sizeof(short));
            var results = new int[nelems];
            for (int i = 0; i < nelems; i++)
            {
                results[i] = reader.ReadInt16();
            }
            _controlValues = results;
        }
    }
    class PrepTable : TableEntry
    {
        public const string Name = "prep";
        //

        internal byte[] _programBuffer;
        //
        internal PrepTable(TableHeader header, BinaryReader reader) : base(header, reader)
        {
            _programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
    class FpgmTable : TableEntry
    {
        public const string Name = "fpgm";
        //

        internal byte[] _programBuffer;
        internal FpgmTable(TableHeader header, BinaryReader reader) : base(header, reader)
        {
            _programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
}