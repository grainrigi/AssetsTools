using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AssetsTools {
    public static class BinaryWriterExtensions {
        public static void AlignStream(this BinaryWriter writer, int alignment) {
            var pos = writer.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0) {
                writer.Write(new byte[alignment - mod]);
            }
        }

        public static void WriteAlignedString(this BinaryWriter writer, string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            writer.AlignStream(4);
        }

        public static void WriteUTF8StringZero(this BinaryWriter writer, string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write((byte)0);
        }
    }
}
