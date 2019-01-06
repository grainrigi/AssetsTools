using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
