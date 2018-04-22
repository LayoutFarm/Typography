//Apapche2, 2018, apache/pdfbox Authors ( https://github.com/apache/pdfbox) 
//MIT, 2018, WinterDev  

using System;
using System.Collections.Generic;


namespace Typography.OpenFont.CFF
{

    class CffEvaluationEngine
    {

        CFF.Cff1Font _cff1Font;
        public void Run(IGlyphTranslator tx, CFF.Cff1Font cff1Font, Type2GlyphInstructionList instructionList)
        {

            this._cff1Font = cff1Font;
            Type2EvaluationStack _evalStack = new Type2EvaluationStack();
            List<Type2Instruction> insts = instructionList.Insts;
            _evalStack.GlyphTranslator = tx;
            int j = insts.Count;


            for (int i = 0; i < j; ++i)
            {
                Type2Instruction inst = insts[i];

                if (inst.Op != OperatorName.LoadInt)
                {

                }
                switch (inst.Op)
                {
                    default: throw new NotSupportedException();
                    case OperatorName.GlyphWidth:
                        //TODO: 
                        break;
                    case OperatorName.LoadInt:
                        _evalStack.Push(inst.Value);
                        break;                    //
                    case OperatorName.endchar:
                        _evalStack.EndChar();
                        break;
                    case OperatorName.flex: _evalStack.Flex(); break;
                    case OperatorName.hflex: _evalStack.H_Flex(); break;
                    case OperatorName.hflex1: _evalStack.H_Flex1(); break;
                    case OperatorName.flex1: _evalStack.Flex1(); break;
                    //-------------------------
                    //4.4: Arithmetic Operators
                    case OperatorName.abs: _evalStack.Op_Abs(); break;
                    case OperatorName.add: _evalStack.Op_Add(); break;
                    case OperatorName.sub: _evalStack.Op_Sub(); break;
                    case OperatorName.div: _evalStack.Op_Div(); break;
                    case OperatorName.neg: _evalStack.Op_Neg(); break;
                    case OperatorName.random: _evalStack.Op_Random(); break;
                    case OperatorName.mul: _evalStack.Op_Mul(); break;
                    case OperatorName.sqrt: _evalStack.Op_Sqrt(); break;
                    case OperatorName.drop: _evalStack.Op_Drop(); break;
                    case OperatorName.exch: _evalStack.Op_Exch(); break;
                    case OperatorName.index: _evalStack.Op_Index(); break;
                    case OperatorName.roll: _evalStack.Op_Roll(); break;
                    case OperatorName.dup: _evalStack.Op_Dup(); break;

                    //-------------------------
                    //4.5: Storage Operators 
                    case OperatorName.put: _evalStack.Put(); break;
                    case OperatorName.get: _evalStack.Get(); break;
                    //-------------------------
                    //4.6: Conditional
                    case OperatorName.and: _evalStack.Op_And(); break;
                    case OperatorName.or: _evalStack.Op_Or(); break;
                    case OperatorName.not: _evalStack.Op_Not(); break;
                    case OperatorName.eq: _evalStack.Op_Eq(); break;
                    case OperatorName.ifelse: _evalStack.Op_IfElse(); break;
                    // 
                    case OperatorName.rlineto: _evalStack.R_LineTo(); break;
                    case OperatorName.hlineto: _evalStack.H_LineTo(); break;
                    case OperatorName.vlineto: _evalStack.V_LineTo(); break;
                    case OperatorName.rrcurveto: _evalStack.RR_CurveTo(); break;
                    case OperatorName.hhcurveto: _evalStack.HH_CurveTo(); break;
                    case OperatorName.hvcurveto: _evalStack.HV_CurveTo(); break;
                    case OperatorName.rcurveline: _evalStack.R_CurveLine(); break;
                    case OperatorName.rlinecurve: _evalStack.R_LineCurve(); break;
                    case OperatorName.vhcurveto: _evalStack.VH_CurveTo(); break;
                    case OperatorName.vvcurveto: _evalStack.VV_CurveTo(); break;
                    //-------------------------------------------------------------------                     
                    case OperatorName.rmoveto: _evalStack.R_MoveTo(); break;
                    case OperatorName.hmoveto: _evalStack.H_MoveTo(); break;
                    case OperatorName.vmoveto: _evalStack.V_MoveTo(); break;
                    //-------------------------------------------------------------------
                    //4.3 Hint Operators
                    case OperatorName.hstem: _evalStack.H_Stem(); break;
                    case OperatorName.vstem: _evalStack.V_Stem(); break;
                    case OperatorName.vstemhm: _evalStack.V_StemHM(); break;
                    case OperatorName.hstemhm: _evalStack.H_StemHM(); break;
                    case OperatorName.hintmask:
                        {
                            //hintmask | -hintmask(19 + mask) | -
                            //The mask data bytes are defined as follows:
                            //• The number of data bytes is exactly the number needed, one
                            //bit per hint, to reference the number of stem hints declared
                            //at the beginning of the charstring program.
                            //• Each bit of the mask, starting with the most-significant bit of
                            //the first byte, represents the corresponding hint zone in the
                            //order in which the hints were declared at the beginning of
                            //the charstring.
                            //• For each bit in the mask, a value of ‘1’ specifies that the
                            //corresponding hint shall be active. A bit value of ‘0’ specifies
                            //that the hint shall be inactive.
                            //• Unused bits in the mask, if any, must be zero.

                            //hintCount += _evalStack.StackCount/2; 
                            _evalStack.HintMask();
                        }
                        break;
                    case OperatorName.cntrmask: _evalStack.CounterSpaceMask(); break;
                    //-------------------------
                    //4.7: Subroutine Operators
                    case OperatorName._return: _evalStack.Ret(); break;
                    case OperatorName.callsubr:
                        {
                            //resolve local subrountine
                            int rawSubRoutineNum = _evalStack.Pop();

                            //from Technical Note #5176 (CFF spec)
                            //resolve with bias
                            //Card16 bias;
                            //Card16 nSubrs = subrINDEX.count;
                            //if (CharstringType == 1)
                            //    bias = 0;
                            //else if (nSubrs < 1240)
                            //    bias = 107;
                            //else if (nSubrs < 33900)
                            //    bias = 1131;
                            //else
                            //    bias = 32768;

                            int nsubrs = cff1Font._localSubrs.Count;
                            int bias = (nsubrs < 1240) ? 107 :
                                            (nsubrs < 33900) ? 1131 : 32769;

                            //find local subroutine
                            Type2GlyphInstructionList resolvedSubroutine = _cff1Font._localSubrs[rawSubRoutineNum + bias];
                            //then we move to another context
                            Run(tx, cff1Font, resolvedSubroutine);

                        }
                        break;
                    case OperatorName.callgsubr: throw new NotSupportedException();

                }
            }
        }
    }

