//MIT, 2019-present, WinterDev 
using System.IO;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.IO;
using Typography.OpenFont.Tables;

//see https://www.w3.org/TR/WOFF2/

namespace Typography.WebFont
{
    class Woff2Header
    {
        //WOFF2 Header
        //UInt32 signature   0x774F4632 'wOF2'
        //UInt32 flavor  The "sfnt version" of the input font.
        //UInt32 length  Total size of the WOFF file.
        //UInt16 numTables   Number of entries in directory of font tables.
        //UInt16 reserved    Reserved; set to 0.
        //UInt32 totalSfntSize   Total size needed for the uncompressed font data, including the sfnt header,
        //directory, and font tables(including padding).
        //UInt32  totalCompressedSize Total length of the compressed data block.
        //UInt16  majorVersion    Major version of the WOFF file.
        //UInt16  minorVersion    Minor version of the WOFF file.
        //UInt32  metaOffset  Offset to metadata block, from beginning of WOFF file.
        //UInt32  metaLength  Length of compressed metadata block.
        //UInt32  metaOrigLength  Uncompressed size of metadata block.
        //UInt32  privOffset  Offset to private data block, from beginning of WOFF file.
        //UInt32  privLength Length of private data block.

        public uint flavor;
        public uint length;
        public uint numTables;
        //public ushort reserved;
        public uint totalSfntSize;
        public uint totalCompressSize; //***
        public ushort majorVersion;
        public ushort minorVersion;
        public uint metaOffset;
        public uint metaLength;
        public uint metaOriginalLength;
        public uint privOffset;
        public uint privLength;
    }
    class Woff2TableDirectory
    {
        //TableDirectoryEntry
        //UInt8         flags           table type and flags
        //UInt32        tag	            4-byte tag(optional)
        //UIntBase128   origLength      length of original table
        //UIntBase128   transformLength transformed length(if applicable)

        public uint origLength;
        public uint transformLength;
        //translated values 
        public string Name { get; set; } //translate from tag
        public byte PreprocessingTransformation { get; set; }
        public long ExpectedStartAt { get; set; }
#if DEBUG
        public override string ToString()
        {
            return Name + " " + PreprocessingTransformation;
        }
#endif
    }


    public delegate bool BrotliDecompressStreamFunc(byte[] compressedInput, Stream decompressStream);

    public static class Woff2DefaultBrotliDecompressFunc
    {
        public static BrotliDecompressStreamFunc DecompressHandler;
    }

    class TransformedGlyf : UnreadTableEntry
    {
        public TransformedGlyf(TableHeader header, Woff2TableDirectory tableDir) : base(header)
        {
            HasCustomContentReader = true;
            TableDir = tableDir;
        }
        public Woff2TableDirectory TableDir { get; }

        public override T CreateTableEntry<T>(BinaryReader reader, T expectedResult)
        {
            Glyf glyfTable = expectedResult as Glyf;

            if (glyfTable == null) throw new System.NotSupportedException();

            ReconstructGlyfTable(reader, TableDir, glyfTable);


            //return base.CreateTableEntry(reader, expectedResult);

            return expectedResult;
        }

