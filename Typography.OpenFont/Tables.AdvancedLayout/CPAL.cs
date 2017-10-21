// Copyright © 2017 Sam Hocevar <sam@hocevar.net>, WinterDev
// Apache2


using System.IO;

namespace Typography.OpenFont.Tables
{
    public class CPAL : TableEntry
    {
        public override string Name { get { return "CPAL"; } }


        byte[] _colorRBGABuffer;
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
            _colorRBGABuffer = reader.ReadBytes(4 * ColorCount);
        }
        public ushort[] Palettes { get; private set; }
        public ushort ColorCount { get; private set; }
        public void GetColor(int colorIndex, out byte r, out byte g, out byte b, out byte a)
        {
            byte[] colorRBGABuffer = _colorRBGABuffer;
            int startAt = colorIndex * 4;//rgba
            r = colorRBGABuffer[startAt];
            g = colorRBGABuffer[startAt + 1];
            b = colorRBGABuffer[startAt + 2];
            a = colorRBGABuffer[startAt + 3];
        }
    }
}

