////BSD, 2014-2016, WinterDev
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;

//namespace PixelFarm.Agg.Image
//{
//    public static class ImageTgaIO
//    {
//        // Header of a TGA file
//        public struct STargaHeader
//        {
//            public byte PostHeaderSkip;
//            public byte ColorMapType;		// 0 = RGB, 1 = Palette
//            public byte ImageType;			// 1 = Palette, 2 = RGB, 3 = mono, 9 = RLE Palette, 10 = RLE RGB, 11 RLE mono
//            public ushort ColorMapStart;
//            public ushort ColorMapLength;
//            public byte ColorMapBits;
//            public ushort XStart;				// offsets the image would like to have (ignored)
//            public ushort YStart;				// offsets the image would like to have (ignored)
//            public ushort Width;
//            public ushort Height;
//            public byte BPP;				// bit depth of the image
//            public byte Descriptor;

//            public void BinaryWrite(BinaryWriter writerToWriteTo)
//            {
//                writerToWriteTo.Write(PostHeaderSkip);
//                writerToWriteTo.Write(ColorMapType);
//                writerToWriteTo.Write(ImageType);
//                writerToWriteTo.Write(ColorMapStart);
//                writerToWriteTo.Write(ColorMapLength);
//                writerToWriteTo.Write(ColorMapBits);
//                writerToWriteTo.Write(XStart);
//                writerToWriteTo.Write(YStart);
//                writerToWriteTo.Write(Width);
//                writerToWriteTo.Write(Height);
//                writerToWriteTo.Write(BPP);
//                writerToWriteTo.Write(Descriptor);
//            }
//        };

//        private const int TargaHeaderSize = 18;
//        const int RGB_BLUE = 2;
//        const int RGB_GREEN = 1;
//        const int RGB_RED = 0;
//        const int RGBA_ALPHA = 3;

//        // these are used during loading (only valid during load)
//        static int TGABytesPerLine;

//        static void Do24To8Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            throw new System.NotImplementedException();
//#if false

//            int i;
//            if (Width) 
//            {
//                i = 0;
//                Dest = &Dest[Height*Width];
//                do 
//                {
//                    if(p[RGB_RED] == 0 && p[RGB_GREEN] == 0 && p[RGB_BLUE] == 0)
//                    {
//                        Dest[i] = 0;
//                    }
//                    else
//                    {
//                        // no other color can map to color 0
//                        Dest[i] =(byte) pStaticRemap->GetColorIndex(p[RGB_RED], p[RGB_GREEN], p[RGB_BLUE], 1);
//                    }
//                    p += 3;
//                } while (++i<Width);
//            }
//#endif
//        }

//        static void Do32To8Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            throw new System.NotImplementedException();

//#if false
//            int i;
//            if (Width) 
//            {
//                i = 0;
//                Dest = &Dest[Height*Width];
//                do 
//                {
//                    if(p[RGB_RED] == 0 && p[RGB_GREEN] == 0 && p[RGB_BLUE] == 0)
//                    {
//                        Dest[i] = 0;
//                    }
//                    else
//                    {
//                        // no other color can map to color 0
//                        Dest[i] = (byte)pStaticRemap->GetColorIndex(p[RGB_RED], p[RGB_GREEN], p[RGB_BLUE], 1);
//                    }
//                    p += 4;
//                } while (++i < Width);
//            }
//#endif
//        }

//        static unsafe void Do24To24Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            if (Width > 0)
//            {
//                int DestOffset = Height * Width * 3;
//                for (int i = 0; i < Width * 3; i++)
//                {
//                    Dest[DestOffset + i] = Source[SourceOffset + i];
//                }
//            }
//        }

//        static unsafe void Do32To24Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            if (Width > 0)
//            {
//                int i = 0;
//                int DestOffest = Height * Width * 3;
//                do
//                {
//                    Dest[DestOffest + i * 3 + RGB_BLUE] = Source[SourceOffset + RGB_BLUE];
//                    Dest[DestOffest + i * 3 + RGB_GREEN] = Source[SourceOffset + RGB_GREEN];
//                    Dest[DestOffest + i * 3 + RGB_RED] = Source[SourceOffset + RGB_RED];
//                    SourceOffset += 4;
//                } while (++i < Width);
//            }
//        }

