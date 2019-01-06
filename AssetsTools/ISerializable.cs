using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AssetsTools {
    public interface ISerializable {
        void Read(UnityBinaryReader reader);
        void Write(UnityBinaryWriter writer);
    }

    public class UnknownFormatException : Exception {
        public UnknownFormatException() { }
        public UnknownFormatException(string message) : base(message) { }
    }
}
