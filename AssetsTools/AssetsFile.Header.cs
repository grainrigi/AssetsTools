using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        public struct HeaderType : ISerializable {
            public int MetadataSize;
            public int FileSize;
            public int Version;
            public int DataOffset;
            // version >= 9
            public bool IsBigEndian;
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
