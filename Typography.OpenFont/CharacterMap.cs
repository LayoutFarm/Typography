//Apache2, 2017-present, WinterDev, Sam Hocevar
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont
{
    class CharMapFormat4 : CharacterMap
    {
        public override ushort Format { get { return 4; } }

        readonly ushort[] _startCode; //Starting character code for each segment
        readonly ushort[] _endCode;//Ending character code for each segment, last = 0xFFFF.      
        readonly ushort[] _idDelta; //Delta for all character codes in segment
        readonly ushort[] _idRangeOffset; //Offset in bytes to glyph indexArray, or 0 (not offset in bytes unit)
        readonly ushort[] _glyphIdArray;
        public CharMapFormat4(ushort[] startCode, ushort[] endCode, ushort[] idDelta, ushort[] idRangeOffset, ushort[] glyphIdArray)
        {
            _startCode = startCode;
            _endCode = endCode;
            _idDelta = idDelta;
            _idRangeOffset = idRangeOffset;
            _glyphIdArray = glyphIdArray;
        }

        protected override ushort RawCharacterToGlyphIndex(int codepoint)
        {
            // This lookup table only supports 16-bit codepoints
            if (codepoint > ushort.MaxValue)
            {
                return 0;
            }

            // https://www.microsoft.com/typography/otspec/cmap.htm#format4
            // "You search for the first endCode that is greater than or equal to the character code you want to map"
            // "The segments are sorted in order of increasing endCode values"
            // -> binary search is valid here
            int i = Array.BinarySearch(_endCode, (ushort)codepoint);
            i = i < 0 ? ~i : i;

            // https://www.microsoft.com/typography/otspec/cmap.htm#format4
            // "If the corresponding startCode is [not] less than or equal to the character code,
            // then [...] the missingGlyph is returned"
            // Index i should never be out of range, because the list ends with a
            // 0xFFFF value. However, we also use this charmap for format 0, which
            // does not have that final endcode, so there is a chance to overflow.
            if (i >= _endCode.Length || _startCode[i] > codepoint)
            {
                return 0;
            }

            if (_idRangeOffset[i] == 0)
            {
                //TODO: review 65536 => use bitflags
                return (ushort)((codepoint + _idDelta[i]) % 65536);
            }
            else
            {
                //If the idRangeOffset value for the segment is not 0,
                //the mapping of character codes relies on glyphIdArray.
                //The character code offset from startCode is added to the idRangeOffset value.
                //This sum is used as an offset from the current location within idRangeOffset itself to index out the correct glyphIdArray value.
                //This obscure indexing trick works because glyphIdArray immediately follows idRangeOffset in the font file.
                //The C expression that yields the glyph index is:

                //*(idRangeOffset[i]/2
                //+ (c - startCount[i])
                //+ &idRangeOffset[i])

                var offset = _idRangeOffset[i] / 2 + (codepoint - _startCode[i]);
                // I want to thank Microsoft for this clever pointer trick
                // TODO: What if the value fetched is inside the _idRangeOffset table?
                // TODO: e.g. (offset - _idRangeOffset.Length + i < 0)
                return _glyphIdArray[offset - _idRangeOffset.Length + i];
            }
        }
    }

    class CharMapFormat12 : CharacterMap
    {
        public override ushort Format { get { return 12; } }

        uint[] startCharCodes, endCharCodes, startGlyphIds;
        internal CharMapFormat12(uint[] startCharCodes, uint[] endCharCodes, uint[] startGlyphIds)
        {
            this.startCharCodes = startCharCodes;
            this.endCharCodes = endCharCodes;
            this.startGlyphIds = startGlyphIds;
        }

        protected override ushort RawCharacterToGlyphIndex(int codepoint)
        {
            // https://www.microsoft.com/typography/otspec/cmap.htm#format12
            // "Groups must be sorted by increasing startCharCode."
            // -> binary search is valid here
            int i = Array.BinarySearch(startCharCodes, (uint)codepoint);
            i = i < 0 ? ~i - 1 : i;

            if (i >= 0 && codepoint <= endCharCodes[i])
            {
                return (ushort)(startGlyphIds[i] + codepoint - startCharCodes[i]);
            }
            return 0;
        }
    }

    class CharMapFormat6 : CharacterMap
    {
        public override ushort Format { get { return 6; } }

        internal CharMapFormat6(ushort startCode, ushort[] glyphIdArray)
        {
            _glyphIdArray = glyphIdArray;
            _startCode = startCode;
        }

        protected override ushort RawCharacterToGlyphIndex(int codepoint)
        {
            // The firstCode and entryCount values specify a subrange (beginning at firstCode,
            // length = entryCount) within the range of possible character codes.
            // Codes outside of this subrange are mapped to glyph index 0.
            // The offset of the code (from the first code) within this subrange is used as
            // index to the glyphIdArray, which provides the glyph index value.
            int i = codepoint - _startCode;
            return i >= 0 && i < _glyphIdArray.Length ? _glyphIdArray[i] : (ushort)0;
        }

        private readonly ushort _startCode;
        private readonly ushort[] _glyphIdArray;
    }


    //https://www.microsoft.com/typography/otspec/cmap.htm#format14
    // Subtable format 14 specifies the Unicode Variation Sequences(UVSes) supported by the font.
    // A Variation Sequence, according to the Unicode Standard, comprises a base character followed
    // by a variation selector; e.g. <U+82A6, U+E0101>.
    //
    // The subtable partitions the UVSes supported by the font into two categories: “default” and
    // “non-default” UVSes.Given a UVS, if the glyph obtained by looking up the base character of
    // that sequence in the Unicode cmap subtable(i.e.the UCS-4 or the BMP cmap subtable) is the
    // glyph to use for that sequence, then the sequence is a “default” UVS; otherwise it is a
    // “non-default” UVS, and the glyph to use for that sequence is specified in the format 14
    // subtable itself.
    class CharMapFormat14 : CharacterMap
    {
        public override ushort Format { get { return 14; } }
        protected override ushort RawCharacterToGlyphIndex(int character) { return 0; }

        public ushort CharacterPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint)
        {
            // Only check codepoint if nextCodepoint is a variation selector
            VariationSelector sel;
            if (_variationSelectors.TryGetValue(nextCodepoint, out sel))
            {
                // If the sequence is a non-default UVS, return the mapped glyph
                ushort ret = 0;
                if (sel.UVSMappings.TryGetValue(codepoint, out ret))
                {
                    return ret;
                }

                // If the sequence is a default UVS, return the default glyph
                for (int i = 0; i < sel.DefaultStartCodes.Count; ++i)
                {
                    if (codepoint >= sel.DefaultStartCodes[i] && codepoint < sel.DefaultEndCodes[i])
                    {
                        return defaultGlyphIndex;
                    }
                }

                // At this point we are neither a non-default UVS nor a default UVS,
                // but we know the nextCodepoint is a variation selector. Unicode says
                // this glyph should be invisible: “no visible rendering for the VS”
                // (http://unicode.org/faq/unsup_char.html#4)
                return defaultGlyphIndex;
            }

            // In all other cases, return 0
            return 0;
        }

        public static CharMapFormat14 Create(BinaryReader reader)
        {
            // 'cmap' Subtable Format 14:
            // Type                 Name                                Description
            // uint16               format                              Subtable format.Set to 14.
            // uint32               length                              Byte length of this subtable (including this header)
            // uint32               numVarSelectorRecords               Number of variation Selector Records 
            // VariationSelector    varSelector[numVarSelectorRecords]  Array of VariationSelector records.
            // ---                       
            //
            // Each variation selector records specifies a variation selector character, and
            // offsets to “default” and “non-default” tables used to map variation sequences using
            // that variation selector.
            //
            // VariationSelector Record:
            // Type      Name                 Description
            // uint24    varSelector          Variation selector
            // Offset32  defaultUVSOffset     Offset from the start of the format 14 subtable to
            //                                Default UVS Table.May be 0.
            // Offset32  nonDefaultUVSOffset  Offset from the start of the format 14 subtable to
            //                                Non-Default UVS Table. May be 0.
            //
            // The Variation Selector Records are sorted in increasing order of ‘varSelector’. No
            // two records may have the same ‘varSelector’.
            // A Variation Selector Record and the data its offsets point to specify those UVSes
            // supported by the font for which the variation selector is the ‘varSelector’ value
            // of the record. The base characters of the UVSes are stored in the tables pointed
            // to by the offsets.The UVSes are partitioned by whether they are default or
            // non-default UVSes.
            // Glyph IDs to be used for non-default UVSes are specified in the Non-Default UVS table.

            long beginAt = reader.BaseStream.Position - 2; // account for header format entry 
            uint length = reader.ReadUInt32(); // Byte length of this subtable (including the header)
            uint numVarSelectorRecords = reader.ReadUInt32();

            var variationSelectors = new Dictionary<int, VariationSelector>();
            int[] varSelectors = new int[numVarSelectorRecords];
            uint[] defaultUVSOffsets = new uint[numVarSelectorRecords];
            uint[] nonDefaultUVSOffsets = new uint[numVarSelectorRecords];
            for (int i = 0; i < numVarSelectorRecords; ++i)
            {
                varSelectors[i] = Utils.ReadUInt24(reader);
                defaultUVSOffsets[i] = reader.ReadUInt32();
                nonDefaultUVSOffsets[i] = reader.ReadUInt32();
            }


            for (int i = 0; i < numVarSelectorRecords; ++i)
            {
                var sel = new VariationSelector();

                if (defaultUVSOffsets[i] != 0)
                {
                    // Default UVS table
                    //
                    // A Default UVS Table is simply a range-compressed list of Unicode scalar
                    // values, representing the base characters of the default UVSes which use
                    // the ‘varSelector’ of the associated Variation Selector Record.
                    //
                    // DefaultUVS Table:
                    // Type          Name                           Description
                    // uint32        numUnicodeValueRanges          Number of Unicode character ranges.
                    // UnicodeRange  ranges[numUnicodeValueRanges]  Array of UnicodeRange records.
                    //
                    // Each Unicode range record specifies a contiguous range of Unicode values.
                    //
                    // UnicodeRange Record:
                    // Type    Name               Description
                    // uint24  startUnicodeValue  First value in this range
                    // uint8   additionalCount    Number of additional values in this range
                    //
                    // For example, the range U+4E4D&endash; U+4E4F (3 values) will set
                    // ‘startUnicodeValue’ to 0x004E4D and ‘additionalCount’ to 2. A singleton
                    // range will set ‘additionalCount’ to 0.
                    // (‘startUnicodeValue’ + ‘additionalCount’) must not exceed 0xFFFFFF.
                    // The Unicode Value Ranges are sorted in increasing order of
                    // ‘startUnicodeValue’. The ranges must not overlap; i.e.,
                    // (‘startUnicodeValue’ + ‘additionalCount’) must be less than the
                    // ‘startUnicodeValue’ of the following range (if any).

                    reader.BaseStream.Seek(beginAt + defaultUVSOffsets[i], SeekOrigin.Begin);
                    uint numUnicodeValueRanges = reader.ReadUInt32();
                    for (int n = 0; n < numUnicodeValueRanges; ++n)
                    {
                        int startCode = (int)Utils.ReadUInt24(reader);
                        sel.DefaultStartCodes.Add(startCode);
                        sel.DefaultEndCodes.Add(startCode + reader.ReadByte());
                    }
                }

                if (nonDefaultUVSOffsets[i] != 0)
                {
                    // Non-Default UVS table
                    //
                    // A Non-Default UVS Table is a list of pairs of Unicode scalar values and
                    // glyph IDs.The Unicode values represent the base characters of all
                    // non -default UVSes which use the ‘varSelector’ of the associated Variation
                    // Selector Record, and the glyph IDs specify the glyph IDs to use for the
                    // UVSes.
                    //
                    // NonDefaultUVS Table:
                    // Type        Name                         Description
                    // uint32      numUVSMappings               Number of UVS Mappings that follow
                    // UVSMapping  uvsMappings[numUVSMappings]  Array of UVSMapping records.
                    //
                    // Each UVSMapping record provides a glyph ID mapping for one base Unicode
                    // character, when that base character is used in a variation sequence with
                    // the current variation selector.
                    //
                    // UVSMapping Record:
                    // Type    Name          Description
                    // uint24  unicodeValue  Base Unicode value of the UVS
                    // uint16  glyphID       Glyph ID of the UVS
                    //
                    // The UVS Mappings are sorted in increasing order of ‘unicodeValue’. No two
                    // mappings in this table may have the same ‘unicodeValue’ values.

                    reader.BaseStream.Seek(beginAt + nonDefaultUVSOffsets[i], SeekOrigin.Begin);
                    uint numUVSMappings = reader.ReadUInt32();
                    for (int n = 0; n < numUVSMappings; ++n)
                    {
                        int unicodeValue = (int)Utils.ReadUInt24(reader);
                        ushort glyphID = reader.ReadUInt16();
                        sel.UVSMappings.Add(unicodeValue, glyphID);
                    }
                }

                variationSelectors.Add(varSelectors[i], sel);
            }

            return new CharMapFormat14 { _variationSelectors = variationSelectors };
        }

        private class VariationSelector
        {
            public List<int> DefaultStartCodes = new List<int>();
            public List<int> DefaultEndCodes = new List<int>();
            public Dictionary<int, ushort> UVSMappings = new Dictionary<int, ushort>();
        }

        private Dictionary<int, VariationSelector> _variationSelectors;
    }

    /// <summary>
    /// An empty character map that maps all characters to glyph 0
    /// </summary>
    class NullCharMap : CharacterMap
    {
        public override ushort Format { get { return 0; } }

        protected override ushort RawCharacterToGlyphIndex(int character) { return 0; }
    }

    abstract class CharacterMap
    {
        //https://www.microsoft.com/typography/otspec/cmap.htm

        public abstract ushort Format { get; }
        public ushort PlatformId { get; set; }
        public ushort EncodingId { get; set; }
        public ushort CharacterToGlyphIndex(int codepoint)
        {
            return RawCharacterToGlyphIndex(codepoint);
        }

        protected abstract ushort RawCharacterToGlyphIndex(int codepoint);

        //public void CollectGlyphIndexListFromSampleChar(char starAt, char endAt, GlyphIndexCollector collector)
        //{
        //    // TODO: Fast segment lookup using bit operations?
        //    switch (this._cmapFormat)
        //    {
        //        default: throw new NotSupportedException();
        //        case 4:
        //            {
        //                for (int i = 0; i < _segCount; i++)
        //                { 
        //                    if (_endCode[i] >= sampleChar && _startCode[i] <= sampleChar)
        //                    {

        //                        //found on this range *** 
        //                        if (_idRangeOffset[i] == 0)
        //                        {
        //                            //add entire range
        //                            if (!collector.HasRegisterSegment(i))
        //                            {

        //                                List<ushort> glyphIndexList = new List<ushort>();
        //                                char beginAt = (char)_startCode[i];
        //                                char endAt = (char)_endCode[i];
        //                                int delta = _idDelta[i];
        //                                for (char m = beginAt; m <= endAt; ++m)
        //                                {
        //                                    glyphIndexList.Add((ushort)((m + delta) % 65536));
        //                                }
        //                                collector.RegisterGlyphRangeIndex(i, glyphIndexList);
        //                            }
        //                            return;
        //                        }
        //                        else
        //                        {
        //                            //If the idRangeOffset value for the segment is not 0,
        //                            //the mapping of character codes relies on glyphIdArray. 
        //                            //The character code offset from startCode is added to the idRangeOffset value.
        //                            //This sum is used as an offset from the current location within idRangeOffset itself to index out the correct glyphIdArray value. 
        //                            //This obscure indexing trick works because glyphIdArray immediately follows idRangeOffset in the font file.
        //                            //The C expression that yields the glyph index is:

        //                            //*(idRangeOffset[i]/2 
        //                            //+ (c - startCount[i]) 
        //                            //+ &idRangeOffset[i])

        //                            if (!collector.HasRegisterSegment(i))
        //                            {
        //                                List<ushort> glyphIndexList = new List<ushort>();
        //                                char beginAt = (char)_startCode[i];
        //                                char endAt = (char)_endCode[i];
        //                                for (char m = beginAt; m <= endAt; ++m)
        //                                {
        //                                    var offset = _idRangeOffset[i] / 2 + (m - _startCode[i]);
        //                                    // I want to thank Microsoft for this clever pointer trick
        //                                    // TODO: What if the value fetched is inside the _idRangeOffset table?
        //                                    // TODO: e.g. (offset - _idRangeOffset.Length + i < 0)
        //                                    glyphIndexList.Add(_glyphIdArray[offset - _idRangeOffset.Length + i]);
        //                                }
        //                                collector.RegisterGlyphRangeIndex(i, glyphIndexList);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            break;
        //        case 6:
        //            {
        //                //The firstCode and entryCount values specify a subrange (beginning at firstCode, length = entryCount)
        //                //within the range of possible character codes.
        //                //Codes outside of this subrange are mapped to glyph index 0.
        //                //The offset of the code (from the first code) within this subrange is used as index to the glyphIdArray,
        //                //which provides the glyph index value. 
        //                if (sampleChar >= _fmt6_start && sampleChar < _fmt6_end)
        //                {
        //                    //in range            
        //                    if (!collector.HasRegisterSegment(0))
        //                    {
        //                        List<ushort> glyphIndexList = new List<ushort>();
        //                        for (ushort m = _fmt6_start; m < _fmt6_end; ++m)
        //                        {
        //                            glyphIndexList.Add((ushort)(m - _fmt6_start));
        //                        }
        //                        collector.RegisterGlyphRangeIndex(0, glyphIndexList);
        //                    }
        //                }
        //            }
        //            break;
        //    }
        //}
    }



    public class GlyphIndexCollector
    {
        Dictionary<int, List<ushort>> registerSegments = new Dictionary<int, List<ushort>>();
        public bool HasRegisterSegment(int segmentNumber)
        {
            return registerSegments.ContainsKey(segmentNumber);
        }
        public void RegisterGlyphRangeIndex(int segmentNumber, List<ushort> glyphIndexList)
        {
            registerSegments.Add(segmentNumber, glyphIndexList);
        }
        public IEnumerable<ushort> GetGlyphIndexIter()
        {
            foreach (List<ushort> list in registerSegments.Values)
            {
                int j = list.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return list[i];
                }
            }
        }
    }
}
