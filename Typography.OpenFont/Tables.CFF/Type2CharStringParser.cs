//Apache2, 2018, Villu Ruusmann , Apache/PdfBox Authors ( https://github.com/apache/pdfbox)  
//Apache2, 2018, WinterDev 

//ref http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Typography.OpenFont.CFF
{


    //The Type 2 Charstring Format
    //...
    //must be used in a CFF (Compact Font Format) or OpenType font 
    //file to create a complete font program




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


    class Type2EvaluationStack
    {

        double[] _argStack = new double[50];
        int _currentIndex = 0; //current stack index

        public Type2EvaluationStack()
        {
        }
        public void Push(double value)
        {
            _argStack[_currentIndex] = value;
            _currentIndex++;
        }
        public void Eval(Type2Operator1 op)
        {

            //Many operators take their arguments from the bottom-most
            //entries in the Type 2 argument stack; this behavior is indicated
            //by the stack bottom symbol ‘| -’ appearing to the left of the first
            //argument.Operators that clear the argument stack are
            //indicated by the stack bottom symbol ‘| -’ in the result position
            //of the operator definition

            //[NOTE4]:
            //The first stack - clearing operator, which must be one of...

            //  hstem, hstemhm, vstem, vstemhm, cntrmask, 
            //hintmask, hmoveto, vmoveto, rmoveto, or endchar,

            //...
            //takes an additional argument — the width(as
            //described earlier), which may be expressed as zero or one numeric
            //argument

            switch (op)
            {
                default:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                //-------------------------
                //4.1: Path Construction Operators
                case Type2Operator1.rmoveto:
                    {

                        //|- dx1 dy1 rmoveto(21) |-

                        //moves the current point to
                        //a position at the relative coordinates(dx1, dy1) 
                        //see [NOTE4]
                    }
                    break;
                case Type2Operator1.hmoveto:
                    {

                        //|- dx1 hmoveto(22) |-

                        //moves the current point 
                        //dx1 units in the horizontal direction
                        //see [NOTE4]

                        _currentIndex = 0; //clear stack 

                    }
                    break;
                case Type2Operator1.vmoveto:
                    {
                        //|- dy1 vmoveto (4) |-
                        //moves the current point 
                        //dy1 units in the vertical direction.
                        //see [NOTE4]

                    }
                    break;
                case Type2Operator1.rlineto:
                    {
                        //|- {dxa dya}+  rlineto (5) |-

                        //appends a line from the current point to 
                        //a position at the relative coordinates dxa, dya. 

                        //Additional rlineto operations are 
                        //performed for all subsequent argument pairs. 

                        //The number of 
                        //lines is determined from the number of arguments on the stack

                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.hlineto:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.vlineto:
                    {
                        //|- dy1 {dxa dyb}*  vlineto (7) |-
                        //|- {dya dxb}+  vlineto (7) |-

                        //appends a vertical line of length 
                        //dy1 to the current point. 

                        //With an odd number of arguments, subsequent argument pairs are 
                        //interpreted as alternating values of dx and dy, for which additional 
                        //lineto operators draw alternating horizontal and 
                        //vertical lines.

                        //With an even number of arguments, the 
                        //arguments are interpreted as alternating vertical and 
                        //horizontal lines. The number of lines is determined from the 
                        //number of arguments on the stack.

                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.rrcurveto:
                    {


                        //|- {dxa dya dxb dyb dxc dyc}+  rrcurveto (8) |-

                        //appends a Bézier curve, defined by  dy1 to the current point. 
                        //With dxa...dyc, to the current point.

                        //For each subsequent set of six arguments, an additional 
                        //curve is appended to the current point. 

                        //The number of curve segments is determined from 
                        //the number of arguments on the number stack and 
                        //is limited only by the size of the number stack

                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.hhcurveto:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.hvcurveto:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.rcurveline:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.rlinecurve:
                    {
                        _currentIndex = 0; //clear stack 
                    }
                    break;
                case Type2Operator1.vhcurveto:
                    {

                    }
                    break;
                case Type2Operator1.vvcurveto:
                    {

                    }
                    break;
                //-------------------------------------------------------------------
                //4.3 Hint Operators
                case Type2Operator1.hstem:
                    {

                    }
                    break;
                case Type2Operator1.vstem:
                    {

                    }
                    break;
                case Type2Operator1.hstemhm:
                    {

                        //|- y dy {dya dyb}*  hstemhm (18) |-

                        //has the same meaning as 
                        //hstem (1),
                        //except that it must be used 
                        //in place of hstem  if the charstring contains one or more 
                        //hintmask operators.
                        _currentIndex = 0; //clear stack?

                    }
                    break;
                case Type2Operator1.hintmask:
                    {


                    }
                    break;
                case Type2Operator1.cntrmask:
                    {
                        _currentIndex = 0;
                        //|- cntrmask(20 + mask) |-

                        //specifies the counter spaces to be controlled, and their relative
                        //priority.The mask bits in the bytes, following the operator, 
                        //reference the stem hint declarations; the most significant bit of
                        //the first byte refers to the first stem hint declared, through to
                        //the last hint declaration.The counters to be controlled are
                        //those that are delimited by the referenced stem hints.Bits set to
                        //1 in the first cntrmask command have top priority; subsequent
                        //cntrmask commands specify lower priority counters(see Figure
                        //1 and the accompanying example). 
                    }
                    break;

                //-------------------------
                //4.7: Subroutine Operators
                case Type2Operator1.callsubr:
                case Type2Operator1.callgsubr:
                case Type2Operator1._return:
                    break;
            }

            //TEMP, NOT CORRECT
            _currentIndex = 0; //current stack index
        }
        public void Eval(Type2Operator2 op)
        {
            switch (op)
            {
                default: throw new NotSupportedException();
                //-------------------------
                //4.1: Path Construction Operators
                case Type2Operator2.flex:
                    {

                    }
                    break;
                case Type2Operator2.hflex:
                    {

                    }
                    break;
                case Type2Operator2.hflex1:
                    {

                    }
                    break;
                case Type2Operator2.flex1:
                    {

                    }
                    break;//

                //-------------------------
                //4.4: Arithmetic Operators
                case Type2Operator2.abs:
                case Type2Operator2.add:
                case Type2Operator2.sub:
                case Type2Operator2.div:
                case Type2Operator2.neg:
                case Type2Operator2.random:
                case Type2Operator2.mul:
                case Type2Operator2.sqrt:
                case Type2Operator2.drop:
                case Type2Operator2.exch:
                case Type2Operator2.index:
                case Type2Operator2.roll:
                case Type2Operator2.dup:
                    break;

                //-------------------------
                //4.5: Storage Operators
                //The storage operators utilize a transient array and provide 
                //facilities for storing and retrieving transient array data. 

                //The transient array provides non-persistent storage for 
                //intermediate values. 
                //There is no provision to initialize this array, 
                //except explicitly using the put operator, 
                //and values stored in the 
                //array do not persist beyond the scope of rendering an individual 
                //character. 
                case Type2Operator2.put:
                case Type2Operator2.get:
                    break;

                //-------------------------
                //4.6:
                case Type2Operator2.and:
                case Type2Operator2.or:
                case Type2Operator2.not:
                case Type2Operator2.eq:
                case Type2Operator2.ifelse:
                    break;


            }

            //TEMP, NOT CORRECT
            _currentIndex = 0; //current stack index, 
        }


    }

    class Type2CharStringParser : IDisposable
    {
        MemoryStream _msBuffer;
        BinaryReader _reader;
        Type2EvaluationStack _type2EvalStack = new Type2EvaluationStack();


        public Type2CharStringParser()
        {
            _msBuffer = new MemoryStream();
            _reader = new BinaryReader(_msBuffer);
        }
        public void ParseType2CharsString(byte[] buffer)
        {
            //TODO: implement this
            //reset
            _msBuffer.SetLength(0);
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

                if (b0 >= 32 && b0 <= 255)
                {
                    int num = ReadIntegerNumber(b0);
                    _type2EvalStack.Push(num);
                }
                else if (b0 == 0)
                {
                    //reserve
                }
                else if (b0 == 12)
                {
                    // First byte of a 2-byte operator 
                    _type2EvalStack.Eval((Type2Operator2)_reader.ReadByte());
                }
                else if (b0 < 28)
                {
                    _type2EvalStack.Eval((Type2Operator1)b0);
                }
                else if (b0 == 28)
                {
                    //shortint
                    //First byte of a 3-byte sequence specifying a number.
                    _type2EvalStack.Push(_reader.ReadUInt16());
                }
                else if (b0 >= 29 && b0 < 32)
                {
                    //29,30,31
                    _type2EvalStack.Eval((Type2Operator1)b0);
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
            else if (b0 == 255)
            {
                //First byte of a 5-byte sequence specifying a number.
                return _reader.ReadInt32();
            }
            else
            {
                throw new NotSupportedException();
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