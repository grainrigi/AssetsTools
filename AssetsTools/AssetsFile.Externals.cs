using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        /// <summary>
        /// External file entry of AssetsFile.
        /// </summary>
        public struct ExternalFileType : ISerializable {
            /// <summary>
            /// GUID of the file.
            /// </summary>
            public Guid Guid;
            /// <summary>
            /// Type of the file.
            /// </summary>
            public int Type;
            /// <summary>
            /// Path to the file.
            /// </summary>
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