        void ReconstructGlyfTable(BinaryReader reader, Woff2TableDirectory woff2TableDir, Glyf glyfTable)
        {
            //fill the information to glyfTable


            //reader.BaseStream.Position += woff2TableDir.transformLength;
            //For greater compression effectiveness,
            //the glyf table is split into several substreams, to group like data together. 

            //The transformed table consists of a number of fields specifying the size of each of the substreams,
            //followed by the substreams in sequence.

            //During the decoding process the reverse transformation takes place,
            //where data from various separate substreams are recombined to create a complete glyph record
            //for each entry of the original glyf table.

            //Transformed glyf Table
            //Data-Type Semantic                Description and value type(if applicable)
            //Fixed     version                 = 0x00000000
            //UInt16    numGlyphs               Number of glyphs
            //UInt16    indexFormatOffset      format for loca table, 
            //                                 should be consistent with indexToLocFormat of 
            //                                 the original head table(see[OFF] specification)

            //UInt32    nContourStreamSize      Size of nContour stream in bytes
            //UInt32    nPointsStreamSize       Size of nPoints stream in bytes
            //UInt32    flagStreamSize          Size of flag stream in bytes
            //UInt32    glyphStreamSize         Size of glyph stream in bytes(a stream of variable-length encoded values, see description below)
            //UInt32    compositeStreamSize     Size of composite stream in bytes(a stream of variable-length encoded values, see description below)
            //UInt32    bboxStreamSize          Size of bbox data in bytes representing combined length of bboxBitmap(a packed bit array) and bboxStream(a stream of Int16 values)
            //UInt32    instructionStreamSize   Size of instruction stream(a stream of UInt8 values)

            //Int16     nContourStream[]        Stream of Int16 values representing number of contours for each glyph record
            //255UInt16 nPointsStream[]         Stream of values representing number of outline points for each contour in glyph records
            //UInt8     flagStream[]            Stream of UInt8 values representing flag values for each outline point.
            //Vary      glyphStream[]           Stream of bytes representing point coordinate values using variable length encoding format(defined in subclause 5.2)
            //Vary      compositeStream[]       Stream of bytes representing component flag values and associated composite glyph data
            //UInt8     bboxBitmap[]            Bitmap(a numGlyphs - long bit array) indicating explicit bounding boxes
            //Int16     bboxStream[]            Stream of Int16 values representing glyph bounding box data
            //UInt8     instructionStream[]	    Stream of UInt8 values representing a set of instructions for each corresponding glyph

            reader.BaseStream.Position = woff2TableDir.ExpectedStartAt;

            long start = reader.BaseStream.Position;

            uint version = reader.ReadUInt32();
            ushort numGlyphs = reader.ReadUInt16();
            ushort indexFormatOffset = reader.ReadUInt16();

            uint nContourStreamSize = reader.ReadUInt32(); //in bytes
            uint nPointsStreamSize = reader.ReadUInt32(); //in bytes
            uint flagStreamSize = reader.ReadUInt32(); //in bytes
            uint glyphStreamSize = reader.ReadUInt32(); //in bytes
            uint compositeStreamSize = reader.ReadUInt32(); //in bytes
            uint bboxStreamSize = reader.ReadUInt32(); //in bytes
            uint instructionStreamSize = reader.ReadUInt32(); //in bytes

            long pos1 = reader.BaseStream.Position;
            long expected_nCountStartAt = pos1;
            long expected_nPointStartAt = expected_nCountStartAt + nContourStreamSize;
            long expected_FlagStreamStartAt = expected_nPointStartAt + nPointsStreamSize;
            long expected_GlyphStreamStartAt = expected_FlagStreamStartAt + flagStreamSize;
            long expected_CompositeStreamStartAt = expected_GlyphStreamStartAt + glyphStreamSize;
            //---------------------------------------------
            //flags stream 
            //---------------------------------------------



            TempGlyph[] tmpGlyphs = new TempGlyph[numGlyphs];
            //glyph and nCountourStream
            List<GlyphContour> contours = new List<GlyphContour>();
            List<TempGlyph> compositeGlyphs = new List<TempGlyph>();
            List<TempGlyph> emptyGyphs = new List<TempGlyph>();

            for (ushort i = 0; i < numGlyphs; ++i)
            {
                TempGlyph glyph = new TempGlyph(i);
                short numContour = reader.ReadInt16();
                glyph.numContour = numContour; //num contour per glyph
                if (numContour > 0)
                {
                    //-1 = compound
                    //0 = empty glyph
                    GlyphContour[] myContours = new GlyphContour[numContour];
                    for (int n = 0; n < numContour; ++n)
                    {
                        contours.Add(myContours[n] = new GlyphContour());
                    }
                    glyph.contours = myContours;
                }
                else if (numContour < 0)
                {
                    //composite glyph, resolve later
                    compositeGlyphs.Add(glyph);
                }
                else
                {
                    emptyGyphs.Add(glyph);
                }
                tmpGlyphs[i] = glyph; //store
            }



            //--------------------------------------------------------------------------------------------
            //glyphStream 
            //5.2.Decoding of variable-length X and Y coordinates

            //Simple glyph data structure defines all contours that comprise a glyph outline,
            //which are presented by a sequence of on- and off-curve coordinate points. 

            //These point coordinates are encoded as delta values representing the incremental values 
            //between the previous and current corresponding X and Y coordinates of a point,
            //the first point of each outline is relative to (0, 0) point.

            //To minimize the size of the dataset of point coordinate values, 
            //each point is presented as a (flag, xCoordinate, yCoordinate) triplet.

            //The flag value is stored in a separate data stream 
            //and the coordinate values are stored as part of the glyph data stream using a variable-length encoding format
            //consuming a total of 2 - 5 bytes per point.

            //Decoding of Simple Glyphs:

            //For a simple glyph(when nContour > 0), the process continues as follows:
            //    1) Read numberOfContours 255UInt16 values from the nPoints stream.
            //    Each of these is the number of points of that contour.
            //    Convert this into the endPtsOfContours[] array by computing the cumulative sum, then subtracting one.
            //    For example, if the values in the stream are[2, 4], then the endPtsOfContours array is [1, 5].Also,
            //      the sum of all the values in the array is the total number of points in the glyph, nPoints.
            //      In the example given, the value of nPoints is 6.

            //    2) Read nPoints UInt8 values from the flags stream.Each corresponds to one point in the reconstructed glyph outline.
            //       The interpretation of the flag byte is described in details in subclause 5.2.

            //    3) For each point(i.e.nPoints times), read a number of point coordinate bytes from the glyph stream.
            //       The number of point coordinate bytes is a function of the flag byte read in the previous step: 
            //       for (flag < 0x7f) in the range 0 to 83 inclusive, it is one byte.
            //       In the range 84 to 119 inclusive, it is two bytes. 
            //       In the range 120 to 123 inclusive, it is three bytes, 
            //       and in the range 124 to 127 inclusive, it is four bytes. 
            //       Decode these bytes according to the procedure specified in the subclause 5.2 to reconstruct delta-x and delta-y values of the glyph point coordinates.
            //       Store these delta-x and delta-y values in the reconstructed glyph using the standard TrueType glyph encoding[OFF] subclause 5.3.3.

            //    4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
            //    5) Read instructionLength bytes from instructionStream, and store these in the reconstituted glyph as instructions.
            //--------
            if (reader.BaseStream.Position != expected_nPointStartAt)
            {

            }
            //
            //1) nPoints stream,  npoint for each contour
            int contourCount = contours.Count;
            ushort[] pntPerContours = new ushort[contourCount];
            for (int i = 0; i < contourCount; ++i)
            {
                // Each of these is the number of points of that contour.
                pntPerContours[i] = Woff2Utils.Read255UShort(reader);
            }

            if (reader.BaseStream.Position != expected_FlagStreamStartAt)
            {

            }
            //2) flagStream, flags value for each point
            //each byte in flags stream represents one point
            byte[] flagStream = reader.ReadBytes((int)flagStreamSize);

            if (reader.BaseStream.Position != expected_GlyphStreamStartAt)
            {

            }

            TripleEncodingTable tripleEncodeTable = new TripleEncodingTable();
            int curFlagsIndex = 0;
            int pntContourIndex = 0;
            for (int i = 0; i < tmpGlyphs.Length; ++i)
            {
                tmpGlyphs[i].BuildSimpleGlyphStructure(reader,
                    pntPerContours, ref pntContourIndex,
                    flagStream, ref curFlagsIndex,
                    tripleEncodeTable);
            }





            //--------------------------------------------------------------------------------------------
            //compositeStream
            //--------------------------------------------------------------------------------------------
            if (expected_CompositeStreamStartAt != reader.BaseStream.Position)
            {
                reader.BaseStream.Position = expected_CompositeStreamStartAt;
            }
            int j = compositeGlyphs.Count;
            for (ushort i = 0; i < j; ++i)
            {

                int compositeGlyphIndex = compositeGlyphs[i].glyphIndex;
                tmpGlyphs[compositeGlyphIndex] = ReadCompositeGlyph(reader, tmpGlyphs, compositeGlyphs, i);
            }


            long stop = reader.BaseStream.Position;
            long len = stop - start;
            if (len < woff2TableDir.transformLength)
            {
                reader.BaseStream.Position += (woff2TableDir.transformLength - len);
            }
            else if (len == woff2TableDir.transformLength)
            {

            }
            else
            {
                reader.BaseStream.Position -= (len - woff2TableDir.transformLength);
            }


            //-----------
            Glyph[] glyphs = new Glyph[numGlyphs];
            for (int i = 0; i < tmpGlyphs.Length; ++i)
            {
                glyphs[i] = tmpGlyphs[i].CreateGlyph();
            }

            glyfTable.Glyphs = glyphs;
        }
        class TempGlyph
        {
            public ushort glyphIndex;
            public short numContour;
            public GlyphContour[] contours;
            public ushort _instructionLen;
            byte[] _instructions;
            public TempGlyph(ushort glyphIndex)
            {
                this.glyphIndex = glyphIndex;
            }
            public void BuildSimpleGlyphStructure(BinaryReader glyphStreamReader,
                ushort[] pntPerContours, ref int pntContourIndex,
                byte[] flagStream, ref int flagsStreamIndex,
                TripleEncodingTable encTable)
            {
                //reading from glyphstream***

                //Building a SimpleGlyph 
                //    1) Read numberOfContours 255UInt16 values from the nPoints stream.
                //    Each of these is the number of points of that contour.
                //    Convert this into the endPtsOfContours[] array by computing the cumulative sum, then subtracting one.
                //    For example, if the values in the stream are[2, 4], then the endPtsOfContours array is [1, 5].Also,
                //      the sum of all the values in the array is the total number of points in the glyph, nPoints.
                //      In the example given, the value of nPoints is 6.

                //    2) Read nPoints UInt8 values from the flags stream.Each corresponds to one point in the reconstructed glyph outline.
                //       The interpretation of the flag byte is described in details in subclause 5.2.

                //    3) For each point(i.e.nPoints times), read a number of point coordinate bytes from the glyph stream.
                //       The number of point coordinate bytes is a function of the flag byte read in the previous step: 
                //       for (flag < 0x7f)
                //       in the range 0 to 83 inclusive, it is one byte.
                //       In the range 84 to 119 inclusive, it is two bytes. 
                //       In the range 120 to 123 inclusive, it is three bytes, 
                //       and in the range 124 to 127 inclusive, it is four bytes. 
                //       Decode these bytes according to the procedure specified in the subclause 5.2 to reconstruct delta-x and delta-y values of the glyph point coordinates.
                //       Store these delta-x and delta-y values in the reconstructed glyph using the standard TrueType glyph encoding[OFF] subclause 5.3.3.

                //    4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
                //    5) Read instructionLength bytes from instructionStream, and store these in the reconstituted glyph as instructions. 



                if (numContour < 1) return; //skip empty glyph 

                //-----
                int curX = 0;
                int curY = 0;
                for (int i = 0; i < numContour; ++i)
                {
                    //step 3) 

                    //foreach contour
                    //read 1 byte flags for each contour

                    //1) The most significant bit of a flag indicates whether the point is on- or off-curve point,
                    //2) the remaining seven bits of the flag determine the format of X and Y coordinate values and 
                    //specify 128 possible combinations of indices that have been assigned taking into consideration 
                    //typical statistical distribution of data found in TrueType fonts. 

                    //When X and Y coordinate values are recorded using nibbles(either 4 bits per coordinate or 12 bits per coordinate)
                    //the bits are packed in the byte stream with most significant bit of X coordinate first, 
                    //followed by the value for Y coordinate (most significant bit first). 
                    //As a result, the size of the glyph dataset is significantly reduced, 
                    //and the grouping of the similar values(flags, coordinates) in separate and contiguous data streams allows 
                    //more efficient application of the entropy coding applied as the second stage of encoding process. 

                    GlyphContour contour = contours[i];
                    ushort numPoint = pntPerContours[pntContourIndex++];//increament pntContourIndex AFTER

                    GlyphPointF[] glyphPoints = new GlyphPointF[numPoint];
                    contour.glyphPoints = glyphPoints;


                    for (int p = 0; p < glyphPoints.Length; ++p)
                    {

                        byte f = flagStream[flagsStreamIndex++]; //increment the flagStreamIndex AFTER read

                        //int f1 = (f >> 7); // most significant 1 bit -> on/off curve

                        int xyFormat = f & 0x7F; // remainging 7 bits x,y format 


                        TripleEncodingRecord enc = encTable[xyFormat]; //0-128 

                        byte[] packedXY = glyphStreamReader.ReadBytes(enc.ByteCount - 1); //byte count include 1 byte flags, so actual read=> byteCount-1
                                                                                          //read x and y

                        int x = 0;
                        int y = 0;

                        switch (enc.XBits)
                        {
                            default:
                                throw new System.NotSupportedException();//???
                            case 0: //0,8, 
                                x = 0;
                                y = enc.Ty(packedXY[0]);
                                break;
                            case 4: //4,4
                                x = enc.Tx(packedXY[0] >> 4);
                                y = enc.Ty(packedXY[0] & 0xF);
                                break;
                            case 8: //8,0 or 8,8
                                x = enc.Tx(packedXY[0]);
                                y = (enc.YBits == 8) ?
                                        enc.Ty(packedXY[1]) :
                                        0;
                                break;
                            case 12: //12,12
                                x = enc.Tx((packedXY[0] << 8) | (packedXY[1] >> 4));
                                y = enc.Ty(((packedXY[1] & 0xF) << 8) | (packedXY[2] >> 4));
                                break;
                            case 16: //16,16
                                x = enc.Tx((packedXY[0] << 8) | packedXY[1]);
                                y = enc.Ty((packedXY[2] << 8) | packedXY[3]);
                                break;
                        }

                        //incremental point format***
                        glyphPoints[p] = new GlyphPointF(curX += x, curY += y, (f >> 7) == 0); // most significant 1 bit -> on/off curve 
                    }
                }
                //----
                //step 4) Read one 255UInt16 value from the glyph stream, which is instructionLength, the number of instruction bytes.
                _instructionLen = Woff2Utils.Read255UShort(glyphStreamReader);

                //step 5) resolve it later
            }
            public void LoadInstructions(BinaryReader instructionStream)
            {
                if (_instructionLen > 0)
                {
                    _instructions = instructionStream.ReadBytes(_instructionLen);
                }
            }
            public Glyph CreateGlyph()
            {
                if (numContour == 0) return null;//empty glyph
                List<GlyphPointF> glyphPoints = new List<GlyphPointF>();
                List<ushort> endPoints = new List<ushort>();
                for (int i = 0; i < contours.Length; ++i)
                {
                    endPoints.Add(contours[i].EndPoint);
                    glyphPoints.AddRange(contours[i].glyphPoints);
                }
                return new Glyph(glyphPoints.ToArray(), endPoints.ToArray(), new Bounds(), _instructions, glyphIndex);
            }
        }

