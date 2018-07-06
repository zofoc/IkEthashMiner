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

namespace IKMINER.Cryptography
{
    public class SHA3
    {
        public readonly RoundConstants RoundConstants = new RoundConstants();

        protected ulong[] state;

        protected byte[] buffer;

        protected int buffLength;

        protected byte[] HashValue;

        protected int HashSizeValue;

        protected int keccakR;

        public int KeccakR
        {
            get
            {
                return this.keccakR;
            }
            protected set
            {
                this.keccakR = value;
            }
        }

        public int SizeInBytes
        {
            get
            {
                return this.KeccakR / 8;
            }
        }

        public int HashByteLength
        {
            get
            {
                return this.HashSizeValue / 8;
            }
        }

        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public byte[] Hash
        {
            get
            {
                return this.HashValue;
            }
        }

        public int HashSize
        {
            get
            {
                return this.HashSizeValue;
            }
        }

        public SHA3(int hashBitLength)
        {
            if (hashBitLength != 224 && hashBitLength != 256 && hashBitLength != 384 && hashBitLength != 512)
            {
                throw new ArgumentException("hashBitLength must be 224, 256, 384, or 512", "hashBitLength");
            }
            this.Initialize();
            this.HashSizeValue = hashBitLength;
            if (hashBitLength != 224)
            {
                if (hashBitLength != 256)
                {
                    if (hashBitLength != 384)
                    {
                        if (hashBitLength == 512)
                        {
                            this.KeccakR = 576;
                        }
                    }
                    else
                    {
                        this.KeccakR = 832;
                    }
                }
                else
                {
                    this.KeccakR = 1088;
                }
            }
            else
            {
                this.KeccakR = 1152;
            } 
        }

        public byte[] ComputeHash(byte[] array)
        {
            this.HashCore(array, 0, array.Length);
            byte[] result = this.HashFinal();
            this.Initialize();
            return result;
        }

        public byte[] ComputeHash(byte[] array, int offset, int length)
        {
            this.HashCore(array, offset, length);
            byte[] result = this.HashFinal();
            this.Initialize();
            return result;
        }

        protected ulong ROL(ulong a, int offset)
        {
            return a << offset % 64 ^ a >> 64 - offset % 64;
        }

        protected void AddToBuffer(byte[] array, ref int offset, ref int count)
        {
            int num = Math.Min(count, this.buffer.Length - this.buffLength);
            Buffer.BlockCopy(array, offset, this.buffer, this.buffLength, num);
            offset += num;
            this.buffLength += num;
            count -= num;
        }

        public void Initialize()
        {
            this.buffLength = 0;
            this.state = new ulong[25];
            this.HashValue = null;
        }

        protected void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (cbSize == 0)
            {
                return;
            }
            int sizeInBytes = this.SizeInBytes;
            if (this.buffer == null)
            {
                this.buffer = new byte[sizeInBytes];
            }
            int num = sizeInBytes >> 3;
            ulong[] array2 = new ulong[num];
            if (this.buffLength == sizeInBytes)
            {
                throw new Exception("Unexpected error, the internal buffer is full");
            }
            this.AddToBuffer(array, ref ibStart, ref cbSize);
            if (this.buffLength == sizeInBytes)
            {
                Buffer.BlockCopy(this.buffer, 0, array2, 0, sizeInBytes);
                this.KeccakF(array2, num);
                this.buffLength = 0;
            }
            while (cbSize >= sizeInBytes)
            {
                Buffer.BlockCopy(array, ibStart, array2, 0, sizeInBytes);
                this.KeccakF(array2, num);
                cbSize -= sizeInBytes;
                ibStart += sizeInBytes;
            }
            if (cbSize > 0)
            {
                Buffer.BlockCopy(array, ibStart, this.buffer, this.buffLength, cbSize);
                this.buffLength += cbSize;
            }
        }

        protected byte[] HashFinal()
        {
            int sizeInBytes = this.SizeInBytes;
            byte[] array = new byte[this.HashByteLength];
            if (this.buffer == null)
            {
                this.buffer = new byte[sizeInBytes];
            }
            else
            {
                Array.Clear(this.buffer, this.buffLength, sizeInBytes - this.buffLength);
            }
            this.buffer[this.buffLength++] = 1;
            byte[] expr_6F_cp_0 = this.buffer;
            int expr_6F_cp_1 = sizeInBytes - 1;
            expr_6F_cp_0[expr_6F_cp_1] |= 128;
            int num = sizeInBytes >> 3;
            ulong[] array2 = new ulong[num];
            Buffer.BlockCopy(this.buffer, 0, array2, 0, sizeInBytes);
            this.KeccakF(array2, num);
            Buffer.BlockCopy(this.state, 0, array, 0, this.HashByteLength);
            return array;
        }

