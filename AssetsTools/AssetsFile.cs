using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetsFile : ISerializable {
        public HeaderType Header;

        public MetadataHeaderType MetadataHeader;
        public SerializedType[] Types;
        public ObjectType[] Objects;
        public ScriptIdentifierType[] Scripts;
        public ExternalFileType[] Externals;
        public string UserInformation;

        public void Read(UnityBinaryReader reader) {
            // Read Header
            Header.Read(reader);

            // Only Supports version = 17
            if (Header.Version != 17)
                throw new NotSupportedException("Version " + Header.Version.ToString() + " is not supported");
            // Only Supports LittleEndian
            if (Header.IsBigEndian)
                throw new NotSupportedException("BigEndian file is not supported");

            // Read Metadata
            readMetadata(reader);

            // Read Objects
            readObjects(reader);

            // Read Scripts
            readScripts(reader);

            // Read Externals
            readExternals(reader);

            // Read UserInformation
            UserInformation = reader.ReadStringToNull();
        }

        public void Write(UnityBinaryWriter writer) {
            // Skip Header since MetadataSize and DataOffset are unknown
            int header_pos = writer.Position;
            writer.Position += Header.CalcSize();

            // Write Metadata
            writeMetadata(writer);

            // Write Objects
            byte[] body = writeObjects(writer);

            // Write Scripts
            writeScripts(writer);

            // Write Externals
            writeExternals(writer);

            // Write UserInformation
            writer.WriteStringToNull(UserInformation);

            Header.MetadataSize = writer.Position - Header.CalcSize();

            // Align body
            if (writer.Position < 0x1000)
                writer.Position = 0x1000;
            else
                writer.Align(16);
            Header.DataOffset = writer.Position;

            // Write body
            writer.WriteBytes(body);

            // Write Header
            Header.FileSize = writer.Position;
            writer.Position = header_pos;
            Header.Write(writer);
        }
    }
}
