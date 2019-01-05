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
        public void ReadShortLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<short>(0x2301, r.ReadShortLE());
            Assert.AreEqual<short>(0x6745, r.ReadShortLE());
            Assert.AreEqual<short>(unchecked((short)0xAB89), r.ReadShortLE());
            Assert.AreEqual<short>(unchecked((short)0xEFCD), r.ReadShortLE());
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
        public void ReadUShortLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            // r_bin
            Assert.AreEqual<ushort>(0x2301, r.ReadUShortLE());
            Assert.AreEqual<ushort>(0x6745, r.ReadUShortLE());
            Assert.AreEqual<ushort>(0xAB89, r.ReadUShortLE());
            Assert.AreEqual<ushort>(0xEFCD, r.ReadUShortLE());
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
        public void ReadIntLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<int>(0x67452301, r.ReadIntLE());
            Assert.AreEqual<int>(unchecked((int)0xEFCDAB89), r.ReadIntLE());
        }

        [TestMethod]
        public void ReadIntBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<int>(0x01234567, r.ReadIntBE());
            Assert.AreEqual<int>(unchecked((int)0x89ABCDEF), r.ReadIntBE());
        }

        [TestMethod]
        public void ReadUIntLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<uint>(0x67452301, r.ReadUIntLE());
            Assert.AreEqual<uint>(0xEFCDAB89, r.ReadUIntLE());
        }

        [TestMethod]
        public void ReadUIntBE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<uint>(0x01234567, r.ReadUIntBE());
            Assert.AreEqual<uint>(0x89ABCDEF, r.ReadUIntBE());
        }

        [TestMethod]
        public void ReadLongLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            // r_bin
            Assert.AreEqual<long>(unchecked((long)0xEFCDAB8967452301), r.ReadLongLE());
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
        public void ReadULongLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestData);

            Assert.AreEqual<ulong>(0xEFCDAB8967452301, r.ReadULongLE());
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
        public void ReadSingleLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestFloatBytes);

            Assert.AreEqual<float>(TestFloat, r.ReadFloatLE());
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
        public void ReadDoubleLE() {
            UnityBinaryReader r = new UnityBinaryReader(TestDoubleBytes);

            Assert.AreEqual<double>(TestDouble, r.ReadDoubleLE());
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

            // ReadShortLE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadShortLE(); }, "ReadShortLE failed");
            // ReadShortBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadShortBE(); }, "ReadShortBE failed");
            // ReadUShortLE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUShortLE(); }, "ReadUShortLE failed");
            // ReadUShortBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 7; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUShortBE(); }, "ReadUShortBE failed");

            // ReadIntLE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadIntLE(); }, "ReadIntLE failed");
            // ReadIntBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadIntBE(); }, "ReadIntBE failed");
            // ReadUIntLE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUIntLE(); }, "ReadUIntLE failed");
            // ReadUIntBE
            r = new UnityBinaryReader(TestData);
            for (int i = 0; i < 5; i++)
                r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadUIntBE(); }, "ReadUIntBE failed");

            // ReadLongLE
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadLongLE(); }, "ReadLongLE failed");
            // ReadLongBE
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadLongBE(); }, "ReadLongBE failed");
            // ReadULongLE
            r = new UnityBinaryReader(TestData);
            r.ReadByte();
            Assert.ThrowsException<IndexOutOfRangeException>(delegate () { r.ReadULongLE(); }, "ReadULongLE failed");
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
