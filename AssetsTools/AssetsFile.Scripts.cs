using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile {
        /// <summary>
        /// Script Identifier for AssetsFile.
        /// </summary>
        public struct ScriptIdentifierType : ISerializable {
            public int Index;
            public long Identifier;

            public void Read(UnityBinaryReader reader) {
                Index = reader.ReadInt();
                reader.Align(4);
                Identifier = reader.ReadLong();
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteInt(Index);
                writer.Align(4);
                writer.WriteLong(Identifier);
            }
        }

        // version >= 11
        private void readScripts(UnityBinaryReader reader) {
            int script_count = reader.ReadInt();
            Scripts = new ScriptIdentifierType[script_count];
            Scripts.Read(reader);
        }

        private void writeScripts(UnityBinaryWriter writer) {
            writer.WriteInt(Scripts.Length);
            Scripts.Write(writer);
        }
    }
}