//        static unsafe void Do24To32Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            if (Width > 0)
//            {
//                int i = 0;
//                int DestOffest = Height * Width * 4;
//                do
//                {
//                    Dest[DestOffest + i * 4 + RGB_BLUE] = Source[SourceOffset + RGB_BLUE];
//                    Dest[DestOffest + i * 4 + RGB_GREEN] = Source[SourceOffset + RGB_GREEN];
//                    Dest[DestOffest + i * 4 + RGB_RED] = Source[SourceOffset + RGB_RED];
//                    Dest[DestOffest + i * 4 + 3] = 255;
//                    SourceOffset += 3;
//                } while (++i < Width);
//            }
//        }

//        static unsafe void Do32To32Bit(byte[] Dest, byte[] Source, int SourceOffset, int Width, int Height)
//        {
//            if (Width > 0)
//            {
//                int i = 0;
//                int DestOffest = Height * Width * 4;
//                do
//                {
//                    Dest[DestOffest + RGB_BLUE] = Source[SourceOffset + RGB_BLUE];
//                    Dest[DestOffest + RGB_GREEN] = Source[SourceOffset + RGB_GREEN];
//                    Dest[DestOffest + RGB_RED] = Source[SourceOffset + RGB_RED];
//                    Dest[DestOffest + RGBA_ALPHA] = Source[SourceOffset + RGBA_ALPHA];
//                    SourceOffset += 4;
//                    DestOffest += 4;
//                } while (++i < Width);
//            }
//        }

//        static bool ReadTGAInfo(byte[] WorkPtr, out STargaHeader TargaHeader)
//        {
//            TargaHeader.PostHeaderSkip = WorkPtr[0];
//            TargaHeader.ColorMapType = WorkPtr[1];
//            TargaHeader.ImageType = WorkPtr[2];
//            TargaHeader.ColorMapStart = BitConverter.ToUInt16(WorkPtr, 3);
//            TargaHeader.ColorMapLength = BitConverter.ToUInt16(WorkPtr, 5);
//            TargaHeader.ColorMapBits = WorkPtr[7];
//            TargaHeader.XStart = BitConverter.ToUInt16(WorkPtr, 8);
//            TargaHeader.YStart = BitConverter.ToUInt16(WorkPtr, 10);
//            TargaHeader.Width = BitConverter.ToUInt16(WorkPtr, 12);
//            TargaHeader.Height = BitConverter.ToUInt16(WorkPtr, 14);
//            TargaHeader.BPP = WorkPtr[16];
//            TargaHeader.Descriptor = WorkPtr[17];



//            // check the header
//            if (TargaHeader.ColorMapType != 0 ||	// 0 = RGB, 1 = Palette
//                // 1 = Palette, 2 = RGB, 3 = mono, 9 = RLE Palette, 10 = RLE RGB, 11 RLE mono
//                (TargaHeader.ImageType != 2 && TargaHeader.ImageType != 10 && TargaHeader.ImageType != 9) ||
//                (TargaHeader.BPP != 24 && TargaHeader.BPP != 32))
//            {
//#if ASSERTS_ENABLED
//                if ( ((byte*)pTargaHeader)[0] == 'B' && ((byte*)pTargaHeader)[1] == 'M' )
//                {
//                    assert(!"This TGA's header looks like a BMP!"); //  look at the first two bytes and see if they are 'BM'
//                    // if so it's a BMP not a TGA
//                }
//                else
//                {
//                    byte * pColorMapType = NULL;
//                    switch (TargaHeader.ColorMapType)
//                    {
//                        case 0:
//                            pColorMapType = "RGB Color Map";
//                            break;
//                        case 1:
//                            pColorMapType = "Palette Color Map";
//                            break;
//                        default:
//                            pColorMapType = "<Illegal Color Map>";
//                            break;
//                    }
//                    byte * pImageType = NULL;
//                    switch (TargaHeader.ImageType)
//                    {
//                        case 1:
//                            pImageType = "Palette Image Type";
//                            break;
//                        case 2:
//                            pImageType = "RGB Image Type";
//                            break;
//                        case 3:
//                            pImageType = "mono Image Type";
//                            break;
//                        case 9:
//                            pImageType = "RLE Palette Image Type";
//                            break;
//                        case 10:
//                            pImageType = "RLE RGB Image Type";
//                            break;
//                        case 11:
//                            pImageType = "RLE mono Image Type";
//                            break;
//                        default:
//                            pImageType = "<Illegal Image Type>";
//                            break;
//                    }
//                    int ColorDepth = TargaHeader.BPP;
//                    CJString ErrorString;
//                    ErrorString.Format( "Image type %s %s (%u bpp) not supported!", pColorMapType, pImageType, ColorDepth);
//                    ShowSystemMessage("TGA File IO Error", ErrorString.GetBytePtr(), "TGA Error");
//                }
//#endif // ASSERTS_ENABLED
//                return false;
//            }

