using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools {
    public class UnityBinaryWriter {
        private byte[] file = new byte[256];
        private int offset = 0;
        private int bound = 0;
        private int capacity = 256;

        public int Position {
            get {
                return offset;
            }
            set {
                // Update bound
                if (offset > bound)
                    bound = offset;
                if (capacity < value) {
                    EnsureCapacity(value);
                }
                offset = value;
                if (offset > bound) {
                    bound = offset;
                }
            }
        }

        public int Length => bound > offset ? bound : offset;

        public UnityBinaryWriter() {
            if(!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian platform is not supported");
        }

        public unsafe void EnsureCapacity(int value) {
            int newCapacity = value;
            if (newCapacity < capacity * 2) {
                newCapacity = capacity * 2;
            }
            fixed (byte* p = &file[0]) {
                byte[] nary = new byte[newCapacity];
                fixed (byte* q = &nary[0]) {
                    Buffer.MemoryCopy(p, q, newCapacity, file.LongLength);
                    file = nary;
                }
            }
            capacity = newCapacity;
        }

        public unsafe byte[] ToBytes() {
            // Update bound
            if (offset > bound)
                bound = offset;

            byte[] result = new byte[bound];
            if (bound == 0)
                return result;
            fixed (byte* q = &result[0]) {
                fixed (byte* p = &file[0]) {
                    Buffer.MemoryCopy(p, q, bound, bound);
                }
            }
            return result;
        }

        #region Byte
        public void WriteByte(byte value) {
            if (capacity < offset + 1) {
                EnsureCapacity(offset + 1);
            }
            file[offset] = value;
            offset = offset + 1;
        }

        public void WriteSByte(sbyte value) {
            if (capacity < offset + 1) {
                EnsureCapacity(offset + 1);
            }
            file[offset] = (byte)value;
            offset = offset + 1;
        }

        public void WriteBool(bool value) {
            if (capacity < offset + 1) {
                EnsureCapacity(offset + 1);
            }
            file[offset] = (value ? ((byte)1) : ((byte)0));
            offset = offset + 1;
        }
        #endregion

        #region LittleEndian
        public unsafe void WriteShort(short value) {
            if (capacity < offset + 2) {
                EnsureCapacity(offset + 2);
            }
            fixed (byte*p = &file[offset]) {
                *(short*)p = value;
                offset += 2;
            }
        }

        public unsafe void WriteInt(int value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte*p = &file[offset]) {
                *(int*)p = value;
                offset += 4;
            }
        }

        public unsafe void WriteLong(long value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte*p = &file[offset]) {
                *(long*)p = value;
                offset += 8;
            }
        }

        public unsafe void WriteFloat(float value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte*p = &file[offset]) {
                *(float*)p = value;
                offset += 4;
            }
        }

        public unsafe void WriteDouble(double value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte*p = &file[offset]) {
                *(double*)p = value;
                offset += 8;
            }
        }

        public unsafe void WriteUShort(ushort value) {
            if (capacity < offset + 2) {
                EnsureCapacity(offset + 2);
            }
            fixed (byte*p = &file[offset]) {
                *(ushort*)p = value;
                offset += 2;
            }
        }

        public unsafe void WriteUInt(uint value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte*p = &file[offset]) {
                *(uint*)p = value;
                offset += 4;
            }
        }

        public unsafe void WriteULong(ulong value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte*p = &file[offset]) {
                *(ulong*)p = value;
                offset += 8;
            }
        }
        #endregion

        #region BigEndian
        public unsafe void WriteShortBE(short value) {
            if(capacity < offset + 2) {
                EnsureCapacity(offset + 2);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[1];
                p[1] = q[0];
                offset += 2;
            }
        }

        public unsafe void WriteIntBE(int value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[3];
                p[1] = q[2];
                p[2] = q[1];
                p[3] = q[0];
                offset += 4;
            }
        }

        public unsafe void WriteLongBE(long value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[7];
                p[1] = q[6];
                p[2] = q[5];
                p[3] = q[4];
                p[4] = q[3];
                p[5] = q[2];
                p[6] = q[1];
                p[7] = q[0];
                offset += 8;
            }
        }
        
        public unsafe void WriteFloatBE(float value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[3];
                p[1] = q[2];
                p[2] = q[1];
                p[3] = q[0];
                offset += 4;
            }
        }

        public unsafe void WriteDoubleBE(double value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[7];
                p[1] = q[6];
                p[2] = q[5];
                p[3] = q[4];
                p[4] = q[3];
                p[5] = q[2];
                p[6] = q[1];
                p[7] = q[0];
                offset += 8;
            }
        }

        public unsafe void WriteUShortBE(ushort value) {
            if(capacity < offset + 2) {
                EnsureCapacity(offset + 2);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[1];
                p[1] = q[0];
                offset += 2;
            }
        }

        public unsafe void WriteUIntBE(uint value) {
            if (capacity < offset + 4) {
                EnsureCapacity(offset + 4);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[3];
                p[1] = q[2];
                p[2] = q[1];
                p[3] = q[0];
                offset += 4;
            }
        }

        public unsafe void WriteULongBE(ulong value) {
            if (capacity < offset + 8) {
                EnsureCapacity(offset + 8);
            }
            fixed (byte* p = &file[offset]) {
                byte* q = (byte*)&value;
                p[0] = q[7];
                p[1] = q[6];
                p[2] = q[5];
                p[3] = q[4];
                p[4] = q[3];
                p[5] = q[2];
                p[6] = q[1];
                p[7] = q[0];
                offset += 8;
            }
        }
        #endregion

        #region VarLengthData
        public unsafe void WriteBytes(byte[] src, int offset, int length) {
            if (length != 0) {
                if (src == null) {
                    throw new NullReferenceException("src");
                }
                if (offset < 0 || offset >= src.Length) {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (length < 0 || length > src.Length - offset) {
                    throw new ArgumentOutOfRangeException("length");
                }
                if (length + this.offset > capacity) {
                    EnsureCapacity(length + this.offset);
                }
                fixed (byte*q = &file[this.offset])
                fixed (byte*p = &src[offset]) { 
                    Buffer.MemoryCopy(p, q, length, length);
                    this.offset += length;
                }
            }
        }

        public void WriteBytes(byte[] src) {
            //Error decoding local variables: Signature type sequence must have at least one element.
            WriteBytes(src, 0, src.Length);
        }

        public unsafe void WriteString(string value) {
            if (value == null) {
                WriteInt(0);
            }
            else if (value.Length == 0) {
                WriteInt(0);
            }
            else {
                int num;
                int num2 = Encoding.UTF8.GetMaxByteCount(value.Length) + (num = offset + 4);
                if (num2 > capacity) {
                    EnsureCapacity(num2);
                }
                int bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, file, num);
                fixed (byte* p = &file[offset]) {
                    *(int*)p = bytes;
                    offset = num + bytes;
                }
            }
        }

        public void WriteStringToNull(string value) {
            if (value == null) {
                WriteByte(0);
            }
            else if (value.Length == 0) {
                WriteByte(0);
            }
            else {
                int num;
                int num2 = Encoding.UTF8.GetMaxByteCount(value.Length + 1) + (num = offset);
                if (num2 > capacity) {
                    EnsureCapacity(num2);
                }
                num = Encoding.UTF8.GetBytes(value, 0, value.Length, file, num) + num;
                file[num] = 0;
                offset = num + 1;
            }
        }

        public unsafe void WriteValueArray<T>(T[] array) where T : unmanaged {
            if (array == null || array.Length == 0) {
                WriteInt(0);
            }
            else {
                long num2;
                num2 = sizeof(T) * array.LongLength;
                if (num2 + offset + 4 > capacity) {
                    EnsureCapacity((int)num2 + offset + 4);
                }
                fixed (byte* q = &file[offset])
                fixed (T* p = &array[0]) {
                    *(int*)q = array.Length;
                    Buffer.MemoryCopy(p, q+4, num2, num2);
                    offset += (int)num2 + 4;
                }
            }
        }

        public int WriteLZ4Data(byte[] bin, int offset, int length) {
            if (this.offset + length > capacity) {
                EnsureCapacity(this.offset + length);
            }
            int num = LZ4.LZ4Codec.Encode(bin, offset, length, file, this.offset, capacity - this.offset);
            this.offset += num;
            return num;
        }

        public int WriteLZ4Data(byte[] bin) {
            return WriteLZ4Data(bin, 0, bin.Length);
        }
        #endregion
    }
}
