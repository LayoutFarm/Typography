//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
namespace NRasterizer.Tables
{
    class Glyf : TableEntry
    {
        List<Glyph> _glyphs;
        public Glyf(GlyphLocations glyphLocations)
        {
            this.GlyphLocations = glyphLocations;
        }
        public List<Glyph> Glyphs
        {
            get { return _glyphs; }
        }
        public override string Name
        {
            get { return "glyf"; }
        }
        public GlyphLocations GlyphLocations
        {
            get;
            private set;
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            _glyphs = new List<Glyph>();
            GlyphLocations locations = this.GlyphLocations;
            int glyphCount = locations.GlyphCount;
            uint tableOffset = this.Header.Offset;
            for (int i = 0; i < glyphCount; i++)
            {
                reader.BaseStream.Seek(tableOffset, SeekOrigin.Begin);//reset 
                reader.BaseStream.Seek(locations.Offsets[i], SeekOrigin.Current);
                uint length = locations.Offsets[i + 1] - locations.Offsets[i];
                if (length > 0)
                {
                    short contoursCount = reader.ReadInt16();
                    Bounds bounds = BoundsReader.ReadFrom(reader);
                    if (contoursCount >= 0)
                    {
                        _glyphs.Add(ReadSimpleGlyph(reader, contoursCount, bounds));
                    }
                    else
                    {
                        _glyphs.Add(ReadCompositeGlyph(reader, -contoursCount, bounds));
                    }
                }
                else
                {
                    _glyphs.Add(Glyph.Empty);
                }
            }
        }
        static bool HasFlag(Flag target, Flag test)
        {
            return (target & test) == test;
        }
        static Flag[] ReadFlags(BinaryReader input, int flagCount)
        {
            var result = new Flag[flagCount];
            int i = 0;
            int repeatCount = 0;
            var flag = (Flag)0;
            while (i < flagCount)
            {
                if (repeatCount > 0)
                {
                    repeatCount--;
                }
                else
                {
                    flag = (Flag)input.ReadByte();
                    if (HasFlag(flag, Flag.Repeat))
                    {
                        repeatCount = input.ReadByte();
                    }
                }
                result[i++] = flag;
            }
            return result;
        }

        static short[] ReadCoordinates(BinaryReader input, int pointCount, Flag[] flags, Flag isByte, Flag signOrSame)
        {
            var xs = new short[pointCount];
            int x = 0;
            for (int i = 0; i < pointCount; i++)
            {
                int dx;
                if (HasFlag(flags[i], isByte))
                {
                    byte b = input.ReadByte();
                    dx = HasFlag(flags[i], signOrSame) ? b : -b;
                }
                else
                {
                    if (HasFlag(flags[i], signOrSame))
                    {
                        dx = 0;
                    }
                    else
                    {
                        dx = input.ReadInt16();
                    }
                }
                x += dx;
                xs[i] = (short)x; // TODO: overflow?
            }
            return xs;
        }

        static Glyph ReadSimpleGlyph(BinaryReader input, int count, Bounds bounds)
        {
            var endPoints = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                endPoints[i] = input.ReadUInt16();
            }

            ushort instructionSize = input.ReadUInt16();
            byte[] instructions = input.ReadBytes(instructionSize);
            // TODO: should this take the max points rather?
            int pointCount = endPoints[count - 1] + 1; // TODO: count can be zero?
            Flag[] flags = ReadFlags(input, pointCount);
            short[] xs = ReadCoordinates(input, pointCount, flags, Flag.XByte, Flag.XSignOrSame);
            short[] ys = ReadCoordinates(input, pointCount, flags, Flag.YByte, Flag.YSignOrSame);

            bool[] onCurves = new bool[flags.Length];
            for (int i = onCurves.Length - 1; i >= 0; --i)
            {
                onCurves[i] = HasFlag(flags[i], Flag.OnCurve);
            }

            return new Glyph(xs, ys, onCurves, endPoints, bounds);
        }

        static Glyph ReadCompositeGlyph(BinaryReader input, int count, Bounds bounds)
        {
            // TODO: Parse composite glyphs
            return Glyph.Empty;
        }

        [Flags]
        enum Flag : byte
        {
            OnCurve = 1,
            XByte = 1 << 1,
            YByte = 1 << 2,
            Repeat = 1 << 3,
            XSignOrSame = 1 << 4,
            YSignOrSame = 1 << 5
        }

    }
}
