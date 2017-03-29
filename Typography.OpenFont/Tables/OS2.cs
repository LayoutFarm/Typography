//Apache2, 2016-2017, WinterDev 
using System.IO;

namespace Typography.OpenFont.Tables
{

    //https://www.microsoft.com/typography/otspec/os2.htm
    /// <summary>
    /// OS2 and Windows matrics
    /// </summary>
    class OS2Table : TableEntry
    {
        //USHORT 	version 	0x0005
        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        public ushort version;
        public short xAvgCharWidth;
        public ushort usWeightClass;
        public ushort usWidthClass;
        public ushort fsType;
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        public short ySubscriptXSize;
        public short ySubscriptYSize;
        public short ySubscriptXOffset;
        public short ySubscriptYOffset;
        public short ySuperscriptXSize;
        public short ySuperscriptYSize;
        public short ySuperscriptXOffset;
        public short ySuperscriptYOffset;
        public short yStrikeoutSize;
        public short yStrikeoutPosition;
        public short sFamilyClass;

        //BYTE 	panose[10] 	 
        public byte[] panose;
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        public uint ulUnicodeRange1;
        public uint ulUnicodeRange2;
        public uint ulUnicodeRange3;
        public uint ulUnicodeRange4;

        //CHAR 	achVendID[4] 	 
        public uint achVendID;
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 
        public ushort fsSelection;
        public ushort usFirstCharIndex;
        public ushort usLastCharIndex;
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        public short sTypoAscender;
        public short sTypoDescender;
        public short sTypoLineGap;
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        public ushort usWinAscent;
        public ushort usWinDescent;
        public uint ulCodePageRange1;
        public uint ulCodePageRange2;
        //SHORT 	sxHeight 	 
        //SHORT 	sCapHeight 	 
        //USHORT 	usDefaultChar 	 
        //USHORT 	usBreakChar 	 
        //USHORT 	usMaxContext 	 
        //USHORT 	usLowerOpticalPointSize 	 
        //USHORT 	usUpperOpticalPointSize 	 
        public short sxHeight;
        public short sCapHeight;
        public ushort usDefaultChar;
        public ushort usBreakChar;
        public ushort usMaxContext;
        public ushort usLowerOpticalPointSize;
        public ushort usUpperOpticalPointSize;

        public override string Name
        {
            get { return "OS/2"; }
        }
#if DEBUG
        public override string ToString()
        {
            return version + "," + Utils.TagToString(this.achVendID);
        }
#endif
        protected override void ReadContentFrom(BinaryReader reader)
        {
            //USHORT 	version 	0x0005
            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	  
            switch (this.version = reader.ReadUInt16())
            {
                default: throw new System.NotSupportedException();
                case 0:
                    ReadVersion0(reader);
                    break;
                case 1:
                    ReadVersion1(reader);
                    break;
                case 2:
                    ReadVersion2(reader);
                    break;
                case 3:
                    ReadVersion3(reader);
                    break;
                case 4:
                    ReadVersion4(reader);
                    break;
                case 5:
                    ReadVersion5(reader);
                    break;
            }
        }
        void ReadVersion0(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/os2ver0.htm
            //USHORT 	version 	0x0000
            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	 
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();

            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();
            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulCharRange[4] 	Bits 0-31
            this.ulUnicodeRange1 = reader.ReadUInt32();
            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 	 
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 	 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
        }

