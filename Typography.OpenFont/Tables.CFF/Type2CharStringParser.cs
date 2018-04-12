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

            int i = 0; //start at bottom
            double w = _argStack[i];
            _currentX += _argStack[i + 1];


            _glyphTranslator.MoveTo((float)(_currentX), (float)_currentY);

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

            for (int i = 0; i < _currentIndex;)
            {
                _glyphTranslator.MoveTo((float)(_currentX += _argStack[i]), (float)(_currentY += _argStack[i + 1]));
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
                //odd number                
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY));
                i++;
            }

            for (; i < _currentIndex;)
            {
                //line to
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY += _argStack[i + 1]));
                //
                i += 2;
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
                //odd number                
                _glyphTranslator.LineTo((float)_currentX, (float)(_currentY += _argStack[i]));
                i++;
            }

            for (; i < _currentIndex;)
            {
                //line to
                _glyphTranslator.LineTo((float)(_currentX += _argStack[i]), (float)(_currentY += _argStack[i + 1]));
                //
                i += 2;
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

            for (int i = 0; i < _currentIndex;)
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

            //If there is a multiple of four arguments, the curve starts 
            //horizontal and ends vertical. 

            //Note that the curves alternate between 
            //start horizontal, end vertical, and start vertical, and end horizontal. 

            //The last curve (the odd argument case) need not 
            //end horizontal/vertical.
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

                        double curX = _currentX;
                        double curY = _currentY;

                        _glyphTranslator.Curve4(
                                (float)(curX += _argStack[i + 0]), (float)(curY), //dx1,0
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dx2,dy2
                                (float)(curX), (float)(curY += _argStack[i + 3])  //+0,dy3
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
                                (float)(curX), (float)(curY += _argStack[i + 0]), //+0,dy1
                                (float)(curX += _argStack[i + 1]), (float)(curY += _argStack[i + 2]), //dx2,dy2
                                (float)(curX += _argStack[i + 3]), (float)(curY)  //dx3,+0
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
                              (float)(curX += _argStack[i + 4]), (float)(curY), //+0,dyd
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

            _currentIndex = 0; //clear stack
        }
        public void V_Stem()
        {
            //|- x dx {dxa dxb}*  vstem (3) |-

            _currentIndex = 0; //clear stack
        }
        public void H_StemHM()
        {
            //|- y dy {dya dyb}*  hstemhm (18) |-

            //has the same meaning as 
            //hstem (1),
            //except that it must be used 
            //in place of hstem  if the charstring contains one or more 
            //hintmask operators.
            _currentIndex = 0; //clear stack
        }
        public void HintMask()
        {
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


        //-------------------------
        //4.7: Subroutine Operators
        public void CallSubr()
        {
            var subrIndex = _argStack[_currentIndex - 1];

            _currentIndex = 0;
        }
        public void CallGSubr()
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

    class EmptyGlyphTranslator : IGlyphTranslator
    {
        public void BeginRead(int contourCount)
        {

        }

        public void CloseContour()
        {

        }

        public void Curve3(float x1, float y1, float x2, float y2)
        {

        }

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {

        }

        public void EndRead()
        {

        }

        public void LineTo(float x1, float y1)
        {

        }

        public void MoveTo(float x0, float y0)
        {

        }
    }



    class Type2CharStringSubroutine
    {

        byte[] _rawCharStringBuffer;
        public Type2CharStringSubroutine(byte[] rawCharStingBuffer)
        {
            this._rawCharStringBuffer = rawCharStingBuffer;
        }

    }

    class Type2CharStringParser : IDisposable
    {
        MemoryStream _msBuffer;
        BinaryReader _reader;
        Type2EvaluationStack _evalStack = new Type2EvaluationStack();


        public Type2CharStringParser()
        {
            _msBuffer = new MemoryStream();
            _reader = new BinaryReader(_msBuffer);
        }
        public void SetGlyphTranslator(IGlyphTranslator glyphTranslator)
        {
            this._evalStack.GlyphTranslator = glyphTranslator;
        }
#if DEBUG

        int _dbugCount = 0;
#endif


        public void ParseType2CharString(byte[] buffer)
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

#if DEBUG
                _dbugCount++;
                //if (_dbugCount >= 20)
                //{

                //}
#endif
                //read first byte 
                //translate *** 
                byte b0 = _reader.ReadByte();
                switch (b0)
                {
                    default: //else 32 -255
                        {

#if DEBUG
                            if (b0 < 32)
                            {
                                throw new Exception();
                            }
#endif

                            int num = ReadIntegerNumber(b0);
                            _evalStack.Push(num);
                        }
                        break;
                    case 0:
                        //reserve, do nothing
                        break;
                    case (byte)Type2Operator1.endchar:
                        _evalStack.EndChar();
                        break;
                    case (byte)Type2Operator1.shortint: // 28

                        //shortint
                        //First byte of a 3-byte sequence specifying a number.
                        _evalStack.Push(_reader.ReadUInt16());
                        break;
                    case (byte)Type2Operator1.escape: //12
                        {
                            b0 = _reader.ReadByte();
                            switch ((Type2Operator2)b0)
                            {
                                default: throw new NotSupportedException();
                                //-------------------------
                                //4.1: Path Construction Operators
                                case Type2Operator2.flex: _evalStack.Flex(); break;
                                case Type2Operator2.hflex: _evalStack.H_Flex(); break;
                                case Type2Operator2.hflex1: _evalStack.H_Flex1(); break;
                                case Type2Operator2.flex1: _evalStack.Flex1(); break;
                                //-------------------------
                                //4.4: Arithmetic Operators
                                case Type2Operator2.abs: _evalStack.Op_Abs(); break;
                                case Type2Operator2.add: _evalStack.Op_Add(); break;
                                case Type2Operator2.sub: _evalStack.Op_Sub(); break;
                                case Type2Operator2.div: _evalStack.Op_Div(); break;
                                case Type2Operator2.neg: _evalStack.Op_Neg(); break;
                                case Type2Operator2.random: _evalStack.Op_Random(); break;
                                case Type2Operator2.mul: _evalStack.Op_Mul(); break;
                                case Type2Operator2.sqrt: _evalStack.Op_Sqrt(); break;
                                case Type2Operator2.drop: _evalStack.Op_Drop(); break;
                                case Type2Operator2.exch: _evalStack.Op_Exch(); break;
                                case Type2Operator2.index: _evalStack.Op_Index(); break;
                                case Type2Operator2.roll: _evalStack.Op_Roll(); break;
                                case Type2Operator2.dup: _evalStack.Op_Dup(); break;

                                //-------------------------
                                //4.5: Storage Operators 
                                case Type2Operator2.put: _evalStack.Put(); break;
                                case Type2Operator2.get: _evalStack.Get(); break;
                                //-------------------------
                                //4.6: Conditional
                                case Type2Operator2.and: _evalStack.Op_And(); break;
                                case Type2Operator2.or: _evalStack.Op_Or(); break;
                                case Type2Operator2.not: _evalStack.Op_Not(); break;
                                case Type2Operator2.eq: _evalStack.Op_Eq(); break;
                                case Type2Operator2.ifelse: _evalStack.Op_IfElse(); break;
                            }
                        }
                        break;
                    case (byte)Type2Operator1.rmoveto: _evalStack.R_MoveTo(); break;
                    case (byte)Type2Operator1.hmoveto: _evalStack.H_MoveTo(); break;
                    case (byte)Type2Operator1.vmoveto: _evalStack.V_MoveTo(); break;
                    case (byte)Type2Operator1.rlineto: _evalStack.R_LineTo(); break;
                    case (byte)Type2Operator1.hlineto: _evalStack.H_LineTo(); break;
                    case (byte)Type2Operator1.vlineto: _evalStack.V_LineTo(); break;
                    case (byte)Type2Operator1.rrcurveto: _evalStack.RR_CurveTo(); break;
                    case (byte)Type2Operator1.hhcurveto: _evalStack.HH_CurveTo(); break;
                    case (byte)Type2Operator1.hvcurveto: _evalStack.HV_CurveTo(); break;
                    case (byte)Type2Operator1.rcurveline: _evalStack.R_CurveLine(); break;
                    case (byte)Type2Operator1.rlinecurve: _evalStack.R_LineCurve(); break;
                    case (byte)Type2Operator1.vhcurveto: _evalStack.VH_CurveTo(); break;
                    case (byte)Type2Operator1.vvcurveto: _evalStack.VV_CurveTo(); break;
                    //-------------------------------------------------------------------
                    //4.3 Hint Operators
                    case (byte)Type2Operator1.hstem: _evalStack.H_Stem(); break;
                    case (byte)Type2Operator1.vstem: _evalStack.V_Stem(); break;
                    case (byte)Type2Operator1.hstemhm: _evalStack.H_StemHM(); break;
                    case (byte)Type2Operator1.hintmask: _evalStack.HintMask(); break;
                    case (byte)Type2Operator1.cntrmask: _evalStack.CounterSpaceMask(); break;
                    //-------------------------
                    //4.7: Subroutine Operators
                    case (byte)Type2Operator1.callsubr: _evalStack.CallSubr(); break;
                    case (byte)Type2Operator1.callgsubr: _evalStack.CallGSubr(); break;
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