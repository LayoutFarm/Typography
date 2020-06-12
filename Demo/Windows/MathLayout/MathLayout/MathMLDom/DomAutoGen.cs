/*x0*///Autogen,2020-04-06T12:39:45
/*x1*///-----

/*x2*/using System;
/*x3*/using System.IO;

/*x5*/namespace MathLayout{
/*x6*///PART1: node-definitions
/*x7*///identifier
/*x8*/public partial class mi:MathNode{
/*x9*/public override string Name=>"mi";
/*x10*/}
/*x11*///object
/*x12*/public partial class mo:MathNode{
/*x13*/public override string Name=>"mo";
/*x14*/}
/*x15*///number
/*x16*/public partial class mn:MathNode{
/*x17*/public override string Name=>"mn";
/*x18*/}
/*x19*///text
/*x20*/public partial class mtext:MathNode{
/*x21*/public override string Name=>"mtext";
/*x22*/}
/*x23*///space
/*x24*/public partial class mspace:MathNode{
/*x25*/public override string Name=>"mspace";
/*x26*/}
/*x27*///string literal
/*x28*/public partial class ms:MathNode{
/*x29*/public override string Name=>"ms";
/*x30*/}
/*x31*///math row
/*x32*/public partial class mrow:MathNode{
/*x33*/public override string Name=>"mrow";
/*x34*/}
/*x35*///fraction
/*x36*/public partial class mfrac:MathNode{
/*x37*/public override string Name=>"mfrac";
/*x38*/}
/*x39*///square root
/*x40*/public partial class msqrt:MathNode{
/*x41*/public override string Name=>"msqrt";
/*x42*/}
/*x43*///form a radical with specified index
/*x44*/public partial class mroot:MathNode{
/*x45*/public override string Name=>"mroot";
/*x46*/}
/*x47*///style change
/*x48*/public partial class mstyle:MathNode{
/*x49*/public override string Name=>"mstyle";
/*x50*/}
/*x51*///enclose a syntax error message from a preprocessor
/*x52*/public partial class merror:MathNode{
/*x53*/public override string Name=>"merror";
/*x54*/}
/*x55*///adjust space around content
/*x56*/public partial class mpadded:MathNode{
/*x57*/public override string Name=>"mpadded";
/*x58*/}
/*x59*///make content invisible but preserve its size
/*x60*/public partial class mphantom:MathNode{
/*x61*/public override string Name=>"mphantom";
/*x62*/}
/*x63*///surround content with a pair of fences
/*x64*/public partial class mfenced:MathNode{
/*x65*/public override string Name=>"mfenced";
/*x66*/}
/*x67*///enclose content with a stretching symbol such as a long division sign.
/*x68*/public partial class menclose:MathNode{
/*x69*/public override string Name=>"menclose";
/*x70*/}
/*x71*///attach a subscript to a base
/*x72*/public partial class msub:MathNode{
/*x73*/public override string Name=>"msub";
/*x74*/}
/*x75*///attach a superscript to a base
/*x76*/public partial class msup:MathNode{
/*x77*/public override string Name=>"msup";
/*x78*/}
/*x79*///attach a subscript-superscript pair to a base
/*x80*/public partial class msubsup:MathNode{
/*x81*/public override string Name=>"msubsup";
/*x82*/}
/*x83*///attach an underscript to a base
/*x84*/public partial class munder:MathNode{
/*x85*/public override string Name=>"munder";
/*x86*/}
/*x87*///attach an overscript to a base
/*x88*/public partial class mover:MathNode{
/*x89*/public override string Name=>"mover";
/*x90*/}
/*x91*///attach an underscript-overscript pair to a base
/*x92*/public partial class munderover:MathNode{
/*x93*/public override string Name=>"munderover";
/*x94*/}
/*x95*///attach prescripts and tensor indices to a base
/*x96*/public partial class mmultiscripts:MathNode{
/*x97*/public override string Name=>"mmultiscripts";
/*x98*/}
/*x99*///prescripts
/*x100*/public partial class mprescripts:MathNode{
/*x101*/public override string Name=>"mprescripts";
/*x102*/}
/*x103*///empty element
/*x104*/public partial class none:MathNode{
/*x105*/public override string Name=>"none";
/*x106*/}
/*x107*///table or matrix
/*x108*/public partial class mtable:MathNode{
/*x109*/public override string Name=>"mtable";
/*x110*/}
/*x111*///row in a table or matrix with a label or equation number
/*x112*/public partial class mlabeledtr:MathNode{
/*x113*/public override string Name=>"mlabeledtr";
/*x114*/}
/*x115*///row in a table or matrix
/*x116*/public partial class mtr:MathNode{
/*x117*/public override string Name=>"mtr";
/*x118*/}
/*x119*///one entry in a table or matrix
/*x120*/public partial class mtd:MathNode{
/*x121*/public override string Name=>"mtd";
/*x122*/}
/*x123*///alignment markers
/*x124*/public partial class maligngroup:MathNode{
/*x125*/public override string Name=>"maligngroup";
/*x126*/}
/*x127*///alignment markers
/*x128*/public partial class malignmark:MathNode{
/*x129*/public override string Name=>"malignmark";
/*x130*/}
/*x131*///columns of aligned characters
/*x132*/public partial class mstack:MathNode{
/*x133*/public override string Name=>"mstack";
/*x134*/}
/*x135*///similar to msgroup, with the addition of a divisor and result
/*x136*/public partial class mlongdiv:MathNode{
/*x137*/public override string Name=>"mlongdiv";
/*x138*/}
/*x139*///a group of rows in an mstack that are shifted by similar amounts
/*x140*/public partial class msgroup:MathNode{
/*x141*/public override string Name=>"msgroup";
/*x142*/}
/*x143*///a row in an mstack
/*x144*/public partial class msrow:MathNode{
/*x145*/public override string Name=>"msrow";
/*x146*/}
/*x147*///row in an mstack that whose contents represent carries or borrows
/*x148*/public partial class mscarries:MathNode{
/*x149*/public override string Name=>"mscarries";
/*x150*/}
/*x151*///one entry in an mscarries
/*x152*/public partial class mscarry:MathNode{
/*x153*/public override string Name=>"mscarry";
/*x154*/}
/*x155*///horizontal line inside of mstack
/*x156*/public partial class msline:MathNode{
/*x157*/public override string Name=>"msline";
/*x158*/}
/*x159*///bind actions to a sub-expression
/*x160*/public partial class maction:MathNode{
/*x161*/public override string Name=>"maction";
/*x162*/}
/*x163*///PART2: node-parse-registrations
/*x164*/partial class DomNodeDefinitionStore{
/*x165*/partial void LoadNodeDefinition(){
/*x166*/Register("mi",()=> new mi());
/*x167*/Register("mo",()=> new mo());
/*x168*/Register("mn",()=> new mn());
/*x169*/Register("mtext",()=> new mtext());
/*x170*/Register("mspace",()=> new mspace());
/*x171*/Register("ms",()=> new ms());
/*x172*/Register("mrow",()=> new mrow());
/*x173*/Register("mfrac",()=> new mfrac());
/*x174*/Register("msqrt",()=> new msqrt());
/*x175*/Register("mroot",()=> new mroot());
/*x176*/Register("mstyle",()=> new mstyle());
/*x177*/Register("merror",()=> new merror());
/*x178*/Register("mpadded",()=> new mpadded());
/*x179*/Register("mphantom",()=> new mphantom());
/*x180*/Register("mfenced",()=> new mfenced());
/*x181*/Register("menclose",()=> new menclose());
/*x182*/Register("msub",()=> new msub());
/*x183*/Register("msup",()=> new msup());
/*x184*/Register("msubsup",()=> new msubsup());
/*x185*/Register("munder",()=> new munder());
/*x186*/Register("mover",()=> new mover());
/*x187*/Register("munderover",()=> new munderover());
/*x188*/Register("mmultiscripts",()=> new mmultiscripts());
/*x189*/Register("mprescripts",()=> new mprescripts());
/*x190*/Register("none",()=> new none());
/*x191*/Register("mtable",()=> new mtable());
/*x192*/Register("mlabeledtr",()=> new mlabeledtr());
/*x193*/Register("mtr",()=> new mtr());
/*x194*/Register("mtd",()=> new mtd());
/*x195*/Register("maligngroup",()=> new maligngroup());
/*x196*/Register("malignmark",()=> new malignmark());
/*x197*/Register("mstack",()=> new mstack());
/*x198*/Register("mlongdiv",()=> new mlongdiv());
/*x199*/Register("msgroup",()=> new msgroup());
/*x200*/Register("msrow",()=> new msrow());
/*x201*/Register("mscarries",()=> new mscarries());
/*x202*/Register("mscarry",()=> new mscarry());
/*x203*/Register("msline",()=> new msline());
/*x204*/Register("maction",()=> new maction());
/*x205*/}
/*x206*/}
/*x207*/}
