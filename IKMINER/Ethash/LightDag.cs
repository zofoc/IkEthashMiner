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
using System.IO;
using System.Linq;
using System.Collections.Generic;

using IKMINER.Cryptography;

namespace IKMINER.Ethash
{
    public class LightDag
    {
        private const uint ETHASH_EPOCH_LENGTH = 30000u;

        private const uint ETHASH_DATASET_BYTES_INIT = 1073741824u;

        private const uint ETHASH_DATASET_BYTES_GROWTH = 8388608u;

        private const uint ETHASH_CACHE_BYTES_INIT = 1073741824u;

        private const uint ETHASH_CACHE_BYTES_GROWTH = 131072u;

        private const uint ETHASH_HASH_BYTES = 64u;

        private const uint ETHASH_DATASET_PARENTS = 256u;

        private const uint ETHASH_CACHE_ROUNDS = 3u;

        private const uint ETHASH_ACCESSES = 64u;

        private const uint NODE_WORDS = 16u;

        private const uint MIX_WORDS = 32u;

        private const uint MIX_NODES = 2u;

        private const uint FNV_PRIME = 16777619u;

        public byte[] cache;

        public ulong cache_size;

        public ulong block_number;

        private static Dictionary<string, LightDag> staticCache = new Dictionary<string, LightDag>();

        private static Dictionary<string, uint> m_epochs = new Dictionary<string, uint>();

        public LightDag(string seed, byte[] seedbytes, byte[] bytes)
        {
            ulong blockNumber = this.GetBlockNumber(seed);
            this.cache = bytes;
            this.block_number = blockNumber;
            this.cache_size = LightDag.GetCachesize(blockNumber);
        }

        public LightDag(string seed, byte[] seedbytes)
        {
            try
            {
                this.block_number = this.GetBlockNumber(seed);
                this.cache_size = LightDag.GetCachesize(this.block_number);
                this.cache = new byte[this.cache_size];
                this.GenerateLightPackage(seedbytes);
            }
            catch
            {

            }
        }

        public static ulong GetCachesize(ulong block_number)
        {
            return Sizes.cache_sizes[(int)(checked((IntPtr)(block_number / 30000uL)))];
        }

        public static ulong GetDatasize(ulong block_number)
        {
            return Sizes.dag_sizes[(int)(checked((IntPtr)(block_number / 30000uL)))];
        }

        public static LightDag GetLight(string seedHash, byte[] seedbytes)
        {
            string path = "light_" + seedHash + ".dat";
            if (!LightDag.staticCache.ContainsKey(seedHash) && File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                LightDag lightDag = new LightDag(seedHash, seedbytes, bytes);
                LightDag.staticCache.Add(seedHash, lightDag);
                return lightDag;
            }
            if (LightDag.staticCache.ContainsKey(seedHash))
            {
                return LightDag.staticCache[seedHash];
            }
            LightDag lightDag2 = new LightDag(seedHash, seedbytes);
            File.WriteAllBytes(path, lightDag2.cache);
            LightDag.staticCache.Add(seedHash, lightDag2);
            return lightDag2;
        }

        public byte[] GetMixHash(byte[] header_hash, ulong nonce)
        {
            ulong datasize = LightDag.GetDatasize(this.block_number);
            SHA3 sHA = new SHA3(512);
            if (datasize % 32uL != 0uL)
            {
                return null;
            }
            byte[] array = new byte[64];
            Array.Copy(header_hash, array, 32);
            Array.Copy(BitConverter.GetBytes(nonce), 0, array, 32, 8);
            array = sHA.ComputeHash(array, 0, 40);
            byte[] array2 = new byte[128];
            Array.Copy(array, 0, array2, 0, 64);
            Array.Copy(array, 0, array2, 64, 64);
            uint num = 128u;
            uint num2 = (uint)(datasize / (ulong)num);
            for (uint num3 = 0u; num3 != 64u; num3 += 1u)
            {
                uint num4 = BitConverter.ToUInt32(array, 0);
                uint num5 = BitConverter.ToUInt32(array2, (int)(num3 % 32u * 4u));
                uint num6 = ((num4 ^ num3) * 16777619u ^ num5) % num2;
                for (uint num7 = 0u; num7 != 2u; num7 += 1u)
                {
                    byte[] value = this.CalculateDagItem(num6 * 2u + num7);
                    for (uint num8 = 0u; num8 != 16u; num8 += 1u)
                    {
                        int num9 = (int)(num7 * 64u + num8 * 4u);
                        uint num10 = BitConverter.ToUInt32(array2, num9);
                        uint num11 = BitConverter.ToUInt32(value, (int)(num8 * 4u));
                        uint value2 = num10 * 16777619u ^ num11;
                        byte[] bytes = BitConverter.GetBytes(value2);
                        Array.Copy(bytes, 0, array2, num9, 4);
                    }
                }
            }
            int num12 = 0;
            while ((long)num12 != 32L)
            {
                uint num13 = BitConverter.ToUInt32(array2, num12 * 4);
                uint num14 = BitConverter.ToUInt32(array2, (num12 + 1) * 4);
                uint num15 = BitConverter.ToUInt32(array2, (num12 + 2) * 4);
                uint num16 = BitConverter.ToUInt32(array2, (num12 + 3) * 4);
                uint num17 = num13;
                num17 = (num17 * 16777619u ^ num14);
                num17 = (num17 * 16777619u ^ num15);
                num17 = (num17 * 16777619u ^ num16);
                byte[] bytes2 = BitConverter.GetBytes(num17);
                Array.Copy(bytes2, 0, array2, num12 / 4 * 4, 4);
                num12 += 4;
            }
            byte[] array3 = new byte[32];
            Array.Copy(array2, array3, 32);
            return array3;
        }

