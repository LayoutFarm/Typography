//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{
    class CmapReader
    {
        private static UInt16[] ReadUInt16Array(BinaryReader input, int length)
        {
            var result = new UInt16[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input.ReadUInt16();
            }
            return result;
        }

        private static CharacterMap ReadCharacterMap(CMapEntry entry, BinaryReader input)
        {
            // I want to thank Microsoft for not giving a simple count on the glyphIdArray
            long tableStart = input.BaseStream.Position;

            ushort format = input.ReadUInt16();
            ushort length = input.ReadUInt16();
            if (format == 4)
            {
                ushort version = input.ReadUInt16();
                ushort segCountX2 = input.ReadUInt16();
                ushort searchRange = input.ReadUInt16();
                ushort entrySelector = input.ReadUInt16();
                ushort rangeShift = input.ReadUInt16();

                int segCount = segCountX2 / 2;

                ushort[] endCode = ReadUInt16Array(input, segCount); // last = 0xffff. What does that mean??

                input.ReadUInt16(); // Reserved = 0               

                ushort[] startCode = ReadUInt16Array(input, segCount);
                ushort[] idDelta = ReadUInt16Array(input, segCount);
                ushort[] idRangeOffset = ReadUInt16Array(input, segCount);

                // I want to thank Microsoft for not giving a simple count on the glyphIdArray
                int glyphIdArrayLength = (int)((input.BaseStream.Position - tableStart) / sizeof(UInt16));
                ushort[] glyphIdArray = ReadUInt16Array(input, glyphIdArrayLength);

                return new CharacterMap(segCount, startCode, endCode, idDelta, idRangeOffset, glyphIdArray);
            }
            throw new ApplicationException("Unknown cmap subtable: " + format); // TODO: Replace all applicationexceptions
        }

        class CMapEntry
        {
            readonly UInt16 _platformId;
            readonly UInt16 _encodingId;
            readonly UInt32 _offset;

            public CMapEntry(UInt16 platformId, UInt16 encodingId, UInt32 offset)
            {
                _platformId = platformId;
                _encodingId = encodingId;
                _offset = offset;
            }
            public UInt16 PlatformId { get { return _platformId; } }
            public UInt16 EncodingId { get { return _encodingId; } }
            public UInt32 Offset { get { return _offset; } }
        }

        internal static List<CharacterMap> From(TableEntry table)
        {
            BinaryReader input = table.GetDataReader();

            ushort version = input.ReadUInt16(); // 0
            ushort tableCount = input.ReadUInt16();

            var entries = new List<CMapEntry>(tableCount);
            for (int i = 0; i < tableCount; i++)
            {
                ushort platformId = input.ReadUInt16();
                ushort encodingId = input.ReadUInt16();
                uint offset = input.ReadUInt32();
                entries.Add(new CMapEntry(platformId, encodingId, offset));
            }

            var result = new List<CharacterMap>(tableCount);
            foreach (var entry in entries)
            {
                BinaryReader subtable = table.GetDataReader();
                subtable.BaseStream.Seek(entry.Offset, SeekOrigin.Current);
                result.Add(ReadCharacterMap(entry, subtable));
            }

            return result;
        }
    }
}