//            return true;
//        }

//        const int IS_PIXLE_RUN = 0x80;
//        const int RUN_LENGTH_MASK = 0x7f;

//        static unsafe int Decompress(byte[] pDecompressBits, byte[] pBitsToPars, int ParsOffset, int Width, int Depth, int LineBeingRead)
//        {
//            int DecompressOffset = 0;
//            int Total = 0;
//            do
//            {
//                int i;
//                int NumPixels = (pBitsToPars[ParsOffset] & RUN_LENGTH_MASK) + 1;
//                Total += NumPixels;
//                if ((pBitsToPars[ParsOffset++] & IS_PIXLE_RUN) != 0)
//                {
//                    // decompress the run for NumPixels
//                    byte r, g, b, a;
//                    b = pBitsToPars[ParsOffset++];
//                    g = pBitsToPars[ParsOffset++];
//                    r = pBitsToPars[ParsOffset++];
//                    switch (Depth)
//                    {
//                        case 24:
//                            for (i = 0; i < NumPixels; i++)
//                            {
//                                pDecompressBits[DecompressOffset++] = b;
//                                pDecompressBits[DecompressOffset++] = g;
//                                pDecompressBits[DecompressOffset++] = r;
//                            }
//                            break;

//                        case 32:
//                            a = pBitsToPars[ParsOffset++];
//                            for (i = 0; i < NumPixels; i++)
//                            {
//                                pDecompressBits[DecompressOffset++] = b;
//                                pDecompressBits[DecompressOffset++] = g;
//                                pDecompressBits[DecompressOffset++] = r;
//                                pDecompressBits[DecompressOffset++] = a;
//                            }
//                            break;

//                        default:
//                            throw new System.Exception("Bad bit depth.");
//                    }
//                }
//                else // store NumPixels normally
//                {
//                    switch (Depth)
//                    {
//                        case 24:
//                            for (i = 0; i < NumPixels * 3; i++)
//                            {
//                                pDecompressBits[DecompressOffset++] = pBitsToPars[ParsOffset++];
//                            }
//                            break;

//                        case 32:
//                            for (i = 0; i < NumPixels * 4; i++)
//                            {
//                                pDecompressBits[DecompressOffset++] = pBitsToPars[ParsOffset++];
//                            }
//                            break;

//                        default:
//                            throw new System.Exception("Bad bit depth.");
//                    }
//                }
//            } while (Total < Width);

//            if (Total > Width)
//            {
//                throw new System.Exception("The TGA you loaded is corrupt (line " + LineBeingRead.ToString() + ").");
//            }

//            return ParsOffset;
//        }


//        static unsafe int LowLevelReadTGABitsFromBuffer(ImageReaderWriterBase imageToReadTo, byte[] wholeFileBuffer, int DestBitDepth)
//        {
//            throw new NotSupportedException();

//            //STargaHeader TargaHeader = new STargaHeader();
//            //int FileReadOffset;

//            //if (!ReadTGAInfo(wholeFileBuffer, out TargaHeader))
//            //{
//            //    return 0;
//            //}

//            //// if the frame we are loading is different then the one we have allocated
//            //// or we don't have any bits allocated

//            //if ((imageToReadTo.Width * imageToReadTo.Height) != (TargaHeader.Width * TargaHeader.Height))
//            //{
//            //    imageToReadTo.Allocate(TargaHeader.Width, TargaHeader.Height, TargaHeader.Width * DestBitDepth / 8, DestBitDepth);
//            //}

