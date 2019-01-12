using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssetsTools;

namespace IOLibTest {
    [TestClass]
    public class UnityBinaryWriterTest {
        private static byte[] TestData =
            new byte[8] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
        private static byte[] TestArray =
            new byte[12] { 0x08, 0x00, 0x00, 0x00,
                0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

        private const float TestFloat = 0.133333333333f;
        private const double TestDouble = 0.1333333333333333333;
        private const string TestString = "kitty on your lap あああああいうえお漢字漢字";

        private readonly byte[] TestFloatBytes = BitConverter.GetBytes(TestFloat);
        private readonly byte[] TestDoubleBytes = BitConverter.GetBytes(TestDouble);
        private byte[] TestStringBytes = Encoding.UTF8.GetBytes(TestString);

        private const string filename = "test.dat";

        private static byte[] TestLZ4Data =
            new byte[0x57] {
                0x1E, 0x00, 0x01, 0x00, 0x30, 0x04, 0x00, 0x02,
                0x07, 0x00, 0x42, 0x1F, 0x21, 0x00, 0x03, 0x0A,
                0x00, 0x24, 0x1B, 0x71, 0x0A, 0x00, 0xC0, 0x1F,
                0x2E, 0x00, 0x03, 0x00, 0x00, 0xD2, 0x90, 0x00,
                0x00, 0x0F, 0x47, 0x0A, 0x00, 0x29, 0x00, 0x01,
                0x3A, 0x00, 0x10, 0x06, 0x1A, 0x00, 0xF0, 0x18,
                0x00, 0x04, 0x43, 0x41, 0x42, 0x2D, 0x35, 0x66,
                0x36, 0x66, 0x39, 0x66, 0x36, 0x61, 0x32, 0x39,
                0x35, 0x61, 0x65, 0x32, 0x37, 0x34, 0x36, 0x35,
                0x38, 0x65, 0x33, 0x39, 0x39, 0x34, 0x34, 0x31,
                0x39, 0x33, 0x31, 0x33, 0x35, 0x65, 0x00
            };
        private static byte[] TestLZ4DecompressedData =
            new byte[0x79] {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x04, 0x00, 0x02, 0x00, 0x00,
                0x00, 0x00, 0x1F, 0x21, 0x00, 0x03, 0x00, 0x02,
                0x00, 0x00, 0x00, 0x00, 0x1B, 0x71, 0x00, 0x03,
                0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x2E,
                0x00, 0x03, 0x00, 0x00, 0xD2, 0x90, 0x00, 0x00,
                0x0F, 0x47, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0xD2, 0x90,
                0x00, 0x00, 0x00, 0x04, 0x43, 0x41, 0x42, 0x2D,
                0x35, 0x66, 0x36, 0x66, 0x39, 0x66, 0x36, 0x61,
                0x32, 0x39, 0x35, 0x61, 0x65, 0x32, 0x37, 0x34,
                0x36, 0x35, 0x38, 0x65, 0x33, 0x39, 0x39, 0x34,
                0x34, 0x31, 0x39, 0x33, 0x31, 0x33, 0x35, 0x65,
                0x00
            };

        [ClassInitialize]
        public static void ClassInit(TestContext c) {

        }

        [TestMethod]
        public void WriteByte() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            for (int i = 0; i < 8; i++)
                w.WriteByte(TestData[i]);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteSByte() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            for (int i = 0; i < 8; i++)
                w.WriteSByte((sbyte)TestData[i]);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteShort() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteShort(0x2301);
            w.WriteShort(0x6745);
            w.WriteShort(unchecked((short)0xAB89));
            w.WriteShort(unchecked((short)0xEFCD));
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteShortBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteShortBE(0x0123);
            w.WriteShortBE(0x4567);
            w.WriteShortBE(unchecked((short)0x89AB));
            w.WriteShortBE(unchecked((short)0xCDEF));
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteUShort() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteUShort(0x2301);
            w.WriteUShort(0x6745);
            w.WriteUShort(0xAB89);
            w.WriteUShort(0xEFCD);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteUShortBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteUShortBE(0x0123);
            w.WriteUShortBE(0x4567);
            w.WriteUShortBE(0x89AB);
            w.WriteUShortBE(0xCDEF);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteInt() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteInt(0x67452301);
            w.WriteInt(unchecked((int)0xEFCDAB89));
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteIntBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteIntBE(0x01234567);
            w.WriteIntBE(unchecked((int)0x89ABCDEF));
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteUInt() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteUInt(0x67452301);
            w.WriteUInt(0xEFCDAB89);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteUIntBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteUIntBE(0x01234567);
            w.WriteUIntBE(0x89ABCDEF);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }


