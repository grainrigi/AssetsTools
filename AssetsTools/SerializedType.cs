using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public struct SerializedType : ISerializable {
        public int ClassID;
        public bool IsStrippedType; // version >= 16
        public short ScriptTypeIndex; // version >= 17
        public byte[] ScriptID;
        public byte[] OldTypeHash;
        public TypeTree TypeTree;

        public void Read(UnityBinaryReader reader) {
            ClassID = reader.ReadInt();
            IsStrippedType = reader.ReadByte() != 0;
            ScriptTypeIndex = reader.ReadShort();

            if (ClassID == (int)ClassIDType.MonoBehaviour)
                ScriptID = reader.ReadBytes(16);
            OldTypeHash = reader.ReadBytes(16);
        }

        public void Write(UnityBinaryWriter writer) {
            writer.WriteInt(ClassID);
            writer.WriteByte((byte)(IsStrippedType ? 1 : 0));
            writer.WriteShort(ScriptTypeIndex);

            if (ClassID == (int)ClassIDType.MonoBehaviour)
                writer.WriteBytes(ScriptID, 0, 16);
            writer.WriteBytes(OldTypeHash, 0, 16);
        }
    }
}
