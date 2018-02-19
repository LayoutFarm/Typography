//Apapche, 2018, apache/pdfbox Authors ( https://github.com/apache/pdfbox) 
//
//Apache PDFBox
//Copyright 2014 The Apache Software Foundation

//This product includes software developed at
//The Apache Software Foundation(http://www.apache.org/).

//Based on source code originally developed in the PDFBox and
//FontBox projects.

//Copyright (c) 2002-2007, www.pdfbox.org

//Based on source code originally developed in the PaDaF project.
//Copyright (c) 2010 Atos Worldline SAS

//Includes the Adobe Glyph List
//Copyright 1997, 1998, 2002, 2007, 2010 Adobe Systems Incorporated.

//Includes the Zapf Dingbats Glyph List
//Copyright 2002, 2010 Adobe Systems Incorporated.

//Includes OSXAdapter
//Copyright (C) 2003-2007 Apple, Inc., All Rights Reserved

//----------------
//Adobe's The Compact Font Format Specification
//from http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf

//------------------------------------------------------------------
//many areas are ported from Java code
//Apache2, 2018, WinterDev


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Typography.OpenFont.CFF
{


    class Cff1FontSet
    {
        internal List<string> fontNames;
        internal List<Cff1Font> _fonts = new List<Cff1Font>();
    }
    class Cff1Font
    {
        internal string FontName { get; set; }
    }
    class Cff1Parser
    {

        //Table 2 CFF Data Types
        //Name       Range          Description
        //Card8      0 – 255   	    1-byte unsigned number
        //Card16     0 – 65535 	    2-byte unsigned number
        //Offset     varies 	  	1, 2, 3, or 4 byte offset(specified by  OffSize field)
        //OffSize	 1–4			1-byte unsigned number specifies the
        //                          size of an Offset field or fields
        //SID		0 – 64999       2-byte string identifier
        //-----------------   

        //Table 1 CFF Data Layout
        //Entry                     Comments
        //Header      		        –
        //Name INDEX  		        –
        //Top DICT INDEX 		    –
        //String INDEX		        –
        //Global Subr INDEX	        – 	
        //Encodings			        –		
        //Charsets			        –
        //FDSelect                  CIDFonts only
        //CharStrings INDEX         per-font
        //Font DICT INDEX           per-font, CIDFonts only
        //Private DICT              per-font
        //Local Subr INDEX          per-font or per-Private DICT for CIDFonts
        //Copyright and Trademark	-
        // Notices  	
        //-----------------


        //from Apache's PDF box/FontBox
        //@author Villu Ruusmann

        BinaryReader _reader;

        Cff1FontSet _cff1FontSet;

        List<CffDataDicEntry> _topDic;
        public void ParseAfterHader(BinaryReader reader)
        {
            _cff1FontSet = new Cff1FontSet();
            _reader = reader;
            //
            ReadNameIndex();
            ReadTopDICTIndex();
            ReadStringIndex();
            ReadGlobalSubrIndex();
            ReadEncodings();
            ReadCharsets();
            ReadFDSelect();
            ReadCharStringsIndex();
            //...
        }

        void ReadNameIndex()
        {
            //7. Name INDEX
            //This contains the PostScript language names(FontName or
            //CIDFontName) of all the fonts in the FontSet stored in an INDEX
            //structure.The font names are sorted, thereby permitting a
            //binary search to be performed when locating a specific font
            //within a FontSet. The sort order is based on character codes
            //treated as 8 - bit unsigned integers. A given font name precedes
            //  another font name having the first name as its prefix.There
            //  must be at least one entry in this INDEX, i.e.the FontSet must
            // contain at least one font.

            //For compatibility with client software, such as PostScript
            //interpreters and Acrobat®, font names should be no longer
            //than 127 characters and should not contain any of the following
            //ASCII characters: [, ], (, ), {, }, <, >, /, %, null(NUL), space, tab, 
            //carriage return, line feed, form feed.It is recommended that
            //font names be restricted to the printable ASCII subset, codes 33
            //through 126.Adobe Type Manager® (ATM®) software imposes
            //a further restriction on the font name length of 63 characters.

            //Note 3
            //For compatibility with earlier PostSc
            //ript interpreters, see Technical Note
            //#5088, “Font Naming Issues.”

            //A font may be deleted from a FontSet without removing its data
            //by setting the first byte of its name in the Name INDEX to 0
            //(NUL).This kind of deletion offers a simple way to handle font
            //upgrades without rebuilding entire fontsets.Binary search
            //software must detect deletions and restart the search at the
            //previous or next name in the INDEX to ensure that all
            //appropriate names are matched. 

            CffIndexOffset[] nameIndexElems = ReadIndexDataOffsets();
            if (nameIndexElems == null) return;
            //

            int count = nameIndexElems.Length;
            List<string> fontNames = new List<string>();
            for (int i = 0; i < count; ++i)
            {
                //read each FontName or CIDFontName
                CffIndexOffset indexElem = nameIndexElems[i];
                //TODO: review here again, 
                //check if we need to set _reader.BaseStream.Position or not
                fontNames.Add(System.Text.Encoding.UTF8.GetString(_reader.ReadBytes(indexElem.len)));
            }

            //
            _cff1FontSet.fontNames = fontNames;


        }

        void ReadTopDICTIndex()
        {
            //8. Top DICT INDEX
            //This contains the top - level DICTs of all the fonts in the FontSet
            //stored in an INDEX structure.Objects contained within this
            //INDEX correspond to those in the Name INDEX in both order
            //and number. Each object is a DICT structure that corresponds to
            //the top-level dictionary of a PostScript font.
            //A font is identified by an entry in the Name INDEX and its data
            //is accessed via the corresponding Top DICT
            CffIndexOffset[] offsets = ReadIndexDataOffsets();
            //9. Top DICT Data
            //The names of the Top DICT operators shown in 
            //Table 9 are, where possible, the same as the corresponding Type 1 dict key. 
            //Operators that have no corresponding Type1 dict key are noted 
            //in the table below along with a default value, if any. (Several
            //operators have been derived from FontInfo dict keys but have
            //been grouped together with the Top DICT operators for
            //simplicity.The keys from the FontInfo dict are indicated in the
            //Default, notes  column of Table 9)
            int count = offsets.Length;
            if (count > 1)
            {
                //temp...
                throw new NotSupportedException();
            }
            for (int i = 0; i < count; ++i)
            {
                //read DICT data
                CffIndexOffset offset = offsets[i];
                List<CffDataDicEntry> dicData = ReadDICTData(offset.len);

                _topDic = dicData;
            }
        }

        void ReadStringIndex()
        {
            //10 String INDEX


            //All the strings, with the exception of the FontName and
            //CIDFontName strings which appear in the Name INDEX, used by
            //different fonts within the FontSet are collected together into an
            //INDEX structure and are referenced by a 2 - byte unsigned
            //number called a string identifier or SID.


            //Only unique strings are stored in the table
            //thereby removing duplication across fonts.

            //Further space saving is obtained by allocating commonly
            //occurring strings to predefined SIDs.
            //These strings, known as the standard strings, 
            //describe all the names used in the ISOAdobe and 
            //Expert character sets along with a few other strings
            //common to Type 1 fonts.

            //A complete list of standard strings is given in Appendix A

            //The client program will contain an array of standard strings with
            //nStdStrings elements.
            //Thus, the standard strings take SIDs in the
            //range 0 to(nStdStrings –1).

            //The first string in the String INDEX
            //corresponds to the SID whose value is equal to nStdStrings, the
            //first non - standard string, and so on.

            //When the client needs to
            //determine the string that corresponds to a particular SID it
            //performs the following: test if SID is in standard range then
            //fetch from internal table,
            //otherwise, fetch string from the String
            //INDEX using a value of(SID – nStdStrings) as the index.


            CffIndexOffset[] offsets = ReadIndexDataOffsets();
            if (offsets == null) return;
            //

            int count = offsets.Length;
            List<string> uniqueStringTable = new List<string>();
            for (int i = 0; i < count; ++i)
            {
                CffIndexOffset offset = offsets[i];
                //TODO: review here again, 
                //check if we need to set _reader.BaseStream.Position or not 
                //TODO: Is Charsets.ISO_8859_1 Encoding supported in .netcore 
                uniqueStringTable.Add(System.Text.Encoding.UTF8.GetString(_reader.ReadBytes(offset.len)));
            }



        }

        void ReadGlobalSubrIndex()
        {
            //16. Local / Global Subrs INDEXes
            //Both Type 1 and Type 2 charstrings support the notion of
            //subroutines or subrs. 

            //A subr is typically a sequence of charstring
            //bytes representing a sub - program that occurs in more than one
            //  place in a font’s charstring data.

            //This subr may be stored once
            //but referenced many times from within one or more charstrings
            //by the use of the call subr  operator whose operand is the
            //number of the subr to be called.

            //The subrs are local to a  particular font and
            //cannot be shared between fonts. 

            //Type 2 charstrings also permit global subrs which function in the same
            //way but are called by the call gsubr operator and may be shared
            //across fonts. 

            //Local subrs are stored in an INDEX structure which is located via
            //the offset operand of the Subrs  operator in the Private DICT.
            //A font without local subrs has no Subrs operator in the Private DICT.

            //Global subrs are stored in an INDEX structure which follows the
            //String INDEX. A FontSet without any global subrs is represented
            //by an empty Global Subrs INDEX.


            CffIndexOffset[] offsets = ReadIndexDataOffsets();
            if (offsets == null) return;

            //TODO: review here
            throw new NotImplementedException();
        }

        void ReadEncodings()
        {
            //Encoding data is located via the offset operand to the
            //Encoding operator in the Top DICT.

            //Only one Encoding operator can be
            //specified per font except for CIDFonts which specify no
            //encoding.A glyph’s encoding is specified by a 1 - byte code that
            //permits values in the range 0 - 255.


            //Each encoding is described by a format-type identifier byte
            //followed by format-specific data.Two formats are currently
            //defined as specified in Tables 11(Format 0) and 12(Format 1). 
            byte format = _reader.ReadByte();
            switch (format)
            {
                case 0:
                    {
                        ReadFormat0Encoding();
                    }
                    break;
                case 1:
                    {
                        ReadFormat1Encoding();
                    }
                    break;
            }



            //TODO: ...
        }
        void ReadCharsets()
        {
            //Charset data is located via the offset operand to the
            //charset operator in the Top DICT.

            //Each charset is described by a format-
            //type identifier byte followed by format-specific data.
            //Three formats are currently defined as shown in Tables
            //17, 18, and 20.

            //TODO: ...
            byte format = _reader.ReadByte();
            switch (format)
            {
                default:
                    throw new NotSupportedException();
                case 0:
                    throw new NotSupportedException();
                    break;
                case 1:
                    ReadCharsetsFormat1();
                    break;
                case 2:
                    throw new NotSupportedException();
                    break;
            }
        }
        void ReadCharsetsFormat0()
        {

        }

        void ReadCharsetsFormat1()
        {
            //Table 19 Range1 Format (Charset)
            //Type            Name          Description
            //SID             first         First glyph in range
            //Card8           nLeft         Glyphs left in range(excluding first)


            //Each Range1 describes a group of sequential SIDs. The number
            //of ranges is not explicitly specified in the font. Instead, software
            //utilizing this data simply processes ranges until all glyphs in the
            //font are covered. This format is particularly suited to charsets
            //that are well ordered

            int sid = _reader.ReadUInt16();//string iden (SID)
            byte nleft = _reader.ReadByte();
        }
        void ReadCharsetsFormat2()
        {

        }
        void ReadFDSelect()
        {
            //19. FDSelect

            // The FDSelect associates an FD(Font DICT) with a glyph by
            //specifying an FD index for that glyph. The FD index is used to
            //access one of the Font DICTs stored in the Font DICT INDEX.

            //FDSelect data is located via the offset operand to the
            //FDSelect operator in the Top DICT.FDSelect data specifies a format - type
            //identifier byte followed by format-specific data.Two formats
            //are currently defined, as shown in Tables  27 and 28.


            //TODO: ...


        }
        void ReadCharStringsIndex()
        {
            //14. CharStrings INDEX

            //This contains the charstrings of all the glyphs in a font stored in 
            //an INDEX structure.

            //Charstring objects contained within this
            //INDEX are accessed by GID.

            //The first charstring(GID 0) must be
            //the.notdef glyph. 

            //The number of glyphs available in a font may
            //be determined from the count field in the INDEX. 

            //

            //The format of the charstring data, and therefore the method of
            //interpretation, is specified by the
            //CharstringType  operator in the Top DICT.

            //The CharstringType operator has a default value
            //of 2 indicating the Type 2 charstring format which was designed
            //in conjunction with CFF.

            //Type 1 charstrings are documented in 
            //the “Adobe Type 1 Font Format” published by Addison - Wesley.

            //Type 2 charstrings are described in Adobe Technical Note #5177: 
            //“Type 2 Charstring Format.” Other charstring types may also be
            //supported by this method.



            CffIndexOffset[] offsets = ReadIndexDataOffsets();



            //TODO: ...



        }
        void ReadFormat0Encoding()
        {

            //Table 11: Format 0
            //Type      Name            Description
            //Card8     format          = 0
            //Card8     nCodes          Number of encoded glyphs
            //Card8     code[nCodes]    Code array
            //-------
            //Each element of the code array represents the encoding for the
            //corresponding glyph.This format should be used when the
            //codes are in a fairly random order

            //we have read format field( 1st field) ..
            //so start with 2nd field

            int nCodes = _reader.ReadByte();
            byte[] codes = _reader.ReadBytes(nCodes);

        }
        void ReadFormat1Encoding()
        {
            //Table 12 Format 1
            //Type      Name              Description
            //Card8     format             = 1
            //Card8     nRanges           Number of code ranges
            //struct    Range1[nRanges]   Range1 array(see Table  13)
            //--------------
            int nRanges = _reader.ReadByte();




            //Table 13 Range1 Format(Encoding)
            //Type        Name        Description
            //Card8       first       First code in range
            //Card8       nLeft       Codes left in range(excluding first)
            //--------------
            //Each Range1 describes a group of sequential codes. For 
            //example, the codes 51 52 53 54 55 could be represented by the
            //Range1: 51 4, and a perfectly ordered encoding of 256 codes can
            //be described with the Range1: 0 255.

            //This format is particularly suited to encodings that are well ordered.


            //A few fonts have multiply - encoded glyphs which are not
            //supported directly by any of the above formats. This situation is
            //indicated by setting the high - order bit in the format byte and
            //supplementing the encoding, regardless of format type, as
            //shown in Table 14.


            //Table 14 Supplemental Encoding Data            
            //Type 	    Name	    		Description
            //Card8	    nSups		    	Number of supplementary mappings
            //struct    Supplement[nSups]   Supplementary encoding array(see Table  15 below)


            //Table 15 Supplement Format
            //Type      Name        Description
            //Card8     code        Encoding
            //SID       glyph       Name
        }

        List<CffDataDicEntry> ReadDICTData(int len)
        {
            //4. DICT Data

            //Font dictionary data comprising key-value pairs is represented 
            //in a compact tokenized format that is similar to that used to 
            //represent Type 1 charstrings.

            //Dictionary keys are encoded as 1- or 2-byte operators and dictionary values are encoded as 
            //variable-size numeric operands that represent either integer or 
            //real values. 

            //-----------------------------
            //A DICT is simply a sequence of 
            //operand(s)/operator bytes concatenated together. 


            int endBefore = (int)(_reader.BaseStream.Position + len);

            List<CffDataDicEntry> dicData = new List<CffDataDicEntry>();
            while (_reader.BaseStream.Position < endBefore)
            {
                CffDataDicEntry dicEntry = ReadEntry();
                dicData.Add(dicEntry);
            }
            return dicData;
        }
        CffDataDicEntry ReadEntry()
        {
            //-----------------------------
            //An operator is preceded by the operand(s) that 
            //specify its value.
            //--------------------------------


            //-----------------------------
            //Operators and operands may be distinguished by inspection of
            //their first byte:
            //0–21 specify operators and
            //28, 29, 30, and 32–254 specify operands(numbers). 
            //Byte values 22–27, 31, and 255 are reserved.

            //An operator may be preceded by up to a maximum of 48 operands

            CffDataDicEntry dicEntry = new CffDataDicEntry();
            List<CffOperand> operands = new List<CffOperand>();

            while (true)
            {
                byte b0 = _reader.ReadByte();

                if (b0 >= 0 && b0 < 21)
                {
                    //operators
                    dicEntry._operator = ReadOperator(b0);
                    break; //**break after found operator
                }
                else if (b0 == 28 || b0 == 29)
                {
                    int num = ReadIntegerNumber(b0);
                    operands.Add(new CffOperand(num, OperandKind.IntNumber));
                }
                else if (b0 == 30)
                {
                    double num = ReadRealNumber();
                    operands.Add(new CffOperand(num, OperandKind.RealNumber));
                }
                else if (b0 >= 32 && b0 <= 254)
                {
                    int num = ReadIntegerNumber(b0);
                    operands.Add(new CffOperand(num, OperandKind.IntNumber));
                }
                else
                {
                    throw new NotSupportedException("invalid DICT data b0 byte: " + b0);
                }
            }

            dicEntry.operands = operands.ToArray();
            return dicEntry;
        }

        CFFOperator ReadOperator(byte b0)
        {
            //read operator key
            byte b1 = 0;
            if (b0 == 12)
            {
                //2 bytes
                b1 = _reader.ReadByte();
            }
            //get registered operator by its key
            return CFFOperator.GetOperatorByKey(b0, b1);
        }
        double ReadRealNumber()
        {

            StringBuilder sb = new StringBuilder();
            bool done = false;
            bool exponentMissing = false;

            int[] nibbles = { 0, 0 };

            while (!done)
            {
                int b = _reader.ReadByte();
                //TODO: review here
                nibbles[0] = b / 16;
                nibbles[1] = b % 16;

                foreach (int nibble in nibbles)
                {
                    switch (nibble)
                    {
                        case 0x0:
                        case 0x1:
                        case 0x2:
                        case 0x3:
                        case 0x4:
                        case 0x5:
                        case 0x6:
                        case 0x7:
                        case 0x8:
                        case 0x9:
                            sb.Append(nibble);
                            exponentMissing = false;
                            break;
                        case 0xa:
                            sb.Append(".");
                            break;
                        case 0xb:
                            sb.Append("E");
                            exponentMissing = true;
                            break;
                        case 0xc:
                            sb.Append("E-");
                            exponentMissing = true;
                            break;
                        case 0xd:
                            break;
                        case 0xe:
                            sb.Append("-");
                            break;
                        case 0xf:
                            done = true;
                            break;
                        default:
                            throw new Exception("IllegalArgumentException");
                    }
                }
            }
            if (exponentMissing)
            {
                // the exponent is missing, just append "0" to avoid an exception
                // not sure if 0 is the correct value, but it seems to fit
                // see PDFBOX-1522
                sb.Append("0");
            }
            if (sb.Length == 0)
            {
                return 0d;
            }

            //TODO: use TryParse
            double value;
            if (!double.TryParse(sb.ToString(), out value))
            {
                throw new NotSupportedException();
            }
            return value;
        }
        int ReadIntegerNumber(byte b0)
        {
            if (b0 == 28)
            {
                return _reader.ReadInt16();
            }
            else if (b0 == 29)
            {
                return _reader.ReadInt32();
            }
            else if (b0 >= 32 && b0 < 246)
            {
                return b0 - 139;
            }
            else if (b0 >= 247 && b0 <= 250)
            {
                int b1 = _reader.ReadByte();
                return (b0 - 247) * 256 + b1 + 108;
            }
            else if (b0 >= 251 && b0 <= 254)
            {
                int b1 = _reader.ReadByte();
                return -(b0 - 251) * 256 - b1 - 108;
            }
            else
            {
                throw new Exception();
            }

        }
        CffIndexOffset[] ReadIndexDataOffsets()
        {

            //INDEX Data
            //An INDEX is an array of variable-sized objects.It comprises a
            //header, an offset array, and object data. 
            //The offset array specifies offsets within the object data.
            //An object is retrieved by
            //indexing the offset array and fetching the object at the
            //specified offset.
            //The object’s length can be determined by subtracting its offset
            //from the next offset in the offset array.
            //An additional offset is added at the end of the offset array so the
            //length of the last object may be determined.
            //The INDEX format is shown in Table 7

            //Table 7 INDEX Format
            //Type        Name                  Description
            //Card16      count                 Number of objects stored in INDEX
            //OffSize     offSize               Offset array element size
            //Offset      offset[count + 1]     Offset array(from byte preceding object data)
            //Card8       data[<varies>]        Object data

            //Offsets in the offset array are relative to the byte that precedes
            //the object data. Therefore the first element of the offset array
            //is always 1. (This ensures that every object has a corresponding
            //offset which is always nonzero and permits the efficient
            //implementation of dynamic object loading.)

            //An empty INDEX is represented by a count field with a 0 value
            //and no additional fields.Thus, the total size of an empty INDEX
            //is 2 bytes.

            //Note 2
            //An INDEX may be skipped by jumping to the offset specified by the last
            //element of the offset array


            ushort count = _reader.ReadUInt16();
            if (count == 0)
            {
                return null;
            }

            int offSize = _reader.ReadByte(); //
            int[] offsets = new int[count + 1];
            CffIndexOffset[] indexElems = new CffIndexOffset[count];
            for (int i = 0; i <= count; ++i)
            {
                offsets[i] = _reader.ReadOffset(offSize);
            }
            for (int i = 0; i < count; ++i)
            {
                indexElems[i] = new CffIndexOffset(offsets[i], offsets[i + 1] - offsets[i]);
            }
            return indexElems;
        }


        struct CffIndexOffset
        {
            /// <summary>
            /// start offset
            /// </summary>
            public readonly int startOffset;
            public readonly int len;

            public CffIndexOffset(int startOffset, int len)
            {
                this.startOffset = startOffset;
                this.len = len;
            }
        }

    }


    static class CFFBinaryReaderExtension
    {

        public static int ReadOffset(this BinaryReader reader, int offsetSize)
        {
            switch (offsetSize)
            {
                default: throw new NotSupportedException();
                case 1:
                    return reader.ReadByte();
                case 2:
                    return (reader.ReadByte() << 8) | (reader.ReadByte() << 0);
                case 3:
                    return (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);
                case 4:
                    return (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);
            }
        }
    }

    class CffDataDicEntry
    {
        public CffOperand[] operands;
        public CFFOperator _operator;


#if DEBUG
        public override string ToString()
        {

            StringBuilder stbuilder = new StringBuilder();
            int j = operands.Length;
            for (int i = 0; i < j; ++i)
            {
                if (i > 0)
                {
                    stbuilder.Append(" ");
                }
                stbuilder.Append(operands[i].ToString());
            }

            stbuilder.Append(" ");
            stbuilder.Append(_operator.ToString());
            return stbuilder.ToString();
        }
#endif
    }


    enum OperandKind
    {
        IntNumber,
        RealNumber
    }


    struct CffOperand
    {
        public readonly OperandKind _kind;
        public readonly double _realNumValue;
        public CffOperand(double number, OperandKind kind)
        {
            this._kind = kind;
            this._realNumValue = number;
        }
#if DEBUG
        public override string ToString()
        {
            switch (_kind)
            {
                case OperandKind.IntNumber:
                    return ((int)_realNumValue).ToString();
                default:
                    return _realNumValue.ToString();
            }
        }
#endif

    }


    enum OperatorOperandKind
    {
        SID,
        Boolean,
        Number,
        Array,
        Delta,

        //compound
        NumberNumber,
        SID_SID_Number,
    }

    class CFFOperator
    {

        byte b0;
        byte b1;
        OperatorOperandKind _operatorOperandKind;

        //b0 the first byte of a two byte value
        //b1 the second byte of a two byte value
        private CFFOperator(string name, byte b0, byte b1, OperatorOperandKind operatorOperandKind)
        {
            this.b0 = b0;
            this.b1 = b1;
            this.Name = name;
            this._operatorOperandKind = operatorOperandKind;
        }
        public string Name { get; private set; }

        public static CFFOperator GetOperatorByKey(byte b0, byte b1)
        {
            CFFOperator found;
            s_registered_Operators.TryGetValue((b1 << 8) | b0, out found);
            return found;
        }


        static Dictionary<int, CFFOperator> s_registered_Operators = new Dictionary<int, CFFOperator>();
        static void Register(byte b0, byte b1, string operatorName, OperatorOperandKind opopKind)
        {
            s_registered_Operators.Add((b1 << 8) | b0, new CFFOperator(operatorName, b0, b1, opopKind));
        }
        static void Register(byte b0, string operatorName, OperatorOperandKind opopKind)
        {
            s_registered_Operators.Add(b0, new CFFOperator(operatorName, b0, 0, opopKind));
        }
        static CFFOperator()
        {
            //Table 9: Top DICT Operator Entries          
            Register(0, "version", OperatorOperandKind.SID);
            Register(1, "Notice", OperatorOperandKind.SID);
            Register(12, 0, "Copyright", OperatorOperandKind.SID);
            Register(2, "FullName", OperatorOperandKind.SID);
            Register(3, "FamilyName", OperatorOperandKind.SID);
            Register(4, "Weight", OperatorOperandKind.SID);
            Register(12, 1, "isFixedPitch", OperatorOperandKind.Boolean);
            Register(12, 2, "ItalicAngle", OperatorOperandKind.Number);
            Register(12, 3, "UnderlinePosition", OperatorOperandKind.Number);
            Register(12, 4, "UnderlineThickness", OperatorOperandKind.Number);
            Register(12, 5, "PaintType", OperatorOperandKind.Number);
            Register(12, 6, "CharstringType", OperatorOperandKind.Number);
            Register(12, 7, "FontMatrix", OperatorOperandKind.Array);
            Register(13, "UniqueID", OperatorOperandKind.Number);
            Register(5, "FontBBox", OperatorOperandKind.Array);
            Register(12, 8, "StrokeWidth", OperatorOperandKind.Number);
            Register(14, "XUID", OperatorOperandKind.Array);
            Register(15, "charset", OperatorOperandKind.Number);
            Register(16, "Encoding", OperatorOperandKind.Number);
            Register(17, "CharStrings", OperatorOperandKind.Number);
            Register(18, "Private", OperatorOperandKind.NumberNumber);
            Register(12, 20, "SyntheticBase", OperatorOperandKind.Number);
            Register(12, 21, "PostScript", OperatorOperandKind.SID);
            Register(12, 22, "BaseFontName", OperatorOperandKind.SID);
            Register(12, 23, "BaseFontBlend", OperatorOperandKind.SID);

            //Table 10: CIDFont Operator Extensions
            Register(12, 30, "ROS", OperatorOperandKind.SID_SID_Number);
            Register(12, 31, "CIDFontVersion", OperatorOperandKind.Number);
            Register(12, 32, "CIDFontRevision", OperatorOperandKind.Number);
            Register(12, 33, "CIDFontType", OperatorOperandKind.Number);
            Register(12, 34, "CIDCount", OperatorOperandKind.Number);
            Register(12, 35, "UIDBase", OperatorOperandKind.Number);
            Register(12, 36, "FDArray", OperatorOperandKind.Number);
            Register(12, 37, "FDSelect", OperatorOperandKind.Number);
            Register(12, 38, "FontName", OperatorOperandKind.SID);

            //Table 23: Private DICT Operators
            Register(6, "BlueValues", OperatorOperandKind.Delta);
            Register(7, "OtherBlues", OperatorOperandKind.Delta);
            Register(8, "FamilyBlues", OperatorOperandKind.Delta);
            Register(9, "FamilyOtherBlues", OperatorOperandKind.Delta);
            Register(12, 9, "BlueScale", OperatorOperandKind.Number);
            Register(12, 10, "BlueShift", OperatorOperandKind.Number);
            Register(12, 11, "BlueFuzz", OperatorOperandKind.Number);
            Register(10, "StdHW", OperatorOperandKind.Number);
            Register(11, "StdVW", OperatorOperandKind.Number);
            Register(12, 12, "StemSnapH", OperatorOperandKind.Delta);
            Register(12, 13, "StemSnapV", OperatorOperandKind.Delta);
            Register(12, 14, "ForceBold", OperatorOperandKind.Boolean);
            Register(12, 15, "LanguageGroup", OperatorOperandKind.Number);
            Register(12, 16, "ExpansionFactor", OperatorOperandKind.Number);
            Register(12, 17, "initialRandomSeed", OperatorOperandKind.Number);
            Register(19, "Subrs", OperatorOperandKind.Number);
            Register(20, "defaultWidthX", OperatorOperandKind.Number);
            Register(21, "nominalWidthX", OperatorOperandKind.Number);
        }

#if DEBUG
        public override string ToString()
        {
            return this.Name;
        }
#endif
    }


    class Cff2Parser
    {

        //https://docs.microsoft.com/en-us/typography/opentype/spec/cff2
        //Table 1: CFF2 Data Layout
        //Entry         Comments
        //Header        Fixed location
        //Top DICT      Fixed location
        //Global Subr   INDEX Fixed location
        //VariationStore
        //FDSelect Present only if there is more than one Font DICT in the Font DICT INDEX.
        //Font DICT INDEX
        //Array of Font DICT  Included in Font DICT INDEX.
        //Private DICT    One per Font DICT.
        public void ParseAfterHader(BinaryReader reader)
        {

        }
    }


}