        void ReadVersion1(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/os2ver1.htm

            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	 
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();
            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();

            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulUnicodeRange1 	Bits 0-31
            //ULONG 	ulUnicodeRange2 	Bits 32-63
            //ULONG 	ulUnicodeRange3 	Bits 64-95
            //ULONG 	ulUnicodeRange4 	Bits 96-127
            this.ulUnicodeRange1 = reader.ReadUInt32();
            this.ulUnicodeRange2 = reader.ReadUInt32();
            this.ulUnicodeRange3 = reader.ReadUInt32();
            this.ulUnicodeRange4 = reader.ReadUInt32();
            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 	
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 	 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent 	 
            //ULONG 	ulCodePageRange1 	Bits 0-31
            //ULONG 	ulCodePageRange2 	Bits 32-63
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
            this.ulCodePageRange1 = reader.ReadUInt32();
            this.ulCodePageRange2 = reader.ReadUInt32();
        }
        void ReadVersion2(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/os2ver2.htm

            // 
            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	 
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();
            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();
            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulUnicodeRange1 	Bits 0-31
            //ULONG 	ulUnicodeRange2 	Bits 32-63
            //ULONG 	ulUnicodeRange3 	Bits 64-95
            //ULONG 	ulUnicodeRange4 	Bits 96-127
            this.ulUnicodeRange1 = reader.ReadUInt32();
            this.ulUnicodeRange2 = reader.ReadUInt32();
            this.ulUnicodeRange3 = reader.ReadUInt32();
            this.ulUnicodeRange4 = reader.ReadUInt32();
            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 	 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent 	 
            //ULONG 	ulCodePageRange1 	Bits 0-31
            //ULONG 	ulCodePageRange2 	Bits 32-63
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
            this.ulCodePageRange1 = reader.ReadUInt32();
            this.ulCodePageRange2 = reader.ReadUInt32();
            //SHORT 	sxHeight 	 
            //SHORT 	sCapHeight 	 
            //USHORT 	usDefaultChar 	 
            //USHORT 	usBreakChar 	 
            //USHORT 	usMaxContext
            this.sxHeight = reader.ReadInt16();
            this.sCapHeight = reader.ReadInt16();
            this.usDefaultChar = reader.ReadUInt16();
            this.usBreakChar = reader.ReadUInt16();
            this.usMaxContext = reader.ReadUInt16();
        }
        void ReadVersion3(BinaryReader reader)
        {

            //https://www.microsoft.com/typography/otspec/os2ver3.htm
            //            USHORT 	version 	0x0003
            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	 
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();
            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();
            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulUnicodeRange1 	Bits 0-31
            //ULONG 	ulUnicodeRange2 	Bits 32-63
            //ULONG 	ulUnicodeRange3 	Bits 64-95
            //ULONG 	ulUnicodeRange4 	Bits 96-127
            this.ulUnicodeRange1 = reader.ReadUInt32();
            this.ulUnicodeRange2 = reader.ReadUInt32();
            this.ulUnicodeRange3 = reader.ReadUInt32();
            this.ulUnicodeRange4 = reader.ReadUInt32();
            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 	 
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent 	 
            //ULONG 	ulCodePageRange1 	Bits 0-31
            //ULONG 	ulCodePageRange2 	Bits 32-63
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
            this.ulCodePageRange1 = reader.ReadUInt32();
            this.ulCodePageRange2 = reader.ReadUInt32();
            //SHORT 	sxHeight 	 
            //SHORT 	sCapHeight 	 
            //USHORT 	usDefaultChar 	 
            //USHORT 	usBreakChar 	 
            //USHORT 	usMaxContext
            this.sxHeight = reader.ReadInt16();
            this.sCapHeight = reader.ReadInt16();
            this.usDefaultChar = reader.ReadUInt16();
            this.usBreakChar = reader.ReadUInt16();
            this.usMaxContext = reader.ReadUInt16();
        }
        void ReadVersion4(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/os2ver4.htm

            //SHORT 	xAvgCharWidth 	 
            //USHORT 	usWeightClass 	 
            //USHORT 	usWidthClass 	 
            //USHORT 	fsType 	 
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();
            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();
            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulUnicodeRange1 	Bits 0-31
            //ULONG 	ulUnicodeRange2 	Bits 32-63
            //ULONG 	ulUnicodeRange3 	Bits 64-95
            //ULONG 	ulUnicodeRange4 	Bits 96-127
            this.ulUnicodeRange1 = reader.ReadUInt32();
            this.ulUnicodeRange2 = reader.ReadUInt32();
            this.ulUnicodeRange3 = reader.ReadUInt32();
            this.ulUnicodeRange4 = reader.ReadUInt32();
            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 	 
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 	 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent 	 
            //ULONG 	ulCodePageRange1 	Bits 0-31
            //ULONG 	ulCodePageRange2 	Bits 32-63
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
            this.ulCodePageRange1 = reader.ReadUInt32();
            this.ulCodePageRange2 = reader.ReadUInt32();
            //SHORT 	sxHeight 	 
            //SHORT 	sCapHeight 	 
            //USHORT 	usDefaultChar 	 
            //USHORT 	usBreakChar 	 
            //USHORT 	usMaxContext
            this.sxHeight = reader.ReadInt16();
            this.sCapHeight = reader.ReadInt16();
            this.usDefaultChar = reader.ReadUInt16();
            this.usBreakChar = reader.ReadUInt16();
            this.usMaxContext = reader.ReadUInt16();
        }

