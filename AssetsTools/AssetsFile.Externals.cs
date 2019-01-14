using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        public struct ExternalFileType : ISerializable {
            public Guid Guid;
            public int Type;
            public string PathName;

            public void Read(UnityBinaryReader reader) {
                var typeEmpty = reader.ReadStringToNull();
                Guid = new Guid(reader.ReadBytes(16));
                Type = reader.ReadInt();
                PathName = reader.ReadStringToNull();
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteStringToNull("");
                writer.WriteBytes(Guid.ToByteArray());
                writer.WriteInt(Type);
                writer.WriteStringToNull(PathName);
            }
        }

        private void readExternals(UnityBinaryReader reader) {
            int external_count = reader.ReadInt();
            Externals = new ExternalFileType[external_count];
            Externals.Read(reader);
        }

        private void writeExternals(UnityBinaryWriter writer) {
            writer.WriteInt(Externals.Length);
            Externals.Write(writer);
        }
    }
}
