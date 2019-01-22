using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    /// <summary>
    /// Unity3d AssetBundleFile.
    /// </summary>
    public partial class AssetBundleFile : ISerializable {
        /// <summary>
        /// Header of this AssetBundle.
        /// </summary>
        public HeaderType Header;
        /// <summary>
        /// Files contained in this AssetBundle.
        /// </summary>
        public FileType[] Files;
        /// <summary>
        /// Whether compression is enabled.
        /// </summary>
        /// <remarks>The default value is 'false' whether the original file is compressed or not.</remarks>
        public bool EnableCompression = false;

        /// <summary>
        /// Read AssetBundle from binary using UnityBinaryReader.
        /// </summary>
        /// <remarks>Start of AssetBundle must be on the start Position of the reader.</remarks>
        /// <exception cref="UnknownFormatException">Format of given AssetBundle is not supported.</exception>
        /// <param name="reader">UnityBinaryReader to read from.</param>
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

        /// <summary>
        /// Write AssetBundle using UnityBinaryWriter.
        /// </summary>
        /// <remarks>Compression is disabled by default. To enable, set 'EnableCompression' to true.</remarks>
        /// <param name="writer">UnityBinaryWriter to write to.</param>
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
