using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigitalFountain;

namespace DigitalFountainTest
{
    [TestClass]
    public class FountainTest
    {
        byte[] data = new byte[]
        {
            190,167,243,201,105,195,141,28,189,98,
            151,175,75,82,95,182,24,79,74,121,
            99,32,246,200,173,28,2,70,137,111,
            196,16,40,32,46,116,16,202,243,18,
            77,41,132,90,49,186,132,49,160,226,
            1,134,9,75,244,75,239,164,25,112,
            54,22,51,97,241,238,234,192,194,130,
            0,122,220,198,238,247,144,146,66,196,
            179,5,177,176,157,90,53,160,208,32,
            116,50,75,156,195,221,2,166,23,203,
        };

        [TestMethod]
        public void CreateFountain()
        {
            Fountain f = new Fountain(1337, data, 4);
        }

        [TestMethod]
        public void CreateBucket()
        {
            Bucket b = new Bucket(1, 2);
        }

        [TestMethod]
        public void EncodeDecode()
        {
            Fountain f = new Fountain(1234, data, 4);
            Bucket bucket = new Bucket(f.BlockSize, f.BlockCount);

            DigitalFountain.Packet packet;
            bool cont = true;
            do
            {
                packet = f.CreatePacket();

                cont = !bucket.AddPacket(packet);
            } while (cont);

            byte[] decoded = bucket.GetData();

            Assert.AreEqual(decoded.Length, data.Length);

            for (int i = 0; i < decoded.Length; i++)
                Assert.AreEqual(decoded[i], data[i]);
        }
    }
}
