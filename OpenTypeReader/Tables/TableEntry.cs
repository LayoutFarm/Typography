//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;
namespace NRasterizer.Tables
{
    abstract class TableEntry
    {
        public TableEntry()
        {
        }
        public TableHeader Header { get; set; }
        protected abstract void ReadContentFrom(BinaryReader reader);
        public abstract string Name { get; }
        public void LoadDataFrom(BinaryReader reader)
        {
            reader.BaseStream.Seek(this.Header.Offset, SeekOrigin.Begin);
            ReadContentFrom(reader);
        }
    }
    class UnreadTableEntry : TableEntry
    {
        public UnreadTableEntry(TableHeader header)
        {
            this.Header = header;
        }
        public override string Name
        {
            get { return this.Header.Tag; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            //intend ***
            throw new NotImplementedException();
        }
#if DEBUG
        public override string ToString()
        {
            return this.Name;
        }
#endif
    }
}