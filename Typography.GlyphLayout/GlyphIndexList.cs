//MIT, 2016-2017, WinterDev
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
        List<int> _originalChars = new List<int>();
        ushort _originalOffset = 0;
        List<GlyphIndexToUserChar> _mapGlyphIndexToUserChar = new List<GlyphIndexToUserChar>();
        internal List<UserCharToGlyphIndexMap> _mapUserCharToGlyphIndices = new List<UserCharToGlyphIndexMap>();

        /// <summary>
        /// map from glyph index to original user char
        /// </summary>
        struct GlyphIndexToUserChar
        {
            /// <summary>
            /// offset from start layout char
            /// </summary>
            public readonly ushort o_user_charOffset;
            public readonly ushort len;
#if DEBUG
            public ushort dbug_glyphIndex;
#endif
            public GlyphIndexToUserChar(ushort o_user_charOffset, ushort len)
            {
                this.len = len;
                this.o_user_charOffset = o_user_charOffset;
#if DEBUG
                this.dbug_glyphIndex = 0;
#endif
            }
#if DEBUG
            public override string ToString()
            {
                return "char_offset: " + o_user_charOffset + " : len" + len;
            }
#endif
        }

        public void Clear()
        {
            _glyphIndices.Clear();
            _originalOffset = 0;
            _originalChars.Clear();

            _mapGlyphIndexToUserChar.Clear();
            _mapUserCharToGlyphIndices.Clear();
        }
        /// <summary>
        /// add original char and its glyph index
        /// </summary>
        /// <param name="glyphIndex"></param>
        public void AddGlyph(int originalChar, ushort glyphIndex)
        {
            _glyphIndices.Add(glyphIndex);
            //so we can monitor what substituion process
            _originalChars.Add(originalChar);

            var glyphIndexToCharMap = new GlyphIndexToUserChar(_originalOffset, 1);
#if DEBUG
            glyphIndexToCharMap.dbug_glyphIndex = glyphIndex;
#endif
            _mapGlyphIndexToUserChar.Add(glyphIndexToCharMap);
            _originalOffset++;
        }

        /// <summary>
        /// glyph count may be more or less than original user char list (from substitution process)
        /// </summary>
        public int Count { get { return _glyphIndices.Count; } }
        public ushort this[int index]
        {
            get
            {
                return _glyphIndices[index];
            }
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
        List<GlyphIndexToUserChar> _tmpGlypIndexBackup = new List<GlyphIndexToUserChar>();
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
             
            GlyphIndexToUserChar firstRemove = _mapGlyphIndexToUserChar[index];

#if DEBUG
            _tmpGlypIndexBackup.Clear();
            int endAt = index + removeLen;
            for (int i = index; i < endAt; ++i)
            {
                _tmpGlypIndexBackup.Add(_mapGlyphIndexToUserChar[i]);
            }
            _tmpGlypIndexBackup.Clear();
#endif
            //TODO: check if removeLen > ushort.Max
            GlyphIndexToUserChar newMap = new GlyphIndexToUserChar(firstRemove.o_user_charOffset, (ushort)removeLen);
#if DEBUG
            newMap.dbug_glyphIndex = newGlyphIndex;
#endif

            //------------------------------------------------ 
            _mapGlyphIndexToUserChar.RemoveRange(index, removeLen);
            _mapGlyphIndexToUserChar.Insert(index, newMap);

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
            GlyphIndexToUserChar cur = _mapGlyphIndexToUserChar[index];
            _mapGlyphIndexToUserChar.RemoveAt(index);
            //insert 
            int j = newGlyphIndices.Length;
            for (int i = 0; i < j; ++i)
            {
                var newglyph = new GlyphIndexToUserChar(cur.o_user_charOffset, 1);
#if DEBUG
                newglyph.dbug_glyphIndex = newGlyphIndices[i];
#endif
                //can point to the same user char                 
                _mapGlyphIndexToUserChar.Insert(index, newglyph);
            }
        }


        public void CreateMapFromUserCharToGlyphIndices()
        {
            //(optional)
            //this method should be called after we finish the substitution process
            _mapUserCharToGlyphIndices.Clear();
            //--------------------------------------
            int userCharCount = _originalChars.Count;

            for (int i = 0; i < userCharCount; ++i)
            {
                var charToGlyphMap = new UserCharToGlyphIndexMap();
#if DEBUG
                charToGlyphMap.dbug_userCharIndex = (ushort)i;
                charToGlyphMap.dbug_userChar = _originalChars[i];
#endif
                _mapUserCharToGlyphIndices.Add(charToGlyphMap);
            }
            //--------------------------------------
            //then fill with glyphindex to user char information 

            int glyphIndexCount = _glyphIndices.Count;
            for (int i = 0; i < glyphIndexCount; ++i)
            {
                GlyphIndexToUserChar glyphIndexToUserChar = _mapGlyphIndexToUserChar[i];
                //
                UserCharToGlyphIndexMap charToGlyphIndexMap = _mapUserCharToGlyphIndices[glyphIndexToUserChar.o_user_charOffset];
                charToGlyphIndexMap.AppendData((ushort)(i + 1), (glyphIndexToUserChar.len));
                //replace with the changed value
                _mapUserCharToGlyphIndices[glyphIndexToUserChar.o_user_charOffset] = charToGlyphIndexMap;

            }

        }

    }

}
