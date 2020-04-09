//BSD, 2014-present, WinterDev
//MIT, 2018-present, WinterDev
using System;
using System.IO;
namespace PixelFarm.Platforms
{
    public abstract class StorageServiceProvider
    {
        public abstract string[] GetDataDirNameList(string dir);
        public abstract string[] GetDataNameList(string dir);
        public abstract bool DataExists(string dataName);
        public abstract void SaveData(string dataName, byte[] content);
        public abstract byte[] ReadData(string dataName);
        public Stream ReadDataStream(string dataName)
        {
            byte[] data = ReadData(dataName);
            return new MemoryStream(data);
        }
    }

    public static class StorageService
    {
        static StorageServiceProvider s_provider;
        public static void RegisterProvider(StorageServiceProvider provider)
        {
#if DEBUG
            if (s_provider != null)
            {

            }
#endif
            s_provider = provider;
        }
        public static StorageServiceProvider Provider => s_provider;
    }
}