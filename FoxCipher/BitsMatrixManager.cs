using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxCipher
{
    public class BitMatrixManager
    {
        /*
         *  1 2 3 4 5 6 7 8 9 (32 bits - 3 Faux bytes + 1 byte Data)
         *  
            0 1 1 0 1 0 1 0 1 0 1 0 1 0 0 1 0 0 1 0 0 1 1 0 1 0 1 0 1 0 0 0   1  (Data ∞ long)
            0 1 0 1 0 1 0 0 1 0 0 1 1 0 1 0 1 0 1 0 0 1 0 1 0 0 1 0 0 1 0 1   2
            1 0 1 0 1 0 0 0 1 0 1 1 0 1 0 1 1 0 0 0 1 0 1 0 0 0 1 1 1 0 1 1   3
            0 1 0 0 1 0 0 1 0 0 1 1 0 0 0 1 1 1 1 0 1 0 1 0 1 0 1 1 1 0 0 0   4
            0 0 0 0 0 0 1 1 0 1 0 1 0 1 1 1 0 1 0 1 0 0 1 0 0 1 1 0 0 1 0 0   5
            0 1 0 1 1 0 1 0 1 0 1 1 0 0 1 0 1 1 0 0 1 0 1 1 1 1 0 0 1 0 0 0   6
            0 0 0 1 1 1 0 1 0 1 1 1 0 1 0 1 0 1 1 0 1 0 1 0 0 1 0 1 1 1 1 1   7
            0 1 1 0 1 0 1 0 1 0 1 0 1 0 0 1 0 0 1 0 0 1 1 0 1 0 1 0 1 0 0 1   8
            0 1 0 1 0 1 0 0 1 0 0 1 1 0 1 0 1 0 1 0 0 1 0 1 0 0 1 0 0 1 0 0   9
    */
        private bool[][] BitMatrix { get; set; }

        public BitMatrixManager(byte[] compressedChunks)
        {
            BitArray array = new BitArray(compressedChunks);
            bool[] bitArray = new bool[compressedChunks.Length * 8];
            array.CopyTo(bitArray, 0);

            BitMatrix = new bool[(bitArray.Length / 32)][];

            for (int x = 0; x < BitMatrix.Length; x++)
                BitMatrix[x] = bitArray.Skip(x * 32).Take(32).ToArray();
        }

        public byte[] Compress()
        {
            bool[] compressed = new bool[BitMatrix.Length * 32];
            for (int x = 0; x < BitMatrix.Length; x++)
                BitMatrix[x].CopyTo(compressed, x * 32);

            BitMatrix = new bool[0][];
            return BitArrayToByteArray(new BitArray(compressed));
        }

        private byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public void ShiftUp(int amount, int index)
        {
            bool[] columnArray = BitMatrix.Select(i => i[index]).ToArray();
            columnArray = columnArray.Skip(amount).Concat(columnArray.Take(amount)).ToArray();

            for (int x = 0; x < columnArray.Length; x++)
                BitMatrix[x][index] = columnArray[x];
        }

        public void ShiftDown(int amount, int index)
        {
            bool[] columnArray = BitMatrix.Select(i => i[index]).ToArray();
            columnArray = columnArray.Skip(columnArray.Length - amount).Concat(columnArray.Take(columnArray.Length - amount)).ToArray();

            for (int x = 0; x < columnArray.Length; x++)
                BitMatrix[x][index] = columnArray[x];
        }

        public void ShiftRight(int amount, int index) => BitMatrix[index] = BitMatrix[index].Skip(amount).Concat(BitMatrix[index].Take(amount)).ToArray();

        public void ShiftLeft(int amount, int index) => BitMatrix[index] = BitMatrix[index].Skip(BitMatrix[index].Length - amount).Concat(BitMatrix[index].Take(BitMatrix[index].Length - amount)).ToArray();
    }
}
