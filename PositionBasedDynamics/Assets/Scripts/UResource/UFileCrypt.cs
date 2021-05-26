using Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UResource
{
    public class UFileCrypt
    {
        private bool toggleScan = false;

        private static string MAGICS_STR = "HLF0";

        private static byte[] MAGICS_BYTES = Encoding.UTF8.GetBytes(MAGICS_STR); // HLF0
        private const int FILE_LENGTH = sizeof(int); // ab文件实际长度
        private const int FILE_VERSION = sizeof(int); // ab版本号
        private const int ENCRYPT_FLAG = sizeof(byte); // ab文件是否加密

        private static int PREFFIX_LEN = MAGICS_BYTES.Length + FILE_LENGTH + FILE_VERSION + ENCRYPT_FLAG;
        private byte[] CacheMagic;

        private const int m_nCacheSize = 1 * 1024 * 1024;
        private byte[] m_cacheData;

        public UFileCrypt()
        {
            CacheMagic = new byte[PREFFIX_LEN];
            m_cacheData = new byte[m_nCacheSize];
        }

        ~UFileCrypt()
        {

        }

        // 解密一个文件 
        public byte[] DecryptFileNew(string filePath, ref int len, bool isAb = true)
        {
            byte[] buffer = null;
            try
            {
                // int id = HLProfiler.StartProfiler();
                // 文件判断是否存在
                if (File.Exists(filePath))
                {
                    // 获取 文件名
                    string strFile = Path.GetFileName(filePath);
                    using (Stream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        buffer = new byte[len];
                        file.Seek(0, SeekOrigin.Begin);
                        file.Read(buffer, 0, len);
                    }
                }
                else
                {
                    Log.Error("HLFileCrypt: DecryptFileNew", filePath + " Not Found File");
                    buffer = null;
                    if (!toggleScan)
                    {
                        toggleScan = true;
                        string dir = Path.GetDirectoryName(filePath);
                        string[] files = Directory.GetFiles(dir);
                        foreach (string file in files)
                        {
                            Log.Info("ab file: ", file);
                        }
                    }
                }
                // HLProfiler.StopProfiler(id, "DecryptFileNew " + filePath, true);
            }
            catch (Exception ex)
            {
                buffer = null;
                Log.Error("HLFileCrypt", filePath + " DecryptFileNew: [err]= " + ex.ToString());
            }
            return buffer;
        }
    }
}
