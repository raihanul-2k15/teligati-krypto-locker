using System;
using System.IO;

namespace TeligatiKrypto
{
    public static class Config
    {
        public static string AppName = "Teligati Krypto";
        public static string AppDataFileName = "data.txt";
        public static int CryptoKeySize = 1024;
        public static byte XORKey = 38;

        private static string _userFileExt = ".txt";

        public static string AppDataFolderPath
        {
            get
            {
                return  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    AppName);
            }
        }
        public static string AppDataFilePath
        {
            get
            {
                return Path.Combine(AppDataFolderPath,
                    AppDataFileName);
            }
        }

        public static string GetAppDataUserFile(string u)
        {
            return Path.Combine(AppDataFolderPath, u + _userFileExt);
        }
    }
}
