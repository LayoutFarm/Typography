//Apache2, 2018, Villu Ruusmann , Apache/PdfBox Authors ( https://github.com/apache/pdfbox)  
//Apache2, 2018-present, WinterDev 

//ref http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Typography.OpenFont.CFF
{


    //The Type 2 Charstring Format
    //...
    //must be used in a CFF (Compact Font Format) or OpenType font 
    //file to create a complete font program



    struct Type2Instruction
    {
        public readonly int Value;
        public readonly OperatorName Op;
        public Type2Instruction(OperatorName op, int value)
        {
            this.Op = op;
            this.Value = value;
#if DEBUG
            _dbug_OnlyOp = false;
#endif
        }
        public Type2Instruction(OperatorName op)
        {
            this.Op = op;
            this.Value = 0;
#if DEBUG
            _dbug_OnlyOp = true;
#endif
        }
#if DEBUG
        bool _dbug_OnlyOp;
        public override string ToString()
        {
            if (_dbug_OnlyOp)
            {
                return Op.ToString();
            }
            else
            {
                switch (Op)
                {
                    case OperatorName.hintmask1:
                    case OperatorName.hintmask2:
                    case OperatorName.hintmask3:
                    case OperatorName.hintmask4:
                    case OperatorName.hintmask_bits:
                        return Op.ToString() + " " + Convert.ToString(Value, 2);
                    default:
                        return Op.ToString() + " " + Value.ToString();
                }

            }

        }
#endif
    }


    class OriginalType2OperatorAttribute : Attribute
    {
        public OriginalType2OperatorAttribute(Type2Operator1 type2Operator1)
        {
        }
        public OriginalType2OperatorAttribute(Type2Operator2 type2Operator1)
        {
        }
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

    /// <summary>
    /// Merged ccf operators,(op1 and op2, note on attribute of each field)
    /// </summary>
    enum OperatorName : byte
    {
        Unknown,
        //
        LoadInt,
        GlyphWidth,
        //---------------------
        //type2Operator1
        //---------------------
        [OriginalType2Operator(Type2Operator1.hstem)] hstem,
        [OriginalType2Operator(Type2Operator1.vstem)] vstem,
        [OriginalType2Operator(Type2Operator1.vmoveto)] vmoveto,
        [OriginalType2Operator(Type2Operator1.rlineto)] rlineto,
        [OriginalType2Operator(Type2Operator1.hlineto)] hlineto,
        [OriginalType2Operator(Type2Operator1.vlineto)] vlineto,
        [OriginalType2Operator(Type2Operator1.rrcurveto)] rrcurveto,
        [OriginalType2Operator(Type2Operator1.callsubr)] callsubr,
        //---------------------
        [OriginalType2Operator(Type2Operator1._return)] _return,
        //[OriginalType2Operator(Type2Operator1.escape)] escape, //not used!
        [OriginalType2Operator(Type2Operator1.endchar)] endchar,
        [OriginalType2Operator(Type2Operator1.hstemhm)] hstemhm,

        //---------
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask1, //my hint-mask extension, contains 1 byte hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask2, //my hint-mask extension, contains 2 bytes hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask3, //my hint-mask extension, contains 3 bytes hint
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask4, //my hint-mask extension, contains 4 bytes hint 
        [OriginalType2Operator(Type2Operator1.hintmask)] hintmask_bits,//my hint-mask extension, contains n bits of hint

        //---------

        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask1, //my counter-mask extension, contains 1 byte hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask2, //my counter-mask extension, contains 2 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask3, //my counter-mask extension, contains 3 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask4, //my counter-mask extension, contains 4 bytes hint
        [OriginalType2Operator(Type2Operator1.cntrmask)] cntrmask_bits, //my counter-mask extension, contains n bits of hint

        //---------------------
        [OriginalType2Operator(Type2Operator1.rmoveto)] rmoveto,
        [OriginalType2Operator(Type2Operator1.hmoveto)] hmoveto,
        [OriginalType2Operator(Type2Operator1.vstemhm)] vstemhm,
        [OriginalType2Operator(Type2Operator1.rcurveline)] rcurveline,
        [OriginalType2Operator(Type2Operator1.rlinecurve)] rlinecurve,
        [OriginalType2Operator(Type2Operator1.vvcurveto)] vvcurveto,
        [OriginalType2Operator(Type2Operator1.hhcurveto)] hhcurveto,
        [OriginalType2Operator(Type2Operator1.shortint)] shortint,
        [OriginalType2Operator(Type2Operator1.callgsubr)] callgsubr,
        [OriginalType2Operator(Type2Operator1.vhcurveto)] vhcurveto,
        //-----------------------
        [OriginalType2Operator(Type2Operator1.hvcurveto)] hvcurveto,
        //--------------------- 
        //Two-byte Type 2 Operators 
        [OriginalType2Operator(Type2Operator2.and)] and,
        [OriginalType2Operator(Type2Operator2.or)] or,
        [OriginalType2Operator(Type2Operator2.not)] not,
        [OriginalType2Operator(Type2Operator2.abs)] abs,
        [OriginalType2Operator(Type2Operator2.add)] add,
        //------------------
        [OriginalType2Operator(Type2Operator2.sub)] sub,
        [OriginalType2Operator(Type2Operator2.div)] div,
        [OriginalType2Operator(Type2Operator2.neg)] neg,
        [OriginalType2Operator(Type2Operator2.eq)] eq,
        [OriginalType2Operator(Type2Operator2.drop)] drop,
        [OriginalType2Operator(Type2Operator2.put)] put,
        //------------------ 
        [OriginalType2Operator(Type2Operator2.get)] get,
        [OriginalType2Operator(Type2Operator2.ifelse)] ifelse,
        [OriginalType2Operator(Type2Operator2.random)] random,
        [OriginalType2Operator(Type2Operator2.mul)] mul,
        [OriginalType2Operator(Type2Operator2.sqrt)] sqrt,
        [OriginalType2Operator(Type2Operator2.dup)] dup,
        [OriginalType2Operator(Type2Operator2.exch)] exch,
        [OriginalType2Operator(Type2Operator2.index)] index,
        [OriginalType2Operator(Type2Operator2.roll)] roll,
        [OriginalType2Operator(Type2Operator2.hflex)] hflex,
        [OriginalType2Operator(Type2Operator2.flex)] flex,
        [OriginalType2Operator(Type2Operator2.hflex1)] hflex1,
        [OriginalType2Operator(Type2Operator2.flex1)] flex1
    }




    enum Type2GlyphInstructionListKind
    {
        GlyphDescription,
        LocalSubroutine,
        GlobalSubroutine,
    }

    class Type2GlyphInstructionList
    {

        List<Type2Instruction> insts;
        public Type2GlyphInstructionList()
        {
            this.insts = new List<Type2Instruction>();
        }
        public List<Type2Instruction> Insts
        {
            get { return insts; }
        }

        public Type2GlyphInstructionListKind Kind
        {
            get;
            set;
        }

        public void AddInt(int intValue)
        {
#if DEBUG
            debugCheck();
#endif
            insts.Add(new Type2Instruction(OperatorName.LoadInt, intValue));
        }
        public void AddOp(OperatorName opName)
        {
#if DEBUG
            debugCheck();
#endif
            insts.Add(new Type2Instruction(opName));
        }
        public void AddOp(OperatorName opName, int value)
        {
#if DEBUG
            debugCheck();
#endif
            insts.Add(new Type2Instruction(opName, value));
        }
        internal void ChangeFirstInstToGlyphWidthValue()
        {
            //check the first element must be loadint
            Type2Instruction firstInst = insts[0];
            if (firstInst.Op != OperatorName.LoadInt) { throw new NotSupportedException(); }
            //the replace
            insts[0] = new Type2Instruction(OperatorName.GlyphWidth, firstInst.Value);
        }
#if DEBUG
        void debugCheck()
        {
            if (this._dbugMark == 5 && insts.Count > 50)
            {

            }
        }
        public int dbugInstCount { get { return insts.Count; } }
        int _dbugMark;
        public int dbugMark
        {
            get { return _dbugMark; }
            set
            {
                _dbugMark = value;
                //if (value == 7)
                //{
                //    Type2CharStringParser.dbugDumpInstructionListToFile(insts, "d:\\WImageTest\\test_type2.txt");

                //}
            }
        }

        internal void dbugDumpInstructionListToFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (StreamWriter w = new StreamWriter(fs))
            {

                int j = insts.Count;
                for (int i = 0; i < j; ++i)
                {
                    Type2Instruction inst = insts[i];

                    w.Write("[" + i + "] ");
                    if (inst.Op == OperatorName.LoadInt)
                    {
                        w.Write(inst.Value.ToString());
                        w.Write(' ');
                    }
                    else
                    {
                        w.Write(inst.ToString());
                        w.WriteLine();
                    }

                }
            }
        }
#endif

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

#if DEBUG 
        int _dbugCount = 0;
        int _dbugInstructionListMark = 0;
#endif

        bool foundSomeStem = false;
        Type2GlyphInstructionList insts;
        int current_int_count = 0;
        bool doStemCount = true;

        public Type2GlyphInstructionList ParseType2CharString(byte[] buffer)
        {
            //reset
            current_int_count = 0;
            foundSomeStem = false;
            doStemCount = true;
            _msBuffer.SetLength(0);
            _msBuffer.Position = 0;
            _msBuffer.Write(buffer, 0, buffer.Length);
            _msBuffer.Position = 0;
            int len = buffer.Length;
            //
            insts = new Type2GlyphInstructionList();
#if DEBUG
            insts.dbugMark = _dbugInstructionListMark;
            //if (_dbugInstructionListMark == 5)
            //{

            //}
            _dbugInstructionListMark++;
#endif

            byte b0 = 0;
            int hintStemCount = 0;

            bool cont = true;

            while (cont && _reader.BaseStream.Position < len)
            {
                b0 = _reader.ReadByte();
#if DEBUG
                //easy for debugging here
                _dbugCount++;
                if (b0 < 32)
                {

                }
#endif
                switch (b0)
                {
                    default: //else 32 -255
                        {
                            if (b0 < 32)
                            {
                                Debug.WriteLine("err!:" + b0);
                                return null;
                            }
                            insts.AddInt(ReadIntegerNumber(b0));
                            if (doStemCount)
                            {
                                current_int_count++;
                            }
                        }
                        break;
                    case (byte)Type2Operator1.shortint: // 28

                        //shortint
                        //First byte of a 3-byte sequence specifying a number.
                        //a ShortInt value is specified by using the operator (28) followed by two bytes
                        //which represent numbers between –32768 and + 32767.The
                        //most significant byte follows the(28)
                        byte s_b0 = _reader.ReadByte();
                        byte s_b1 = _reader.ReadByte();
                        insts.AddInt((short)((s_b0 << 8) | (s_b1)));
                        //
                        if (doStemCount)
                        {
                            current_int_count++;
                        }
                        break;
                    //---------------------------------------------------
                    case (byte)Type2Operator1._Reserved0_://???
                    case (byte)Type2Operator1._Reserved2_://???
                    case (byte)Type2Operator1._Reserved9_://???
                    case (byte)Type2Operator1._Reserved13_://???
                    case (byte)Type2Operator1._Reserved15_://???
                    case (byte)Type2Operator1._Reserved16_: //???
                    case (byte)Type2Operator1._Reserved17_: //???
                        //reserved, do nothing ?
                        break;
                    case (byte)Type2Operator1.endchar:
                        insts.AddOp(OperatorName.endchar);
                        cont = false;
                        //when we found end char
                        //stop reading this...
                        break;
                    case (byte)Type2Operator1.escape: //12
                        {

                            b0 = _reader.ReadByte();
                            switch ((Type2Operator2)b0)
                            {
                                default:
                                    if (b0 <= 38)
                                    {
                                        Debug.WriteLine("err!:" + b0);
                                        return null;
                                    }
                                    break;
                                //-------------------------
                                //4.1: Path Construction Operators
                                case Type2Operator2.flex: insts.AddOp(OperatorName.flex); break;
                                case Type2Operator2.hflex: insts.AddOp(OperatorName.hflex); break;
                                case Type2Operator2.hflex1: insts.AddOp(OperatorName.hflex1); break;
                                case Type2Operator2.flex1: insts.AddOp(OperatorName.flex1); ; break;
                                //-------------------------
                                //4.4: Arithmetic Operators
                                case Type2Operator2.abs: insts.AddOp(OperatorName.abs); break;
                                case Type2Operator2.add: insts.AddOp(OperatorName.add); break;
                                case Type2Operator2.sub: insts.AddOp(OperatorName.sub); break;
                                case Type2Operator2.div: insts.AddOp(OperatorName.div); break;
                                case Type2Operator2.neg: insts.AddOp(OperatorName.neg); break;
                                case Type2Operator2.random: insts.AddOp(OperatorName.random); break;
                                case Type2Operator2.mul: insts.AddOp(OperatorName.mul); break;
                                case Type2Operator2.sqrt: insts.AddOp(OperatorName.sqrt); break;
                                case Type2Operator2.drop: insts.AddOp(OperatorName.drop); break;
                                case Type2Operator2.exch: insts.AddOp(OperatorName.exch); break;
                                case Type2Operator2.index: insts.AddOp(OperatorName.index); break;
                                case Type2Operator2.roll: insts.AddOp(OperatorName.roll); break;
                                case Type2Operator2.dup: insts.AddOp(OperatorName.dup); break;

                                //-------------------------
                                //4.5: Storage Operators 
                                case Type2Operator2.put: insts.AddOp(OperatorName.put); break;
                                case Type2Operator2.get: insts.AddOp(OperatorName.get); break;
                                //-------------------------
                                //4.6: Conditional
                                case Type2Operator2.and: insts.AddOp(OperatorName.and); break;
                                case Type2Operator2.or: insts.AddOp(OperatorName.or); break;
                                case Type2Operator2.not: insts.AddOp(OperatorName.not); break;
                                case Type2Operator2.eq: insts.AddOp(OperatorName.eq); break;
                                case Type2Operator2.ifelse: insts.AddOp(OperatorName.ifelse); break;
                            }

                            StopStemCount();
                        }
                        break;
                    case (byte)Type2Operator1.rmoveto: insts.AddOp(OperatorName.rmoveto); StopStemCount(); break;
                    case (byte)Type2Operator1.hmoveto: insts.AddOp(OperatorName.hmoveto); StopStemCount(); break;
                    case (byte)Type2Operator1.vmoveto: insts.AddOp(OperatorName.vmoveto); StopStemCount(); break;
                    //---------------------------------------------------------------------------
                    case (byte)Type2Operator1.rlineto: insts.AddOp(OperatorName.rlineto); StopStemCount(); break;
                    case (byte)Type2Operator1.hlineto: insts.AddOp(OperatorName.hlineto); StopStemCount(); break;
                    case (byte)Type2Operator1.vlineto: insts.AddOp(OperatorName.vlineto); StopStemCount(); break;
                    case (byte)Type2Operator1.rrcurveto: insts.AddOp(OperatorName.rrcurveto); StopStemCount(); break;
                    case (byte)Type2Operator1.hhcurveto: insts.AddOp(OperatorName.hhcurveto); StopStemCount(); break;
                    case (byte)Type2Operator1.hvcurveto: insts.AddOp(OperatorName.hvcurveto); StopStemCount(); break;
                    case (byte)Type2Operator1.rcurveline: insts.AddOp(OperatorName.rcurveline); StopStemCount(); break;
                    case (byte)Type2Operator1.rlinecurve: insts.AddOp(OperatorName.rlinecurve); StopStemCount(); break;
                    case (byte)Type2Operator1.vhcurveto: insts.AddOp(OperatorName.vhcurveto); StopStemCount(); break;
                    case (byte)Type2Operator1.vvcurveto: insts.AddOp(OperatorName.vvcurveto); StopStemCount(); break;
                    //-------------------------------------------------------------------
                    //4.3 Hint Operators
                    case (byte)Type2Operator1.hstem: AddStemToList(OperatorName.hstem, ref hintStemCount); break;
                    case (byte)Type2Operator1.vstem: AddStemToList(OperatorName.vstem, ref hintStemCount); break;
                    case (byte)Type2Operator1.vstemhm: AddStemToList(OperatorName.vstemhm, ref hintStemCount); break;
                    case (byte)Type2Operator1.hstemhm: AddStemToList(OperatorName.hstemhm, ref hintStemCount); break;
                    //-------------------------------------------------------------------
                    case (byte)Type2Operator1.hintmask: AddHintMaskToList(_reader, ref hintStemCount); StopStemCount(); break;
                    case (byte)Type2Operator1.cntrmask: AddCounterMaskToList(_reader, ref hintStemCount); StopStemCount(); break;
                    //-------------------------------------------------------------------
                    //4.7: Subroutine Operators
                    case (byte)Type2Operator1.callsubr: insts.AddOp(OperatorName.callsubr); StopStemCount(); break;
                    case (byte)Type2Operator1.callgsubr: insts.AddOp(OperatorName.callgsubr); StopStemCount(); break;
                    case (byte)Type2Operator1._return: insts.AddOp(OperatorName._return); StopStemCount(); break;
                }
            }
            return insts;
        }

        void StopStemCount()
        {
            current_int_count = 0;
            doStemCount = false;
        }
        OperatorName _latestOpName = OperatorName.Unknown;
        void AddStemToList(OperatorName stemName, ref int hintStemCount)
        {
            //support 4 kinds 

            //1. 
            //|- y dy {dya dyb}*  hstemhm (18) |-
            //2.
            //|- x dx {dxa dxb}* vstemhm (23) |-
            //3.
            //|- y dy {dya dyb}*  hstem (1) |-
            //4. 
            //|- x dx {dxa dxb}*  vstem (3) |- 
            //-----------------------

            //notes
            //The sequence and form of a Type 2 charstring program may be
            //represented as:
            //w? { hs* vs*cm * hm * mt subpath}? { mt subpath} *endchar 

            if ((current_int_count % 2) != 0)
            {
                //all kind has even number of stem               

                if (foundSomeStem)
                {
#if DEBUG
                    insts.dbugDumpInstructionListToFile("d:\\WImageTest\\test_type2_" + (_dbugInstructionListMark - 1) + ".txt");
#endif
                    throw new NotSupportedException();
                }
                else
                {
                    //the first one is 'width'
                    insts.ChangeFirstInstToGlyphWidthValue();
                    current_int_count--;
                }
            }
            hintStemCount += (current_int_count / 2); //save a snapshot of stem count
            insts.AddOp(stemName);
            current_int_count = 0;//clear
            foundSomeStem = true;
            _latestOpName = stemName;
        }
        void AddHintMaskToList(BinaryReader reader, ref int hintStemCount)
        {


            if (foundSomeStem && current_int_count > 0)
            {

                //type2 5177.pdf
                //...
                //If hstem and vstem hints are both declared at the beginning of
                //a charstring, and this sequence is followed directly by the
                //hintmask or cntrmask operators, ...
                //the vstem hint operator need not be included ***

#if DEBUG
                if ((current_int_count % 2) != 0)
                {
                    throw new NotSupportedException();
                }
                else
                {

                }
#endif

                switch (_latestOpName)
                {
                    case OperatorName.hstem:
                        //add vstem  ***( from reason above)

                        hintStemCount += (current_int_count / 2); //save a snapshot of stem count
                        insts.AddOp(OperatorName.vstem);

                        _latestOpName = OperatorName.vstem;
                        current_int_count = 0; //clear
                        break;
                    case OperatorName.hstemhm:
                        //add vstem  ***( from reason above) ??
                        hintStemCount += (current_int_count / 2); //save a snapshot of stem count
                        insts.AddOp(OperatorName.vstem);
                        _latestOpName = OperatorName.vstem;
                        current_int_count = 0;//clear
                        break;
                    case OperatorName.vstemhm:
                        //-------
                        //TODO: review here? 
                        //found this in xits.otf
                        hintStemCount += (current_int_count / 2); //save a snapshot of stem count
                        insts.AddOp(OperatorName.vstem);
                        _latestOpName = OperatorName.vstem;
                        current_int_count = 0;//clear
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (hintStemCount == 0)
            {
                if (!foundSomeStem)
                {
                    hintStemCount = (current_int_count / 2);
                    foundSomeStem = true;//?
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            //---------------------- 
            //this is my hintmask extension, => to fit with our Evaluation stack
            int properNumberOfMaskBytes = (hintStemCount + 7) / 8;
            if (_reader.BaseStream.Position + properNumberOfMaskBytes >= _reader.BaseStream.Length)
            {
                throw new NotSupportedException();
            }
            if (properNumberOfMaskBytes > 4)
            {
                int remaining = properNumberOfMaskBytes;

                for (; remaining > 3;)
                {
                    insts.AddInt((
                       (_reader.ReadByte() << 24) |
                       (_reader.ReadByte() << 16) |
                       (_reader.ReadByte() << 8) |
                       (_reader.ReadByte())
                       ));
                    remaining -= 4; //*** 
                }
                switch (remaining)
                {
                    case 0:
                        //do nothing
                        break;
                    case 1:
                        insts.AddInt(_reader.ReadByte() << 24);
                        break;
                    case 2:
                        insts.AddInt(
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16));

                        break;
                    case 3:
                        insts.AddInt(
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8));
                        break;
                    default: throw new NotSupportedException();//should not occur !
                }

                insts.AddOp(OperatorName.hintmask_bits, properNumberOfMaskBytes);
            }
            else
            {
                //last remaining <4 bytes 
                switch (properNumberOfMaskBytes)
                {
                    case 0:
                    default: throw new NotSupportedException();//should not occur !
                    case 1:
                        insts.AddOp(OperatorName.hintmask1, (_reader.ReadByte() << 24));
                        break;
                    case 2:
                        insts.AddOp(OperatorName.hintmask2,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)
                            );
                        break;
                    case 3:
                        insts.AddOp(OperatorName.hintmask3,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)
                            );
                        break;
                    case 4:
                        insts.AddOp(OperatorName.hintmask4,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8) |
                            (_reader.ReadByte())
                            );
                        break;
                }
            }

        }
        void AddCounterMaskToList(BinaryReader reader, ref int hintStemCount)
        {
            if (hintStemCount == 0)
            {
                if (!foundSomeStem)
                {
                    //????
                    hintStemCount = (current_int_count / 2);
                    foundSomeStem = true;//?
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            //---------------------- 
            //this is my hintmask extension, => to fit with our Evaluation stack
            int properNumberOfMaskBytes = (hintStemCount + 7) / 8;
            if (_reader.BaseStream.Position + properNumberOfMaskBytes >= _reader.BaseStream.Length)
            {
                throw new NotSupportedException();
            }

            if (properNumberOfMaskBytes > 4)
            {
                int remaining = properNumberOfMaskBytes;

                for (; remaining > 3;)
                {
                    insts.AddInt((
                       (_reader.ReadByte() << 24) |
                       (_reader.ReadByte() << 16) |
                       (_reader.ReadByte() << 8) |
                       (_reader.ReadByte())
                       ));
                    remaining -= 4; //*** 
                }
                switch (remaining)
                {
                    case 0:
                        //do nothing
                        break;
                    case 1:
                        insts.AddInt(_reader.ReadByte() << 24);
                        break;
                    case 2:
                        insts.AddInt(
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16));

                        break;
                    case 3:
                        insts.AddInt(
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8));
                        break;
                    default: throw new NotSupportedException();//should not occur !
                }

                insts.AddOp(OperatorName.cntrmask_bits, properNumberOfMaskBytes);
            }
            else
            {
                //last remaining <4 bytes 
                switch (properNumberOfMaskBytes)
                {
                    case 0:
                    default: throw new NotSupportedException();//should not occur !
                    case 1:
                        insts.AddOp(OperatorName.cntrmask1, (_reader.ReadByte() << 24));
                        break;
                    case 2:
                        insts.AddOp(OperatorName.cntrmask2,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)
                            );
                        break;
                    case 3:
                        insts.AddOp(OperatorName.cntrmask3,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)
                            );
                        break;
                    case 4:
                        insts.AddOp(OperatorName.cntrmask4,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8) |
                            (_reader.ReadByte())
                            );
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
            else if (b0 <= 250)  // && b0 >= 247 , *** if-else sequence is important! ***
            {
                int b1 = _reader.ReadByte();
                return (b0 - 247) * 256 + b1 + 108;
            }
            else if (b0 <= 254)  //&&  b0 >= 251 ,*** if-else sequence is important! ***
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