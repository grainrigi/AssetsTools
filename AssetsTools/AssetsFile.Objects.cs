using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        public struct ObjectType {
            public WeakReference<AssetsFile> parent;

            public long PathID;
            public byte[] Data;
            public int TypeID;
        }

        private void readObjects(UnityBinaryReader reader) {
            int object_count = reader.ReadInt();
            Objects = new ObjectType[object_count];
            for(int i = 0; i < object_count; i++) {
                Objects[i].parent.SetTarget(this);

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
