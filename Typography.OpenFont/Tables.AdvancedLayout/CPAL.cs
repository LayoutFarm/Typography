// Copyright © 2017 Sam Hocevar <sam@hocevar.net>, WinterDev
// Apache2


using System.IO;

namespace Typography.OpenFont.Tables
{
    public class CPAL : TableEntry
    {
        public override string Name { get { return "CPAL"; } }


        byte[] _colorBGRABuffer;
        // Read the CPAL table
        // https://www.microsoft.com/typography/otspec/cpal.htm
        protected override void ReadContentFrom(BinaryReader reader)
        {
            long offset = reader.BaseStream.Position;

            ushort version = reader.ReadUInt16();
            ushort entryCount = reader.ReadUInt16(); // XXX: unused?
            ushort paletteCount = reader.ReadUInt16();
            ColorCount = reader.ReadUInt16();
            uint colorsOffset = reader.ReadUInt32();

            Palettes = Utils.ReadUInt16Array(reader, paletteCount);

            reader.BaseStream.Seek(offset + colorsOffset, SeekOrigin.Begin);
            _colorBGRABuffer = reader.ReadBytes(4 * ColorCount);
        }
        public ushort[] Palettes { get; private set; }
        public ushort ColorCount { get; private set; }
        public void GetColor(int colorIndex, out byte r, out byte g, out byte b, out byte a)
        {
            //Each color record has BGRA values. The color space for these values is sRGB.
            //Type Name    Description
            //uint8   blue    Blue value(B0).
            //uint8   green   Green value(B1).
            //uint8   red     Red value(B2).
            //uint8   alpha   Alpha value(B3).

            byte[] colorBGRABuffer = _colorBGRABuffer;
            int startAt = colorIndex * 4;//bgra
            b = colorBGRABuffer[startAt];
            g = colorBGRABuffer[startAt + 1];
            r = colorBGRABuffer[startAt + 2];
            a = colorBGRABuffer[startAt + 3]; 
        }
    }
}

