using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace IOLibTest {
    [TestClass]
    public class UnityBinaryReaderTest {
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
        public static void ClassInit(TestContext context) {
            using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                fs.Write(TestData, 0, TestData.Length);
            }
        }

        [TestMethod]
        public void ConstructTest_Binary() {
            UnityBinaryReader reader = new UnityBinaryReader(TestData);
            Assert.ThrowsException<NullReferenceException>(delegate () { new UnityBinaryReader((byte[])null); });
        }

        [TestMethod]
        public void ConstructTest_File() {
            UnityBinaryReader reader = new UnityBinaryReader(filename);
        }

        [TestMethod]
        public void ReadByte() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            foreach (var b in TestData) {
                Assert.AreEqual<byte>(b, r.ReadByte());
            }
        }

        [TestMethod]
        public void ReadSByte() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            foreach (var b in TestData) {
                Assert.AreEqual<sbyte>((sbyte)b, r.ReadSByte());
            }
        }

        [TestMethod]
        public void ReadShort() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<short>(0x2301, r.ReadShort());
            Assert.AreEqual<short>(0x6745, r.ReadShort());
            Assert.AreEqual<short>(unchecked((short)0xAB89), r.ReadShort());
            Assert.AreEqual<short>(unchecked((short)0xEFCD), r.ReadShort());
        }

        [TestMethod]
        public void ReadShortBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<short>(0x0123, r.ReadShortBE());
            Assert.AreEqual<short>(0x4567, r.ReadShortBE());
            Assert.AreEqual<short>(unchecked((short)0x89AB), r.ReadShortBE());
            Assert.AreEqual<short>(unchecked((short)0xCDEF), r.ReadShortBE());
        }

        [TestMethod]
        public void ReadUShort() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            // r_bin
            Assert.AreEqual<ushort>(0x2301, r.ReadUShort());
            Assert.AreEqual<ushort>(0x6745, r.ReadUShort());
            Assert.AreEqual<ushort>(0xAB89, r.ReadUShort());
            Assert.AreEqual<ushort>(0xEFCD, r.ReadUShort());
        }

        [TestMethod]
        public void ReadUShortBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<ushort>(0x0123, r.ReadUShortBE());
            Assert.AreEqual<ushort>(0x4567, r.ReadUShortBE());
            Assert.AreEqual<ushort>(0x89AB, r.ReadUShortBE());
            Assert.AreEqual<ushort>(0xCDEF, r.ReadUShortBE());
        }

        [TestMethod]
        public void ReadInt() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<int>(0x67452301, r.ReadInt());
            Assert.AreEqual<int>(unchecked((int)0xEFCDAB89), r.ReadInt());
        }

        [TestMethod]
        public void ReadIntBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<int>(0x01234567, r.ReadIntBE());
            Assert.AreEqual<int>(unchecked((int)0x89ABCDEF), r.ReadIntBE());
        }

        [TestMethod]
        public void ReadUInt() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<uint>(0x67452301, r.ReadUInt());
            Assert.AreEqual<uint>(0xEFCDAB89, r.ReadUInt());
        }

        [TestMethod]
        public void ReadUIntBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<uint>(0x01234567, r.ReadUIntBE());
            Assert.AreEqual<uint>(0x89ABCDEF, r.ReadUIntBE());
        }

        [TestMethod]
        public void ReadLong() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            // r_bin
            Assert.AreEqual<long>(unchecked((long)0xEFCDAB8967452301), r.ReadLong());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadLongBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<long>(0x0123456789ABCDEF, r.ReadLongBE());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadULong() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<ulong>(0xEFCDAB8967452301, r.ReadULong());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadULongBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<ulong>(0x0123456789ABCDEF, r.ReadULongBE());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadSingle() {
            UnityBinaryReader r = new UnityBinaryReader(TestFloatBytes);

            Assert.AreEqual<float>(TestFloat, r.ReadFloat());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadSingleBE() {
            byte[] rev = BitConverter.GetBytes(TestFloat);
            Array.Reverse(rev);
            UnityBinaryReader r = new UnityBinaryReader(rev);

            Assert.AreEqual<float>(TestFloat, r.ReadFloatBE());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadDouble() {
            UnityBinaryReader r = new UnityBinaryReader(TestDoubleBytes);

            Assert.AreEqual<double>(TestDouble, r.ReadDouble());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadDoubleBE() {
            byte[] rev = BitConverter.GetBytes(TestDouble);
            Array.Reverse(rev);
            UnityBinaryReader r = new UnityBinaryReader(rev);

            Assert.AreEqual<double>(TestDouble, r.ReadDoubleBE());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadStringToNull() {
            byte[] sz = new byte[TestStringBytes.Length + 1];
            Array.Copy(TestStringBytes, sz, TestStringBytes.Length);
            sz[TestStringBytes.Length] = 0x00;
            UnityBinaryReader r = new UnityBinaryReader(sz);

            Assert.AreEqual<string>(TestString, r.ReadStringToNull());
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void ReadValueArray() {
            UnityBinaryReader r = new UnityBinaryReader(TestArray);

            byte[] read = r.ReadValueArray<byte>();
            for (int i = 0; i < 8; i++)
                Assert.AreEqual<byte>(TestData[i], read[i]);
            // Forward Test
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadByte(); });
        }

        [TestMethod]
        public void OutBound() {
            UnityBinaryReader r;

            // ReadByte
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 8; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate() { r.ReadByte(); }, "ReadByte failed");
            // ReadSByte
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 8; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadSByte(); }, "ReadSByte failed");

            // ReadShort
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadShort(); }, "ReadShort failed");
            // ReadShortBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadShortBE(); }, "ReadShortBE failed");
            // ReadUShort
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUShort(); }, "ReadUShort failed");
            // ReadUShortBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUShortBE(); }, "ReadUShortBE failed");

            // ReadInt
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadInt(); }, "ReadInt failed");
            // ReadIntBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadIntBE(); }, "ReadIntBE failed");
            // ReadUInt
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUInt(); }, "ReadUInt failed");
            // ReadUIntBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUIntBE(); }, "ReadUIntBE failed");

            // ReadLong
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadLong(); }, "ReadLong failed");
            // ReadLongBE
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadLongBE(); }, "ReadLongBE failed");
            // ReadULong
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadULong(); }, "ReadULong failed");
            // ReadULongBE
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadULongBE(); }, "ReadULongBE failed");

            // ReadStringToNull
            r = new UnityBinaryReader(TestStringBytes);
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadStringToNull(); }, "ReadStringToNull failed");

            // ReadValueArray
            TestArray[0] = 0x09;
            r = new UnityBinaryReader(TestArray);
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadValueArray<byte>(); }, "ReadValueArray failed");
            TestArray[0] = 0x08;
        }
    }
}
