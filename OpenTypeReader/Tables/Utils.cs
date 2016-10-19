//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System; 
using System.Text;
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
    }
}