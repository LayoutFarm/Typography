//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
using System.IO;
namespace NRasterizer.Tables
{
    static class Utils
    {
        public static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return Encoding.ASCII.GetString(bytes);
        }
        public static short[] ReadInt16Array(BinaryReader reader, int nRecords)
        {
            short[] arr = new short[nRecords];
            for (int i = 0; i < nRecords; ++i)
            {
                arr[i] = reader.ReadInt16();
            }
            return arr;
        }
    }
}