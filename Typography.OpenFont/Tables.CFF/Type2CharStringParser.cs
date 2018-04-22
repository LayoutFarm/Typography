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
        public Type2GlyphInstructionList(List<Type2Instruction> insts)
        {
            this.insts = insts;
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

#if DEBUG
        public int dbugMark;
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

        public Type2GlyphInstructionList ParseType2CharString(byte[] buffer)
        {
            //TODO: implement this
            foundSomeStem = false;
            _msBuffer.SetLength(0);
            _msBuffer.Position = 0;
            _msBuffer.Write(buffer, 0, buffer.Length);
            _msBuffer.Position = 0;
            int len = buffer.Length;
            var insts = new List<Type2Instruction>();
#if DEBUG
            //if (_dbugInstructionListMark == 20)
            //{

            //}
            _dbugInstructionListMark++;
#endif

            byte b0 = 0;
            int hintStemCount = 0;
            int current_stem_Count = 0;

            bool cont = true;

            while (cont && _reader.BaseStream.Position < len)
            {

#if DEBUG
                _dbugCount++;
#endif

                switch (b0 = _reader.ReadByte())
                {
                    default: //else 32 -255
                        {

#if DEBUG
                            if (b0 < 32)
                            {
                                Console.WriteLine("err!:" + b0);
                                return null;
                            }
                            try
                            {
                                insts.Add(new Type2Instruction(OperatorName.LoadInt, ReadIntegerNumber(b0)));
                                current_stem_Count++;
                            }
                            catch
                            {
                                //dbugDumpInstructionListToFile(insts, "d:\\WImageTest\\test_type2_" + (_dbugInstructionListMark - 1) + ".txt");
                            }
#else
                            insts.Add(new Type2Instruction(OperatorName.LoadInt, ReadIntegerNumber(b0)));
                            load_intCount++;
                            current_stem_Count++;
#endif

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
                        insts.Add(new Type2Instruction(OperatorName.LoadInt, (s_b0 << 8 | s_b1)));
                        current_stem_Count++;
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
                        insts.Add(new Type2Instruction(OperatorName.endchar));
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
                                        Console.WriteLine("err!:" + b0);
                                        return null;
                                    }
                                    break;
                                //-------------------------
                                //4.1: Path Construction Operators
                                case Type2Operator2.flex: insts.Add(new Type2Instruction(OperatorName.flex)); break;
                                case Type2Operator2.hflex: insts.Add(new Type2Instruction(OperatorName.hflex)); break;
                                case Type2Operator2.hflex1: insts.Add(new Type2Instruction(OperatorName.hflex1)); break;
                                case Type2Operator2.flex1: insts.Add(new Type2Instruction(OperatorName.flex1)); ; break;
                                //-------------------------
                                //4.4: Arithmetic Operators
                                case Type2Operator2.abs: insts.Add(new Type2Instruction(OperatorName.abs)); break;
                                case Type2Operator2.add: insts.Add(new Type2Instruction(OperatorName.add)); break;
                                case Type2Operator2.sub: insts.Add(new Type2Instruction(OperatorName.sub)); break;
                                case Type2Operator2.div: insts.Add(new Type2Instruction(OperatorName.div)); break;
                                case Type2Operator2.neg: insts.Add(new Type2Instruction(OperatorName.neg)); break;
                                case Type2Operator2.random: insts.Add(new Type2Instruction(OperatorName.random)); break;
                                case Type2Operator2.mul: insts.Add(new Type2Instruction(OperatorName.mul)); break;
                                case Type2Operator2.sqrt: insts.Add(new Type2Instruction(OperatorName.sqrt)); break;
                                case Type2Operator2.drop: insts.Add(new Type2Instruction(OperatorName.drop)); break;
                                case Type2Operator2.exch: insts.Add(new Type2Instruction(OperatorName.exch)); break;
                                case Type2Operator2.index: insts.Add(new Type2Instruction(OperatorName.index)); break;
                                case Type2Operator2.roll: insts.Add(new Type2Instruction(OperatorName.roll)); break;
                                case Type2Operator2.dup: insts.Add(new Type2Instruction(OperatorName.dup)); break;

                                //-------------------------
                                //4.5: Storage Operators 
                                case Type2Operator2.put: insts.Add(new Type2Instruction(OperatorName.put)); break;
                                case Type2Operator2.get: insts.Add(new Type2Instruction(OperatorName.get)); break;
                                //-------------------------
                                //4.6: Conditional
                                case Type2Operator2.and: insts.Add(new Type2Instruction(OperatorName.and)); break;
                                case Type2Operator2.or: insts.Add(new Type2Instruction(OperatorName.or)); break;
                                case Type2Operator2.not: insts.Add(new Type2Instruction(OperatorName.not)); break;
                                case Type2Operator2.eq: insts.Add(new Type2Instruction(OperatorName.eq)); break;
                                case Type2Operator2.ifelse: insts.Add(new Type2Instruction(OperatorName.ifelse)); break;
                            }
                        }
                        break;
                    case (byte)Type2Operator1.rmoveto: insts.Add(new Type2Instruction(OperatorName.rmoveto)); break;
                    case (byte)Type2Operator1.hmoveto: insts.Add(new Type2Instruction(OperatorName.hmoveto)); break;
                    case (byte)Type2Operator1.vmoveto: insts.Add(new Type2Instruction(OperatorName.vmoveto)); break;
                    case (byte)Type2Operator1.rlineto: insts.Add(new Type2Instruction(OperatorName.rlineto)); break;
                    case (byte)Type2Operator1.hlineto: insts.Add(new Type2Instruction(OperatorName.hlineto)); break;
                    case (byte)Type2Operator1.vlineto: insts.Add(new Type2Instruction(OperatorName.vlineto)); break;
                    case (byte)Type2Operator1.rrcurveto: insts.Add(new Type2Instruction(OperatorName.rrcurveto)); break;
                    case (byte)Type2Operator1.hhcurveto: insts.Add(new Type2Instruction(OperatorName.hhcurveto)); break;
                    case (byte)Type2Operator1.hvcurveto: insts.Add(new Type2Instruction(OperatorName.hvcurveto)); break;
                    case (byte)Type2Operator1.rcurveline: insts.Add(new Type2Instruction(OperatorName.rcurveline)); break;
                    case (byte)Type2Operator1.rlinecurve: insts.Add(new Type2Instruction(OperatorName.rlinecurve)); break;
                    case (byte)Type2Operator1.vhcurveto: insts.Add(new Type2Instruction(OperatorName.vhcurveto)); break;
                    case (byte)Type2Operator1.vvcurveto: insts.Add(new Type2Instruction(OperatorName.vvcurveto)); break;
                    //-------------------------------------------------------------------
                    //4.3 Hint Operators
                    case (byte)Type2Operator1.hstem: AddStemToList(insts, OperatorName.hstem, ref hintStemCount, ref current_stem_Count); break;
                    case (byte)Type2Operator1.vstem: AddStemToList(insts, OperatorName.vstem, ref hintStemCount, ref current_stem_Count); break;
                    case (byte)Type2Operator1.vstemhm: AddStemToList(insts, OperatorName.vstemhm, ref hintStemCount, ref current_stem_Count); break;
                    case (byte)Type2Operator1.hstemhm: AddStemToList(insts, OperatorName.hstemhm, ref hintStemCount, ref current_stem_Count); break;
                    case (byte)Type2Operator1.hintmask: AddHintMaskToList(insts, _reader, ref hintStemCount, ref current_stem_Count); break;
                    case (byte)Type2Operator1.cntrmask:


                        break;
                    //-------------------------
                    //4.7: Subroutine Operators
                    case (byte)Type2Operator1.callsubr: insts.Add(new Type2Instruction(OperatorName.callsubr)); break;
                    case (byte)Type2Operator1.callgsubr: insts.Add(new Type2Instruction(OperatorName.callgsubr)); break;
                    case (byte)Type2Operator1._return: insts.Add(new Type2Instruction(OperatorName._return)); break;
                }
            }