        class GlyphContour
        {
            public GlyphPointF[] glyphPoints;
            public int PointCount => glyphPoints.Length;
            public ushort EndPoint => (ushort)(glyphPoints.Length - 1);
        }


        TempGlyph ReadCompositeGlyph(BinaryReader reader, TempGlyph[] createdGlyphs, List<TempGlyph> compositeGlyphs, ushort compositeGlyphIndex)
        {

            //Decoding of Composite Glyphs
            //For a composite glyph(nContour == -1), the following steps take the place of (Building Simple Glyph, steps 1 - 5 above):

            //1a.Read a UInt16 from compositeStream.
            //  This is interpreted as a component flag word as in the TrueType spec.
            //  Based on the flag values, there are between 4 and 14 additional argument bytes,
            //  interpreted as glyph index, arg1, arg2, and optional scale or affine matrix.

            //2a.Read the number of argument bytes as determined in step 2a from the composite stream, 
            //and store these in the reconstructed glyph.
            //If the flag word read in step 2a has the FLAG_MORE_COMPONENTS bit(bit 5) set, go back to step 2a.

            //3a.If any of the flag words had the FLAG_WE_HAVE_INSTRUCTIONS bit(bit 8) set,
            //then read the instructions from the glyph and store them in the reconstructed glyph, 
            //using the same process as described in steps 4 and 5 above (see Building Simple Glyph).

            //Finally, for both simple and composite glyphs,
            //if the corresponding bit in the bounding box bit vector is set, 
            //then additionally read 4 Int16 values from the bbox stream, 
            //representing xMin, yMin, xMax, and yMax, respectively, 
            //and record these into the corresponding fields of the reconstructed glyph.
            //For simple glyphs, if the corresponding bit in the bounding box bit vector is not set,
            //then derive the bounding box by computing the minimum and maximum x and y coordinates in the outline, and storing that.

            //A composite glyph MUST have an explicitly supplied bounding box. 
            //The motivation is that computing bounding boxes is more complicated,
            //and would require resolving references to component glyphs taking into account composite glyph instructions and
            //the specified scales of individual components, which would conflict with a purely streaming implementation of font decoding.

            //A decoder MUST check for presence of the bounding box info as part of the composite glyph record 
            //and MUST NOT load a font file with the composite bounding box data missing.


            //------------------------------------------------------ 
            //https://www.microsoft.com/typography/OTSPEC/glyf.htm
            //Composite Glyph Description

            //This is the table information needed for composite glyphs (numberOfContours is -1). 
            //A composite glyph starts with two USHORT values (“flags” and “glyphIndex,” i.e. the index of the first contour in this composite glyph); 
            //the data then varies according to “flags”).
            //Type 	Name 	Description
            //USHORT 	flags 	component flag
            //USHORT 	glyphIndex 	glyph index of component
            //VARIABLE 	argument1 	x-offset for component or point number; type depends on bits 0 and 1 in component flags
            //VARIABLE 	argument2 	y-offset for component or point number; type depends on bits 0 and 1 in component flags
            //---------
            //see more at https://fontforge.github.io/assets/old/Composites/index.html
            //---------

            ////move to composite glyph position
            //reader.BaseStream.Seek(tableOffset + GlyphLocations.Offsets[compositeGlyphIndex], SeekOrigin.Begin);//reset
            ////------------------------
            //short contoursCount = reader.ReadInt16(); // ignored
            //Bounds bounds = Utils.ReadBounds(reader);

            //Glyph finalGlyph = null;
            //CompositeGlyphFlags flags;
            TempGlyph finalGlyph = null;

            Glyf.CompositeGlyphFlags flags;
            do
            {
                flags = (Glyf.CompositeGlyphFlags)reader.ReadUInt16(); //1a

#if DEBUG
                if (flags > Glyf.CompositeGlyphFlags.UNSCALED_COMPONENT_OFFSET)
                {
                    //check out of range flags
                }
#endif

                ushort glyphIndex = reader.ReadUInt16();

#if DEBUG
                if (glyphIndex >= createdGlyphs.Length)
                {

                }
#endif

                if (createdGlyphs[glyphIndex].numContour == -1)
                {
                    // This glyph is not read yet, resolve it first!
                    long storedOffset = reader.BaseStream.Position;
                    TempGlyph missingGlyph = ReadCompositeGlyph(reader, createdGlyphs, compositeGlyphs, glyphIndex);
                    createdGlyphs[glyphIndex] = missingGlyph;
                    reader.BaseStream.Position = storedOffset;
                }

                //Glyph newGlyph = Glyph.Clone(createdGlyphs[glyphIndex], compositeGlyphIndex);
                TempGlyph newGlyph = new TempGlyph(compositeGlyphIndex);

                short arg1 = 0;
                short arg2 = 0;
                ushort arg1and2 = 0;

                if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
                {
                    arg1 = reader.ReadInt16();
                    arg2 = reader.ReadInt16();
                }
                else
                {
                    arg1and2 = reader.ReadUInt16();
                }
                //-----------------------------------------
                float xscale = 1;
                float scale01 = 0;
                float scale10 = 0;
                float yscale = 1;

                bool useMatrix = false;
                //-----------------------------------------
                bool hasScale = false;
                if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.WE_HAVE_A_SCALE))
                {
                    //If the bit WE_HAVE_A_SCALE is set,
                    //the scale value is read in 2.14 format-the value can be between -2 to almost +2.
                    //The glyph will be scaled by this value before grid-fitting. 
                    xscale = yscale = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    hasScale = true;
                }
                else if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
                {
                    xscale = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    yscale = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    hasScale = true;
                }
                else if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
                {

                    //The bit WE_HAVE_A_TWO_BY_TWO allows for linear transformation of the X and Y coordinates by specifying a 2 × 2 matrix.
                    //This could be used for scaling and 90-degree*** rotations of the glyph components, for example.

                    //2x2 matrix

                    //The purpose of USE_MY_METRICS is to force the lsb and rsb to take on a desired value.
                    //For example, an i-circumflex (U+00EF) is often composed of the circumflex and a dotless-i. 
                    //In order to force the composite to have the same metrics as the dotless-i,
                    //set USE_MY_METRICS for the dotless-i component of the composite. 
                    //Without this bit, the rsb and lsb would be calculated from the hmtx entry for the composite 
                    //(or would need to be explicitly set with TrueType instructions).

                    //Note that the behavior of the USE_MY_METRICS operation is undefined for rotated composite components. 
                    useMatrix = true;
                    hasScale = true;
                    xscale = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    scale01 = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    scale10 = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */
                    yscale = ((float)reader.ReadInt16()) / (1 << 14); /* Format 2.14 */

                    if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.UNSCALED_COMPONENT_OFFSET))
                    {
                    }
                    else
                    {
                    }
                    if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.USE_MY_METRICS))
                    {

                    }
                }

                //--------------------------------------------------------------------
                if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.ARGS_ARE_XY_VALUES))
                {
                    //Argument1 and argument2 can be either x and y offsets to be added to the glyph or two point numbers.  
                    //x and y offsets to be added to the glyph
                    //When arguments 1 and 2 are an x and a y offset instead of points and the bit ROUND_XY_TO_GRID is set to 1,
                    //the values are rounded to those of the closest grid lines before they are added to the glyph.
                    //X and Y offsets are described in FUnits. 

                    if (useMatrix)
                    {
                        //use this matrix  
                        //TODO: replement this
                        //Glyph.TransformNormalWith2x2Matrix(newGlyph, xscale, scale01, scale10, yscale);
                        //Glyph.OffsetXY(newGlyph, arg1, arg2);
                    }
                    else
                    {
                        if (hasScale)
                        {
                            if (xscale == 1.0 && yscale == 1.0)
                            {

                            }
                            else
                            {
                                //TODO: replement this
                                //Glyph.TransformNormalWith2x2Matrix(newGlyph, xscale, 0, 0, yscale);
                            }
                            //TODO: replement this
                            //Glyph.OffsetXY(newGlyph, arg1, arg2);

                        }
                        else
                        {
                            if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.ROUND_XY_TO_GRID))
                            {
                                //TODO: implement round xy to grid***
                                //----------------------------
                            }
                            //just offset***
                            //Glyph.OffsetXY(newGlyph, arg1, arg2);
                            //TODO: replement this
                        }
                    }


                }
                else
                {
                    //two point numbers. 
                    //the first point number indicates the point that is to be matched to the new glyph. 
                    //The second number indicates the new glyph's “matched” point. 
                    //Once a glyph is added,its point numbers begin directly after the last glyphs (endpoint of first glyph + 1) 
                }

                //
                if (finalGlyph == null)
                {
                    finalGlyph = newGlyph;
                }
                else
                {
                    //merge 
                    //TODO: impl 
                    //Glyph.AppendGlyph(finalGlyph, newGlyph);
                }

            } while (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.MORE_COMPONENTS));


            //
            if (Glyf.HasFlag(flags, Glyf.CompositeGlyphFlags.WE_HAVE_INSTRUCTIONS))
            {
                //ushort numInstr = reader.ReadUInt16();
                //finalGlyph._instructionLen = reader.ReadUInt16();//load instruction later
                //byte[] insts = reader.ReadBytes(numInstr);
                // finalGlyph.GlyphInstructions = insts;
            }


            //F2DOT14 	16-bit signed fixed number with the low 14 bits of fraction (2.14).
            //Transformation Option
            //
            //The C pseudo-code fragment below shows how the composite glyph information is stored and parsed; definitions for “flags” bits follow this fragment:
            //  do {
            //    USHORT flags;
            //    USHORT glyphIndex;
            //    if ( flags & ARG_1_AND_2_ARE_WORDS) {
            //    (SHORT or FWord) argument1;
            //    (SHORT or FWord) argument2;
            //    } else {
            //        USHORT arg1and2; /* (arg1 << 8) | arg2 */
            //    }
            //    if ( flags & WE_HAVE_A_SCALE ) {
            //        F2Dot14  scale;    /* Format 2.14 */
            //    } else if ( flags & WE_HAVE_AN_X_AND_Y_SCALE ) {
            //        F2Dot14  xscale;    /* Format 2.14 */
            //        F2Dot14  yscale;    /* Format 2.14 */
            //    } else if ( flags & WE_HAVE_A_TWO_BY_TWO ) {
            //        F2Dot14  xscale;    /* Format 2.14 */
            //        F2Dot14  scale01;   /* Format 2.14 */
            //        F2Dot14  scale10;   /* Format 2.14 */
            //        F2Dot14  yscale;    /* Format 2.14 */
            //    }
            //} while ( flags & MORE_COMPONENTS ) 
            //if (flags & WE_HAVE_INSTR){
            //    USHORT numInstr
            //    BYTE instr[numInstr]
            //------------------------------------------------------------ 


            return finalGlyph ?? null;// Glyph.Empty;
        }

        struct TripleEncodingRecord
        {
            public readonly byte ByteCount;
            public readonly byte XBits;
            public readonly byte YBits;
            public readonly ushort DeltaX;
            public readonly ushort DeltaY;
            public readonly short Xsign;
            public readonly short Ysign;

            public TripleEncodingRecord(
                byte byteCount,
                byte xbits, byte ybits,
                ushort deltaX, ushort deltaY,
                short xsign, short ysign)
            {
                ByteCount = byteCount;
                XBits = xbits;
                YBits = ybits;
                DeltaX = deltaX;
                DeltaY = deltaY;
                Xsign = xsign;
                Ysign = ysign;
#if DEBUG
                debugIndex = -1;
#endif
            }
#if DEBUG
            public int debugIndex;
            public override string ToString()
            {
                return debugIndex + " " + ByteCount + " " + XBits + " " + YBits + " " + DeltaX + " " + DeltaY + " " + Xsign + " " + Ysign;
            }
#endif
            /// <summary>
            /// translate X
            /// </summary>
            /// <param name="orgX"></param>
            /// <returns></returns>
            public int Tx(int orgX) => (orgX + DeltaX) * Xsign;

            /// <summary>
            /// translate Y
            /// </summary>
            /// <param name="orgY"></param>
            /// <returns></returns>
            public int Ty(int orgY) => (orgY + DeltaY) * Ysign;

        }

        class TripleEncodingTable
        {
            List<TripleEncodingRecord> _records = new List<TripleEncodingRecord>();


            public TripleEncodingTable()
            {

                BuildTable();

#if DEBUG
                if (_records.Count != 128)
                {
                    throw new System.Exception();
                }
                dbugValidateTable();
#endif
            }
#if DEBUG
            void dbugValidateTable()
            {
#if DEBUG
                for (int xyFormat = 0; xyFormat < 128; ++xyFormat)
                {
                    TripleEncodingRecord tripleRec = _records[xyFormat];
                    if (xyFormat < 84)
                    {
                        //0-83 inclusive
                        if ((tripleRec.ByteCount - 1) != 1)
                        {
                            throw new System.NotSupportedException();
                        }
                    }
                    else if (xyFormat < 120)
                    {
                        //84-119 inclusive
                        if ((tripleRec.ByteCount - 1) != 2)
                        {
                            throw new System.NotSupportedException();
                        }
                    }
                    else if (xyFormat < 124)
                    {
                        //120-123 inclusive
                        if ((tripleRec.ByteCount - 1) != 3)
                        {
                            throw new System.NotSupportedException();
                        }
                    }
                    else if (xyFormat < 128)
                    {
                        //124-127 inclusive
                        if ((tripleRec.ByteCount - 1) != 4)
                        {
                            throw new System.NotSupportedException();
                        }
                    }
                }

#endif
            }
#endif
            public TripleEncodingRecord this[int index] => _records[index];

            void BuildTable()
            {
                // Each of the 128 index values define the following properties and specified in details in the table below:

                // Byte count(total number of bytes used for this set of coordinate values including one byte for 'flag' value).
                // Number of bits used to represent X coordinate value(X bits).
                // Number of bits used to represent Y coordinate value(Y bits).
                // An additional incremental amount to be added to X bits value(delta X).
                // An additional incremental amount to be added to Y bits value(delta Y).
                // The sign of X coordinate value(X sign).
                // The sign of Y coordinate value(Y sign).

                //Please note that “Byte Count” field reflects total size of the triplet(flag, xCoordinate, yCoordinate), 
                //including ‘flag’ value that is encoded in a separate stream.


                //Triplet Encoding
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign

                //(set 1.1)
                //0     2            0       8       N/A       0     N/A     -   
                //1                                            0             +
                //2                                           256            -
                //3                                           256            +
                //4                                           512            -
                //5                                           512            +
                //6                                           768            -
                //7                                           768            +
                //8                                           1024           -
                //9                                           1024           +
                BuildRecords(2, 0, 8, null, new ushort[] { 0, 256, 512, 768, 1024 }); //2*5

                //---------------------------------------------------------------------
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 1.2)
                //10    2            8       0        0       N/A     -     N/A
                //11                                  0               +
                //12                                256               -
                //13                                256               +
                //14                                512               -
                //15                                512               +
                //16                                768               -
                //17                                768               +
                //18                                1024              -
                //19                                1024              +
                BuildRecords(2, 8, 0, new ushort[] { 0, 256, 512, 768, 1024 }, null); //2*5

                //---------------------------------------------------------------------
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 2.1)
                //20    2           4       4        1        1       -      -
                //21                                          1       +      -
                //22                                          1       -      +
                //23                                          1       +      +
                //24                                          17      -      -
                //25                                          17      +      -
                //26                                          17      -      +
                //27                                          17      +      +
                //28                                          33      -      - 
                //29                                          33      +      -
                //30                                          33      -      +
                //31                                          33      +      +
                //32                                          49      -      -
                //33                                          49      +      -
                //34                                          49      -      +
                //35                                          49      +      +  
                BuildRecords(2, 4, 4, new ushort[] { 1 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 2.2)
                //36    2           4       4       17        1       -      -
                //37                                          1       +      -
                //38                                          1       -      +
                //39                                          1       +      +
                //40                                          17      -      -
                //41                                          17      +      -
                //42                                          17      -      + 
                //43                                          17      +      +
                //44                                          33      -      - 
                //45                                          33      +      -
                //46                                          33      -      +
                //47                                          33      +      +
                //48                                          49      -      -
                //49                                          49      +      -
                //50                                          49      -      +
                //51                                          49      +      +
                BuildRecords(2, 4, 4, new ushort[] { 17 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 2.3)
                //52    2           4          4     33        1      -      -
                //53                                           1      +      -
                //54                                           1      -      +
                //55                                           1      +      +
                //56                                          17      -      -
                //57                                          17      +      -
                //58                                          17      -      +
                //59                                          17      +      +
                //60                                          33      -      -
                //61                                          33      +      -
                //62                                          33      -      +
                //63                                          33      +      +
                //64                                          49      -      -
                //65                                          49      +      -
                //66                                          49      -      +
                //67                                          49      +      +
                BuildRecords(2, 4, 4, new ushort[] { 33 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 2.4)
                //68    2           4         4     49         1      -      -
                //69                                           1      +      -
                //70                                           1      -      +
                //71                                           1      +      +
                //72                                          17      -      -
                //73                                          17      +      -
                //74                                          17      -     +
                //75                                          17      +     +
                //76                                          33      -     -
                //77                                          33      +     -
                //78                                          33      -     +
                //79                                          33      +     +
                //80                                          49      -     -
                //81                                          49      +     -
                //82                                          49      -     +
                //83                                          49      +     +
                BuildRecords(2, 4, 4, new ushort[] { 49 }, new ushort[] { 1, 17, 33, 49 });// 4*4 => 16 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 3.1)
                //84    3             8       8         1      1      -     -
                //85                                           1      +     -
                //86                                           1      -     +
                //87                                           1      +     +
                //88                                         257      -     -
                //89                                         257      +     -
                //90                                         257      -     +
                //91                                         257      +     +
                //92                                         513      -     -
                //93                                         513      +     -
                //94                                         513      -     +
                //95                                         513      +     +
                BuildRecords(3, 8, 8, new ushort[] { 1 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 3.2)
                //96    3               8       8      257      1     -      -
                //97                                            1     +      -
                //98                                            1     -      +
                //99                                            1     +      +
                //100                                         257     -      -
                //101                                         257     +      -
                //102                                         257     -      +
                //103                                         257     +      +
                //104                                         513     -      -
                //105                                         513     +      -
                //106                                         513     -      +
                //107                                         513     +      +
                BuildRecords(3, 8, 8, new ushort[] { 257 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 3.3)
                //108   3              8        8       513     1     -      -
                //109                                           1     +      -
                //110                                           1     -      +
                //111                                           1     +      +
                //112                                         257     -      -
                //113                                         257     +      -
                //114                                         257     -      +
                //115                                         257     +      +
                //116                                         513     -      -
                //117                                         513     +      -
                //118                                         513     -      +
                //119                                         513     +      +
                BuildRecords(3, 8, 8, new ushort[] { 513 }, new ushort[] { 1, 257, 513 });// 4*3 => 12 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 4)
                //120   4               12     12         0      0    -      -
                //121                                                 +      -
                //122                                                 -      +
                //123                                                 +      +
                BuildRecords(4, 12, 12, new ushort[] { 0 }, new ushort[] { 0 }); // 4*1 => 4 records

                //---------------------------------------------------------------------            
                //Index ByteCount   Xbits   Ybits   DeltaX  DeltaY  Xsign   Ysign
                //(set 5)
                //124   5               16      16      0       0     -      -
                //125                                                 +      -
                //126                                                 -      +
                //127                                                 +      + 
                BuildRecords(5, 16, 16, new ushort[] { 0 }, new ushort[] { 0 });// 4*1 => 4 records

            }
            void AddRecord(byte byteCount, byte xbits, byte ybits, ushort deltaX, ushort deltaY, short xsign, short ysign)
            {
                var rec = new TripleEncodingRecord(byteCount, xbits, ybits, deltaX, deltaY, xsign, ysign);
#if DEBUG
                rec.debugIndex = _records.Count;
#endif
                _records.Add(rec);
            }
            void BuildRecords(byte byteCount, byte xbits, byte ybits, ushort[] deltaXs, ushort[] deltaYs)
            {
                if (deltaXs == null)
                {
                    //(set 1.1)
                    for (int y = 0; y < deltaYs.Length; ++y)
                    {
                        AddRecord(byteCount, xbits, ybits, 0, deltaYs[y], 0, -1);
                        AddRecord(byteCount, xbits, ybits, 0, deltaYs[y], 0, 1);
                    }
                }
                else if (deltaYs == null)
                {
                    //(set 1.2)
                    for (int x = 0; x < deltaXs.Length; ++x)
                    {
                        AddRecord(byteCount, xbits, ybits, deltaXs[x], 0, -1, 0);
                        AddRecord(byteCount, xbits, ybits, deltaXs[x], 0, 1, 0);
                    }
                }
                else
                {
                    //set 2.1, - set5
                    for (int x = 0; x < deltaXs.Length; ++x)
                    {
                        ushort deltaX = deltaXs[x];

                        for (int y = 0; y < deltaYs.Length; ++y)
                        {
                            ushort deltaY = deltaYs[y];

                            AddRecord(byteCount, xbits, ybits, deltaX, deltaY, -1, -1);
                            AddRecord(byteCount, xbits, ybits, deltaX, deltaY, 1, -1);
                            AddRecord(byteCount, xbits, ybits, deltaX, deltaY, -1, 1);
                            AddRecord(byteCount, xbits, ybits, deltaX, deltaY, 1, 1);
                        }
                    }
                }
            }
        }

    }
    class TransformedLoca : UnreadTableEntry
    {
        public TransformedLoca(TableHeader header, Woff2TableDirectory tableDir) : base(header)
        {
            HasCustomContentReader = true;
            TableDir = tableDir;
        }
        public Woff2TableDirectory TableDir { get; }
        public override T CreateTableEntry<T>(BinaryReader reader, T expectedResult)
        {
            GlyphLocations loca = expectedResult as GlyphLocations;
            if (loca == null) throw new System.NotSupportedException();


            return expectedResult;
        }


        void ReconstructGlyphLocationTable(BinaryReader reader, Woff2TableDirectory woff2TableDir)
        {

            //5.3.Transformed loca table format

            //The loca table data can be presented in the WOFF2 file in one of two formats defined by
            //the transformation version number(encoded in the table directory flag bits, see subclause 4.1 for details).

            //The transformation version "3" defines a null transform where
            //the content of the loca table is presented in its original, unmodified format.

            //The transformation version "0", although optional,
            //MUST be applied to the loca table data whenever glyf table data is transformed.
            //In other words, both glyf and loca tables must either be present in their transformed format or 
            //with null transform applied to both tables.

            //The version "0" of the loca table transformation(as defined by the table directory flag bits, 
            //see subclause 4.1 for details) is specified below.

            //The transformLength of the transformed loca table MUST always be zero.//***
            //The origLength MUST be the appropriate size(determined by numGlyphs + 1, 
            //times the size per glyph, where that size per glyph is two bytes when indexFormat(defined in subclause 5.1.Transformed glyf table format) is zero
            //, otherwise four bytes).

            //If the transformLength of the transformed loca table is not equal to zero,
            //or if the encoded origLength does not match the calculated size defined above the decoder MUST reject the WOFF2 file as invalid.

            //The loca table MUST be reconstructed when the glyf table is decoded.
            //The process for reconstructing the loca table is specified in subclause 5.1 as part of the transformed glyf table decoding process.

            //For reconstructed glyph records,
            //a decoder MUST store the corresponding offsets for individual glyphs 
            //using a format that is indicated by the indexFormat field of the Transformed glyf Table.

            //NOT like the glyf,=> format 0, transform len must be 0***
            reader.BaseStream.Position += woff2TableDir.origLength;




        }


    }

    public class Woff2Reader
    {

        Woff2Header _header;

        public BrotliDecompressStreamFunc DecompressHandler;

        public PreviewFontInfo ReadPreview(BinaryReader reader)
        {
            return null;
        }
        public Typeface Read(Stream inputstream)
        {

            using (ByteOrderSwappingBinaryReader reader = new ByteOrderSwappingBinaryReader(inputstream))
            {
                _header = ReadHeader(reader);
                if (_header == null) return null;  //=> return here and notify user too. 
                                                   //
                Woff2TableDirectory[] woff2TablDirs = ReadTableDirectories(reader);


                if (DecompressHandler == null)
                {
                    //if no Brotli decoder=> return here and notify user too.
                    if (Woff2DefaultBrotliDecompressFunc.DecompressHandler != null)
                    {
                        DecompressHandler = Woff2DefaultBrotliDecompressFunc.DecompressHandler;
                    }
                    else
                    {
                        //return here and notify user too. 
                        return null;
                    }
                }



                //try read each compressed tables
                byte[] compressedBuffer = reader.ReadBytes((int)_header.totalCompressSize);
                if (compressedBuffer.Length != _header.totalCompressSize)
                {
                    //error!
                    return null; //can't read this, notify user too.
                }




                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    if (!DecompressHandler(compressedBuffer, decompressedStream))
                    {
                        //...Most notably, 
                        //the data for the font tables is compressed in a SINGLE data stream comprising all the font tables.

                        //if not pass set to null
                        //decompressedBuffer = null;
                        return null;
                    }
                    //from decoded stream we read each table
                    decompressedStream.Position = 0;//reset pos

                    using (ByteOrderSwappingBinaryReader reader2 = new ByteOrderSwappingBinaryReader(decompressedStream))
                    {
                        TableEntryCollection tableEntryCollection = CreateTableEntryCollection(woff2TablDirs);
                        OpenFontReader openFontReader = new OpenFontReader();
                        return openFontReader.ReadTableEntryCollection(tableEntryCollection, reader2);
                    }
                }
            }
        }
        Woff2Header ReadHeader(BinaryReader reader)
        {
            //WOFF2 Header
            //UInt32  signature             0x774F4632 'wOF2'
            //UInt32  flavor                The "sfnt version" of the input font.
            //UInt32  length                Total size of the WOFF file.
            //UInt16  numTables             Number of entries in directory of font tables.
            //UInt16  reserved              Reserved; set to 0.
            //UInt32  totalSfntSize         Total size needed for the uncompressed font data, including the sfnt header,
            //                              directory, and font tables(including padding).
            //UInt32  totalCompressedSize   Total length of the compressed data block.
            //UInt16  majorVersion          Major version of the WOFF file.
            //UInt16  minorVersion          Minor version of the WOFF file.
            //UInt32  metaOffset            Offset to metadata block, from beginning of WOFF file.
            //UInt32  metaLength            Length of compressed metadata block.
            //UInt32  metaOrigLength        Uncompressed size of metadata block.
            //UInt32  privOffset            Offset to private data block, from beginning of WOFF file.
            //UInt32  privLength            Length of private data block.

            Woff2Header header = new Woff2Header();
            byte b0 = reader.ReadByte();
            byte b1 = reader.ReadByte();
            byte b2 = reader.ReadByte();
            byte b3 = reader.ReadByte();
            if (!(b0 == 0x77 && b1 == 0x4f && b2 == 0x46 && b3 == 0x32))
            {
                return null;
            }
            header.flavor = reader.ReadUInt32();
            string flavorName = Utils.TagToString(header.flavor);

            header.length = reader.ReadUInt32();
            header.numTables = reader.ReadUInt16();
            ushort reserved = reader.ReadUInt16();
            header.totalSfntSize = reader.ReadUInt32();
            header.totalCompressSize = reader.ReadUInt32();//***

            header.majorVersion = reader.ReadUInt16();
            header.minorVersion = reader.ReadUInt16();

            header.metaOffset = reader.ReadUInt32();
            header.metaLength = reader.ReadUInt32();
            header.metaOriginalLength = reader.ReadUInt32();

            header.privOffset = reader.ReadUInt32();
            header.privLength = reader.ReadUInt32();

            return header;
        }

        Woff2TableDirectory[] ReadTableDirectories(BinaryReader reader)
        {

            uint tableCount = (uint)_header.numTables; //?
            var tableDirs = new Woff2TableDirectory[tableCount];

            long expectedTableStartAt = 0;

            for (int i = 0; i < tableCount; ++i)
            {
                //TableDirectoryEntry
                //UInt8         flags           table type and flags
                //UInt32        tag	            4-byte tag(optional)
                //UIntBase128   origLength      length of original table
                //UIntBase128   transformLength transformed length(if applicable)

                Woff2TableDirectory table = new Woff2TableDirectory();
                byte flags = reader.ReadByte();
                //The interpretation of the flags field is as follows.

                //Bits[0..5] contain an index to the "known tag" table, 
                //which represents tags likely to appear in fonts.If the tag is not present in this table,
                //then the value of this bit field is 63. 

                //interprete flags 
                int knowTable = flags & 0x1F; //5 bits => known table or not  
                string tableName = null;
                if (knowTable < 63)
                {
                    //this is known table
                    tableName = s_knownTableTags[knowTable];
                }
                else
                {
                    tableName = Utils.TagToString(reader.ReadUInt32()); //other tag 
                }

                table.Name = tableName;

                //Bits 6 and 7 indicate the preprocessing transformation version number(0 - 3) that was applied to each table.

                //For all tables in a font, except for 'glyf' and 'loca' tables,
                //transformation version 0 indicates the null transform where the original table data is passed directly 
                //to the Brotli compressor for inclusion in the compressed data stream.

                //For 'glyf' and 'loca' tables,
                //transformation version 3 indicates the null transform where the original table data was passed directly 
                //to the Brotli compressor without applying any pre - processing defined in subclause 5.1 and subclause 5.3.

                //The transformed table formats and their associated transformation version numbers are 
                //described in details in clause 5 of this specification.


                table.PreprocessingTransformation = (byte)((flags >> 5) & 0x3); //2 bits, preprocessing transformation


                table.ExpectedStartAt = expectedTableStartAt;
                //
                if (!ReadUIntBase128(reader, out table.origLength))
                {
                    //can't read 128=> error
                }

                switch (table.PreprocessingTransformation)
                {
                    default:
                        break;
                    case 0:
                        {
                            if (table.Name == Glyf._N)
                            {
                                if (!ReadUIntBase128(reader, out table.transformLength))
                                {
                                    //can't read 128=> error
                                }
                                expectedTableStartAt += table.transformLength;
                            }
                            else if (table.Name == GlyphLocations._N)
                            {
                                //BUT by spec, transform 'loca' MUST has transformLength=0
                                if (!ReadUIntBase128(reader, out table.transformLength))
                                {
                                    //can't read 128=> error
                                }
                                expectedTableStartAt += table.origLength;
                            }
                            else
                            {
                                expectedTableStartAt += table.origLength;
                            }
                        }
                        break;
                    case 1:
                        {
                            expectedTableStartAt += table.origLength;
                        }
                        break;
                    case 2:
                        {
                            expectedTableStartAt += table.origLength;
                        }
                        break;
                    case 3:
                        {
                            expectedTableStartAt += table.origLength;
                        }
                        break;
                }
                tableDirs[i] = table;
            }

            return tableDirs;
        }

        static TableEntryCollection CreateTableEntryCollection(Woff2TableDirectory[] woffTableDirs)
        {
            TableEntryCollection tableEntryCollection = new TableEntryCollection();
            for (int i = 0; i < woffTableDirs.Length; ++i)
            {
                Woff2TableDirectory woffTableDir = woffTableDirs[i];
                UnreadTableEntry unreadTableEntry = null;


                TableHeader tableHeader = new TableHeader(woffTableDir.Name, 0,
                                        (uint)woffTableDir.ExpectedStartAt,
                                        woffTableDir.origLength);

                if (woffTableDir.Name == Glyf._N && woffTableDir.PreprocessingTransformation == 0)
                {
                    //this is transformed glyf table,
                    //we need another techqniue 
                    unreadTableEntry = new TransformedGlyf(tableHeader, woffTableDir);

                }
                else if (woffTableDir.Name == GlyphLocations._N && woffTableDir.PreprocessingTransformation == 0)
                {
                    //this is transformed glyf table,
                    //we need another techqniue 
                    unreadTableEntry = new TransformedLoca(tableHeader, woffTableDir);
                }
                else
                {
                    unreadTableEntry = new UnreadTableEntry(tableHeader);
                }
                tableEntryCollection.AddEntry(unreadTableEntry);
            }

            return tableEntryCollection;
        }


        static readonly string[] s_knownTableTags = new string[]
        {
             //Known Table Tags
            //Flag  Tag         Flag  Tag       Flag  Tag        Flag    Tag
            //0	 => cmap,	    16 =>EBLC,	    32 =>CBDT,	     48 =>gvar,
            //1  => head,	    17 =>gasp,	    33 =>CBLC,	     49 =>hsty,
            //2	 => hhea,	    18 =>hdmx,	    34 =>COLR,	     50 =>just,
            //3	 => hmtx,	    19 =>kern,	    35 =>CPAL,	     51 =>lcar,
            //4	 => maxp,	    20 =>LTSH,	    36 =>SVG ,	     52 =>mort,
            //5	 => name,	    21 =>PCLT,	    37 =>sbix,	     53 =>morx,
            //6	 => OS/2,	    22 =>VDMX,	    38 =>acnt,	     54 =>opbd,
            //7	 => post,	    23 =>vhea,	    39 =>avar,	     55 =>prop,
            //8	 => cvt ,	    24 =>vmtx,	    40 =>bdat,	     56 =>trak,
            //9	 => fpgm,	    25 =>BASE,	    41 =>bloc,	     57 =>Zapf,
            //10 =>	glyf,	    26 =>GDEF,	    42 =>bsln,	     58 =>Silf,
            //11 =>	loca,	    27 =>GPOS,	    43 =>cvar,	     59 =>Glat,
            //12 =>	prep,	    28 =>GSUB,	    44 =>fdsc,	     60 =>Gloc,
            //13 =>	CFF ,	    29 =>EBSC,	    45 =>feat,	     61 =>Feat,
            //14 =>	VORG,	    30 =>JSTF,	    46 =>fmtx,	     62 =>Sill,
            //15 =>	EBDT,	    31 =>MATH,	    47 =>fvar,	     63 =>arbitrary tag follows,...
            //-------------------------------------------------------------------

            //-- TODO:implement missing table too!
            Cmap._N, //0
            Head._N, //1
            HorizontalHeader._N,//2
            HorizontalMetrics._N,//3
            MaxProfile._N,//4
            NameEntry._N,//5
            OS2Table._N, //6
            PostTable._N,//7
            CvtTable._N,//8
            FpgmTable._N,//9
            Glyf._N,//10
            GlyphLocations._N,//11
            PrepTable._N,//12
            CFFTable._N,//13
            "VORG",//14 
            "EBDT",//15, 

            //---------------
            EBLCTable._N,//16
            Gasp._N,//17
            "hdmx",//18
            Kern._N,//19
            "LTSH",//20 
            "PCLT",//21
            VerticalDeviceMetrics._N,//22
            VerticalHeader._N,//23
            VerticalMetrics._N,//24
            BASE._N,//25
            GDEF._N,//26
            GPOS._N,//27
            GSUB._N,//28            
            "EBSC", //29
            "JSTF", //30
            MathTable._N,//31
             //---------------


            //Known Table Tags (copy,same as above)
            //Flag  Tag         Flag  Tag       Flag  Tag        Flag    Tag
            //0	 => cmap,	    16 =>EBLC,	    32 =>CBDT,	     48 =>gvar,
            //1  => head,	    17 =>gasp,	    33 =>CBLC,	     49 =>hsty,
            //2	 => hhea,	    18 =>hdmx,	    34 =>COLR,	     50 =>just,
            //3	 => hmtx,	    19 =>kern,	    35 =>CPAL,	     51 =>lcar,
            //4	 => maxp,	    20 =>LTSH,	    36 =>SVG ,	     52 =>mort,
            //5	 => name,	    21 =>PCLT,	    37 =>sbix,	     53 =>morx,
            //6	 => OS/2,	    22 =>VDMX,	    38 =>acnt,	     54 =>opbd,
            //7	 => post,	    23 =>vhea,	    39 =>avar,	     55 =>prop,
            //8	 => cvt ,	    24 =>vmtx,	    40 =>bdat,	     56 =>trak,
            //9	 => fpgm,	    25 =>BASE,	    41 =>bloc,	     57 =>Zapf,
            //10 =>	glyf,	    26 =>GDEF,	    42 =>bsln,	     58 =>Silf,
            //11 =>	loca,	    27 =>GPOS,	    43 =>cvar,	     59 =>Glat,
            //12 =>	prep,	    28 =>GSUB,	    44 =>fdsc,	     60 =>Gloc,
            //13 =>	CFF ,	    29 =>EBSC,	    45 =>feat,	     61 =>Feat,
            //14 =>	VORG,	    30 =>JSTF,	    46 =>fmtx,	     62 =>Sill,
            //15 =>	EBDT,	    31 =>MATH,	    47 =>fvar,	     63 =>arbitrary tag follows,...
            //-------------------------------------------------------------------

            "CBDT", //32
            "CBLC",//33
            COLR._N,//34
            CPAL._N,//35,
            SvgTable._N,//36
            "sbix",//37
            "acnt",//38
            "avar",//39
            "bdat",//40
            "bloc",//41
            "bsln",//42
            "cvar",//43
            "fdsc",//44
            "feat",//45
            "fmtx",//46
            "fvar",//47
             //---------------

            "gvar",//48
            "hsty",//49
            "just",//50
            "lcar",//51
            "mort",//52
            "morx",//53
            "opbd",//54
            "prop",//55
            "trak",//56
            "Zapf",//57
            "Silf",//58
            "Glat",//59
            "Gloc",//60
            "Feat",//61
            "Sill",//62
            "...." //63 arbitrary tag follows
        };



        static bool ReadUIntBase128(BinaryReader reader, out uint result)
        {

            //UIntBase128 Data Type

            //UIntBase128 is a different variable length encoding of unsigned integers,
            //suitable for values up to 2^(32) - 1.

            //A UIntBase128 encoded number is a sequence of bytes for which the most significant bit
            //is set for all but the last byte,
            //and clear for the last byte.

            //The number itself is base 128 encoded in the lower 7 bits of each byte.
            //Thus, a decoding procedure for a UIntBase128 is: 
            //start with value = 0.
            //Consume a byte, setting value = old value times 128 + (byte bitwise - and 127).
            //Repeat last step until the most significant bit of byte is false.

            //UIntBase128 encoding format allows a possibility of sub-optimal encoding,
            //where e.g.the same numerical value can be represented with variable number of bytes(utilizing leading 'zeros').
            //For example, the value 63 could be encoded as either one byte 0x3F or two(or more) bytes: [0x80, 0x3f].
            //An encoder must not allow this to happen and must produce shortest possible encoding. 
            //A decoder MUST reject the font file if it encounters a UintBase128 - encoded value with leading zeros(a value that starts with the byte 0x80),
            //if UintBase128 - encoded sequence is longer than 5 bytes,
            //or if a UintBase128 - encoded value exceeds 232 - 1.

            //The "C-like" pseudo - code describing how to read the UIntBase128 format is presented below:
            //bool ReadUIntBase128(data, * result)
            //            {
            //                UInt32 accum = 0;

            //                for (i = 0; i < 5; i++)
            //                {
            //                    UInt8 data_byte = data.getNextUInt8();

            //                    // No leading 0's
            //                    if (i == 0 && data_byte = 0x80) return false;

            //                    // If any of top 7 bits are set then << 7 would overflow
            //                    if (accum & 0xFE000000) return false;

            //                    *accum = (accum << 7) | (data_byte & 0x7F);

            //                    // Spin until most significant bit of data byte is false
            //                    if ((data_byte & 0x80) == 0)
            //                    {
            //                        *result = accum;
            //                        return true;
            //                    }
            //                }
            //                // UIntBase128 sequence exceeds 5 bytes
            //                return false;
            //            }

            uint accum = 0;
            result = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte data_byte = reader.ReadByte();
                // No leading 0's
                if (i == 0 && data_byte == 0x80) return false;

                // If any of top 7 bits are set then << 7 would overflow
                if ((accum & 0xFE000000) != 0) return false;
                //
                accum = (uint)(accum << 7) | (uint)(data_byte & 0x7F);
                // Spin until most significant bit of data byte is false
                if ((data_byte & 0x80) == 0)
                {
                    result = accum;
                    return true;
                }
                //
            }
            // UIntBase128 sequence exceeds 5 bytes
            return false;
        }

      

    }
    class Woff2Utils
    {
        //public static short[] ReadInt16Array(BinaryReader reader, uint nRecords)
        //{
        //    short[] arr = new short[nRecords];
        //    for (uint i = 0; i < nRecords; ++i)
        //    {
        //        arr[i] = reader.ReadInt16();
        //    }
        //    return arr;
        //}

        const byte ONE_MORE_BYTE_CODE1 = 255;
        const byte ONE_MORE_BYTE_CODE2 = 254;
        const byte WORD_CODE = 253;
        const byte LOWEST_UCODE = 253;

        public static short[] ReadInt16Array(BinaryReader reader, int count)
        {
            short[] arr = new short[count];
            for (int i = 0; i < count; ++i)
            {
                arr[i] = reader.ReadInt16();
            }
            return arr;
        }
        public static ushort[] Read255UShortArray(BinaryReader reader, int count)
        {
            ushort[] arr = new ushort[count];
            for (int i = 0; i < count; ++i)
            {
                arr[i] = Read255UShort(reader);
            }
            return arr;
        }
        public static ushort Read255UShort(BinaryReader reader)
        {
            //255UInt16 Variable-length encoding of a 16-bit unsigned integer for optimized intermediate font data storage.
            //255UInt16 Data Type
            //255UInt16 is a variable-length encoding of an unsigned integer 
            //in the range 0 to 65535 inclusive.
            //This data type is intended to be used as intermediate representation of various font values,
            //which are typically expressed as UInt16 but represent relatively small values.
            //Depending on the encoded value, the length of the data field may be one to three bytes,
            //where the value of the first byte either represents the small value itself or is treated as a code that defines the format of the additional byte(s).
            //The "C-like" pseudo-code describing how to read the 255UInt16 format is presented below:
            //   Read255UShort(data )
            //    {
            //                UInt8 code;
            //                UInt16 value, value2;

            //                const oneMoreByteCode1    = 255;
            //                const oneMoreByteCode2    = 254;
            //                const wordCode            = 253;
            //                const lowestUCode         = 253;

            //                code = data.getNextUInt8();
            //                if (code == wordCode)
            //                {
            //                    /* Read two more bytes and concatenate them to form UInt16 value*/
            //                    value = data.getNextUInt8();
            //                    value <<= 8;
            //                    value &= 0xff00;
            //                    value2 = data.getNextUInt8();
            //                    value |= value2 & 0x00ff;
            //                }
            //                else if (code == oneMoreByteCode1)
            //                {
            //                    value = data.getNextUInt8();
            //                    value = (value + lowestUCode);
            //                }
            //                else if (code == oneMoreByteCode2)
            //                {
            //                    value = data.getNextUInt8();
            //                    value = (value + lowestUCode * 2);
            //                }
            //                else
            //                {
            //                    value = code;
            //                }
            //                return value;
            //            } 
            //Note that the encoding is not unique.For example, 
            //the value 506 can be encoded as [255, 253], [254, 0], and[253, 1, 250]. 
            //An encoder may produce any of these, and a decoder MUST accept them all.An encoder should choose shorter encodings,
            //and must be consistent in choice of encoding for the same value, as this will tend to compress better.



            byte code = reader.ReadByte();
            if (code == WORD_CODE)
            {
                /* Read two more bytes and concatenate them to form UInt16 value*/
                int value = (reader.ReadByte() << 8) & 0xff00;
                int value2 = reader.ReadByte();
                return (ushort)(value | (value2 & 0xff));
            }
            else if (code == ONE_MORE_BYTE_CODE1)
            {
                return (ushort)(reader.ReadByte() + LOWEST_UCODE);
            }
            else if (code == ONE_MORE_BYTE_CODE2)
            {
                return (ushort)(reader.ReadByte() + (LOWEST_UCODE * 2));
            }
            else
            {
                return code;
            }
        }
    }


}