//Apache2, 2017, WinterDev, Sam Hocevar
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;

namespace Typography.OpenFont
{
    class CharMapFormat4 : CharacterMap
    {
        readonly int _segCount;
        readonly ushort[] _startCode; //Starting character code for each segment
        readonly ushort[] _endCode;//Ending character code for each segment, last = 0xFFFF.      
        readonly ushort[] _idDelta; //Delta for all character codes in segment
        readonly ushort[] _idRangeOffset; //Offset in bytes to glyph indexArray, or 0 (not offset in bytes unit)
        readonly ushort[] _glyphIdArray;
        public CharMapFormat4(int segCount, ushort[] startCode, ushort[] endCode, ushort[] idDelta, ushort[] idRangeOffset, ushort[] glyphIdArray)
        {
            this.Format = 4;
            _segCount = segCount;
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
        uint[] startCharCodes, endCharCodes, startGlyphIds;
        internal CharMapFormat12(uint[] startCharCodes, uint[] endCharCodes, uint[] startGlyphIds)
        {
            this.Format = 12;
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
        //
        ushort _fmt6_start;
        ushort _fmt6_end;
        ushort[] _glyphIdArray;
        internal CharMapFormat6(ushort startCode, ushort[] glyphIdArray)
        {
            Format = 6;
            _glyphIdArray = glyphIdArray;
            this._fmt6_end = (ushort)(startCode + glyphIdArray.Length);
            this._fmt6_start = startCode;
        }
        protected override ushort RawCharacterToGlyphIndex(int codepoint)
        {
            //The firstCode and entryCount values specify a subrange (beginning at firstCode, length = entryCount)
            //within the range of possible character codes.
            //Codes outside of this subrange are mapped to glyph index 0.
            //The offset of the code (from the first code) within this subrange is used as index to the glyphIdArray,
            //which provides the glyph index value. 
            if (codepoint >= _fmt6_start && codepoint <= _fmt6_end)
            {
                //in range                            
                return _glyphIdArray[codepoint - _fmt6_start];
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// An empty character map that maps all characters to glyph 0
    /// </summary>
    class NullCharMap : CharacterMap
    {
        protected override ushort RawCharacterToGlyphIndex(int character) { return 0; }
    }

    abstract class CharacterMap
    {
        //https://www.microsoft.com/typography/otspec/cmap.htm


        public ushort Format { get; protected set; }
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
        //                if (sampleChar >= _fmt6_start && sampleChar <= _fmt6_end)
        //                {
        //                    //in range            
        //                    if (!collector.HasRegisterSegment(0))
        //                    {
        //                        List<ushort> glyphIndexList = new List<ushort>();
        //                        for (ushort m = _fmt6_start; m <= _fmt6_end; ++m)
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