//            //// work out the line width
//            //switch (imageToReadTo.BitDepth)
//            //{
//            //    case 24:
//            //        TGABytesPerLine = imageToReadTo.Width * 3;
//            //        if (imageToReadTo.GetRecieveBlender() == null)
//            //        {
//            //            imageToReadTo.SetRecieveBlender(new BlenderBGR());
//            //        }
//            //        break;

//            //    case 32:
//            //        TGABytesPerLine = imageToReadTo.Width * 4;
//            //        if (imageToReadTo.GetRecieveBlender() == null)
//            //        {
//            //            imageToReadTo.SetRecieveBlender(new BlenderBGRA());
//            //        }
//            //        break;

//            //    default:
//            //        throw new System.Exception("Bad bit depth.");
//            //}

//            //if (TGABytesPerLine > 0)
//            //{
//            //    byte[] BufferToDecompressTo = null;
//            //    FileReadOffset = TargaHeaderSize + TargaHeader.PostHeaderSkip;

//            //    if (TargaHeader.ImageType == 10) // 10 is RLE compressed
//            //    {
//            //        BufferToDecompressTo = new byte[TGABytesPerLine * 2];
//            //    }

//            //    // read all the lines *
//            //    for (int i = 0; i < imageToReadTo.Height; i++)
//            //    {
//            //        byte[] BufferToCopyFrom;
//            //        int CopyOffset = 0;

//            //        int CurReadLine;

//            //        // bit 5 tells us if the image is stored top to bottom or bottom to top
//            //        if ((TargaHeader.Descriptor & 0x20) != 0)
//            //        {
//            //            // bottom to top
//            //            CurReadLine = imageToReadTo.Height - i - 1;
//            //        }
//            //        else
//            //        {
//            //            // top to bottom
//            //            CurReadLine = i;
//            //        }

//            //        if (TargaHeader.ImageType == 10) // 10 is RLE compressed
//            //        {
//            //            FileReadOffset = Decompress(BufferToDecompressTo, wholeFileBuffer, FileReadOffset, imageToReadTo.Width, TargaHeader.BPP, CurReadLine);
//            //            BufferToCopyFrom = BufferToDecompressTo;
//            //        }
//            //        else
//            //        {
//            //            BufferToCopyFrom = wholeFileBuffer;
//            //            CopyOffset = FileReadOffset;
//            //        }

//            //        int bufferOffset;
//            //        byte[] imageBuffer = imageToReadTo.GetBuffer(out bufferOffset);

//            //        switch (imageToReadTo.BitDepth)
//            //        {
//            //            case 8:
//            //                switch (TargaHeader.BPP)
//            //                {
//            //                    case 24:
//            //                        Do24To8Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;

//            //                    case 32:
//            //                        Do32To8Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;
//            //                }
//            //                break;

//            //            case 24:
//            //                switch (TargaHeader.BPP)
//            //                {
//            //                    case 24:
//            //                        Do24To24Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;

//            //                    case 32:
//            //                        Do32To24Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;
//            //                }
//            //                break;

//            //            case 32:
//            //                switch (TargaHeader.BPP)
//            //                {
//            //                    case 24:
//            //                        Do24To32Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;

//            //                    case 32:
//            //                        Do32To32Bit(imageBuffer, BufferToCopyFrom, CopyOffset, imageToReadTo.Width, CurReadLine);
//            //                        break;
//            //                }
//            //                break;

//            //            default:
//            //                throw new System.Exception("Bad bit depth");
//            //        }

//            //        if (TargaHeader.ImageType != 10) // 10 is RLE compressed
//            //        {
//            //            FileReadOffset += TGABytesPerLine;
//            //        }
//            //    }
//            //}

//            //return TargaHeader.Width;
//        }

//        const int MAX_RUN_LENGTH = 127;
//        static int memcmp(byte[] pCheck, int CheckOffset, byte[] pSource, int SourceOffset, int Width)
//        {
//            for (int i = 0; i < Width; i++)
//            {
//                if (pCheck[CheckOffset + i] < pSource[SourceOffset + i])
//                {
//                    return -1;
//                }
//                if (pCheck[CheckOffset + i] > pSource[SourceOffset + i])
//                {
//                    return 1;
//                }
//            }

