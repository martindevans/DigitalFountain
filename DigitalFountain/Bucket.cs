using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalFountain
{
    public class Bucket
    {
        #region fields and properties
        public readonly int BlockSize;
        public readonly int BlockCount;

        HashSet<ReceivedPacket>[] blockReferences;

        Dictionary<int, HashSet<ReceivedPacket>> packets = new Dictionary<int, HashSet<ReceivedPacket>>(10);

        public bool IsComplete
        {
            get
            {
                HashSet<ReceivedPacket> singlePackets;
                return packets.TryGetValue(1, out singlePackets) && singlePackets.Count == BlockCount;
            }
        }
        #endregion

        #region constructor
        public Bucket(int blockSize, int blockCount)
        {
            this.BlockSize = blockSize;
            this.BlockCount = blockCount;

            blockReferences = new HashSet<ReceivedPacket>[blockCount];
        }
        #endregion

        #region add packets
        Queue<ReceivedPacket> newLeaves = new Queue<ReceivedPacket>();
        /// <summary>
        /// Adds the packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>true; if the message is completed, otherwise false</returns>
        public bool AddPacket(Packet packet)
        {
            ReceivedPacket p = new ReceivedPacket(packet, BlockCount);

            foreach (var smaller in SelectSubsets(p.Blocks.Count, newLeaves, p.Blocks))
                if (smaller.Blocks.IsProperSubsetOf(p.Blocks))
                    p.Xor(smaller);

            if (p.Blocks.Count == 1)
            {
                newLeaves.Clear();   
                newLeaves.Enqueue(p);

                while (newLeaves.Count > 0)
                {
                    var next = newLeaves.Dequeue();
                    if (next.Blocks.Count == 1)
                        SubtractNewLeaf(next, newLeaves);
                }
            }
            
            AddToList(p);

            return IsComplete;
        }

        private void SubtractNewLeaf(ReceivedPacket leaf, Queue<ReceivedPacket> output)
        {
            var supersets = new HashSet<ReceivedPacket>(packets.SelectMany(a => a.Value).Where(a => a.Blocks.IsProperSupersetOf(leaf.Blocks)));

            foreach (var superset in supersets)
            {
                RemoveFromList(superset);
                superset.Xor(leaf);
                AddToList(superset);

                if (superset.Blocks.Count == 1)
                    output.Enqueue(superset);
            }
        }

        private bool RemoveFromList(ReceivedPacket packet)
        {
            HashSet<ReceivedPacket> l;

            if (packets.TryGetValue(packet.Blocks.Count, out l))
                return l.Remove(packet);

            return false;
        }

        private void AddToList(ReceivedPacket packet)
        {
            if (packet.Blocks.Count == 0)
                return;

            HashSet<ReceivedPacket> l;

            if (!packets.TryGetValue(packet.Blocks.Count, out l))
                packets.Add(packet.Blocks.Count, l = new HashSet<ReceivedPacket>());

            l.Add(packet);
        }

        private IEnumerable<ReceivedPacket> SelectSubsets(int count, Queue<ReceivedPacket> output, HashSet<int> superset)
        {
            output.Clear();

            foreach (var item in packets.Where(a => a.Key < count).SelectMany(a => a.Value.Where(b => b.Blocks.IsSubsetOf(superset))))
                output.Enqueue(item);

            return output;
        }

        private HashSet<ReceivedPacket> SelectLargestList(int count)
        {
            HashSet<ReceivedPacket> r;
            for (int i = count; i >= 0; i--)
            {
                if (packets.TryGetValue(i, out r))
                    return r;
            }

            return null;
        }
        #endregion

        #region getdata
        /// <summary>
        /// Gets all the data downloaded so far. Blocks which are not downloaded will be zero. You can check which blocks are present by calling Progress(), which returns a bool for each block
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            byte[] data = new byte[BlockSize * BlockCount];

            foreach (var packet in packets[1])
                Array.Copy(packet.Data, 0, data, packet.Blocks.First() * BlockSize, BlockSize);

            return data;
        }

        /// <summary>
        /// Returns a bool for each block indicating if it has been downloaded yet
        /// </summary>
        /// <returns></returns>
        public IEnumerable<bool> Progress()
        {
            HashSet<ReceivedPacket> singleBlocks;
            if (!packets.TryGetValue(1, out singleBlocks))
            {
                for (int i = 0; i < BlockCount; i++)
                    yield return false;

            }
            else
            {
                for (int i = 0; i < BlockCount; i++)
                    yield return singleBlocks.Where(a => a.Blocks.Contains(i)).FirstOrDefault() != null;
            }
        }
        #endregion

        private class ReceivedPacket
        {
            public readonly byte[] Data;

            private int hashcode;
            public readonly HashSet<int> Blocks;

            public ReceivedPacket(Packet packet, int blockCount)
            {
                Data = packet.Data;

                Blocks = new HashSet<int>();
                Helpers.CalculateBlockSet(packet.PacketSeed, blockCount, Blocks);

                GenerateHash();
            }

            public ReceivedPacket(byte[] data, HashSet<int> blocks)
            {
                Blocks = blocks;
                Data = data;

                GenerateHash();
            }

            public void GenerateHash()
            {
                int h = 0;
                foreach (var item in Blocks)
                    h ^= item;

                hashcode = h;
            }

            public override string ToString()
            {
                return "Count = " + Blocks.Count.ToString();
            }

            public override int GetHashCode()
            {
                return hashcode;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                    return true;

                if (obj is ReceivedPacket)
                {
                    ReceivedPacket p = (ReceivedPacket)obj;

                    if (p.Blocks.Count != Blocks.Count)
                        return false;

                    return p.Blocks.Zip(Blocks, (a, b) => a == b).Where(a => !a).IsEmpty();
                }
                return false;
            }

            public void Xor(ReceivedPacket p)
            {
                if (Equals(p))
                    throw new Exception();

                Xor(p.Data);

                foreach (var r in p.Blocks)
                    if (!Blocks.Remove(r))
                        Blocks.Add(r);

                GenerateHash();
            }

            public void Xor(byte[] b)
            {
                if (b.Length != Data.Length)
                    throw new ArgumentException("Data blocks must be the same length");

                for (int i = 0; i < b.Length; i++)
                    Data[i] = (byte)(Data[i] ^ b[i]);
            }
        }
    }
}