    class Type2EvaluationStack
    {

        double _currentX;
        double _currentY;

        double[] _argStack = new double[50];
        int _currentIndex = 0; //current stack index

        IGlyphTranslator _glyphTranslator;

        public Type2EvaluationStack()
        {
        }
        public IGlyphTranslator GlyphTranslator
        {
            get { return _glyphTranslator; }
            set { _glyphTranslator = value; }
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

        //hstem, hstemhm, vstem, vstemhm, cntrmask, 
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


#if DEBUG
            if ((_currentIndex % 2) != 0)
            {

            }
#endif

            for (int i = 0; i < _currentIndex;)
            {
                _currentX += _argStack[i];
                _currentY += _argStack[i + 1];
                i += 2;
            }

            _glyphTranslator.MoveTo((float)(_currentX), (float)(_currentY));

            _currentIndex = 0; //clear stack 
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
#if DEBUG
            if (_currentIndex != 1)
            {
                throw new NotSupportedException();
            }
#endif
            _glyphTranslator.MoveTo((float)(_currentX += _argStack[0]), (float)_currentY);

            _currentIndex = 0; //clear stack 
        }
        public void V_MoveTo()
        {
            //|- dy1 vmoveto (4) |-
            //moves the current point 
            //dy1 units in the vertical direction.
            //see [NOTE4]


            int rd_index = 0; //start at bottom
            double w = _argStack[rd_index];
            _currentY += _argStack[rd_index + 1];

            _glyphTranslator.MoveTo((float)_currentX, (float)_currentY);

            _currentIndex = 0; //clear stack 
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
#if DEBUG
            if ((_currentIndex % 2) != 0)
            {
                throw new NotSupportedException();
            }
#endif
            for (int i = 0; i < _currentIndex;)
            {
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY += _argStack[i + 1]));
                i += 2;
            }
            _currentIndex = 0; //clear stack 
        }
        public void H_LineTo()
        {

            //|- dx1 {dya dxb}*  hlineto (6) |-
            //|- {dxa dyb}+  hlineto (6) |-

            //appends a horizontal line of length 
            //dx1 to the current point. 

            //With an odd number of arguments, subsequent argument pairs 
            //are interpreted as alternating values of 
            //dy and dx, for which additional lineto
            //operators draw alternating vertical and 
            //horizontal lines.

            //With an even number of arguments, the 
            //arguments are interpreted as alternating horizontal and 
            //vertical lines. The number of lines is determined from the 
            //number of arguments on the stack.

            //first elem
            int i = 0;
            if ((_currentIndex % 2) != 0)
            {
                //|- dx1 {dya dxb}*  hlineto (6) |-
                //odd number                
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)_currentY);
                i++;
                for (; i < _currentIndex;)
                {
                    _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i]));
                    _glyphTranslator.LineTo((float)(_currentX += _argStack[i + 1]), (float)(_currentY));
                    i += 2;
                }
            }
            else
            {
                //even number
                //|- {dxa dyb}+  hlineto (6) |-
                for (; i < _currentIndex;)
                {
                    //line to
                    _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY));
                    _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i + 1]));
                    //
                    i += 2;
                }
            }


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
            //first elem
            int i = 0;
            if ((_currentIndex % 2) != 0)
            {
                //|- dy1 {dxa dyb}*  vlineto (7) |-
                //odd number                
                _glyphTranslator.LineTo((float)_currentX, (float)(_currentY += _argStack[i]));
                i++;

                for (; i < _currentIndex;)
                {
                    //line to
                    _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY));
                    _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i + 1]));
                    //
                    i += 2;
                }
            }
            else
            {
                //even number
                //|- {dya dxb}+  vlineto (7) |-
                for (; i < _currentIndex;)
                {
                    //line to
                    _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i]));
                    _glyphTranslator.LineTo((float)(_currentX += _argStack[i + 1]), (float)(_currentY));
                    //
                    i += 2;
                }
            }
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


            //All Bézier curve path segments are drawn using six arguments,
            //dxa, dya, dxb, dyb, dxc, dyc; where dxa and dya are relative to
            //the current point, and all subsequent arguments are relative to
            //the previous point.A number of the curve operators take
            //advantage of the situation where some tangent points are
            //horizontal or vertical(and hence the value is zero), thus
            //reducing the number of arguments needed.

            int i = 0;
