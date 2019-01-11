using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {

    public partial class AssetBundleFile : ISerializable {
        public HeaderType Header;
        public FileType[] Files;
        public bool EnableCompression = false;

        public void Read(UnityBinaryReader reader) {
            Header.Read(reader);

            // Only supports UnityFS
            if (Header.signature != "UnityFS")
                throw new UnknownFormatException("Signature " + Header.signature + " is not supported");
            // Only supports format6
            if (Header.format != FORMAT)
                throw new UnknownFormatException("Format " + Header.format.ToString() + " is not supported");

            readFiles(reader);
        }

        public void Write(UnityBinaryWriter writer) {
            // Write files before header since filesize is unknown
            int org = writer.Position;
            writer.Position += (int)Header.CalcSize();
            writeFiles(writer);

            // Get the filesize and write header
            Header.bundleSize = writer.Length;
            writer.Position = org;
            Header.Write(writer);
        }
    }
}
