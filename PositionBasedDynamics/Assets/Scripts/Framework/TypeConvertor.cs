using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public static class TypeConvertor
    {
        public static sbyte[] ByteArrayToSbyteArray(byte[] array)
        {
            if (array == null)
            {
                return null;
            }
            sbyte[] sArray = new sbyte[array.Length];
            Buffer.BlockCopy(array, 0, sArray, 0, array.Length);

            return sArray;
        }

        public static byte[] SbyteArrayToByteArray(sbyte[] array, int length = -1)
        {
            if (array == null)
            {
                return null;
            }
            byte[] bArray;
            if (length == -1)
            {
                bArray = new byte[array.Length];
                Buffer.BlockCopy(array, 0, bArray, 0, array.Length);
            }
            else
            {
                bArray = new byte[length];
                Buffer.BlockCopy(array, 0, bArray, 0, length);
            }

            return bArray;
        }

        public static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null)
                return string.Empty;

            if (buffer.Length <= 3)
            {
                return System.Text.Encoding.UTF8.GetString(buffer);
            }

            byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

            if (buffer[0] == bomBuffer[0]
                && buffer[1] == bomBuffer[1]
                && buffer[2] == bomBuffer[2])
            {
                return new System.Text.UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }

            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public static string GetUTF8String(sbyte[] buffer)
        {
            byte[] bytes = SbyteArrayToByteArray(buffer);
            return GetUTF8String(bytes);
        }
    }
}
