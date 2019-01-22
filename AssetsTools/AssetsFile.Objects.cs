using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        /// <summary>
        /// Object entry of AssetsFile.
        /// </summary>
        public struct ObjectType {
            internal WeakReference<AssetsFile> parent;

            /// <summary>
            /// PathID of this object.
            /// </summary>
            public long PathID;
            /// <summary>
            /// Content of this object.
            /// </summary>
            public byte[] Data;
            /// <summary>
            /// Type index of this object.
            /// </summary>
            public int TypeID;
        }

        private void readObjects(UnityBinaryReader reader) {
            int object_count = reader.ReadInt();
            Objects = new ObjectType[object_count];
            for(int i = 0; i < object_count; i++) {
                Objects[i].parent = new WeakReference<AssetsFile>(this);

                reader.Align(4);
                Objects[i].PathID = reader.ReadLong();
                var byteStart = reader.ReadUInt();
                var byteSize = reader.ReadUInt();
                Objects[i].TypeID = reader.ReadInt();

                // Read Body
                var final_pos = reader.Position;
                reader.Position = Header.DataOffset + (int)byteStart;
                Objects[i].Data = reader.ReadBytes((int)byteSize);
                reader.Position = final_pos;
            }
        }

        private byte[] writeObjects(UnityBinaryWriter writer) {
            writer.WriteInt(Objects.Length);
            UnityBinaryWriter objectwriter = new UnityBinaryWriter();
            for(int i = 0; i < Objects.Length; i++) {
                // objects alignment is 8byte
                writer.Align(4);
                writer.WriteLong(Objects[i].PathID);
                objectwriter.Align(8);
                writer.WriteInt(objectwriter.Position);
                writer.WriteInt(Objects[i].Data.Length);
                writer.WriteInt(Objects[i].TypeID);

                objectwriter.WriteBytes(Objects[i].Data);
            }

            // return body
            return objectwriter.ToBytes();
        }
    }
}
