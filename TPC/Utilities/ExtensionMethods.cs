using System;
using System.Text;

namespace TPC.Utilities
{
    public static class ExtensionMethods
    {
        public static string ToHexString(this byte[] data)
        {
            return BitConverter.ToString(data);
        }

        public static byte[] ToByteArray(this string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public static string ToASCIIString(this byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
