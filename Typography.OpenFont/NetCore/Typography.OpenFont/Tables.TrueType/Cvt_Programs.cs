//MIT, 2015-2016, Michael Popoloski, WinterDev
 
using System.IO;
namespace Typography.OpenFont.Tables
{

    class CvtTable : TableEntry
    {
        /// <summary>
        /// control value in font unit
        /// </summary>
        internal int[] controlValues;
        public override string Name
        {
            get { return "cvt "; /*need 4 chars*/}
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            int nelems = (int)(this.TableLength / sizeof(short));
            var results = new int[nelems];
            for (int i = 0; i < nelems; i++)
            {
                results[i] = reader.ReadInt16();
            }
            this.controlValues = results;
        }
    }
    class PrepTable : TableEntry
    {
        internal byte[] programBuffer;
        public override string Name
        {
            get { return "prep"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
    class FpgmTable : TableEntry
    {
        internal byte[] programBuffer;
        public override string Name
        {
            get { return "fpgm"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
}