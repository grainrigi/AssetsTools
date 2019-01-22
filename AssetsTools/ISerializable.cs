using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AssetsTools {
    internal interface ISerializable {
        void Read(UnityBinaryReader reader);
        void Write(UnityBinaryWriter writer);
    }

    internal static class ISerializableArrayExtensions {
        public static void Read<T>(this T[] ary, UnityBinaryReader reader) where T : ISerializable {
            for (int i = 0; i < ary.Length; i++)
                ary[i].Read(reader);
        }

        public static void Write<T>(this T[] ary, UnityBinaryWriter writer) where T : ISerializable {
            for (int i = 0; i < ary.Length; i++)
                ary[i].Write(writer);
        }
    }

    /// <summary>
    /// The Exception that is thrown when the file format is unknown.
    /// </summary>
    public class UnknownFormatException : Exception {
        public UnknownFormatException() { }
        public UnknownFormatException(string message) : base(message) { }
    }
}