        void ReadVersion5(BinaryReader reader)
        {
            this.xAvgCharWidth = reader.ReadInt16();
            this.usWeightClass = reader.ReadUInt16();
            this.usWidthClass = reader.ReadUInt16();
            this.fsType = reader.ReadUInt16();
            //SHORT 	ySubscriptXSize 	 
            //SHORT 	ySubscriptYSize 	 
            //SHORT 	ySubscriptXOffset 	 
            //SHORT 	ySubscriptYOffset 	 
            //SHORT 	ySuperscriptXSize 	 
            //SHORT 	ySuperscriptYSize 	 
            //SHORT 	ySuperscriptXOffset 	 
            //SHORT 	ySuperscriptYOffset 	 
            //SHORT 	yStrikeoutSize 	 
            //SHORT 	yStrikeoutPosition 	 
            //SHORT 	sFamilyClass 	 
            this.ySubscriptXSize = reader.ReadInt16();
            this.ySubscriptYSize = reader.ReadInt16();
            this.ySubscriptXOffset = reader.ReadInt16();
            this.ySubscriptYOffset = reader.ReadInt16();
            this.ySuperscriptXSize = reader.ReadInt16();
            this.ySuperscriptYSize = reader.ReadInt16();
            this.ySuperscriptXOffset = reader.ReadInt16();
            this.ySuperscriptYOffset = reader.ReadInt16();
            this.yStrikeoutSize = reader.ReadInt16();
            this.yStrikeoutPosition = reader.ReadInt16();
            this.sFamilyClass = reader.ReadInt16();

            //BYTE 	panose[10] 	 
            this.panose = reader.ReadBytes(10);
            //ULONG 	ulUnicodeRange1 	Bits 0-31
            //ULONG 	ulUnicodeRange2 	Bits 32-63
            //ULONG 	ulUnicodeRange3 	Bits 64-95
            //ULONG 	ulUnicodeRange4 	Bits 96-127
            this.ulUnicodeRange1 = reader.ReadUInt32();
            this.ulUnicodeRange2 = reader.ReadUInt32();
            this.ulUnicodeRange3 = reader.ReadUInt32();
            this.ulUnicodeRange4 = reader.ReadUInt32();

            //CHAR 	achVendID[4] 	 
            this.achVendID = reader.ReadUInt32();
            //USHORT 	fsSelection 	 
            //USHORT 	usFirstCharIndex 	 
            //USHORT 	usLastCharIndex 
            this.fsSelection = reader.ReadUInt16();
            this.usFirstCharIndex = reader.ReadUInt16();
            this.usLastCharIndex = reader.ReadUInt16();
            //SHORT 	sTypoAscender 	 
            //SHORT 	sTypoDescender 	 
            //SHORT 	sTypoLineGap 	 
            this.sTypoAscender = reader.ReadInt16();
            this.sTypoDescender = reader.ReadInt16();
            this.sTypoLineGap = reader.ReadInt16();
            //USHORT 	usWinAscent 	 
            //USHORT 	usWinDescent 	 
            //ULONG 	ulCodePageRange1 	Bits 0-31
            //ULONG 	ulCodePageRange2 	Bits 32-63
            this.usWinAscent = reader.ReadUInt16();
            this.usWinDescent = reader.ReadUInt16();
            this.ulCodePageRange1 = reader.ReadUInt32();
            this.ulCodePageRange2 = reader.ReadUInt32();
            //SHORT 	sxHeight 	 
            //SHORT 	sCapHeight 	 
            //USHORT 	usDefaultChar 	 
            //USHORT 	usBreakChar 	 
            //USHORT 	usMaxContext 	 
            this.sxHeight = reader.ReadInt16();
            this.sCapHeight = reader.ReadInt16();
            this.usDefaultChar = reader.ReadUInt16();
            this.usBreakChar = reader.ReadUInt16();
            this.usMaxContext = reader.ReadUInt16();
            //USHORT 	usLowerOpticalPointSize 	 
            //USHORT 	usUpperOpticalPointSize 	 

            this.usLowerOpticalPointSize = reader.ReadUInt16();
            this.usUpperOpticalPointSize = reader.ReadUInt16();
        }
    }


    //unicode range 
    public struct UnicodeRangeInfo
    {
        public readonly int BitNo;
        public readonly int StartAt;
        public readonly int EndAt;
        public UnicodeRangeInfo(int bitNo, int startAt, int endAt)
        {
            BitNo = bitNo;
            StartAt = startAt;
            EndAt = endAt;
        }
#if DEBUG
        public override string ToString()
        {
            return BitNo + ",[" + StartAt + "," + EndAt + "]";
        }
#endif
    }