        private void KeccakF(ulong[] inb, int laneCount)
        {
            while (--laneCount >= 0)
            {
                this.state[laneCount] ^= inb[laneCount];
            }
            ulong num = this.state[0];
            ulong num2 = this.state[1];
            ulong num3 = this.state[2];
            ulong num4 = this.state[3];
            ulong num5 = this.state[4];
            ulong num6 = this.state[5];
            ulong num7 = this.state[6];
            ulong num8 = this.state[7];
            ulong num9 = this.state[8];
            ulong num10 = this.state[9];
            ulong num11 = this.state[10];
            ulong num12 = this.state[11];
            ulong num13 = this.state[12];
            ulong num14 = this.state[13];
            ulong num15 = this.state[14];
            ulong num16 = this.state[15];
            ulong num17 = this.state[16];
            ulong num18 = this.state[17];
            ulong num19 = this.state[18];
            ulong num20 = this.state[19];
            ulong num21 = this.state[20];
            ulong num22 = this.state[21];
            ulong num23 = this.state[22];
            ulong num24 = this.state[23];
            ulong num25 = this.state[24];
            for (int i = 0; i < 24; i += 2)
            {
                ulong num26 = num ^ num6 ^ num11 ^ num16 ^ num21;
                ulong num27 = num2 ^ num7 ^ num12 ^ num17 ^ num22;
                ulong num28 = num3 ^ num8 ^ num13 ^ num18 ^ num23;
                ulong num29 = num4 ^ num9 ^ num14 ^ num19 ^ num24;
                ulong num30 = num5 ^ num10 ^ num15 ^ num20 ^ num25;
                ulong num31 = num30 ^ this.ROL(num27, 1);
                ulong num32 = num26 ^ this.ROL(num28, 1);
                ulong num33 = num27 ^ this.ROL(num29, 1);
                ulong num34 = num28 ^ this.ROL(num30, 1);
                ulong num35 = num29 ^ this.ROL(num26, 1);
                num ^= num31;
                num26 = num;
                num7 ^= num32;
                num27 = this.ROL(num7, 44);
                num13 ^= num33;
                num28 = this.ROL(num13, 43);
                num19 ^= num34;
                num29 = this.ROL(num19, 21);
                num25 ^= num35;
                num30 = this.ROL(num25, 14);
                ulong num36 = num26 ^ (~num27 & num28);
                num36 ^= this.RoundConstants[i];
                ulong num37 = num27 ^ (~num28 & num29);
                ulong num38 = num28 ^ (~num29 & num30);
                ulong num39 = num29 ^ (~num30 & num26);
                ulong num40 = num30 ^ (~num26 & num27);
                num4 ^= num34;
                num26 = this.ROL(num4, 28);
                num10 ^= num35;
                num27 = this.ROL(num10, 20);
                num11 ^= num31;
                num28 = this.ROL(num11, 3);
                num17 ^= num32;
                num29 = this.ROL(num17, 45);
                num23 ^= num33;
                num30 = this.ROL(num23, 61);
                ulong num41 = num26 ^ (~num27 & num28);
                ulong num42 = num27 ^ (~num28 & num29);
                ulong num43 = num28 ^ (~num29 & num30);
                ulong num44 = num29 ^ (~num30 & num26);
                ulong num45 = num30 ^ (~num26 & num27);
                num2 ^= num32;
                num26 = this.ROL(num2, 1);
                num8 ^= num33;
                num27 = this.ROL(num8, 6);
                num14 ^= num34;
                num28 = this.ROL(num14, 25);
                num20 ^= num35;
                num29 = this.ROL(num20, 8);
                num21 ^= num31;
                num30 = this.ROL(num21, 18);
                ulong num46 = num26 ^ (~num27 & num28);
                ulong num47 = num27 ^ (~num28 & num29);
                ulong num48 = num28 ^ (~num29 & num30);
                ulong num49 = num29 ^ (~num30 & num26);
                ulong num50 = num30 ^ (~num26 & num27);
                num5 ^= num35;
                num26 = this.ROL(num5, 27);
                num6 ^= num31;
                num27 = this.ROL(num6, 36);
                num12 ^= num32;
                num28 = this.ROL(num12, 10);
                num18 ^= num33;
                num29 = this.ROL(num18, 15);
                num24 ^= num34;
                num30 = this.ROL(num24, 56);
                ulong num51 = num26 ^ (~num27 & num28);
                ulong num52 = num27 ^ (~num28 & num29);
                ulong num53 = num28 ^ (~num29 & num30);
                ulong num54 = num29 ^ (~num30 & num26);
                ulong num55 = num30 ^ (~num26 & num27);
                num3 ^= num33;
                num26 = this.ROL(num3, 62);
                num9 ^= num34;
                num27 = this.ROL(num9, 55);
                num15 ^= num35;
                num28 = this.ROL(num15, 39);
                num16 ^= num31;
                num29 = this.ROL(num16, 41);
                num22 ^= num32;
                num30 = this.ROL(num22, 2);
                ulong num56 = num26 ^ (~num27 & num28);
                ulong num57 = num27 ^ (~num28 & num29);
                ulong num58 = num28 ^ (~num29 & num30);
                ulong num59 = num29 ^ (~num30 & num26);
                ulong num60 = num30 ^ (~num26 & num27);
                num26 = (num36 ^ num41 ^ num46 ^ num51 ^ num56);
                num27 = (num37 ^ num42 ^ num47 ^ num52 ^ num57);
                num28 = (num38 ^ num43 ^ num48 ^ num53 ^ num58);
                num29 = (num39 ^ num44 ^ num49 ^ num54 ^ num59);
                num30 = (num40 ^ num45 ^ num50 ^ num55 ^ num60);
                num31 = (num30 ^ this.ROL(num27, 1));
                num32 = (num26 ^ this.ROL(num28, 1));
                num33 = (num27 ^ this.ROL(num29, 1));
                num34 = (num28 ^ this.ROL(num30, 1));
                num35 = (num29 ^ this.ROL(num26, 1));
                num36 ^= num31;
                num26 = num36;
                num42 ^= num32;
                num27 = this.ROL(num42, 44);
                num48 ^= num33;
                num28 = this.ROL(num48, 43);
                num54 ^= num34;
                num29 = this.ROL(num54, 21);
                num60 ^= num35;
                num30 = this.ROL(num60, 14);
                num = (num26 ^ (~num27 & num28));
                num ^= this.RoundConstants[i + 1];
                num2 = (num27 ^ (~num28 & num29));
                num3 = (num28 ^ (~num29 & num30));
                num4 = (num29 ^ (~num30 & num26));
                num5 = (num30 ^ (~num26 & num27));
                num39 ^= num34;
                num26 = this.ROL(num39, 28);
                num45 ^= num35;
                num27 = this.ROL(num45, 20);
                num46 ^= num31;
                num28 = this.ROL(num46, 3);
                num52 ^= num32;
                num29 = this.ROL(num52, 45);
                num58 ^= num33;
                num30 = this.ROL(num58, 61);
                num6 = (num26 ^ (~num27 & num28));
                num7 = (num27 ^ (~num28 & num29));
                num8 = (num28 ^ (~num29 & num30));
                num9 = (num29 ^ (~num30 & num26));
                num10 = (num30 ^ (~num26 & num27));
                num37 ^= num32;
                num26 = this.ROL(num37, 1);
                num43 ^= num33;
                num27 = this.ROL(num43, 6);
                num49 ^= num34;
                num28 = this.ROL(num49, 25);
                num55 ^= num35;
                num29 = this.ROL(num55, 8);
                num56 ^= num31;
                num30 = this.ROL(num56, 18);
                num11 = (num26 ^ (~num27 & num28));
                num12 = (num27 ^ (~num28 & num29));
                num13 = (num28 ^ (~num29 & num30));
                num14 = (num29 ^ (~num30 & num26));
                num15 = (num30 ^ (~num26 & num27));
                num40 ^= num35;
                num26 = this.ROL(num40, 27);
                num41 ^= num31;
                num27 = this.ROL(num41, 36);
                num47 ^= num32;
                num28 = this.ROL(num47, 10);
                num53 ^= num33;
                num29 = this.ROL(num53, 15);
                num59 ^= num34;
                num30 = this.ROL(num59, 56);
                num16 = (num26 ^ (~num27 & num28));
                num17 = (num27 ^ (~num28 & num29));
                num18 = (num28 ^ (~num29 & num30));
                num19 = (num29 ^ (~num30 & num26));
                num20 = (num30 ^ (~num26 & num27));
                num38 ^= num33;
                num26 = this.ROL(num38, 62);
                num44 ^= num34;
                num27 = this.ROL(num44, 55);
                num50 ^= num35;
                num28 = this.ROL(num50, 39);
                num51 ^= num31;
                num29 = this.ROL(num51, 41);
                num57 ^= num32;
                num30 = this.ROL(num57, 2);
                num21 = (num26 ^ (~num27 & num28));
                num22 = (num27 ^ (~num28 & num29));
                num23 = (num28 ^ (~num29 & num30));
                num24 = (num29 ^ (~num30 & num26));
                num25 = (num30 ^ (~num26 & num27));
            }
            this.state[0] = num;
            this.state[1] = num2;
            this.state[2] = num3;
            this.state[3] = num4;
            this.state[4] = num5;
            this.state[5] = num6;
            this.state[6] = num7;
            this.state[7] = num8;
            this.state[8] = num9;
            this.state[9] = num10;
            this.state[10] = num11;
            this.state[11] = num12;
            this.state[12] = num13;
            this.state[13] = num14;
            this.state[14] = num15;
            this.state[15] = num16;
            this.state[16] = num17;
            this.state[17] = num18;
            this.state[18] = num19;
            this.state[19] = num20;
            this.state[20] = num21;
            this.state[21] = num22;
            this.state[22] = num23;
            this.state[23] = num24;
            this.state[24] = num25;
        }
    }
}
