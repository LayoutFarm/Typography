//MIT, 2016-present, WinterDev 
namespace Typography.TextLayout
{

    public struct UserCodePointToGlyphIndex
    {
        //from user codepoint index to offset in _glyphIndics     
        //this index is 1-based ***
        //if glyphIndexListOffset_1==0 then no map data in  _glyphIndices*** 
        public ushort glyphIndexListOffset_plus1;
        public ushort len;

        internal int userCodePointIndex;
#if DEBUG

        public override string ToString()
        {
            return glyphIndexListOffset_plus1 + ":" + len;
        }
#endif
        internal void AppendData(ushort glyphIndexListOffset_plus1, ushort len)
        {

#if DEBUG
            if (len != 1)
            {

            }
#endif
            if (this.glyphIndexListOffset_plus1 != 0)
            {
                //extend ***
                //some user char may be represented by >1 glyphs
                if (this.glyphIndexListOffset_plus1 + 1 == glyphIndexListOffset_plus1)
                {
                    //ok
                    if (len == 1)
                    {
                        this.len += 1;
                        return; //***
                    }
                    else
                    {
                        throw new System.NotSupportedException();
                    }
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }

            this.glyphIndexListOffset_plus1 = glyphIndexListOffset_plus1;
            this.len = len;
        }
    }
}