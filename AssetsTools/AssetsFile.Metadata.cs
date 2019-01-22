using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        public struct MetadataHeaderType : ISerializable {
            /// <summary>
            /// Version of UnityEngine
            /// </summary>
            public string UnityVersion; // version >= 7
            /// <summary>
            /// Target platform of this file. (<see cref="BuildTarget"/>)
            /// </summary>
            public int TargetPlatform; // version >= 8
            /// <summary>
            /// If typetree is enabled.
            /// </summary>
            public bool EnableTypeTree; // version >= 13

            public void Read(UnityBinaryReader reader) {
                UnityVersion = reader.ReadStringToNull();
                TargetPlatform = reader.ReadInt();
                EnableTypeTree = reader.ReadByte() != 0;
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteStringToNull(UnityVersion);
                writer.WriteInt(TargetPlatform);
                writer.WriteByte((byte)(EnableTypeTree ? 1 : 0));
            }
        }

        private void readMetadata(UnityBinaryReader reader) {
            // Read Header
            MetadataHeader.Read(reader);

            // Read Types
            int type_count = reader.ReadInt();
            Types = new SerializedType[type_count];
            for(int i = 0; i < type_count; i++) {
                Types[i].Read(reader);
                if(MetadataHeader.EnableTypeTree) {
                    Types[i].TypeTree = new TypeTree();
                    Types[i].TypeTree.Read(reader);
                }
            }
        }

        private void writeMetadata(UnityBinaryWriter writer) {
            // Write Header
            MetadataHeader.Write(writer);

            // Write Types
            writer.WriteInt(Types.Length);
            for (int i = 0; i < Types.Length; i++) {
                Types[i].Write(writer);
                if (MetadataHeader.EnableTypeTree)
                    Types[i].TypeTree.Write(writer);
            }
        }
    }
}
