using System;
using System.IO;
using System.Text;

namespace ExportDnf
{
    public static class Global
    {
        public static T ReadBytes<T>(this FileStream fs,int count,int offset = 0)
        {
            byte[] bytes;
            fs.Read(bytes = new byte[count], offset, count);
            Type type = typeof (T);
            if (type == typeof (int))
                return (T)(object)BitConverter.ToInt32(bytes, 0);
            if (type == typeof(uint))
                return (T)(object)BitConverter.ToUInt32(bytes, 0);
            if (type == typeof(string))
                return (T)(object)Encoding.UTF8.GetString(bytes);
            if (type == typeof(byte[]))
                return (T)(object)bytes;
            return default(T);
        }

        public static int ToCharArray2(this int value, char[] buffer, int bufferIndex)
        {
            const int maxLength = 10;

            if (value == 0)
            {
                buffer[bufferIndex] = '0';
                return 1;
            }

            int startIndex = bufferIndex + maxLength - 1;
            int index = startIndex;
            do
            {
                buffer[index] = (char)('0' + value % 10);
                value /= 10;
                --index;
            }
            while (value != 0);

            int length = startIndex - index;

            if (bufferIndex != index + 1)
            {
                while (index != startIndex)
                {
                    ++index;
                    buffer[bufferIndex] = buffer[index];
                    ++bufferIndex;
                }
            }

            return length;
        }

        public static int LoopNumber(this int num,int min ,int max)
        {
            if (num > max)
                num %= max;
            else if (num < min)
                num = min;
            return num;

        }
    }
}