//            return 0;
//        }

//        static int GetSameLength(byte[] checkBufer, int checkOffset, byte[] sourceBuffer, int sourceOffsetToNextPixel, int numBytesInPixel, int maxSameLengthWidth)
//        {
//            int Count = 0;
//            while (memcmp(checkBufer, checkOffset, sourceBuffer, sourceOffsetToNextPixel, numBytesInPixel) == 0 && Count < maxSameLengthWidth)
//            {
//                Count++;
//                sourceOffsetToNextPixel += numBytesInPixel;
//            }

//            return Count;
//        }

//        static int GetDifLength(byte[] pCheck, byte[] pSource, int SourceOffset, int numBytesInPixel, int Max)
//        {
//            int Count = 0;
//            while (memcmp(pCheck, 0, pSource, SourceOffset, numBytesInPixel) != 0 && Count < Max)
//            {
//                Count++;
//                for (int i = 0; i < numBytesInPixel; i++)
//                {
//                    pCheck[i] = pSource[SourceOffset + i];
//                }
//                SourceOffset += numBytesInPixel;
//            }

//            return Count;
//        }

//        const int MIN_RUN_LENGTH = 2;

//        static int CompressLine8(byte[] destBuffer, byte[] sourceBuffer, int sourceOffset, int Width)
//        {
//            int WritePos = 0;
//            int pixelsProcessed = 0;

//            while (pixelsProcessed < Width)
//            {
//                // always get as many as you can that are the same first
//                int Max = System.Math.Min(MAX_RUN_LENGTH, (Width - 1) - pixelsProcessed);
//                int SameLength = GetSameLength(sourceBuffer, sourceOffset, sourceBuffer, sourceOffset + 1, 1, Max);
//                if (SameLength >= MIN_RUN_LENGTH)
//                //if(SameLength)
//                {
//                    // write in the count
//                    if (SameLength > MAX_RUN_LENGTH)
//                    {
//                        throw new System.Exception("Bad Length");
//                    }
//                    destBuffer[WritePos++] = (byte)((SameLength) | IS_PIXLE_RUN);

//                    // write in the same length pixel value
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset];

//                    pixelsProcessed += SameLength + 1;
//                }
//                else
//                {
//                    byte CheckPixel = sourceBuffer[sourceOffset];
//                    int DifLength = Max;

//                    if (DifLength == 0)
//                    {
//                        DifLength = 1;
//                    }
//                    // write in the count (if there is only one the count is 0)
//                    if (DifLength > MAX_RUN_LENGTH)
//                    {
//                        throw new System.Exception("Bad Length");
//                    }

//                    destBuffer[WritePos++] = (byte)(DifLength - 1);

//                    while (DifLength-- != 0)
//                    {
//                        // write in the same length pixel value
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset++];
//                        pixelsProcessed++;
//                    }
//                }
//            }

//            return WritePos;
//        }

//        static byte[] differenceHold = new byte[4];

//        static int CompressLine24(byte[] destBuffer, byte[] sourceBuffer, int sourceOffset, int Width)
//        {
//            int WritePos = 0;
//            int pixelsProcessed = 0;

//            while (pixelsProcessed < Width)
//            {
//                // always get as many as you can that are the same first
//                int Max = System.Math.Min(MAX_RUN_LENGTH, (Width - 1) - pixelsProcessed);
//                int SameLength = GetSameLength(sourceBuffer, sourceOffset, sourceBuffer, sourceOffset + 3, 3, Max);
//                if (SameLength > 0)
//                {
//                    // write in the count
//                    if (SameLength > MAX_RUN_LENGTH)
//                    {
//                        throw new Exception();
//                    }

//                    destBuffer[WritePos++] = (byte)((SameLength) | IS_PIXLE_RUN);

//                    // write in the same length pixel value
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 0];
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 1];
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 2];