        private byte[] CalculateDagItem(uint node_index)
        {
            SHA3 sHA = new SHA3(512);
            byte[] array = new byte[64];
            uint num = (uint)(this.cache_size / 64uL);
            byte[] array2 = new byte[64];
            uint num2 = node_index % num;
            Array.Copy(this.cache, (long)((ulong)(num2 * 64u)), array2, 0L, 64L);
            Array.Copy(array2, array, 64);
            uint num3 = BitConverter.ToUInt32(array, 0);
            uint value = num3 ^ node_index;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, array, 4);
            array = sHA.ComputeHash(array);
            for (uint num4 = 0u; num4 != 256u; num4 += 1u)
            {
                uint num5 = BitConverter.ToUInt32(array, (int)(num4 % 16u * 4u));
                uint num6 = ((node_index ^ num4) * 16777619u ^ num5) % num;
                byte[] array3 = new byte[64];
                Array.Copy(this.cache, (long)((ulong)(num6 * 64u)), array3, 0L, 64L);
                int num7 = 0;
                while ((long)num7 != 16L)
                {
                    uint num8 = BitConverter.ToUInt32(array3, num7 * 4);
                    uint num9 = BitConverter.ToUInt32(array, num7 * 4);
                    uint value2 = num9 * 16777619u ^ num8;
                    byte[] bytes2 = BitConverter.GetBytes(value2);
                    Array.Copy(bytes2, 0, array, num7 * 4, 4);
                    num7++;
                }
            }
            return sHA.ComputeHash(array);
        }

        private ulong GetBlockNumber(string seed)
        {
            if (!LightDag.m_epochs.ContainsKey(seed))
            {
                uint num = 0u;
                LightDag.m_epochs.Clear();
                string text = string.Empty;
                byte[] array = new byte[32];
                LightDag.m_epochs.Add("0x" + BlockHeader.ToHex(array), num);
                SHA3 sHA = new SHA3(256);
                while (!text.Equals(seed) && num < 2048u)
                {
                    num += 1u;
                    array = sHA.ComputeHash(array);
                    text = "0x" + BlockHeader.ToHex(array);
                    LightDag.m_epochs.Add(text, num);
                }
            }
            return (ulong)(LightDag.m_epochs[seed] * 30000u);
        }

        private void GenerateLightPackage(byte[] seed)
        {
            int num = (int)(this.cache_size / 64uL);
            byte[] array = seed;
            Array.Resize<byte>(ref array, 32);
            SHA3 sHA = new SHA3(512);
            array = sHA.ComputeHash(array).ToArray<byte>();
            Array.Copy(array, this.cache, array.Length);
            for (int num2 = 1; num2 != num; num2++)
            {
                byte[] array2 = new byte[64];
                Array.Copy(this.cache, (num2 - 1) * 64, array2, 0, 64);
                Array.Copy(sHA.ComputeHash(array2), 0, this.cache, num2 * 64, 64);
            }
            int num3 = 0;
            while ((long)num3 != 3L)
            {
                for (int num4 = 0; num4 != num; num4++)
                {
                    uint num5 = BitConverter.ToUInt32(this.cache, num4 * 64);
                    uint num6 = num5 % (uint)num;
                    byte[] array3 = new byte[64];
                    int num7 = (num - 1 + num4) % num;
                    Array.Copy(this.cache, num7 * 64, array3, 0, 64);
                    byte[] array4 = new byte[64];
                    Array.Copy(this.cache, (long)((ulong)(num6 * 64u)), array4, 0L, 64L);
                    int num8 = 0;
                    while ((long)num8 != 16L)
                    {
                        int num9 = BitConverter.ToInt32(array3, num8 * 4);
                        int num10 = BitConverter.ToInt32(array4, num8 * 4);
                        int value = num9 ^ num10;
                        byte[] bytes = BitConverter.GetBytes(value);
                        Array.Copy(bytes, 0, array3, num8 * 4, 4);
                        num8++;
                    }
                    array3 = sHA.ComputeHash(array3).ToArray<byte>();
                    Array.Copy(array3, 0, this.cache, num4 * 64, 64);
                }
                num3++;
            }
        }
    }
}
