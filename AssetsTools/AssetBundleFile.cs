using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {

    public class AssetBundleFile {
        public struct Header : ISerializable {
            public string signature;
            public const int format = 6;
            public string versionPlayer;
            public string versionEngine;

            long bundleSize;
            int compressedSize;
            int uncompressedSize;
            int flag;


            public void Read(EndianBinaryReader reader) {
                signature = reader.ReadStringToNull();
                if (signature != "UnityFS")
                    throw new UnknownFormatException("Signature " + signature + " is not supported");
                var readformat = reader.ReadInt32();
                if (readformat != 6)
                    throw new UnknownFormatException("Format " + readformat.ToString() + " is not supported");
                versionPlayer = reader.ReadStringToNull();
                versionEngine = reader.ReadStringToNull();

                // Read Header6
                bundleSize = reader.ReadInt64();
                compressedSize = reader.ReadInt32();
                uncompressedSize = reader.ReadInt32();
                flag = reader.ReadInt32();
            }

            public void Write(BinaryWriter writer) {
                var utf8 = Encoding.UTF8;
                writer.WriteUTF8StringZero("UnityFS");
                writer.Write(format);
                writer.WriteUTF8StringZero(versionPlayer);
                writer.WriteUTF8StringZero(versionEngine);

                // Writer Header6
                writer.Write(bundleSize);
            }
        }
    }
}
