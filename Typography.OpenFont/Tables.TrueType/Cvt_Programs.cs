//MIT, 2015-2016, Michael Popoloski, WinterDev

using System.IO;
namespace Typography.OpenFont.Tables
{

    class CvtTable : TableEntry
    {
        /// <summary>
        /// control value in font unit
        /// </summary>
        internal int[] _controlValues;
        public override string Name => "cvt "; //need 4 chars//***

        protected override void ReadContentFrom(BinaryReader reader)
        {
            int nelems = (int)(this.TableLength / sizeof(short));
            var results = new int[nelems];
            for (int i = 0; i < nelems; i++)
            {
                results[i] = reader.ReadInt16();
            }
            this._controlValues = results;
        }
    }
    class PrepTable : TableEntry
    {
        internal byte[] _programBuffer;
        public override string Name => "prep";
        //
        protected override void ReadContentFrom(BinaryReader reader)
        {
            _programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
    class FpgmTable : TableEntry
    {
        internal byte[] _programBuffer;
        public override string Name => "fpgm";
        protected override void ReadContentFrom(BinaryReader reader)
        {
            _programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
}