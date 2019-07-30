using System;
using System.IO;
using System.Text;

namespace VCBitcoin {

    public class Util {

        public static string nHexDigits2 = "x2";
        public static string nHexDigits4 = "x4";
        public static string nHexDigits8 = "x8";

        public static T[] InstantiateArrayOf<T>(UInt64 length) where T : new() {

            T[] array = new T[length];

            for (UInt64 i = 0; i < length; ++i) {
                array[i] = new T();
            }

            return array;
        }

        public static string ByteArrayToString(byte[] byteArray) {

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);

            foreach (byte b in byteArray) {
                hex.AppendFormat("{0:x2}", b);

            }

            return hex.ToString();
        }

        public static string ByteArrayToStringRevEndian(byte[] byteArray) {

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);

            for (int i = byteArray.Length - 1; i >= 0; --i) {
                hex.AppendFormat("{0:x2}", byteArray[i]);

            }

            return hex.ToString();
        }

        public static byte[] StringToByteArray(string hexString) {

            int nChars = hexString.Length;
            byte[] byteArray = new byte[nChars / 2];

            for (int i = 0; i < nChars; i += 2) {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return byteArray;
        }

    }

}