    public enum UnicodeLangBits : long
    {
        //Bit 	Unicode Range 	Block range
        //0 	Basic Latin 	0000-007F
        //1 	Latin-1 Supplement 	0080-00FF
        //2 	Latin Extended-A 	0100-017F
        //3 	Latin Extended-B 	0180-024F
        BasicLatin = (0 << 32) | (0x0000 << 16) | 0x007F,
        Latin1Supplement = (1 << 32) | (0x0080 << 16) | 0x00FF,
        LatinExtenedA = (2 << 32) | (0x0100 << 16) | 0x017F,
        LatinExtenedB = (3 << 32) | (0x0180 << 16) | 0x024F,
        //4 	IPA Extensions 	0250-02AF
        //    Phonetic Extensions 	1D00-1D7F
        //    Phonetic Extensions Supplement 	1D80-1DBF
        IPAExtensions = (4 << 32) | (0x0250 << 16) | 0x02AF,
        PhoneticExtensions = (4 << 32) | (0x1D00 << 16) | 0x1D7F,
        PhoneticExtensionsSupplement = (4 << 32) | (0x1D80 << 16) | 0x1DBF,
        //5 	Spacing Modifier Letters 	02B0-02FF
        //    Modifier Tone Letters 	A700-A71F
        SpacingModifierLetters = (5 << 32) | (0x02B0 << 16) | 0x02FF,
        ModifierToneLetters = (5 << 32) | (0xA700 << 16) | 0xA71F,
        //6 	Combining Diacritical Marks 	0300-036F
        //    Combining Diacritical Marks Supplement 	1DC0-1DFF
        CombiningDiacriticalMarks = (6 << 32) | (0x0300 << 16) | 0x036F,
        CombiningDiacriticalMarksSupplement = (6 << 32) | (0x1DC0 << 16) | 0x1DFF,
        //7 	Greek and Coptic 	0370-03FF
        GreekAndCoptic = (7 << 32) | (0x0370 << 16) | 0x03FF,
        //8 	Coptic 	2C80-2C80
        Coptic = (8 << 32) | (0x2C80 << 16) | 0x2C80,
        //9 	Cyrillic 	0400-04FF
        //    Cyrillic Supplement 	0500-052F
        //    Cyrillic Extended-A 	2DE0-2DFF
        //    Cyrillic Extended-B 	A640-A69F
        Cyrillic = (9 << 32) | (0x0400 << 16) | 0x04FF,
        CyrillicExtendedA = (9 << 32) | (0x2DE0 << 16) | 0x2DFF,
        CyrillicExtendedB = (9 << 32) | (0xA640 << 16) | 0xA69F,
        //10 	Armenian 	0530-058F
        Armenian = (10 << 32) | (0x0530 << 16) | 0x058F,
        //11 	Hebrew 	0590-05FF
        Hebrew = (11 << 32) | (0x0590 << 16) | 0x05FF,
        //12 	Vai 	A500-A63F
        Vai = (11 << 32) | (0xA500 << 16) | 0xA63F,
        //13 	Arabic 	0600-06FF
        //    Arabic Supplement 	0750-077F
        Arabic = (13 << 32) | (0x0600 << 16) | 0x06FF,
        ArabicSupplement = (13 << 32) | (0x0750 << 16) | 0x077F,
        //14 	NKo 	07C0-07FF
        NKo = (14 << 32) | (0x07C0 << 16) | 0x07FF,
        //15 	Devanagari 	0900-097F
        Devanagari = (15 << 32) | (0x0900 << 16) | 0x097F,
        //16 	Bengali 	0980-09FF
        Bengali = (16 << 32) | (0x0980 << 16) | 0x09FF,
        //17 	Gurmukhi 	0A00-0A7F
        Gurmukhi = (17 << 32) | (0x0A00 << 16) | 0x0A7F,
        //18 	Gujarati 	0A80-0AFF
        Gujarati = (18 << 32) | (0x0A80 << 16) | 0x0AFF,
        //19 	Oriya 	0B00-0B7F
        Oriya = (19 << 32) | (0x0B00 << 16) | 0x0B7F,
        //20 	Tamil 	0B80-0BFF
        Tamil = (20 << 32) | (0x0B80 << 16) | 0x0BFF,
        //21 	Telugu 	0C00-0C7F
        Telugu = (21 << 32) | (0x0C00 << 16) | 0x0C7F,
        //22 	Kannada 	0C80-0CFF
        Kannada = (22 << 32) | (0x0C80 << 16) | 0x0CFF,
        //23 	Malayalam 	0D00-0D7F
        Malayalam = (23 << 32) | (0x0D00 << 16) | 0x0D7F,
        //24 	Thai 	0E00-0E7F
        Thai = (24 << 32) | (0x0E00 << 16) | 0x0E7F,
        //25 	Lao 	0E80-0EFF
        Lao = (25 << 32) | (0x0E80 << 16) | 0x0EFF,
        //26 	Georgian 	10A0-10FF
        //    Georgian Supplement 	2D00-2D2F
        Georgian = (26 << 32) | (0x10A0 << 16) | 0x10FF,
        GeorgianSupplement = (26 << 32) | (0x2D00 << 16) | 0x2D2F,
        //27 	Balinese 	1B00-1B7F
        Balinese = (27 << 32) | (0x1B00 << 16) | 0x1B7F,
        //28 	Hangul Jamo 	1100-11FF
        HangulJamo = (28 << 32) | (0x1100 << 16) | 0x11FF,
        //29 	Latin Extended Additional 	1E00-1EFF
        //    Latin Extended-C 	2C60-2C7F
        //    Latin Extended-D 	A720-A7FF
        LatinExtendedAdditional = (29 << 32) | (0x1E00 << 16) | 0x1EFF,
        LatinExtendedAdditionalC = (29 << 32) | (0x2C60 << 16) | 0x2C7F,
        LatinExtendedAdditionalD = (29 << 32) | (0xA720 << 16) | 0xA7FF,
        //---
        //30 	Greek Extended 	1F00-1FFF
        GreekExtended = (30 << 32) | (0x1F00 << 16) | 0x1FFF,
        //31 	General Punctuation 	2000-206F
        //    Supplemental Punctuation 	2E00-2E7F
        GeneralPunctuation = (31 << 32) | (0x2000 << 16) | 0x206F,
        SupplementPunctuation = (31 << 32) | (0x2E00 << 16) | 0x2E7F,
        //32 	Superscripts And Subscripts 	2070-209F
        Superscripts_And_Subscripts = (32 << 32) | (0x2070 << 16) | 0x209F,
        //33 	Currency Symbols 	20A0-20CF
        Currency_Symbols = (33 << 32) | (0x20A0 << 16) | 0x20CF,
        //34 	Combining Diacritical Marks For Symbols 	20D0-20FF
        Combining_Diacritical_Marks_For_Symbols = (34 << 32) | (0x20D0 << 16) | 0x20FF,
        //35 	Letterlike Symbols 	2100-214F
        Letterlike_Symbols = (35 << 32) | (0x2100 << 16) | 0x214F,
        //36 	Number Forms 	2150-218F
        Number_Forms = (36 << 32) | (0x2150 << 16) | 0x218F,
        //37 	Arrows 	2190-21FF
        //      Supplemental Arrows-A 	27F0-27FF
        //      Supplemental Arrows-B 	2900-297F
        //      Miscellaneous Symbols and Arrows 	2B00-2BFF
        Arrows = (37 << 32) | (0x2190 << 16) | 0x21FF,
        Supplemental_Arrows_A = (37 << 32) | (0x27F0 << 16) | 0x27FF,
        Supplemental_Arrows_B = (37 << 32) | (0x2900 << 16) | 0x297F,
        Miscellaneous_Symbols_and_Arrows = (37 << 32) | (0x2B00 << 16) | 0x2BFF,
        //38 	Mathematical Operators 	2200-22FF
        //    Supplemental Mathematical Operators 	2A00-2AFF
        //    Miscellaneous Mathematical Symbols-A 	27C0-27EF
        //    Miscellaneous Mathematical Symbols-B 	2980-29FF
        Mathematical_Operators = (38 << 32) | (0x2200 << 16) | 0x22FF,
        Supplemental_Mathematical_Operators = (38 << 32) | (0x2A00 << 16) | 0x2AFF,
        Miscellaneous_Mathematical_Symbols_A = (38 << 32) | (0x27C0 << 16) | 0x27EF,
        Miscellaneous_Mathematical_Symbols_B = (38 << 32) | (0x2980 << 16) | 0x29FF,
        //39 	Miscellaneous Technical 	2300-23FF
        Miscellaneous_Technical = (39 << 32) | (0x2300 << 16) | 0x23FF,
        //40 	Control Pictures 	2400-243F
        Control_Pictures = (40 << 32) | (0x2400 << 16) | 0x243F,
        //41 	Optical Character Recognition 	2440-245F
        Optical_Character_Recognition = (41 << 32) | (0x2440 << 16) | 0x245F,
        //42 	Enclosed Alphanumerics 	2460-24FF
        Enclose_Alphanumerics = (42 << 32) | (0x2460 << 16) | 0x24FF,
        //43 	Box Drawing 	2500-257F
        Box_Drawing = (43 << 32) | (0x2500 << 16) | 0x257F,
        //44 	Block Elements 	2580-259F
        Block_Elements = (44 << 32) | (0x2580 << 16) | 0x259F,
        //45 	Geometric Shapes 	25A0-25FF
        Geometric_Shapes = (45 << 32) | (0x2580 << 16) | 0x259F,
        //46 	Miscellaneous Symbols 	2600-26FF
        Miscellaneous_Symbols = (46 << 32) | (0x2600 << 16) | 0x26FF,
        //47 	Dingbats 	2700-27BF
        Dingbats = (47 << 32) | (0x2700 << 16) | 0x27BF,
        //48 	CJK Symbols And Punctuation 	3000-303F
        CJK_Symbols_And_unctuation = (48 << 32) | (0x3000 << 16) | 0x303F,
        //49 	Hiragana 	3040-309F
        Hiragana = (49 << 32) | (0x3040 << 16) | 0x309F,
        //50 	Katakana 	30A0-30FF
        //      Katakana Phonetic Extensions 	31F0-31FF
        Katakana = (50 << 32) | (0x30A0 << 16) | 0x30FF,
        Katakana_Phonetic_Extensions = (50 << 32) | (0x31F0 << 16) | 0x31FF,
        //51 	Bopomofo 	3100-312F
        //      Bopomofo Extended 	31A0-31BF
        Bopomofo = (51 << 32) | (0x3100 << 16) | 0x312F,
        Bopomofo_Extended = (51 << 32) | (0x31A0 << 16) | 0x31BF,
        //52 	Hangul Compatibility Jamo 	3130-318F
        Hangul_Compatibility_Jamo = (52 << 32) | (0x3130 << 16) | 0x318F,
        //53 	Phags-pa 	A840-A87F
        Phags_pa = (53 << 32) | (0xA840 << 16) | 0xA87F,
        //54 	Enclosed CJK Letters And Months 	3200-32FF
        Enclosed_CJK_Letters_And_Months = (54 << 32) | (0x3200 << 16) | 0x32FF,
        //55 	CJK Compatibility 	3300-33FF
        CJK_Compatibility = (55 << 32) | (0x3300 << 16) | 0x33FF,
        //56 	Hangul Syllables 	AC00-D7AF
        Hangul_Syllables = (56 << 32) | (0xAC00 << 16) | 0xD7AF,
        //57 	Non-Plane 0 * 	D800-DFFF
        Non_Plane_0 = (57 << 32) | (0xD800 << 16) | 0xDFFF,
        //58 	Phoenician 	10900-1091F
        Phoenician = (58 << 32) | (0x10900 << 16) | 0x1091F,
        //59 	CJK Unified Ideographs 	4E00-9FFF
        //    CJK Radicals Supplement 	2E80-2EFF
        //    Kangxi Radicals 	2F00-2FDF
        //    Ideographic Description Characters 	2FF0-2FFF
        //    CJK Unified Ideographs Extension A 	3400-4DBF
        //    CJK Unified Ideographs Extension B 	20000-2A6DF
        //    Kanbun 	3190-319F
        CJK_Unified_Ideographs = (59 << 32) | (0x4E00 << 16) | 0x9FFF,
        CJK_Radicals_Supplement = (59 << 32) | (0x2E80 << 16) | 0x2EFF,
        Kangxi_Radicals = (59 << 32) | (0x2F00 << 16) | 0x2FDF,
        Ideographic_Description_Characters = (59 << 32) | (0x2FF0 << 16) | 0x2FFF,
        CJK_Unified_Ideographs_Extension_A = (59 << 32) | (0x3400 << 16) | 0x4DBF,
        CJK_Unified_Ideographs_Extension_B = (59 << 32) | (0x20000 << 16) | 0x2A6DF,
        Kanbun = (59 << 32) | (0x3190 << 16) | 0x319F,
        //60 	Private Use Area (plane 0) 	E000-F8FF
        Private_Use_Area_Plane0 = (60 << 32) | (0xE000 << 16) | 0xF8FF,
        //61 	CJK Strokes 	31C0-31EF
        //    CJK Compatibility Ideographs 	F900-FAFF
        //    CJK Compatibility Ideographs Supplement 	2F800-2FA1F
        CJK_Strokes = (61 << 32) | (0x31C0 << 16) | 0x31EF,
        CJK_Compatibility_Ideographs = (61 << 32) | (0xF900 << 16) | 0xFAFF,
        CJK_Compatibility_Ideographs_Supplement = (61 << 32) | (0x2F800 << 16) | 0x2FA1F,
        //62 	Alphabetic Presentation Forms 	FB00-FB4F
        Alphabetic_Presentation_Forms = (62 << 32) | (0xFB00 << 16) | 0xFB4F,
        //63 	Arabic Presentation Forms-A 	FB50-FDFF
        Arabic_Presentation_Forms_A = (63 << 32) | (0xFB50 << 16) | 0xFDFF,
        //64 	Combining Half Marks 	FE20-FE2F
        Combining_Half_Marks = (64 << 32) | (0xFE20 << 16) | 0xFE2F,
        //65 	Vertical Forms 	FE10-FE1F
        //      CJK Compatibility Forms 	FE30-FE4F
        Vertical_Forms = (65 << 32) | (0xFE10 << 16) | 0xFE1F,
        CJK_Compatibility_Forms = (65 << 32) | (0xFE30 << 16) | 0xFE4F,
        //66 	Small Form Variants 	FE50-FE6F
        Small_Form_Variants = (66 << 32) | (0xFE50 << 16) | 0xFE6F,
        //67 	Arabic Presentation Forms-B 	FE70-FEFF
        Arabic_Presentation_Forms_B = (67 << 32) | (0xFE70 << 16) | 0xFEFF,
        //68 	Halfwidth And Fullwidth Forms 	FF00-FFEF
        Halfwidth_And_Fullwidth_Forms = (68 << 32) | (0xFF00 << 16) | 0xFFEF,
        //69 	Specials 	FFF0-FFFF
        Specials = (69 << 32) | (0xFFF0 << 16) | 0xFFFF,
        //70 	Tibetan 	0F00-0FFF
        Tibetan = (70 << 32) | (0x0F00 << 16) | 0x0FFF,
        //71 	Syriac 	0700-074F
        Syriac = (71 << 32) | (0x0700 << 16) | 0x074F,
        //72 	Thaana 	0780-07BF
        Thaana = (72 << 32) | (0x0780 << 16) | 0x07BF,
        //73 	Sinhala 	0D80-0DFF
        Sinhala = (73 << 32) | (0x0D80 << 16) | 0x0DFF,
        //74 	Myanmar 	1000-109F
        Myanmar = (74 << 32) | (0x1000 << 16) | 0x109F,
        //75 	Ethiopic 	1200-137F
        //    Ethiopic Supplement 	1380-139F
        //    Ethiopic Extended 	2D80-2DDF
        Ethiopic = (75 << 32) | (0x1200 << 16) | 0x137F,
        Ethiopic_Supplement = (75 << 32) | (0x1380 << 16) | 0x139F,
        Ethiopic_Extended = (75 << 32) | (0x2D80 << 16) | 0x2DDF,
        //76 	Cherokee 	13A0-13FF
        Cherokee = (76 << 32) | (0x13A0 << 16) | 0x13FF,
        //77 	Unified Canadian Aboriginal Syllabics 	1400-167F
        Unified_Canadian_Aboriginal_Syllabics = (77 << 32) | (0x1400 << 16) | 0x167F,
        //78 	Ogham 	1680-169F
        Ogham = (78 << 32) | (0x1680 << 16) | 0x169F,
        //79 	Runic 	16A0-16FF
        Runic = (79 << 32) | (0x16A0 << 16) | 0x16FF,
        //80 	Khmer 	1780-17FF
        //    Khmer Symbols 	19E0-19FF
        Khmer = (80 << 32) | (0x1780 << 16) | 0x17FF,
        Khmer_Symbols = (80 << 32) | (0x19E0 << 16) | 0x19FF,
        //81 	Mongolian 	1800-18AF
        Mongolian = (81 << 32) | (0x1800 << 16) | 0x18AF,
        //82 	Braille Patterns 	2800-28FF
        Braille_Patterns = (82 << 32) | (0x2800 << 16) | 0x28FF,
        //83 	Yi Syllables 	A000-A48F
        //      Yi Radicals 	A490-A4CF
        Yi_Syllables = (83 << 32) | (0xA000 << 16) | 0xA48F,
        Yi_Radicals = (83 << 32) | (0xA490 << 16) | 0xA4CF,
        //84 	Tagalog 	1700-171F
        //    Hanunoo 	1720-173F
        //    Buhid 	1740-175F
        //    Tagbanwa 	1760-177F
        Tagalog = (84 << 32) | (0x1700 << 16) | 0x171F,
        Hanunoo = (84 << 32) | (0x1720 << 16) | 0x173F,
        Buhid = (84 << 32) | (0x1740 << 16) | 0x175F,
        Tagbanwa = (84 << 32) | (0x1760 << 16) | 0x177F,
        //85 	Old Italic 	10300-1032F
        Old_Italic = (85 << 32) | (0x10300 << 16) | 0x1032F,
        //86 	Gothic 	10330-1034F
        Gothic = (86 << 32) | (0x10330 << 16) | 0x1034F,
        //87 	Deseret 	10400-1044F
        Deseret = (87 << 32) | (0x10400 << 16) | 0x1044F,
        //88 	Byzantine Musical Symbols 	1D000-1D0FF
        //    Musical Symbols 	1D100-1D1FF
        //    Ancient Greek Musical Notation 	1D200-1D24F
        Byzantine_Musical_Symbols = (88 << 32) | (0x1D000 << 16) | 0x1D0FF,
        Musical_Symbols = (88 << 32) | (0x1D100 << 16) | 0x1D1FF,
        Ancient_Greek_Musical_Notation = (88 << 32) | (0x1D200 << 16) | 0x1D24F,
        //89 	Mathematical Alphanumeric Symbols 	1D400-1D7FF
        Mathematical_Alphanumeric_Symbols = (89 << 32) | (0x1D400 << 16) | 0x1D7FF,
        //90 	Private Use (plane 15) 	FF000-FFFFD
        //    Private Use (plane 16) 	100000-10FFFD
        Private_Use_plane15 = (90 << 32) | (0xFF000 << 16) | 0xFFFFD,
        Private_Use_plane16 = (90 << 32) | (0x100000 << 16) | 0x10FFFD,
        //91 	Variation Selectors 	FE00-FE0F
        //    Variation Selectors Supplement 	E0100-E01EF
        Variation_Selectors = (91 << 32) | (0xFE00 << 16) | 0xFE0F,
        Variation_Selectors_Supplement = (91 << 32) | (0xE0100 << 16) | 0xE01EF,
        //92 	Tags 	E0000-E007F
        Tags = (92 << 32) | (0xE0000 << 16) | 0xE007F,
        //93 	Limbu 	1900-194F
        Limbu = (93 << 32) | (0x1900 << 16) | 0x194F,
        //94 	Tai Le 	1950-197F
        Tai_Le = (94 << 32) | (0x1950 << 16) | 0x197F,
        //95 	New Tai Lue 	1980-19DF
        New_Tai_Lue = (95 << 32) | (0x1980 << 16) | 0x19DF,
        //96 	Buginese 	1A00-1A1F
        Buginese = (96 << 32) | (0x1A00 << 16) | 0x1A1F,
        //97 	Glagolitic 	2C00-2C5F
        Glagolitic = (97 << 32) | (0x2C00 << 16) | 0x2C5F,
        //98 	Tifinagh 	2D30-2D7F
        Tifinagh = (98 << 32) | (0x2D30 << 16) | 0x2D7F,
        //99 	Yijing Hexagram Symbols 	4DC0-4DFF
        Yijing_Hexagram_Symbols = (99 << 32) | (0x4DC0 << 16) | 0x4DFF,
        //100 	Syloti Nagri 	A800-A82F
        Syloti_Nagri = (100 << 32) | (0xA800 << 16) | 0xA82F,
        //101 	Linear B Syllabary 	10000-1007F
        //    Linear B Ideograms 	10080-100FF
        //    Aegean Numbers 	10100-1013F
        Linear_B_Syllabary = (101 << 32) | (0x10000 << 16) | 0x1007F,
        Linear_B_Ideograms = (101 << 32) | (0x10080 << 16) | 0x100FF,
        Aegean_Numbers = (101 << 32) | (0x10100 << 16) | 0x1013F,
        //102 	Ancient Greek Numbers 	10140-1018F
        Ancient_Greek_Numbers = (102 << 32) | (0x10140 << 16) | 0x1018F,
        //103 	Ugaritic 	10380-1039F
        Ugaritic = (103 << 32) | (0x10380 << 16) | 0x1039F,
        //104 	Old Persian 	103A0-103DF
        Old_Persian = (104 << 32) | (0x103A0 << 16) | 0x103DF,
        //105 	Shavian 	10450-1047F
        Shavian = (105 << 32) | (0x10450 << 16) | 0x1047F,
        //106 	Osmanya 	10480-104AF
        Osmanya = (106 << 32) | (0x10480 << 16) | 0x104AF,
        //107 	Cypriot Syllabary 	10800-1083F
        Cypriot_Syllabary = (107 << 32) | (0x10800 << 16) | 0x1083F,
        //108 	Kharoshthi 	10A00-10A5F
        Kharoshthi = (108 << 32) | (0x10A00 << 16) | 0x10A5F,
        //109 	Tai Xuan Jing Symbols 	1D300-1D35F
        Tai_Xuan_Jing_Symbols = (109 << 32) | (0x1D300 << 16) | 0x1D35F,
        //110 	Cuneiform 	12000-123FF
        //    Cuneiform Numbers and Punctuation 	12400-1247F
        Cuneiform = (110 << 32) | (0x12000 << 16) | 0x123FF,
        Cuneiform_Numbers_and_Punctuation = (110 << 32) | (0x12400 << 16) | 0x1247F,
        //111 	Counting Rod Numerals 	1D360-1D37F
        Counting_Rod_Numerals = (111 << 32) | (0x1D360 << 16) | 0x1D37F,
        //112 	Sundanese 	1B80-1BBF
        Sundanese = (112 << 32) | (0x1B80 << 16) | 0x1BBF,
        //113 	Lepcha 	1C00-1C4F
        Lepcha = (113 << 32) | (0x1C00 << 16) | 0x1C4F,
        //114 	Ol Chiki 	1C50-1C7F
        Ol_Chiki = (114 << 32) | (0x1C50 << 16) | 0x1C7F,
        //115 	Saurashtra 	A880-A8DF
        Saurashtra = (115 << 32) | (0xA880 << 16) | 0xA8DF,

