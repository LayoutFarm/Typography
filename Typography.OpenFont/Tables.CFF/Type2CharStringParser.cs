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

        //-------------------------
        //4.1: Path Construction Operators

        /// <summary>
        /// rmoveto
        /// </summary>
        public void R_MoveTo()
        {
            //|- dx1 dy1 rmoveto(21) |-

            //moves the current point to
            //a position at the relative coordinates(dx1, dy1) 
            //see [NOTE4]
        }
        /// <summary>
        /// hmoveto
        /// </summary>
        public void H_MoveTo()
        {
            //|- dx1 hmoveto(22) |-

            //moves the current point 
            //dx1 units in the horizontal direction
            //see [NOTE4]

            _currentIndex = 0; //clear stack 
        }
        public void V_MoveTo()
        {
            //|- dy1 vmoveto (4) |-
            //moves the current point 
            //dy1 units in the vertical direction.
            //see [NOTE4]
        }
        public void R_LineTo()
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
        public void H_LineTo()
        {
            _currentIndex = 0; //clear stack 
        }
        public void V_LineTo()
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

        public void RR_CurveTo()
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
        public void HH_CurveTo()
        {
            _currentIndex = 0; //clear stack 
        }
        public void HV_CurveTo()
        {
            _currentIndex = 0; //clear stack 
        }
        public void R_CurveLine()
        {
            _currentIndex = 0; //clear stack 
        }
        public void R_LineCurve()
        {
            _currentIndex = 0; //clear stack 
        }
        public void VH_CurveTo()
        {

        }
        public void VV_CurveTo()
        {

        }


        public void Flex()
        {

        }
        public void H_Flex()
        {

        }
        public void H_Flex1()
        {

        }
        public void Flex1()
        {

        }


        //-------------------------------------------------------------------
        //4.3 Hint Operators
        public void H_Stem()
        {

        }
        public void V_Stem()
        {

        }
        public void H_StemHM()
        {
            //|- y dy {dya dyb}*  hstemhm (18) |-

            //has the same meaning as 
            //hstem (1),
            //except that it must be used 
            //in place of hstem  if the charstring contains one or more 
            //hintmask operators.
            _currentIndex = 0; //clear stack?
        }
        public void HintMask()
        {

        }
        public void CounterSpaceMask()
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

        //4.4: Arithmetic Operators

        //case Type2Operator2.abs:
        //                case Type2Operator2.add:
        //                case Type2Operator2.sub:
        //                case Type2Operator2.div:
        //                case Type2Operator2.neg:
        //                case Type2Operator2.random:
        //                case Type2Operator2.mul:
        //                case Type2Operator2.sqrt:
        //                case Type2Operator2.drop:
        //                case Type2Operator2.exch:
        //                case Type2Operator2.index:
        //                case Type2Operator2.roll:
        //                case Type2Operator2.dup:

        public void Op_Abs() { }
        public void Op_Add()
        {

        }
        public void Op_Sub() { }
        public void Op_Div() { }
        public void Op_Neg() { }
        public void Op_Random() { }
        public void Op_Mul() { }
        public void Op_Sqrt() { }
        public void Op_Drop() { }
        public void Op_Exch() { }
        public void Op_Index() { }
        public void Op_Roll() { }
        public void Op_Dup() { }


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

        public void Put() { }
        public void Get() { }

        //-------------------------
        //4.6: Conditional 


        public void Op_And() { }
        public void Op_Or() { }
        public void Op_Not() { }
        public void Op_Eq() { }
        public void Op_IfElse() { }


        //-------------------------
        //4.7: Subroutine Operators
        public void CallSubr()
        {

        }
        public void CallGSubr()
        {

        }
        //---------------------

        //4.6



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


                switch (b0)
                {
                    default: //else 32 -255
                        {

#if DEBUG
                            if (b0 < 32) throw new Exception();
#endif

                            int num = ReadIntegerNumber(b0);
                            _type2EvalStack.Push(num);
                        }
                        break;
                    case 0:
                        //reserve, do nothing
                        break;
                    case (byte)Type2Operator1.shortint: // 28

                        //shortint
                        //First byte of a 3-byte sequence specifying a number.
                        _type2EvalStack.Push(_reader.ReadUInt16());

                        break;
                    case (byte)Type2Operator1.escape: //12
                        {
                            b0 = _reader.ReadByte();
                            switch ((Type2Operator2)b0)
                            {
                                default: throw new NotSupportedException();
                                //-------------------------
                                //4.1: Path Construction Operators
                                case Type2Operator2.flex: _type2EvalStack.Flex(); break;
                                case Type2Operator2.hflex: _type2EvalStack.H_Flex(); break;
                                case Type2Operator2.hflex1: _type2EvalStack.H_Flex1(); break;
                                case Type2Operator2.flex1: _type2EvalStack.Flex1(); break;
                                //-------------------------
                                //4.4: Arithmetic Operators
                                case Type2Operator2.abs: _type2EvalStack.Op_Abs(); break;
                                case Type2Operator2.add: _type2EvalStack.Op_Add(); break;
                                case Type2Operator2.sub: _type2EvalStack.Op_Sub(); break;
                                case Type2Operator2.div: _type2EvalStack.Op_Div(); break;
                                case Type2Operator2.neg: _type2EvalStack.Op_Neg(); break;
                                case Type2Operator2.random: _type2EvalStack.Op_Random(); break;
                                case Type2Operator2.mul: _type2EvalStack.Op_Mul(); break;
                                case Type2Operator2.sqrt: _type2EvalStack.Op_Sqrt(); break;
                                case Type2Operator2.drop: _type2EvalStack.Op_Drop(); break;
                                case Type2Operator2.exch: _type2EvalStack.Op_Exch(); break;
                                case Type2Operator2.index: _type2EvalStack.Op_Index(); break;
                                case Type2Operator2.roll: _type2EvalStack.Op_Roll(); break;
                                case Type2Operator2.dup: _type2EvalStack.Op_Dup(); break;

                                //-------------------------
                                //4.5: Storage Operators 
                                case Type2Operator2.put: _type2EvalStack.Put(); break;
                                case Type2Operator2.get: _type2EvalStack.Get(); break;
                                //-------------------------
                                //4.6: Conditional
                                case Type2Operator2.and: _type2EvalStack.Op_And(); break;
                                case Type2Operator2.or: _type2EvalStack.Op_Or(); break;
                                case Type2Operator2.not: _type2EvalStack.Op_Not(); break;
                                case Type2Operator2.eq: _type2EvalStack.Op_Eq(); break;
                                case Type2Operator2.ifelse: _type2EvalStack.Op_IfElse(); break;
                            }
                        }
                        break;
                    case (byte)Type2Operator1.vmoveto: _type2EvalStack.V_MoveTo(); break;
                    case (byte)Type2Operator1.rlineto: _type2EvalStack.R_LineTo(); break;
                    case (byte)Type2Operator1.hlineto: _type2EvalStack.H_LineTo(); break;
                    case (byte)Type2Operator1.vlineto: _type2EvalStack.V_LineTo(); break;
                    case (byte)Type2Operator1.rrcurveto: _type2EvalStack.RR_CurveTo(); break;
                    case (byte)Type2Operator1.hhcurveto: _type2EvalStack.HH_CurveTo(); break;
                    case (byte)Type2Operator1.hvcurveto: _type2EvalStack.HV_CurveTo(); break;
                    case (byte)Type2Operator1.rcurveline: _type2EvalStack.R_CurveLine(); break;
                    case (byte)Type2Operator1.rlinecurve: _type2EvalStack.R_LineCurve(); break;
                    case (byte)Type2Operator1.vhcurveto: _type2EvalStack.VH_CurveTo(); break;
                    case (byte)Type2Operator1.vvcurveto: _type2EvalStack.VV_CurveTo(); break;
                    //-------------------------------------------------------------------
                    //4.3 Hint Operators
                    case (byte)Type2Operator1.hstem: _type2EvalStack.H_Stem(); break;
                    case (byte)Type2Operator1.vstem: _type2EvalStack.V_Stem(); break;
                    case (byte)Type2Operator1.hstemhm: _type2EvalStack.H_StemHM(); break;
                    case (byte)Type2Operator1.hintmask: _type2EvalStack.HintMask(); break;
                    case (byte)Type2Operator1.cntrmask: _type2EvalStack.CounterSpaceMask(); break;
                    //-------------------------
                    //4.7: Subroutine Operators
                    case (byte)Type2Operator1.callsubr: _type2EvalStack.CallSubr(); break;
                    case (byte)Type2Operator1.callgsubr: _type2EvalStack.CallGSubr(); break;
                    case (byte)Type2Operator1._return:

                        break;
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