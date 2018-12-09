//MIT, 2016-present, WinterDev
using System.Collections.Generic;
using Typography.OpenFont.Tables;
namespace Typography.TextLayout
{

    /// <summary>
    /// impl replaceable glyph index list
    /// </summary>
    class GlyphIndexList : IGlyphIndexList
    {
        List<ushort> _glyphIndices = new List<ushort>();
        List<int> _inputCodePointIndexList = new List<int>();
        ushort _originalCodePointOffset = 0;
        List<GlyphIndexToUserCodePoint> _mapGlyphIndexToUserCodePoint = new List<GlyphIndexToUserCodePoint>();

        /// <summary>
        /// map from glyph index to original user char
        /// </summary>
        struct GlyphIndexToUserCodePoint
        {
            /// <summary>
            /// offset from start layout char
            /// </summary>
            public readonly ushort o_codepoint_charOffset;
            public readonly ushort len;
#if DEBUG
            public ushort dbug_glyphIndex;
#endif
            public GlyphIndexToUserCodePoint(ushort o_user_charOffset, ushort len)
            {
                this.len = len;
                this.o_codepoint_charOffset = o_user_charOffset;
#if DEBUG
                this.dbug_glyphIndex = 0;
#endif
            }
#if DEBUG
            public override string ToString()
            {
                return "codepoint_offset: " + o_codepoint_charOffset + " : len" + len;
            }
#endif
        }

        public void Clear()
        {
            _glyphIndices.Clear();
            _originalCodePointOffset = 0;
            _inputCodePointIndexList.Clear();
            _mapGlyphIndexToUserCodePoint.Clear();

        }
        /// <summary>
        ///  add codepoint index and its glyph index
        /// </summary>
        /// <param name="codePointIndex">index to codepoint element in code point array</param>
        /// <param name="glyphIndex">map to glyphindex</param>
        public void AddGlyph(int codePointIndex, ushort glyphIndex)
        {
            //so we can monitor what substituion process

            _inputCodePointIndexList.Add(codePointIndex);
            _glyphIndices.Add(glyphIndex);

            var glyphIndexToCharMap = new GlyphIndexToUserCodePoint(_originalCodePointOffset, 1);
#if DEBUG
            glyphIndexToCharMap.dbug_glyphIndex = glyphIndex;
#endif
            _mapGlyphIndexToUserCodePoint.Add(glyphIndexToCharMap);
            _originalCodePointOffset++;
        }

        /// <summary>
        /// glyph count may be more or less than original user char list (from substitution process)
        /// </summary>
        public int Count => _glyphIndices.Count;
        //
        public ushort this[int index] => _glyphIndices[index];
        //
        public void GetGlyphIndexAndMap(int index, out ushort glyphIndex, out ushort input_codepointOffset, out ushort input_mapLen)
        {
            glyphIndex = _glyphIndices[index];
            GlyphIndexToUserCodePoint glyphIndexToUserCodePoint = _mapGlyphIndexToUserCodePoint[index];
            input_codepointOffset = glyphIndexToUserCodePoint.o_codepoint_charOffset;
            input_mapLen = glyphIndexToUserCodePoint.len;
        }

        /// <summary>
        /// remove:add_new 1:1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGlyphIndex"></param>
        public void Replace(int index, ushort newGlyphIndex)
        {
            _glyphIndices[index] = newGlyphIndex;
        }

#if DEBUG
        List<GlyphIndexToUserCodePoint> _tmpGlypIndexBackup = new List<GlyphIndexToUserCodePoint>();
#endif
        /// <summary>
        /// remove:add_new >=1:1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="removeLen"></param>
        /// <param name="newGlyphIndex"></param>
        public void Replace(int index, int removeLen, ushort newGlyphIndex)
        {
            //eg f-i ligation
            //original f glyph and i glyph are removed 
            //and then replace with a single glyph 
            _glyphIndices.RemoveRange(index, removeLen);
            _glyphIndices.Insert(index, newGlyphIndex);
            //------------------------------------------------  

            GlyphIndexToUserCodePoint firstRemove = _mapGlyphIndexToUserCodePoint[index];

#if DEBUG
            _tmpGlypIndexBackup.Clear();
            int endAt = index + removeLen;
            for (int i = index; i < endAt; ++i)
            {
                _tmpGlypIndexBackup.Add(_mapGlyphIndexToUserCodePoint[i]);
            }
            _tmpGlypIndexBackup.Clear();
#endif
            //TODO: check if removeLen > ushort.Max
            GlyphIndexToUserCodePoint newMap = new GlyphIndexToUserCodePoint(firstRemove.o_codepoint_charOffset, (ushort)removeLen);
#if DEBUG
            newMap.dbug_glyphIndex = newGlyphIndex;
#endif

            //------------------------------------------------ 
            _mapGlyphIndexToUserCodePoint.RemoveRange(index, removeLen);
            _mapGlyphIndexToUserCodePoint.Insert(index, newMap);

        }
        /// <summary>
        /// remove: add_new 1:>=1
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newGlyphIndices"></param>
        public void Replace(int index, ushort[] newGlyphIndices)
        {
            _glyphIndices.RemoveAt(index);
            _glyphIndices.InsertRange(index, newGlyphIndices);
            GlyphIndexToUserCodePoint cur = _mapGlyphIndexToUserCodePoint[index];
            _mapGlyphIndexToUserCodePoint.RemoveAt(index);
            //insert 
            int j = newGlyphIndices.Length;
            for (int i = 0; i < j; ++i)
            {
                var newglyph = new GlyphIndexToUserCodePoint(cur.o_codepoint_charOffset, 1);
#if DEBUG
                newglyph.dbug_glyphIndex = newGlyphIndices[i];
#endif
                //may point to the same user char                 
                _mapGlyphIndexToUserCodePoint.Insert(index, newglyph);
            }
        }


        public void CreateMapFromUserCodePointToGlyphIndices(List<UserCodePointToGlyphIndex> mapUserCodePointToGlyphIndex)
        {
            //(optional)
            //this method should be called after we finish the substitution process 
            //--------------------------------------
            int codePointCount = _inputCodePointIndexList.Count;
            for (int i = 0; i < codePointCount; ++i)
            {
                //
                var codePointToGlyphIndexMap = new UserCodePointToGlyphIndex();
                //set index that point to original codePointIndex
                codePointToGlyphIndexMap.userCodePointIndex = _inputCodePointIndexList[i];
                //
                mapUserCodePointToGlyphIndex.Add(codePointToGlyphIndexMap);
            }
            //--------------------------------------
            //then fill the user-codepoint with glyph information information 

            int glyphIndexCount = _glyphIndices.Count;
            for (int i = 0; i < glyphIndexCount; ++i)
            {
                GlyphIndexToUserCodePoint glyphIndexToUserCodePoint = _mapGlyphIndexToUserCodePoint[i];
                //
                UserCodePointToGlyphIndex charToGlyphIndexMap = mapUserCodePointToGlyphIndex[glyphIndexToUserCodePoint.o_codepoint_charOffset];
                charToGlyphIndexMap.AppendData((ushort)(i + 1), (glyphIndexToUserCodePoint.len));
                //replace with the changed value
                mapUserCodePointToGlyphIndex[glyphIndexToUserCodePoint.o_codepoint_charOffset] = charToGlyphIndexMap;
            }

        }

    }

}
