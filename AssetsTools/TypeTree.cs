using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class TypeTree : ISerializable {
        public Node[] Nodes;

        public struct Node {
            public ushort Version;
            public byte Level;
            public bool IsArray;
            public string Type;
            public string Name;
            public int ByteSize; // -1 for variable length
            public int Index;
            public int MetaFlag;
        }

        public void Read(UnityBinaryReader reader) {
            // Read header
            int node_count = reader.ReadInt();
            int strtable_length = reader.ReadInt();

            // StringTable is at just after nodes
            UnityBinaryReader strtableReader = reader.Slice(reader.Position + 24 * node_count);

            // Read Nodes
            Nodes = new Node[node_count];
            for(int i = 0; i < node_count; i++) {
                Nodes[i].Version = reader.ReadUShort();
                Nodes[i].Level = reader.ReadByte();
                Nodes[i].IsArray = reader.ReadByte() != 0;

                // Read TypeName
                var TypeStringOffset = reader.ReadUShort();
                var temp = reader.ReadUShort();
                if (temp == 0) {
                    strtableReader.Position = TypeStringOffset;
                    Nodes[i].Type = strtableReader.ReadStringToNull();
                }
                else
                    Nodes[i].Type = GetCommonString(TypeStringOffset);

                // Read Name
                var NameStringOffset = reader.ReadUShort();
                temp = reader.ReadUShort();
                if (temp == 0) {
                    strtableReader.Position = NameStringOffset;
                    Nodes[i].Name = strtableReader.ReadStringToNull();
                }
                else
                    Nodes[i].Name = GetCommonString(NameStringOffset);

                Nodes[i].ByteSize = reader.ReadInt();
                Nodes[i].Index = reader.ReadInt();
                Nodes[i].MetaFlag = reader.ReadInt();
            }

            reader.Position += strtable_length;
        }

        public void Write(UnityBinaryWriter writer) {
            // Skip header since strtable_length is unknown
            int header_position = writer.Position;
            writer.Position += 8;

            StringTableBuilder strtable = new StringTableBuilder();

            // Write Nodes
            for(int i = 0; i < Nodes.Length; i++) {
                writer.WriteUShort(Nodes[i].Version);
                writer.WriteByte(Nodes[i].Level);
                writer.WriteByte((byte)(Nodes[i].IsArray ? 1 : 0));

                // Write TypeName
                int TypeNameOffset = GetCommonStringID(Nodes[i].Type);
                if(TypeNameOffset == -1) { // Not a common string
                    writer.WriteUShort(strtable.AddString(Nodes[i].Type));
                    writer.WriteUShort(0);
                }
                else {
                    writer.WriteUShort((ushort)TypeNameOffset);
                    writer.WriteUShort(0x8000);
                }

                // Write Name
                int NameOffset = GetCommonStringID(Nodes[i].Name);
                if (NameOffset == -1) { // Not a common string
                    writer.WriteUShort(strtable.AddString(Nodes[i].Name));
                    writer.WriteUShort(0);
                }
                else {
                    writer.WriteUShort((ushort)NameOffset);
                    writer.WriteUShort(0x8000);
                }

                writer.WriteInt(Nodes[i].ByteSize);
                writer.WriteInt(Nodes[i].Index);
                writer.WriteInt(Nodes[i].MetaFlag);
            }

            // Write StringTable
            byte[] strtable_bytes = strtable.ToBytes();
            writer.WriteBytes(strtable_bytes);

            // Write node_count and strtable_length
            int final_pos = writer.Position;
            writer.Position = header_position;
            writer.WriteInt(Nodes.Length);
            writer.WriteInt(strtable_bytes.Length);
            writer.Position = final_pos;
        }

        private class StringTableBuilder {
            private UnityBinaryWriter _writer = new UnityBinaryWriter();
            private Dictionary<string, int> offset_table = new Dictionary<string, int>();

            /// <summary>
            /// Add string to the table.
            /// </summary>
            /// <param name="str">String to Add.</param>
            /// <returns>Offset of the string.</returns>
            public ushort AddString(string str) {
                if (offset_table.ContainsKey(str))
                    return (ushort)offset_table[str];
                else {
                    offset_table[str] = _writer.Position;
                    _writer.WriteStringToNull(str);
                    return (ushort)offset_table[str];
                }
            }

            public byte[] ToBytes() {
                return _writer.ToBytes();
            }
        }
    }
}