        [TestMethod]
        public void WriteLong() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteLong(unchecked((long)0xEFCDAB8967452301));
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteLongBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteLongBE(0x0123456789ABCDEF);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteULong() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteULong(0xEFCDAB8967452301);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteULongBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteULongBE(0x0123456789ABCDEF);
            byte[] res = w.ToBytes();
            CompareBytes(TestData, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteFloat() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteFloat(TestFloat);
            byte[] res = w.ToBytes();
            CompareBytes(TestFloatBytes, res, 4);
            Assert.AreEqual<long>(4L, res.LongLength);
        }

        [TestMethod]
        public void WriteFloatBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteFloatBE(TestFloat);
            byte[] res = w.ToBytes();
            byte[] test = new byte[4];
            Array.Copy(TestFloatBytes, test, 4);
            Array.Reverse(test);
            CompareBytes(test, res, 4);
            Assert.AreEqual<long>(4L, res.LongLength);
        }

        [TestMethod]
        public void WriteDouble() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteDouble(TestDouble);
            byte[] res = w.ToBytes();
            CompareBytes(TestDoubleBytes, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteDoubleBE() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteDoubleBE(TestDouble);
            byte[] res = w.ToBytes();
            byte[] test = new byte[8];
            Array.Copy(TestDoubleBytes, test, 8);
            Array.Reverse(test);
            CompareBytes(test, res, 8);
            Assert.AreEqual<long>(8L, res.LongLength);
        }

        [TestMethod]
        public void WriteBytes() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteBytes(TestData, 2, 6);
            byte[] dest = w.ToBytes();
            for (int i = 0; i < 6; i++)
                Assert.AreEqual<byte>(TestData[i+2], dest[i]);
            Assert.AreEqual<int>(6, dest.Length);

            Assert.ThrowsException<NullReferenceException>(delegate () { w.WriteBytes(null, 0, 0); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(delegate () { w.WriteBytes(TestData, 2, 7); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(delegate () { w.WriteBytes(TestData, 3, 6); });
        }

        [TestMethod]
        public void WriteStringToNull() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteStringToNull(TestString);
            byte[] res = w.ToBytes();
            CompareBytes(TestStringBytes, res, TestStringBytes.Length);
            Assert.AreEqual<byte>(0, res[TestStringBytes.Length]);
            Assert.AreEqual<long>(TestStringBytes.LongLength + 1, res.LongLength);
        }

        [TestMethod]
        public void WriteValueArray() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            w.WriteValueArray(TestData);
            byte[] res = w.ToBytes();
            CompareBytes(TestArray, res, TestArray.Length);
            Assert.AreEqual<long>(12L, res.LongLength);
        }

        [TestMethod]
        public void WriteLZ4Data() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            int size = w.WriteLZ4Data(TestLZ4DecompressedData);
            byte[] enc = w.ToBytes();
            Assert.AreEqual<int>(enc.Length, size);
            byte[] dec = new byte[TestLZ4DecompressedData.Length];
            AssetsTools.LZ4.LZ4Codec.Decode(enc, 0, enc.Length, dec, 0, dec.Length);
            for (int i = 0; i < TestLZ4DecompressedData.Length; i++)
                Assert.AreEqual<byte>(TestLZ4DecompressedData[i], dec[i]);
        }

        [TestMethod]
        public void Expand() {
            UnityBinaryWriter w = new UnityBinaryWriter();

            int cnt = 1000000;
            for (int i = 0; i < cnt; i++)
                w.WriteByte((byte)(i & 0xFF));
            byte[] res = w.ToBytes();
            for (int i = 0; i < cnt; i++)
                if ((byte)(i & 0xFF) != res[i])
                    Assert.Fail(i.ToString());
            Assert.AreEqual<long>((long)cnt, res.LongLength);
        }

        private static void CompareBytes(byte[] a1, byte[] a2, int count) {
            for (int i = 0; i < count; i++)
                if (a1[i] != a2[i])
                    Assert.AreEqual<byte>(a1[i], a2[i]);
        }
    }
}