//                    sourceOffset += (SameLength) * 3;
//                    pixelsProcessed += SameLength + 1;
//                }
//                else
//                {
//                    differenceHold[0] = sourceBuffer[sourceOffset + 0];
//                    differenceHold[1] = sourceBuffer[sourceOffset + 1];
//                    differenceHold[2] = sourceBuffer[sourceOffset + 2];
//                    int DifLength = GetDifLength(differenceHold, sourceBuffer, sourceOffset + 3, 3, Max);
//                    if (DifLength == 0)
//                    {
//                        DifLength = 1;
//                    }

//                    // write in the count (if there is only one the count is 0)
//                    if (SameLength > MAX_RUN_LENGTH)
//                    {
//                        throw new Exception();
//                    }
//                    destBuffer[WritePos++] = (byte)(DifLength - 1);

//                    while (DifLength-- > 0)
//                    {
//                        // write in the same length pixel value
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 0];
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 1];
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 2];

//                        sourceOffset += 3;
//                        pixelsProcessed++;
//                    }
//                }
//            }

//            return WritePos;
//        }

//        static int CompressLine32(byte[] destBuffer, byte[] sourceBuffer, int sourceOffset, int Width)
//        {
//            int WritePos = 0;
//            int pixelsProcessed = 0;

//            while (pixelsProcessed < Width)
//            {
//                // always get as many as you can that are the same first
//                int Max = System.Math.Min(MAX_RUN_LENGTH, (Width - 1) - pixelsProcessed);
//                int SameLength = GetSameLength(sourceBuffer, sourceOffset, sourceBuffer, sourceOffset + 4, 4, Max);
//                if (SameLength > 0)
//                {
//                    // write in the count
//                    if (SameLength > MAX_RUN_LENGTH)
//                    {
//                        throw new Exception();
//                    }

//                    destBuffer[WritePos++] = (byte)((SameLength) | IS_PIXLE_RUN);

//                    // write in the same length pixel value
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 0];
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 1];
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 2];
//                    destBuffer[WritePos++] = sourceBuffer[sourceOffset + 3];

//                    sourceOffset += (SameLength) * 4;
//                    pixelsProcessed += SameLength + 1;
//                }
//                else
//                {
//                    differenceHold[0] = sourceBuffer[sourceOffset + 0];
//                    differenceHold[1] = sourceBuffer[sourceOffset + 1];
//                    differenceHold[2] = sourceBuffer[sourceOffset + 2];
//                    differenceHold[3] = sourceBuffer[sourceOffset + 3];
//                    int DifLength = GetDifLength(differenceHold, sourceBuffer, sourceOffset + 4, 4, Max);
//                    if (DifLength == 0)
//                    {
//                        DifLength = 1;
//                    }

//                    // write in the count (if there is only one the count is 0)
//                    if (SameLength > MAX_RUN_LENGTH)
//                    {
//                        throw new Exception();
//                    }
//                    destBuffer[WritePos++] = (byte)(DifLength - 1);

//                    while (DifLength-- > 0)
//                    {
//                        // write in the dif length pixel value
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 0];
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 1];
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 2];
//                        destBuffer[WritePos++] = sourceBuffer[sourceOffset + 3];

//                        sourceOffset += 4;
//                        pixelsProcessed++;
//                    }
//                }
//            }

//            return WritePos;
//            /*
//            while(SourcePos < Width)
//            {
//                // always get as many as you can that are the same first
//                int Max = System.Math.Min(MAX_RUN_LENGTH, (Width - 1) - SourcePos);
//                int SameLength = GetSameLength((byte*)&pSource[SourcePos], (byte*)&pSource[SourcePos + 1], 4, Max);
//                if(SameLength)
//                {
//                    // write in the count
//                    assert(SameLength<= MAX_RUN_LENGTH);
//                    pDest[WritePos++] = (byte)((SameLength) | IS_PIXLE_RUN);

//                    // write in the same length pixel value
//                    pDest[WritePos++] = pSource[SourcePos].Blue;
//                    pDest[WritePos++] = pSource[SourcePos].Green;
//                    pDest[WritePos++] = pSource[SourcePos].Red;
//                    pDest[WritePos++] = pSource[SourcePos].Alpha;

