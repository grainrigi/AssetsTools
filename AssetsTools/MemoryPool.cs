using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public static class MemoryPool<T> {
        private const int DEFAULT_SIZE = 65535;

        private static byte[] buf = null;

        public static byte[] GetBuffer(int size) {
            if(buf == null) {
                buf = new byte[size > DEFAULT_SIZE ? size : DEFAULT_SIZE];
                return buf;
            }

            if (buf.Length < size) {
                if (size < buf.Length * 2)
                    buf = new byte[buf.Length * 2];
                else
                    buf = new byte[size];
            }
            return buf;
        }
    }

    public static class MiniMemoryPool<T> {
        private const int DEFAULT_SIZE = 255;

        private static byte[] buf = null;
        private static byte[] buf2 = null;

        public static byte[] GetBuffer(int size) {
            if (buf == null) {
                buf = new byte[size > DEFAULT_SIZE ? size : DEFAULT_SIZE];
                return buf;
            }

            if (buf.Length < size)
                buf = new byte[size];
            return buf;
        }

        public static byte[] GetBuffer2(int size) {
            if (buf == null) {
                buf = new byte[size > DEFAULT_SIZE ? size : DEFAULT_SIZE];
                return buf;
            }
            if (buf2.Length < size)
                buf2 = new byte[size];
            return buf2;
        }
    }
}
