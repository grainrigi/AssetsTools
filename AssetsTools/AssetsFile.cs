using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    /// <summary>
    /// Unity3d aggregated assets file.
    /// </summary>
    public partial class AssetsFile : ISerializable {
        /// <summary>
        /// Header of this file.
        /// </summary>
        public HeaderType Header;

        /// <summary>
        /// Header of metadata.
        /// </summary>
        public MetadataHeaderType MetadataHeader;
        /// <summary>
        /// Types used in this file.
        /// </summary>
        public SerializedType[] Types;
        /// <summary>
        /// Objects contained in this file.
        /// </summary>
        public ObjectType[] Objects;
        /// <summary>
        /// Script Entries of this file.
        /// </summary>
        public ScriptIdentifierType[] Scripts;
        /// <summary>
        /// External file entries of this file.
        /// </summary>
        public ExternalFileType[] Externals;
        /// <summary>
        /// Userinformation of this file.
        /// </summary>
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
