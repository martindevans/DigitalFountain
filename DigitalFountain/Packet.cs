using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalFountain
{
    public struct Packet
    {
        public readonly int PacketSeed;

        public readonly byte[] Data;

        public Packet(int seed, byte[] data)
        {
            PacketSeed = seed;
            Data = data;
        }
    }
}
