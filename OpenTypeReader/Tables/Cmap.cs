//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
namespace NRasterizer.Tables
{
    class Cmap : TableEntry
    {
        List<CharacterMap> charMaps;
        public override string Name
        {
            get { return "cmap"; }
        }
        public List<CharacterMap> CharMaps
        {
            get { return charMaps; }
        }
        protected override void ReadContentFrom(BinaryReader input)
        {
            //https://www.microsoft.com/typography/otspec/cmap.htm
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


            charMaps = new List<CharacterMap>(tableCount);
            uint tableOffset = this.Header.Offset;
            foreach (CMapEntry entry in entries)
            {
                //TODO: review here
                input.BaseStream.Seek(tableOffset, SeekOrigin.Begin);//reset
                input.BaseStream.Seek(entry.Offset, SeekOrigin.Current);
                charMaps.Add(ReadCharacterMap(entry, input));
            }
        }
        static UInt16[] ReadUInt16Array(BinaryReader input, int length)
        {
            var result = new UInt16[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input.ReadUInt16();
            }
            return result;
        }

        static CharacterMap ReadCharacterMap(CMapEntry entry, BinaryReader input)
        {
            // I want to thank Microsoft for not giving a simple count on the glyphIdArray
            long tableStart = input.BaseStream.Position;
            ushort format = input.ReadUInt16();
            ushort length = input.ReadUInt16();
            switch (format)
            {
                default:
                    {
                        throw new ApplicationException("Unknown cmap subtable: " + format); // TODO: Replace all applicationexceptions
                    }
                case 4:
                    {
                        return ReadCmapFormat4(input, tableStart);
                    }
            }
        }
        /// <summary>
        ///  Format 4: Segment mapping to delta values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tableStart"></param>
        static CharacterMap ReadCmapFormat4(BinaryReader input, long tableStart)
        {
            //This is the Microsoft standard character to glyph index mapping table for fonts that support Unicode ranges other than the range [U+D800 - U+DFFF] (defined as Surrogates Area, in Unicode v 3.0) 
            //which is used for UCS-4 characters.
            //If a font supports this character range (i.e. in turn supports the UCS-4 characters) a subtable in this format with a platform specific encoding ID 1 is yet needed,
            //in addition to a subtable in format 12 with a platform specific encoding ID 10. Please see details on format 12 below, for fonts that support UCS-4 characters on Windows.
            //  
            //This format is used when the character codes for the characters represented by a font fall into several contiguous ranges, 
            //possibly with holes in some or all of the ranges (that is, some of the codes in a range may not have a representation in the font). 
            //The format-dependent data is divided into three parts, which must occur in the following order:
            //    A four-word header gives parameters for an optimized search of the segment list;
            //    Four parallel arrays describe the segments (one segment for each contiguous range of codes);
            //    A variable-length array of glyph IDs (unsigned words).
            ushort version = input.ReadUInt16();
            ushort segCountX2 = input.ReadUInt16(); //2 * segCount
            ushort searchRange = input.ReadUInt16(); //2 * (2**FLOOR(log2(segCount)))
            ushort entrySelector = input.ReadUInt16();//2 * (2**FLOOR(log2(segCount)))
            ushort rangeShift = input.ReadUInt16(); //2 * (2**FLOOR(log2(segCount)))
            int segCount = segCountX2 / 2;
            ushort[] endCode = ReadUInt16Array(input, segCount);//Ending character code for each segment, last = 0xFFFF.            
            //>To ensure that the search will terminate, the final endCode value must be 0xFFFF.
            //>This segment need not contain any valid mappings. It can simply map the single character code 0xFFFF to the missing character glyph, glyph 0.

            input.ReadUInt16(); // Reserved = 0               
            ushort[] startCode = ReadUInt16Array(input, segCount); //Starting character code for each segment
            ushort[] idDelta = ReadUInt16Array(input, segCount); //Delta for all character codes in segment
            ushort[] idRangeOffset = ReadUInt16Array(input, segCount); //Offset in bytes to glyph indexArray, or 0 
            int glyphIdArrayRecordNum = FindGlyphIdArrayLenInBytes(idRangeOffset) / 2;
            //------------------------------------------------------------------------------------ 
            // I want to thank Microsoft for not giving a simple count on the glyphIdArray
            //int glyphIdArrayLength = (int)((input.BaseStream.Position - tableStart) / sizeof(UInt16)); //not correct 
            ushort[] glyphIdArray = ReadUInt16Array(input, glyphIdArrayRecordNum);//Glyph index array 
            return new CharacterMap(segCount, startCode, endCode, idDelta, idRangeOffset, glyphIdArray);
        }
        static int FindGlyphIdArrayLenInBytes(ushort[] idRangeOffset)
        {
            //1. find max OffsetValue (in bytes unit)
            //this is the possible value to reach from the idRangeOffsetRecord 
            ushort max = 0;
            int foundAt = 0;
            for (int i = idRangeOffset.Length - 1; i >= 0; --i)
            {
                ushort off = idRangeOffset[i];
                if (off > max)
                {
                    max = off;
                    foundAt = i;
                }
            }
            //----------------------------
            //2. then offset with current found record
            return max - (foundAt * 2); //*2 = to byte unit 
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
    }
}