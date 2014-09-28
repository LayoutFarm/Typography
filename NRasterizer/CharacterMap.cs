using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer
{
    class CharacterMap
    {
        private readonly int _segCount;
        private readonly ushort[] _startCode;
        private readonly ushort[] _endCode;
        private readonly ushort[] _idDelta;
        private readonly ushort[] _idRangeOffset;
        private readonly ushort[] _glyphIdArray;

        internal CharacterMap(int segCount, ushort[] startCode, ushort[] endCode, ushort[] idDelta, ushort[] idRangeOffset, ushort[] glyphIdArray)
        {
            _segCount = segCount;        
            _startCode = startCode;
            _endCode = endCode;
            _idDelta = idDelta;
            _idRangeOffset = idRangeOffset;
            _glyphIdArray = glyphIdArray;
        }

        public int CharacterToGlyphIndex(UInt32 character)
        {
            return (int)RawCharacterToGlyphIndex(character);
        }

        public uint RawCharacterToGlyphIndex(UInt32 character)
        {
            // TODO: Fast fegment lookup using bit operations?
            for (int i = 0; i < _segCount; i++)
            {
                if (_endCode[i] >= character && _startCode[i] <= character)
                {
                    if (_idRangeOffset[i] == 0)
                    {
                        return (character + _idDelta[i]) % 65536; // TODO: bitmask instead?
                    }
                    else
                    {
                        var offset = _idRangeOffset[i] / 2 + (character - _startCode[i]);

                        // I want to thank Microsoft for this clever pointer trick
                        // TODO: What if the value fetched is inside the _idRangeOffset table?
                        // TODO: e.g. (offset - _idRangeOffset.Length + i < 0)
                        return _glyphIdArray[offset - _idRangeOffset.Length + i];
                    }
                }
            }
            return 0;
        }
    }
}
