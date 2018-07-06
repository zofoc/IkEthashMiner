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
using IKMINER.Cryptography;

namespace IKMINER.Ethash
{
    public class BlockHeader
    {
        public int Difficulty;
        public int ExSizeBits = -1;
        public ulong StartNonce;
        public byte[] HeaderHash;
        public byte[] BoundaryBytes;
        public byte[] Seedbytes;
        public string HeaderString = string.Empty;
        public string BoundaryString = string.Empty;
        public string SeedString = string.Empty;

        public BlockHeader() { }

        public BlockHeader(string header, string seed, string boundary)
        {
            this.HeaderString = header;
            this.BoundaryString = boundary;
            this.SeedString = seed;
            this.HeaderHash = BlockHeader.StringToByteArray(header.Replace("0x", string.Empty));
            this.Seedbytes = BlockHeader.StringToByteArray(seed.Replace("0x", string.Empty));
            this.BoundaryBytes = BlockHeader.StringToByteArray(boundary.Replace("0x", string.Empty));
        }

        public BlockHeader(BlockHeader other)
        {
            this.HeaderHash = other.HeaderHash;
            this.Seedbytes = other.Seedbytes;
            this.BoundaryBytes = other.BoundaryBytes;
        }

        public static byte[] StringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] array = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return array;
        }

        public static string ToHex(byte[] _data)
        {
            string text = string.Empty;
            for (int i = 0; i < _data.Length; i++)
            {
                byte b = _data[i];
                text += b.ToString("x2");
            }
            return text;
        }

        private void setHeader()
        {
            byte[] array = new byte[32];
            byte[] array2 = new byte[32];
            byte[] array3 = new byte[20];
            byte[] array4 = new byte[32];
            byte[] array5 = new byte[32];
            byte[] array6 = new byte[32];
            byte[] array7 = new byte[256];
            byte[] bytes = BitConverter.GetBytes(this.Difficulty);
            byte[] array8 = RLP.ToBytesFromNumber(bytes);
            byte[] array9 = new byte[0];
            byte[] array10 = new byte[0];
            byte[] array11 = new byte[0];
            byte[] array12 = new byte[32];
            byte[] array13 = new byte[0];
            for (int i = 0; i < 32; i++)
            {
                array12[i] = 255;
            }
            byte[][] array14 = new byte[13][];
            byte[][] array15 = new byte[13][];
            array14[0] = array;
            array15[0] = RLP.EncodeElement(array);
            array14[1] = array2;
            array15[1] = RLP.EncodeElement(array2);
            array14[2] = array3;
            array15[2] = RLP.EncodeElement(array3);
            array14[3] = array4;
            array15[3] = RLP.EncodeElement(array4);
            array14[4] = array5;
            array15[4] = RLP.EncodeElement(array5);
            array14[5] = array6;
            array15[5] = RLP.EncodeElement(array6);
            array14[6] = array7;
            array15[6] = RLP.EncodeElement(array7);
            array14[7] = array8;
            array15[7] = RLP.EncodeElement(array8);
            array14[8] = array9;
            array15[8] = RLP.EncodeElement(array9);
            array14[9] = array10;
            array15[9] = RLP.EncodeElement(array10);
            array14[10] = array11;
            array15[10] = RLP.EncodeElement(array11);
            array14[11] = array12;
            array15[11] = RLP.EncodeElement(array12);
            array14[12] = array13;
            array15[12] = RLP.EncodeElement(array13);
            int num = 0;
            int num2 = 0;
            for (int j = 0; j < 13; j++)
            {
                num2 += array15[j].Length;
            }
            byte[] array16 = new byte[num2];
            for (int k = 0; k < 13; k++)
            {
                Array.Copy(array15[k], 0, array16, num, array15[k].Length);
                num += array15[k].Length;
            }
            byte[] array17 = RLP.EncodeList(array16);
            this.HeaderHash = new SHA3(256).ComputeHash(array17);
        }
    }
}