//                    SourcePos += SameLength + 1;
//                }
//                else
//                {
//                    Pixel32 CheckPixel = pSource[SourcePos];
//                    int DifLength = GetDifLength((byte*)&CheckPixel, (byte*)&pSource[SourcePos+1], 4, Max);
//                    if(!DifLength)
//                    {
//                        DifLength = 1;
//                    }

//                    // write in the count (if there is only one the count is 0)
//                    assert(DifLength <= MAX_RUN_LENGTH);
//                    pDest[WritePos++] = (byte)(DifLength-1);

//                    while(DifLength--)
//                    {
//                        // write in the same length pixel value
//                        pDest[WritePos++] = pSource[SourcePos].Blue;
//                        pDest[WritePos++] = pSource[SourcePos].Green;
//                        pDest[WritePos++] = pSource[SourcePos].Red;
//                        pDest[WritePos++] = pSource[SourcePos].Alpha;
//                        SourcePos++;
//                    }
//                }
//            }

//            return WritePos;
//             */
//        }

//        static public bool SaveImageData(String fileNameToSaveTo, ImageReaderWriterBase image)
//        {
//            return Save(image, fileNameToSaveTo);
//        }

//        static public bool Save(ImageReaderWriterBase image, String fileNameToSaveTo)
//        {
//            Stream file = File.Open(fileNameToSaveTo, FileMode.Create);
//            return Save(image, file);
//        }

//        static public bool Save(ImageReaderWriterBase image, Stream streamToSaveImageDataTo)
//        {
//            STargaHeader TargaHeader;

//            BinaryWriter writerToSaveTo = new BinaryWriter(streamToSaveImageDataTo);

//            int SourceDepth = image.BitDepth;

//            // make sure there is something to save before opening the file
//            if (image.Width <= 0 || image.Height <= 0)
//            {
//                return false;
//            }

//            // set up the header
//            TargaHeader.PostHeaderSkip = 0;	// no skip after the header
//            if (SourceDepth == 8)
//            {
//                TargaHeader.ColorMapType = 1;		// Color type is Palette
//                TargaHeader.ImageType = 9;		// 1 = Palette, 9 = RLE Palette
//                TargaHeader.ColorMapStart = 0;
//                TargaHeader.ColorMapLength = 256;
//                TargaHeader.ColorMapBits = 24;
//            }
//            else
//            {
//                TargaHeader.ColorMapType = 0;		// Color type is RGB
//#if WRITE_RLE_COMPRESSED
//                TargaHeader.ImageType = 10;		// RLE RGB
//#else
//                TargaHeader.ImageType = 2;		// RGB
//#endif
//                TargaHeader.ColorMapStart = 0;
//                TargaHeader.ColorMapLength = 0;
//                TargaHeader.ColorMapBits = 0;
//            }
//            TargaHeader.XStart = 0;
//            TargaHeader.YStart = 0;
//            TargaHeader.Width = (ushort)image.Width;
//            TargaHeader.Height = (ushort)image.Height;
//            TargaHeader.BPP = (byte)SourceDepth;
//            TargaHeader.Descriptor = 0;	// all 8 bits are used for alpha

//            TargaHeader.BinaryWrite(writerToSaveTo);

//            byte[] pLineBuffer = new byte[Math.Abs(image.Stride) * 2];

//            //int BytesToSave;
//            switch (SourceDepth)
//            {
//                case 8:
//                    /*
//                if (image.HasPalette())
//                {
//                    for(int i=0; i<256; i++)
//                    {
//                        TGAFile.Write(image.GetPaletteIfAllocated()->pPalette[i * RGB_SIZE + RGB_BLUE]);
//                        TGAFile.Write(image.GetPaletteIfAllocated()->pPalette[i * RGB_SIZE + RGB_GREEN]);
//                        TGAFile.Write(image.GetPaletteIfAllocated()->pPalette[i * RGB_SIZE + RGB_RED]);
//                    }
//                } 
//                else 
//                     */
//                    {	// there is no palette for this DIB but we should write something
//                        for (int i = 0; i < 256; i++)
//                        {
//                            writerToSaveTo.Write((byte)i);
//                            writerToSaveTo.Write((byte)i);
//                            writerToSaveTo.Write((byte)i);
//                        }
//                    }
//                    for (int i = 0; i < image.Height; i++)
//                    {
//                        int bufferOffset;
//                        byte[] buffer = image.GetPixelPointerY(i, out bufferOffset);
//#if WRITE_RLE_COMPRESSED
//                    BytesToSave = CompressLine8(pLineBuffer, buffer, bufferOffset, image.Width());
//                    writerToSaveTo.Write(pLineBuffer, 0, BytesToSave);
//#else
//                        writerToSaveTo.Write(buffer, bufferOffset, image.Width);
//#endif
//                    }
//                    break;

