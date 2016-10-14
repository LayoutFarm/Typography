//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System.IO;
namespace NRasterizer.Tables
{
    class GlyphLocations : TableEntry
    {
        uint[] _offsets;
        public GlyphLocations(int glyphCount, bool wideLocations)
        {
            _offsets = new uint[glyphCount + 1];
            this.WideLocations = wideLocations;
        }
        public override string Name
        {
            get { return "loca"; }
        }
        public bool WideLocations { get; private set; }
        public uint[] Offsets { get { return _offsets; } }
        public int GlyphCount { get { return _offsets.Length - 1; } }

        protected override void ReadContentFrom(BinaryReader reader)
        {
            int glyphCount = GlyphCount;
            _offsets = new uint[glyphCount + 1];
            if (WideLocations)
            {
                for (int i = 0; i < glyphCount + 1; i++)
                {
                    _offsets[i] = reader.ReadUInt32();
                }
            }
            else
            {
                for (int i = 0; i < glyphCount + 1; i++)
                {
                    _offsets[i] = (uint)(reader.ReadUInt16() * 2);
                }
            }
        }
    }
}
