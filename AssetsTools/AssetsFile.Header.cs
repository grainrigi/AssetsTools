using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        /// <summary>
        /// Header of AssetsFile.
        /// </summary>
        public struct HeaderType : ISerializable {
            /// <summary>
            /// Size of metadata.
            /// </summary>
            /// <remarks>You don't need to set this field by yourself.</remarks>
            public int MetadataSize;
            /// <summary>
            /// Size of AssetsFile.
            /// </summary>
            /// <remarks>You don't need to set this field by yourself.</remarks>
            public int FileSize;
            /// <summary>
            /// Version of AssetsFile.
            /// </summary>
            public int Version;
            /// <summary>
            /// Start offset of data.
            /// </summary>
            /// <remarks>You don't need to set this field by yourself.</remarks>
            public int DataOffset;
            // version >= 9
            /// <summary>
            /// Whether file uses bigendian.
            /// </summary>
            public bool IsBigEndian;
            /// <summary>
            /// Reserved bytes.
            /// </summary>
            public byte[] Reserved;

            public void Read(UnityBinaryReader reader) {
                MetadataSize = reader.ReadIntBE();
                FileSize = reader.ReadIntBE();
                Version = reader.ReadIntBE();
                DataOffset = reader.ReadIntBE();
                IsBigEndian = reader.ReadByte() == 0 ? false : true;
                Reserved = reader.ReadBytes(3);
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteIntBE(MetadataSize);
                writer.WriteIntBE(FileSize);
                writer.WriteIntBE(Version);
                writer.WriteIntBE(DataOffset);
                writer.WriteByte((byte)(IsBigEndian ? 1 : 0));
                writer.WriteBytes(Reserved, 0, 3);
            }

            internal int CalcSize() {
                return 4 + 4 + 4 + 4
                    + 1 + 3;
            }
        }
    }
}
