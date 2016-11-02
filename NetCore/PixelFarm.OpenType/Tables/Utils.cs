//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
using System.IO;
namespace NOpenType.Tables
{
    static class Utils
    {


        public static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        public static short[] ReadInt16Array(BinaryReader reader, int nRecords)
        {
            short[] arr = new short[nRecords];
            int i = 0;
            for (int n = nRecords - 1; n >= 0; --n)
            {
                arr[i++] = reader.ReadInt16();
            }

            return arr;
        }
        public static ushort[] ReadUInt16Array(BinaryReader reader, int nRecords)
        {
            ushort[] arr = new ushort[nRecords];
            int i = 0;
            for (int n = nRecords - 1; n >= 0; --n)
            {
                arr[i++] = reader.ReadUInt16();
            }
            
            return arr;
        }

    }
}