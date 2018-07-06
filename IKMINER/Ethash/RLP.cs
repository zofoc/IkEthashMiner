/*  Iker Ruiz Arnauda 2015
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<https://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using System.Collections.Generic;

namespace IKMINER.Ethash
{
    public class RLP
    {
        private const int SIZE_THRESHOLD = 56;
        private const byte OFFSET_SHORT_ITEM = 128;
        private const byte OFFSET_LONG_ITEM = 183;
        private const byte OFFSET_SHORT_LIST = 192;
        private const byte OFFSET_LONG_LIST = 247;
        public static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];
        public static readonly byte[] ZERO_BYTE_ARRAY = new byte[1];

        public static int ByteArrayToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        private static bool IsListBiggerThan55Bytes(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] > 247;
        }

        private static bool IsListLessThan55Bytes(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] >= 192 && msgData[currentPosition] <= 247;
        }

        private static bool IsItemBiggerThan55Bytes(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] > 183 && msgData[currentPosition] < 192;
        }

        private static bool IsItemLessThan55Bytes(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] > 128 && msgData[currentPosition] <= 183;
        }

        private static bool IsNullItem(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] == 128;
        }

        private static bool IsSingleByteItem(byte[] msgData, int currentPosition)
        {
            return msgData[currentPosition] < 128;
        }

        public static byte[] ToBytesFromNumber(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse<byte>().ToArray<byte>();
            }
            List<byte> list = new List<byte>();
            bool flag = true;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (!flag || bytes[i] != 0)
                {
                    flag = false;
                    list.Add(bytes[i]);
                }
            }
            return list.ToArray();
        }

        public static byte[] EncodeByte(byte singleByte)
        {
            if (singleByte == 0)
            {
                return new byte[]
                {
                    128
                };
            }
            if (singleByte <= 127)
            {
                return new byte[]
                {
                    singleByte
                };
            }
            return new byte[]
            {
                129,
                singleByte
            };
        }

        public static byte[] EncodeList(byte[] srcData)
        {
            if (RLP.IsNullOrZeroArray(srcData))
            {
                return new byte[]
                {
                    192
                };
            }
            if (RLP.IsSingleZero(srcData))
            {
                return srcData;
            }
            if (srcData.Length == 1 && srcData[0] < 128)
            {
                return srcData;
            }
            if (srcData.Length < 56)
            {
                byte b = (byte)(192 + srcData.Length);
                byte[] array = new byte[srcData.Length + 1];
                Array.Copy(srcData, 0, array, 1, srcData.Length);
                array[0] = b;
                return array;
            }
            int num = srcData.Length;
            byte b2 = 0;
            while (num != 0)
            {
                b2 += 1;
                num >>= 8;
            }
            byte[] array2 = new byte[(int)b2];
            for (int i = 0; i < (int)b2; i++)
            {
                array2[(int)(b2 - 1) - i] = (byte)(srcData.Length >> 8 * i);
            }
            byte[] array3 = new byte[srcData.Length + 1 + (int)b2];
            Array.Copy(srcData, 0, array3, (int)(1 + b2), srcData.Length);
            array3[0] = (byte)(247 + b2);
            Array.Copy(array2, 0, array3, 1, array2.Length);
            return array3;
        }

        public static byte[] EncodeElement(byte[] srcData)
        {
            if (RLP.IsNullOrZeroArray(srcData))
            {
                return new byte[]
                {
                    128
                };
            }
            if (RLP.IsSingleZero(srcData))
            {
                return srcData;
            }
            if (srcData.Length == 1 && srcData[0] < 128)
            {
                return srcData;
            }
            if (srcData.Length < 56)
            {
                byte b = (byte)(128 + srcData.Length);
                byte[] array = new byte[srcData.Length + 1];
                Array.Copy(srcData, 0, array, 1, srcData.Length);
                array[0] = b;
                return array;
            }
            int num = srcData.Length;
            byte b2 = 0;
            while (num != 0)
            {
                b2 += 1;
                num >>= 8;
            }
            byte[] array2 = new byte[(int)b2];
            for (int i = 0; i < (int)b2; i++)
            {
                array2[(int)(b2 - 1) - i] = (byte)(srcData.Length >> 8 * i);
            }
            byte[] array3 = new byte[srcData.Length + 1 + (int)b2];
            Array.Copy(srcData, 0, array3, (int)(1 + b2), srcData.Length);
            array3[0] = (byte)(183 + b2);
            Array.Copy(array2, 0, array3, 1, array2.Length);
            return array3;
        }

        public static byte[] EncodeList(params byte[][] items)
        {
            if (items == null)
            {
                return new byte[]
                {
                    192
                };
            }
            int num = 0;
            for (int i = 0; i < items.Length; i++)
            {
                num += items[i].Length;
            }
            byte[] array;
            int num3;
            if (num < 56)
            {
                int num2 = 1 + num;
                array = new byte[num2];
                array[0] = (byte)(192 + num);
                num3 = 1;
            }
            else
            {
                int num4 = num;
                byte b = 0;
                while (num4 != 0)
                {
                    b += 1;
                    num4 >>= 8;
                }
                num4 = num;
                byte[] array2 = new byte[(int)b];
                for (int j = 0; j < (int)b; j++)
                {
                    array2[(int)(b - 1) - j] = (byte)(num4 >> 8 * j);
                }
                array = new byte[1 + array2.Length + num];
                array[0] = (byte)(247 + b);
                Array.Copy(array2, 0, array, 1, array2.Length);
                num3 = array2.Length + 1;
            }
            for (int k = 0; k < items.Length; k++)
            {
                byte[] array3 = items[k];
                Array.Copy(array3, 0, array, num3, array3.Length);
                num3 += array3.Length;
            }
            return array;
        }

        public static bool IsNullOrZeroArray(byte[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsSingleZero(byte[] array)
        {
            return array.Length == 1 && array[0] == 0;
        }

        private static int CalculateLength(int lengthOfLength, byte[] msgData, int pos)
        {
            byte b = (byte)(lengthOfLength - 1);
            int num = 0;
            for (int i = 1; i <= lengthOfLength; i++)
            {
                num += (int)msgData[pos + i] << (int)(8 * b);
                b -= 1;
            }
            return num;
        }
    }
}