        //116 	Kayah Li 	A900-A92F
        Kayah_Li = (116 << 32) | (0xA900 << 16) | 0xA92F,
        //117 	Rejang 	A930-A95F
        Rejang = (117 << 32) | (0xA930 << 16) | 0xA95F,
        //118 	Cham 	AA00-AA5F
        Cham = (118 << 32) | (0xAA00 << 16) | 0xAA5F,
        //119 	Ancient Symbols 	10190-101CF
        Ancient_Symbols = (119 << 32) | (0x10190 << 16) | 0x101CF,
        //120 	Phaistos Disc 	101D0-101FF
        Phaistos_Disc = (120 << 32) | (0x101D0 << 16) | 0x101FF,
        //121 	Carian 	102A0-102DF
        Carian = (121 << 32) | (0x102A0 << 16) | 0x102DF,
        //    Lycian 	10280-1029F
        Lycian = (121 << 32) | (0x10280 << 16) | 0x1029F,
        //    Lydian 	10920-1093F
        Lydian = (121 << 32) | (0x10920 << 16) | 0x1093F,
        //122 	Domino Tiles 	1F030-1F09F
        //    Mahjong Tiles 	1F000-1F02F
        Domino_Tiles = (122 << 32) | (0x1F030 << 16) | 0x1F09F,
        Mahjong_Tiles = (122 << 32) | (0x1F000 << 16) | 0x1F02F,
        //123-127 	Reserved for process-internal usage
        //
        Reserved123 = (123 << 32),
        Reserved124 = (124 << 32),
        Reserved125 = (125 << 32),
        Reserved126 = (126 << 32),
        Reserved127 = (127 << 32),
    }
}