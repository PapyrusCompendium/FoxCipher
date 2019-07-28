using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoxCipher
{
    public class ChunkManager
    {
        private byte[][] Chunks { get; set; }
        private byte[] Key { get; }
        private byte[] LinkedKey { get; }

        public ChunkManager(byte[] key)
        {
            this.Key = key;
            this.LinkedKey = XorLinkBytes(Key);
        }

        public byte[] Encrypt(byte[] data)
        {
            Chunks = new byte[data.Length][];
            for (int x = 0; x < data.Length; x++)
                Chunks[x] = ProtectChunk(GenerateChunk(data[x]));

            ShiftChunksRight(IntegralSum(LinkedKey));

            BitMatrixManager bitMatrixManager = new BitMatrixManager(Compress());
            for (int i = 0; i < 3; i++)
            {
                for (int y = 0; y < LinkedKey.Length; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        bitMatrixManager.ShiftUp(LinkedKey[y], x);
                        bitMatrixManager.ShiftRight(1, 0);
                    }

                for (int y = 0; y < Key.Length; y++)
                    for (int x = 0; x < Chunks.Length; x++)
                    {
                        bitMatrixManager.ShiftRight(Key[y], x);
                        bitMatrixManager.ShiftUp(2, 0);
                    }
            }

            return bitMatrixManager.Compress();
        }

        public byte[] Decrypt(byte[] data)
        {
            Decompress(data);
            BitMatrixManager bitMatrixManager = new BitMatrixManager(data);


            for (int i = 0; i < 3; i++)
            {
                for (int y = Key.Length - 1; y >= 0; y--)
                    for (int x = Chunks.Length - 1; x >= 0; x--)
                    {
                        bitMatrixManager.ShiftDown(2, 0);
                        bitMatrixManager.ShiftLeft(Key[y], x);
                    }

                for (int y = LinkedKey.Length - 1; y >= 0; y--)
                    for (int x = 31; x >= 0; x--)
                    {
                        bitMatrixManager.ShiftLeft(1, 0);
                        bitMatrixManager.ShiftDown(LinkedKey[y], x);
                    }
            }

            Decompress(bitMatrixManager.Compress());
            bitMatrixManager = null;

            ShiftChunksLeft(IntegralSum(LinkedKey));

           return UnprotectChunks();
        }

        private byte[] Compress()
        {
            byte[] compressed = new byte[Chunks.Length * 4];
            for (int x = 0; x < Chunks.Length; x++)
                Chunks[x].CopyTo(compressed, x * 4);

            return compressed;
        }

        private void Decompress(byte[] compressed)
        {
            Chunks = new byte[compressed.Length / 4][];
            for (int x = 0; x < Chunks.Length; x++)
                Chunks[x] = compressed.Skip(x * 4).Take(4).ToArray();
        }

        private byte[] UnprotectChunks()
        {
            byte[] data = new byte[Chunks.Length];

            for (int x = 0; x < Chunks.Length; x++)
                data[x] = UnprotectChunk(Chunks[x]);
            return data;
        }

        private void ShiftChunksRight(int amount) => Chunks = Chunks.Skip(amount).Concat(Chunks.Take(amount)).ToArray();

        private void ShiftChunksLeft(int amount) => Chunks = Chunks.Skip(Chunks.Length - amount).Concat(Chunks.Take(Chunks.Length - amount)).ToArray();

        private byte[] ProtectChunk(byte[] chunk) => XorCryptData(XorCryptData(XorLinkBytes(chunk), Key), LinkedKey);

        private byte UnprotectChunk(byte[] chunk) => XorDeCryptData(XorDeCryptData(XorLinkBytes(chunk, true), Key), LinkedKey)[0];

        private int IntegralSum(byte[] data) => data.Sum(i => i);

        private byte[] GenerateChunk(byte data)
        {
            byte[] chunk = FauxBytes(4);
            chunk[0] = data;
            return chunk;
        }

        private byte[] FauxBytes(int count)
        {
            byte[] fauxBytes = new byte[count];
            RNGCryptoServiceProvider secureRandom = new RNGCryptoServiceProvider();
            secureRandom.GetNonZeroBytes(fauxBytes);
            return fauxBytes;
        }

        private byte[] XorLinkBytes(byte[] data, bool unlink = false)
        {
            if (!unlink)
                for (int x = 0; x < data.Length; x++)
                    data[x] ^= data[x + 1 == data.Length ? 0 : x + 1];
            else
                for (int x = data.Length - 1; x >= 0; x--)
                    data[x] ^= data[x + 1 == data.Length ? 0 : x + 1];

            return data;
        }

        private byte[] XorCryptData(byte[] data, byte[] key)
        {
            for (int x = 0; x < key.Length; x++)
                for (int y = 0; y < data.Length; y++)
                    data[y] ^= key[x];

            return data;
        }

        private byte[] XorDeCryptData(byte[] data, byte[] key)
        {
            for (int x = key.Length - 1; x >= 0; x--)
                for (int y = 0; y < data.Length; y++)
                    data[y] ^= key[x];

            return data;
        }
    }
}