//                case 24:
//                    for (int i = 0; i < image.Height; i++)
//                    {
//                        int bufferOffset;
//                        byte[] buffer = image.GetPixelPointerY(i, out bufferOffset);
//#if WRITE_RLE_COMPRESSED
//                    BytesToSave = CompressLine24(pLineBuffer, buffer, bufferOffset, image.Width());
//                    writerToSaveTo.Write(pLineBuffer, 0, BytesToSave);
//#else
//                        writerToSaveTo.Write(buffer, bufferOffset, image.Width * 3);
//#endif
//                    }
//                    break;

//                case 32:
//                    for (int i = 0; i < image.Height; i++)
//                    {
//                        int bufferOffset;
//                        byte[] buffer = image.GetPixelPointerY(i, out bufferOffset);
//#if WRITE_RLE_COMPRESSED
//                    BytesToSave = CompressLine32(pLineBuffer, buffer, bufferOffset, image.Width);
//                    writerToSaveTo.Write(pLineBuffer, 0, BytesToSave);
//#else
//                        writerToSaveTo.Write(buffer, bufferOffset, image.Width * 4);
//#endif
//                    }
//                    break;

//                default:
//                    throw new NotSupportedException();
//            }

//            writerToSaveTo.Close();
//            return true;
//        }

//        /*
//        bool SourceNeedsToBeResaved(String pFileName)
//        {
//            CFile TGAFile;
//            if(TGAFile.Open(pFileName, CFile::modeRead))
//            {
//                STargaHeader TargaHeader;
//                byte[] pWorkPtr = new byte[sizeof(STargaHeader)];

//                TGAFile.Read(pWorkPtr, sizeof(STargaHeader));
//                TGAFile.Close();

//                if(ReadTGAInfo(pWorkPtr, &TargaHeader))
//                {
//                    ArrayDeleteAndSetNull(pWorkPtr);
//                    return TargaHeader.ImageType != 10;
//                }

//                ArrayDeleteAndSetNull(pWorkPtr);
//            }

//            return true;
//        }
//         */

//        static public int ReadBitsFromBuffer(ImageReaderWriterBase image, byte[] WorkPtr, int destBitDepth)
//        {  
//            return LowLevelReadTGABitsFromBuffer(image, WorkPtr, destBitDepth);
//        }

//        public static bool LoadImageData(string fileName, ImageReaderWriterBase image)
//        {
//            return LoadImageData(image, fileName);
//        }

//        static public bool LoadImageData(ImageReaderWriterBase image, string fileName)
//        {
//            if (System.IO.File.Exists(fileName))
//            {
//                StreamReader streamReader = new StreamReader(fileName);
//                return LoadImageData(image, streamReader.BaseStream, 32);
//            }

//            return false;
//        }

//        static public bool LoadImageData(ImageReaderWriterBase image, Stream streamToLoadImageDataFrom, int destBitDepth)
//        {
//            byte[] ImageData = new byte[streamToLoadImageDataFrom.Length];
//            streamToLoadImageDataFrom.Read(ImageData, 0, (int)streamToLoadImageDataFrom.Length);
//            return ReadBitsFromBuffer(image, ImageData, destBitDepth) > 0;
//        }

//        static public int GetBitDepth(Stream streamToReadFrom)
//        {
//            STargaHeader TargaHeader;
//            byte[] ImageData = new byte[streamToReadFrom.Length];
//            streamToReadFrom.Read(ImageData, 0, (int)streamToReadFrom.Length);
//            if (ReadTGAInfo(ImageData, out TargaHeader))
//            {
//                return TargaHeader.BPP;
//            }

//            return 0;
//        }
//    }
//}
