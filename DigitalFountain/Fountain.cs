using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DigitalFountain
{
    public class Fountain
    {
        Random r;

        public readonly int BlockSize;

        readonly byte[] data;

        public readonly int BlockCount;

        #region constructor
        public Fountain(int seed, byte[] data, int blockSize)
        {
            r = new Random(seed);

            if (data.Length % blockSize != 0)
                throw new ArgumentException("Data length must be exactly divisible by block size");

            this.BlockSize = blockSize;
            this.data = data;

            BlockCount = data.Length / blockSize;
        }
        #endregion

        public Packet CreatePacket()
        {
            int seed = r.Next();

            return new Packet(seed, CreateData(seed));
        }

        private HashSet<int> randomBlocks = new HashSet<int>();
        private byte[] CreateData(int seed)
        {
            Helpers.CalculateBlockSet(seed, BlockCount, randomBlocks);

            byte[] block = new byte[BlockSize];
            foreach (var blockNumber in randomBlocks)
            {
                int baseBlockIndex = blockNumber * BlockSize;
                for (int b = 0; b < block.Length; b++)
                    block[b] = (byte)(block[b] ^ data[baseBlockIndex + b]);
            }

            return block;
        }
    }
}
