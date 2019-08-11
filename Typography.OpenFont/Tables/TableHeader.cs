//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

namespace Typography.OpenFont.Tables
{
    class TableHeader
    {
        readonly uint _tag;
        readonly uint _checkSum;
        readonly uint _offset;
        readonly uint _length;
        readonly string _tagName;

        public TableHeader(uint tag, uint checkSum, uint offset, uint len)
        {
            _tag = tag;
            _checkSum = checkSum;
            _offset = offset;
            _length = len;
            _tagName = Utils.TagToString(_tag);
        }
        public TableHeader(string tag, uint checkSum, uint offset, uint len)
        {
            _tag = 0;
            _checkSum = checkSum;
            _offset = offset;
            _length = len;
            _tagName = tag;
        }
        //
        public string Tag => _tagName;


        public uint Offset => _offset;
        public uint CheckSum => _checkSum;
        public uint Length => _length;

        public override string ToString()
        {
            return "{" + Tag + "}";
        }
    }
}
