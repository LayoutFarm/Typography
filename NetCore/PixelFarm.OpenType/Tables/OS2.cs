//Apache2, 2016, WinterDev

using System.Collections.Generic;
using System.IO;
using System.Text;
namespace NOpenType.Tables
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

        //TODO: add more util complete
    }
    //32 	Superscripts And Subscripts 	2070-209F
    //33 	Currency Symbols 	20A0-20CF
    //34 	Combining Diacritical Marks For Symbols 	20D0-20FF
    //35 	Letterlike Symbols 	2100-214F
    //36 	Number Forms 	2150-218F
    //37 	Arrows 	2190-21FF
    //    Supplemental Arrows-A 	27F0-27FF
    //    Supplemental Arrows-B 	2900-297F
    //    Miscellaneous Symbols and Arrows 	2B00-2BFF 
    //38 	Mathematical Operators 	2200-22FF
    //    Supplemental Mathematical Operators 	2A00-2AFF
    //    Miscellaneous Mathematical Symbols-A 	27C0-27EF
    //    Miscellaneous Mathematical Symbols-B 	2980-29FF
    //39 	Miscellaneous Technical 	2300-23FF
    //40 	Control Pictures 	2400-243F
    //41 	Optical Character Recognition 	2440-245F
    //42 	Enclosed Alphanumerics 	2460-24FF
    //43 	Box Drawing 	2500-257F
    //44 	Block Elements 	2580-259F
    //45 	Geometric Shapes 	25A0-25FF
    //46 	Miscellaneous Symbols 	2600-26FF
    //47 	Dingbats 	2700-27BF
    //48 	CJK Symbols And Punctuation 	3000-303F
    //49 	Hiragana 	3040-309F
    //50 	Katakana 	30A0-30FF
    //    Katakana Phonetic Extensions 	31F0-31FF
    //51 	Bopomofo 	3100-312F
    //    Bopomofo Extended 	31A0-31BF
    //52 	Hangul Compatibility Jamo 	3130-318F
    //53 	Phags-pa 	A840-A87F
    //54 	Enclosed CJK Letters And Months 	3200-32FF
    //55 	CJK Compatibility 	3300-33FF
    //56 	Hangul Syllables 	AC00-D7AF
    //57 	Non-Plane 0 * 	D800-DFFF
    //58 	Phoenician 	10900-1091F
    //59 	CJK Unified Ideographs 	4E00-9FFF
    //    CJK Radicals Supplement 	2E80-2EFF
    //    Kangxi Radicals 	2F00-2FDF
    //    Ideographic Description Characters 	2FF0-2FFF
    //    CJK Unified Ideographs Extension A 	3400-4DBF
    //    CJK Unified Ideographs Extension B 	20000-2A6DF
    //    Kanbun 	3190-319F
    //60 	Private Use Area (plane 0) 	E000-F8FF
    //61 	CJK Strokes 	31C0-31EF
    //    CJK Compatibility Ideographs 	F900-FAFF
    //    CJK Compatibility Ideographs Supplement 	2F800-2FA1F
    //62 	Alphabetic Presentation Forms 	FB00-FB4F
    //63 	Arabic Presentation Forms-A 	FB50-FDFF
    //64 	Combining Half Marks 	FE20-FE2F
    //65 	Vertical Forms 	FE10-FE1F
    //    CJK Compatibility Forms 	FE30-FE4F
    //66 	Small Form Variants 	FE50-FE6F
    //67 	Arabic Presentation Forms-B 	FE70-FEFF
    //68 	Halfwidth And Fullwidth Forms 	FF00-FFEF
    //69 	Specials 	FFF0-FFFF
    //70 	Tibetan 	0F00-0FFF
    //71 	Syriac 	0700-074F
    //72 	Thaana 	0780-07BF
    //73 	Sinhala 	0D80-0DFF
    //74 	Myanmar 	1000-109F
    //75 	Ethiopic 	1200-137F
    //    Ethiopic Supplement 	1380-139F
    //    Ethiopic Extended 	2D80-2DDF
    //76 	Cherokee 	13A0-13FF
    //77 	Unified Canadian Aboriginal Syllabics 	1400-167F
    //78 	Ogham 	1680-169F
    //79 	Runic 	16A0-16FF
    //80 	Khmer 	1780-17FF
    //    Khmer Symbols 	19E0-19FF
    //81 	Mongolian 	1800-18AF
    //82 	Braille Patterns 	2800-28FF
    //83 	Yi Syllables 	A000-A48F
    //    Yi Radicals 	A490-A4CF
    //84 	Tagalog 	1700-171F
    //    Hanunoo 	1720-173F
    //    Buhid 	1740-175F
    //    Tagbanwa 	1760-177F
    //85 	Old Italic 	10300-1032F
    //86 	Gothic 	10330-1034F
    //87 	Deseret 	10400-1044F
    //88 	Byzantine Musical Symbols 	1D000-1D0FF
    //    Musical Symbols 	1D100-1D1FF
    //    Ancient Greek Musical Notation 	1D200-1D24F
    //89 	Mathematical Alphanumeric Symbols 	1D400-1D7FF
    //90 	Private Use (plane 15) 	FF000-FFFFD
    //    Private Use (plane 16) 	100000-10FFFD
    //91 	Variation Selectors 	FE00-FE0F
    //    Variation Selectors Supplement 	E0100-E01EF
    //92 	Tags 	E0000-E007F
    //93 	Limbu 	1900-194F
    //94 	Tai Le 	1950-197F
    //95 	New Tai Lue 	1980-19DF
    //96 	Buginese 	1A00-1A1F
    //97 	Glagolitic 	2C00-2C5F
    //98 	Tifinagh 	2D30-2D7F
    //99 	Yijing Hexagram Symbols 	4DC0-4DFF
    //100 	Syloti Nagri 	A800-A82F
    //101 	Linear B Syllabary 	10000-1007F
    //    Linear B Ideograms 	10080-100FF
    //    Aegean Numbers 	10100-1013F
    //102 	Ancient Greek Numbers 	10140-1018F
    //103 	Ugaritic 	10380-1039F
    //104 	Old Persian 	103A0-103DF
    //105 	Shavian 	10450-1047F
    //106 	Osmanya 	10480-104AF
    //107 	Cypriot Syllabary 	10800-1083F
    //108 	Kharoshthi 	10A00-10A5F
    //109 	Tai Xuan Jing Symbols 	1D300-1D35F
    //110 	Cuneiform 	12000-123FF
    //    Cuneiform Numbers and Punctuation 	12400-1247F
    //111 	Counting Rod Numerals 	1D360-1D37F
    //112 	Sundanese 	1B80-1BBF
    //113 	Lepcha 	1C00-1C4F
    //114 	Ol Chiki 	1C50-1C7F
    //115 	Saurashtra 	A880-A8DF
    //116 	Kayah Li 	A900-A92F
    //117 	Rejang 	A930-A95F
    //118 	Cham 	AA00-AA5F
    //119 	Ancient Symbols 	10190-101CF
    //120 	Phaistos Disc 	101D0-101FF
    //121 	Carian 	102A0-102DF
    //    Lycian 	10280-1029F
    //    Lydian 	10920-1093F
    //122 	Domino Tiles 	1F030-1F09F
    //    Mahjong Tiles 	1F000-1F02F
    //123-127 	Reserved for process-internal usage
}