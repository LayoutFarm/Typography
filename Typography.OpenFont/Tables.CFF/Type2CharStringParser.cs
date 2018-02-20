//Apache2, 2018, Villu Ruusmann , Apache/PdfBox Authors ( https://github.com/apache/pdfbox)  
//Apache2, 2018, WinterDev 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Typography.OpenFont.CFF
{

    struct Type2Command
    {
        //operands
        //operator 
        internal bool is2BytesOperator;
        internal Type2Operator1 _operator; 
    }

    enum Type2Operator1 : byte
    {
        //Appendix A Type 2 Charstring Command Codes       

        _Reserved0_ = 0,
        hstem, //1
        _Reserved2_,//2
        vstem, //3
        vmoveto,//4
        rlineto, //5
        hlineto, //6
        vlineto,//7,
        rrcurveto,//8
        _Reserved9_, //9
        callsubr, //10

        //---------------------
        _return, //11
        escape,//12
        _Reserved13_,
        endchar,//14
        _Reserved15_,
        _Reserved16_,
        _Reserved17_,
        hstemhm,//18
        hintmask,//19
        cntrmask,//20
        //---------------------
        rmoveto,//21
        hmoveto,//22
        vstemhm,//23
        rcurveline, //24
        rlinecurve,//25
        vvcurveto,//26
        hhcurveto, //27
        shortint, //28
        callgsubr, //29
        vhcurveto, //30
        //-----------------------
        hvcurveto, //31
    }

    enum Type2Operator2 : byte
    {
        //Two-byte Type 2 Operators
        _Reserved0_ = 0,
        _Reserved1_,
        _Reserved2_,
        and, //3
        or, //4
        not, //5
        _Reserved6_,
        _Reserved7_,
        _Reserved8_,
        //
        abs,//9        
        add,//10
        //------------------
        sub,//11
        div,//12
        _Reserved13_,
        neg,//14
        eq, //15
        _Reserved16_,
        _Reserved17_,
        drop,//18
        _Reserved19_,
        put,//20
        //------------------ 
        get, //21
        ifelse,//22
        random,//23
        mul, //24,
        _Reserved25_,
        sqrt,//26
        dup,//27
        exch,//28 , exchanges the top two elements on the argument stack
        index,//29
        roll,//30
        //--------------
        _Reserved31_,
        _Reserved32_,
        _Reserved33_,
        //--------------
        hflex,//34
        flex, //35
        hflex1,//36
        flex1//37


    }

    class Type2CharStringParser : IDisposable
    {
        MemoryStream _msBuffer;
        BinaryReader _reader;

        public Type2CharStringParser()
        {
            _msBuffer = new MemoryStream();
            _reader = new BinaryReader(_msBuffer);
        }
        public void ParseType2CharsString(byte[] buffer)
        {
            //TODO: implement this
            //reset
            _msBuffer.SetLength(9);
            _msBuffer.Position = 0;
            _msBuffer.Write(buffer, 0, buffer.Length);
            _msBuffer.Position = 0; 
            int len = buffer.Length; 
            List<Type2Command> cmds = new List<Type2Command>(); 

            while (_reader.BaseStream.Position < len)
            {
                //read first byte 
                //translate *** 
                byte b0 = _reader.ReadByte();

                if (b0 == 0)
                {
                    //reserve
                }
                else if (b0 == 12)
                {
                    // First byte of a 2-byte operator
                    _reader.ReadByte(); //temp fix , not correct
                }
                else if (b0 < 28)
                {

                }
                else if (b0 == 28)
                {
                    //shortint
                    //First byte of a 3-byte sequence specifying a number.
                    _reader.ReadUInt16(); //temp fix , not correct
                }
                else if (b0 >= 29 && b0 < 32)
                {
                    //29,30,31
                }
                else if (b0 >= 32 && b0 < 255)
                {
                    int num = ReadIntegerNumber(b0);
                    //operands.Add(new CffOperand(num, OperandKind.IntNumber));
                }
                else if (b0 == 255)
                {
                    //int
                    //First byte of a 5-byte sequence specifying a number.
                    _reader.ReadInt32();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        int ReadIntegerNumber(byte b0)
        {

            if (b0 >= 32 && b0 <= 246)
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

        public void Dispose()
        {

            if (_msBuffer != null)
            {
                _msBuffer.Dispose();
                _msBuffer = null;
            }
        }
    }
}