#if DEBUG
            if ((_currentIndex % 6) != 0)
            {
                // i++;
            }
#endif

            for (; i < _currentIndex;)
            {

                double curX = _currentX;
                double curY = _currentY;

                _glyphTranslator.Curve4(
                    (float)(curX += _argStack[i + 0]), (float)(curY += _argStack[i + 1]), //dxa,dya
                    (float)(curX += _argStack[i + 2]), (float)(curY += _argStack[i + 3]), //dxb,dyb
                    (float)(curX += _argStack[i + 4]), (float)(curY += _argStack[i + 5])  //dxc,dyc
                    );

                _currentX = curX;
                _currentY = curY;
                //
                i += 6;
            }

            _currentIndex = 0; //clear stack 
        }
        public void HH_CurveTo()
        {

            //|- dy1? {dxa dxb dyb dxc}+ hhcurveto (27) |-

            //appends one or more Bézier curves, as described by the 
            //dxa...dxc set of arguments, to the current point. 
            //For each curve, if there are 4 arguments, 
            //the curve starts and ends horizontal. 


            //The first curve need not start horizontal (the odd argument 
            //case). Note the argument order for the odd argument case

            int i = 0;
            if ((_currentIndex % 2) != 0)
            {
                //odd number                
                _glyphTranslator.LineTo((float)_currentX, (float)(_currentY += _argStack[i]));
                i++;
            }

            for (; i < _currentIndex;)
            {

                double curX = _currentX;
                double curY = _currentY;

                _glyphTranslator.Curve4(
                    (float)(curX += _argStack[i + 0]), (float)(curY), //dxa,+0
                    (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                    (float)(curX += _argStack[i + 3]), (float)(curY)  //dxc,+0
                    );

                _currentX = curX;
                _currentY = curY;

                //
                i += 4;
            }
            _currentIndex = 0; //clear stack  
        }
        public void HV_CurveTo()
        {
            //|- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto (31) |-

            //|- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto (31) |-

            //appends one or more Bézier curves to the current point.

            //The tangent for the first Bézier must be horizontal, and the second 
            //must be vertical (except as noted below). 

            int i = 0;
            int remaining = 0;

            switch (remaining = (_currentIndex % 8))
            {
                default: throw new NotSupportedException();
                case 0:
                case 1:
                    {
                        //|- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto (31) |-

                        double curX = _currentX;
                        double curY = _currentY;

                        int endBefore = _currentIndex - remaining;
                        for (; i < endBefore;)
                        {
                            //line to 

                            _glyphTranslator.Curve4(
                                (float)(curX += _argStack[i + 0]), (float)(curY), //+dxa,0
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                                (float)(curX), (float)(curY += _argStack[i + 3])  //+0,dyc
                                );

                            _glyphTranslator.Curve4(
                              (float)(curX), (float)(curY += _argStack[i + 4]), //+0,dyd
                              (float)(curX += _argStack[i + 5]), (float)(curY += _argStack[i + 6]), //dxe,dye
                              (float)(curX += _argStack[i + 7]), (float)(curY)  //dxf,+0
                              );
                            //
                            i += 8;
                        }
                        _currentX = curX;
                        _currentY = curY;

                        if (remaining == 1)
                        {
                            //dyf?
                            _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i]));
                        }
                    }
                    break;

                case 4:
                case 5:
                    {

                        //|- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto (31) |-

                        //If there is a multiple of four arguments, the curve starts
                        //horizontal and ends vertical.
                        //Note that the curves alternate between start horizontal, end vertical, and start vertical, and
                        //end horizontal.The last curve(the odd argument case) need not
                        //end horizontal/ vertical.

                        double curX = _currentX;
                        double curY = _currentY;

                        _glyphTranslator.Curve4(
                                (float)(curX += _argStack[i + 0]), (float)(curY), //dx1
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dx2,dy2
                                (float)(curX), (float)(curY += _argStack[i + 3])  //dy3
                                );
                        i += 4;

                        int endBefore = _currentIndex - remaining;
                        for (; i < endBefore;)
                        {
                            _glyphTranslator.Curve4(
                                (float)(curX), (float)(curY += _argStack[i + 0]), //0,dya
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                                (float)(curX += _argStack[i + 3]), (float)(curY)  //dxc, +0
                                );

                            _glyphTranslator.Curve4(
                              (float)(curX += _argStack[i + 4]), (float)(curY), //dxd,0
                              (float)(curX += _argStack[i + 5]), (float)(curY += _argStack[i + 6]), //dxe,dye
                              (float)(curX), (float)(curY += _argStack[i + 7])  //0,dyf
                              );
                            //
                            i += 8;
                        }

                        _currentX = curX;
                        _currentY = curY;

                        if (remaining == 5)
                        {
                            //dxf?
                            _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY));
                        }

                    }
                    break;
            }


            _currentIndex = 0; //clear stack 
        }
        public void R_CurveLine()
        {

            //|- { dxa dya dxb dyb dxc dyc} +dxd dyd rcurveline(24) |-


            //is equivalent to one rrcurveto for each set of six arguments
            //dxa...dyc, followed by exactly one rlineto using
            //the dxd, dyd arguments.

            //The number of curves is determined from the count
            //on the argument stack.

            _currentIndex = 0; //clear stack 
        }
        public void R_LineCurve()
        {

            //|- { dxa dya} +dxb dyb dxc dyc dxd dyd rlinecurve(25) |-

            //is equivalent to one rlineto for each pair of arguments beyond
            //the six arguments dxb...dyd needed for the one
            //rrcurveto command.The number of lines is determined from the count of
            //items on the argument stack.

            _currentIndex = 0; //clear stack 
        }
        public void VH_CurveTo()
        {
            //|- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto (30) |-


            //|- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto (30) |- 

            //appends one or more Bézier curves to the current point, where 
            //the first tangent is vertical and the second tangent is horizontal.

            //This command is the complement of 
            //hvcurveto; 

            //see the description of hvcurveto for more information.
            int i = 0;
            int remaining = 0;

            switch (remaining = (_currentIndex % 8))
            {
                default: throw new NotSupportedException();
                case 0:
                case 1:
                    {
                        //|- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto (30) |-  
                        double curX = _currentX;
                        double curY = _currentY;
                        int endBefore = _currentIndex - remaining;
                        for (; i < endBefore;)
                        {
                            _glyphTranslator.Curve4(
                                (float)(curX), (float)(curY += _argStack[i + 0]), //+0,dya
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                                (float)(curX += _argStack[i + 3]), (float)(curY)  //dxc,+0
                                );

                            _glyphTranslator.Curve4(
                              (float)(curX += _argStack[i + 4]), (float)(curY), //dxd,+0
                              (float)(curX += _argStack[i + 5]), (float)(curY += _argStack[i + 6]), //dxe,dye
                              (float)(curX), (float)(curY += _argStack[i + 7])  //+0,dyf
                              );
                            //
                            i += 8;
                        }
                        _currentX = curX;
                        _currentY = curY;

                        if (remaining == 1)
                        {
                            //dxf?
                            _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)_currentY);
                        }
                    }
                    break;

                case 4:
                case 5:
                    {

                        //|- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto (30) |-
                        double curX = _currentX;
                        double curY = _currentY;

                        _glyphTranslator.Curve4(
                               (float)(curX), (float)(curY += _argStack[i + 0]), //dy1
                               (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dx2,dy2
                               (float)(curX += _argStack[i + 3]), (float)(curY) //dx3
                               );

                        i += 4;

                        int endBefore = _currentIndex - remaining;
                        for (; i < endBefore;)
                        {
                            //line to

                            _glyphTranslator.Curve4(
                                (float)(curX += _argStack[i + 0]), (float)(curY), //dxa,
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                                (float)(curX), (float)(curY += _argStack[i + 3])  //+0, dyc
                                );

                            _glyphTranslator.Curve4(
                              (float)(curX), (float)(curY += _argStack[i + 4]), //+0,dyd
                              (float)(curX += _argStack[i + 5]), (float)(curY += _argStack[i + 6]), //dxe,dye
                              (float)(curX += _argStack[i + 7]), (float)(curY)  //dxf,0
                              );
                            //
                            i += 8;
                        }

                        _currentX = curX;
                        _currentY = curY;

                        if (remaining == 5)
                        {
                            // dyf?
                            _glyphTranslator.LineTo((float)(_currentX), (float)(_currentY += _argStack[i]));
                        }

                    }
                    break;
            }

            _currentIndex = 0; //clear stack 


        }
        public void VV_CurveTo()
        {
            // |- dx1? {dya dxb dyb dyc}+  vvcurveto (26) |-
            //appends one or more curves to the current point. 
            //If the argument count is a multiple of four, the curve starts and ends vertical. 
            //If the argument count is odd, the first curve does not begin with a vertical tangent.

            int i = 0;
            if ((_currentIndex % 2) != 0)
            {
                //odd number                
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY));
                i++;
            }

            for (; i < _currentIndex;)
            {
                //line to
                double curX = _currentX;
                double curY = _currentY;

                _glyphTranslator.Curve4(
                    (float)(curX), (float)(curY += _argStack[i + 0]), //+0,dya
                    (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dxb,dyb
                    (float)(curX), (float)(curY += _argStack[i + 3])  //+0,dyc
                    );

                _currentX = curX;
                _currentY = curY;

                //
                i += 4;
            }

            _currentIndex = 0; //clear stack
        }
        public void EndChar()
        {
            _currentIndex = 0;
        }
        public void Flex()
        {
            //|- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd flex (12 35) |-
            //causes two Bézier curves, as described by the arguments(as
            //shown in Figure 2 below), to be rendered as a straight line when
            //the flex depth is less than fd / 100 device pixels, and as curved lines
            // when the flex depth is greater than or equal to fd/ 100 device pixels


            _currentIndex = 0; //clear stack 
        }
        public void H_Flex()
        {
            //|- dx1 dx2 dy2 dx3 dx4 dx5 dx6 hflex (12 34) |- 
            //causes the two curves described by the arguments
            //dx1...dx6  to be rendered as a straight line when
            //the flex depth is less than 0.5(that is, fd is 50) device pixels,
            //and as curved lines when the flex depth is greater than or equal to 0.5 device pixels. 

            //hflex is used when the following are all true:
            //a) the starting and ending points, first and last control points
            //have the same y value.
            //b) the joining point and the neighbor control points have
            //the same y value.
            //c) the flex depth is 50.

            _currentIndex = 0; //clear stack
        }
        public void H_Flex1()
        {
            //|- dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6 hflex1 (12 36) |-

            //causes the two curves described by the arguments to be 
            //rendered as a straight line when the flex depth is less than 0.5 
            //device pixels, and as curved lines when the flex depth is greater 
            //than or equal to 0.5 device pixels.

            //hflex1 is used if the conditions for hflex
            //are not met but all of the following are true:

            //a) the starting and ending points have the same y value,
            //b) the joining point and the neighbor control points have 
            //the same y value.
            //c) the flex depth is 50.
            _currentIndex = 0; //clear stack
        }
        public void Flex1()
        {
            //|- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 d6 flex1 (12 37) |

            //causes the two curves described by the arguments to be
            //rendered as a straight line when the flex depth is less than 0.5
            //device pixels, and as curved lines when the flex depth is greater
            //than or equal to 0.5 device pixels.

            //The d6 argument will be either a dx or dy value, depending on
            //the curve(see Figure 3). To determine the correct value, 
            //compute the distance from the starting point(x, y), the first
            //point of the first curve, to the last flex control point(dx5, dy5)
            //by summing all the arguments except d6; call this(dx, dy).If
            //abs(dx) > abs(dy), then the last point’s x-value is given by d6, and
            //its y - value is equal to y.
            //  Otherwise, the last point’s x-value is equal to x and its y-value is given by d6.


            _currentIndex = 0; //clear stack
        }


        //-------------------------------------------------------------------
        //4.3 Hint Operators
        public void H_Stem()
        {
            //|- y dy {dya dyb}*  hstem (1) |-


#if DEBUG
            if ((_currentIndex % 2) != 0)
            {
            }
#endif
            //hintCount += _currentIndex / 2;
            _currentIndex = 0; //clear stack
        }
        public void V_Stem()
        {
            //|- x dx {dxa dxb}*  vstem (3) |-
#if DEBUG
            if ((_currentIndex % 2) != 0)
            {
            }
#endif
            //hintCount += _currentIndex / 2;
            _currentIndex = 0; //clear stack
        }
        public void V_StemHM()
        {

            //|- x dx {dxa dxb}* vstemhm (23) |-
#if DEBUG
            if ((_currentIndex % 2) != 0)
            {

            }
#endif
            //hintCount += _currentIndex / 2;
            _currentIndex = 0; //clear stack
        }
        public void H_StemHM()
        {
            //|- y dy {dya dyb}*  hstemhm (18) |-
#if DEBUG
            if ((_currentIndex % 2) != 0)
            {
            }
#endif
            //hintCount += _currentIndex / 2;
            //has the same meaning as 
            //hstem (1),
            //except that it must be used 
            //in place of hstem  if the charstring contains one or more 
            //hintmask operators.
            _currentIndex = 0; //clear stack
        }

        public void HintMask()
        {
            //specifies which hints are active and which are not active. If any
            //hints overlap, hintmask must be used to establish a nonoverlapping
            //subset of hints.
            //hintmask may occur any number of
            //times in a charstring. Path operators occurring after a hintmask
            //are influenced by the new hint set, but the current point is not
            //moved. If stem hint zones overlap and are not properly
            //managed by use of the hintmask operator, the results are
            //undefined. 

            //|- hintmask (19 + mask) |-
            _currentIndex = 0; //clear stack
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

        public void Op_Abs()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Abs));
        }
        public void Op_Add()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Add));
        }
        public void Op_Sub()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Sub));
        }
        public void Op_Div()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Div));
        }
        public void Op_Neg()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Neg));
        }
        public void Op_Random()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Random));
        }
        public void Op_Mul()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Mul));
        }
        public void Op_Sqrt()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Sqrt));
        }
        public void Op_Drop()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Drop));
        }
        public void Op_Exch()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Exch));
        }
        public void Op_Index()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Index));
        }
        public void Op_Roll()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Roll));
        }
        public void Op_Dup()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Dup));
        }


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

        public void Put()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Put));
        }
        public void Get()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Get));
        }

        //-------------------------
        //4.6: Conditional  
        public void Op_And()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_And));
        }
        public void Op_Or()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Or));
        }
        public void Op_Not()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Not));
        }
        public void Op_Eq()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_Eq));
        }
        public void Op_IfElse()
        {
            Console.WriteLine("NOT_IMPLEMENT:" + nameof(Op_IfElse));
        } 
        public int Pop()
        {
            return (int)_argStack[--_currentIndex];//*** use prefix 
        }

        public void Ret()
        {

            _currentIndex = 0;
        }
#if DEBUG
        public void dbugClearEvalStack()
        {
            _currentIndex = 0;
        }
#endif
    }


}