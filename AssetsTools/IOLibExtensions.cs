using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public static class IOLibExtensions {
        public static void Align(this UnityBinaryReader reader, int align) {
            var mod = reader.Position % align;
            if (mod != 0)
                reader.Position += align - mod;
        }

        public static void Align(this UnityBinaryWriter writer, int align) {
            var mod = writer.Position % align;
            if (mod != 0)
                writer.Position += align - mod;
        }
        
        public static string ReadAlignedString(this UnityBinaryReader reader) {
            string str = reader.ReadString(reader.ReadInt());
            reader.Align(4);
            return str;
        }
    }
}
