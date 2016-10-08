using System;

namespace NRasterizer.Tables
{
    internal class GlyphLocations
    {
        private readonly uint[] _offsets;

        public uint[] Offsets { get { return _offsets; } }
        public int GlyphCount { get { return _offsets.Length - 1; } }

        public GlyphLocations(TableEntry table, int glyphCount, bool wideLocations)
        {
            var input = table.GetDataReader();
            _offsets = new uint[glyphCount + 1];

            if (wideLocations)
            {
                for (int i = 0; i < glyphCount + 1; i++)
                {
                    _offsets[i] = input.ReadUInt32();
                }
            }
            else
            {
                for (int i = 0; i < glyphCount + 1; i++)
                {
                    _offsets[i] = (uint)(input.ReadUInt16() * 2);
                }
            }
        }
    }
}
