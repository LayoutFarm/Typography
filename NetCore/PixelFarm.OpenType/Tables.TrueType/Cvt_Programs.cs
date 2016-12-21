//MIT, 2015-2016, Michael Popoloski, WinterDev
using System;
using System.IO;
namespace NOpenType.Tables
{
    struct FUnit
    {
        int value;

        public static explicit operator int(FUnit v) { return v.value; }
        public static explicit operator FUnit(int v) { return new FUnit { value = v }; }

        public static FUnit operator -(FUnit lhs, FUnit rhs) { return (FUnit)(lhs.value - rhs.value); }
        public static FUnit operator +(FUnit lhs, FUnit rhs) { return (FUnit)(lhs.value + rhs.value); }
        public static float operator *(FUnit lhs, float rhs) { return lhs.value * rhs; }

        public static FUnit Max(FUnit a, FUnit b) { return (FUnit)Math.Max(a.value, b.value); }
        public static FUnit Min(FUnit a, FUnit b) { return (FUnit)Math.Min(a.value, b.value); }
#if DEBUG
        public override string ToString()
        {
            return value.ToString();
        }
#endif
    }


    class CvtTable : TableEntry
    {
        FUnit[] funits;
        public override string Name
        {
            get { return "cvt "; /*need 4 chars*/}
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            int nelems = (int)(this.TableLength / sizeof(short));
            var results = new FUnit[nelems];
            for (int i = 0; i < nelems; i++)
            {
                results[i] = (FUnit)reader.ReadInt16();
            }
            this.funits = results;
        }
    }
    class PrepTable : TableEntry
    {   
        internal byte[] programBuffer;
        public override string Name
        {
            get { return "prep"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
    class FpgmTable : TableEntry
    {
        internal byte[] programBuffer;
        public override string Name
        {
            get { return "fpgm"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            programBuffer = reader.ReadBytes((int)this.TableLength);
        }
    }
}