#if DEBUG
            //if (_dbugInstructionListMark == 3520)
            //{
            //    dbugDumpInstructionListToFile(insts, "d:\\WImageTest\\test_type2.txt");
            //}

            return new Type2GlyphInstructionList(insts) { dbugMark = _dbugInstructionListMark };
#else
            return new Type2GlyphInstructionList(insts);
#endif

        }
#if DEBUG
        void dbugDumpInstructionListToFile(List<Type2Instruction> insts, string filename)
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


        void AddStemToList(List<Type2Instruction> insts, OperatorName stemName, ref int hintStemCount, ref int current_stem_Count)
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


            if ((current_stem_Count % 2) != 0)
            {
                //all kind has even number of stem               

                if (foundSomeStem)
                {
                    dbugDumpInstructionListToFile(insts, "d:\\WImageTest\\test_type2_" + (_dbugInstructionListMark - 1) + ".txt");
                    throw new NotSupportedException();
                }
                else
                {
                    //the first one is 'width'
                    ChangeFirstInstToGlyphWidthValue(insts);
                    current_stem_Count--;
                }
            }
            hintStemCount += current_stem_Count; //save a snapshot of stem count
            insts.Add(new Type2Instruction(OperatorName.hstemhm));
            current_stem_Count = 0;//clear
            foundSomeStem = true;
        }
        void AddHintMaskToList(List<Type2Instruction> insts, BinaryReader reader, ref int hintStemCount, ref int current_stem_Count)
        {
            if (hintStemCount == 0)
            {
                if (!foundSomeStem)
                {
                    hintStemCount = current_stem_Count;
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
                    insts.Add(new Type2Instruction(OperatorName.LoadInt,
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
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24)));

                        break;
                    case 2:
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)));

                        break;
                    case 3:
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)));
                        break;
                    default: throw new NotSupportedException();//should not occur !
                }

                insts.Add(new Type2Instruction(OperatorName.hintmask_bits, properNumberOfMaskBytes));
            }
            else
            {
                //last remaining <4 bytes 
                switch (properNumberOfMaskBytes)
                {
                    case 0:
                    default: throw new NotSupportedException();//should not occur !
                    case 1:
                        insts.Add(new Type2Instruction(OperatorName.hintmask1, (_reader.ReadByte() << 24)));
                        break;
                    case 2:
                        insts.Add(new Type2Instruction(OperatorName.hintmask2,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)
                            ));
                        break;
                    case 3:
                        insts.Add(new Type2Instruction(OperatorName.hintmask3,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)
                            ));
                        break;
                    case 4:
                        insts.Add(new Type2Instruction(OperatorName.hintmask4,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8) |
                            (_reader.ReadByte())
                            ));
                        break;
                }
            }
        }
        void AddCounterMaskToList(List<Type2Instruction> insts, BinaryReader reader, ref int hintStemCount, ref int current_stem_Count)
        {


            if (hintStemCount == 0)
            {
                if (!foundSomeStem)
                {
                    hintStemCount = current_stem_Count;
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
                    insts.Add(new Type2Instruction(OperatorName.LoadInt,
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
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24)));

                        break;
                    case 2:
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)));

                        break;
                    case 3:
                        insts.Add(new Type2Instruction(OperatorName.LoadInt,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)));
                        break;
                    default: throw new NotSupportedException();//should not occur !
                }

                insts.Add(new Type2Instruction(OperatorName.cntrmask_bits, properNumberOfMaskBytes));
            }
            else
            {
                //last remaining <4 bytes 
                switch (properNumberOfMaskBytes)
                {
                    case 0:
                    default: throw new NotSupportedException();//should not occur !
                    case 1:
                        insts.Add(new Type2Instruction(OperatorName.cntrmask1, (_reader.ReadByte() << 24)));
                        break;
                    case 2:
                        insts.Add(new Type2Instruction(OperatorName.cntrmask2,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16)
                            ));
                        break;
                    case 3:
                        insts.Add(new Type2Instruction(OperatorName.cntrmask3,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8)
                            ));
                        break;
                    case 4:
                        insts.Add(new Type2Instruction(OperatorName.cntrmask4,
                            (_reader.ReadByte() << 24) |
                            (_reader.ReadByte() << 16) |
                            (_reader.ReadByte() << 8) |
                            (_reader.ReadByte())
                            ));
                        break;
                }
            }
        }

        static void ChangeFirstInstToGlyphWidthValue(List<Type2Instruction> insts)
        {
            //check the first element must be loadint
            Type2Instruction firstInst = insts[0];
            if (firstInst.Op != OperatorName.LoadInt) { throw new NotSupportedException(); }
            //the replace
            insts[0] = new Type2Instruction(OperatorName.GlyphWidth, firstInst.Value);
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