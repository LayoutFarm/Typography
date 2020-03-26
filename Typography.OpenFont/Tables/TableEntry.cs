//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;
namespace Typography.OpenFont.Tables
{
    /// <summary>
    /// this is base class of all 'top' font table
    /// </summary>
    public abstract class TableEntry
    {
        internal TableEntry(TableHeader header, BinaryReader? reader)
        {
            Header = header;
            reader?.BaseStream.Seek(this.Header.Offset, SeekOrigin.Begin);
        }
        internal TableHeader Header { get; }
        public uint TableLength => this.Header.Length;
    }
    class UnreadTableEntry : TableEntry
    {
        public UnreadTableEntry(TableHeader header) : base(header, null)
        {
        }
        public string Name => this.Header.Tag;

        public bool HasCustomContentReader { get; protected set; }
        public virtual T CreateTableEntry<T>(BinaryReader reader, OpenFontReader.TableReader<T> tableReader)
            where T : TableEntry
        {
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
