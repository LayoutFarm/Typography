//Apache2, 2018, Villu Ruusmann , Apache/PdfBox Authors ( https://github.com/apache/pdfbox)  
//Apache2, 2018, WinterDev 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Typography.OpenFont.CFF
{
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