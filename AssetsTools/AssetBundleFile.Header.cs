using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools
{
    public partial class AssetBundleFile
    {
        public struct HeaderType : ISerializable {
            public string signature;
            public const int FORMAT = 6;
            public string versionPlayer;
            public string versionEngine;

            public long bundleSize;
            public int compressedSize;
            public int uncompressedSize;
            public int flag;


            public void Read(UnityBinaryReader reader) {
                // Only Supports UnityFS
                signature = reader.ReadStringToNull();
                if (signature != "UnityFS")
                    throw new UnknownFormatException("Signature " + signature + " is not supported");

                // Only Supports format=6
                var readformat = reader.ReadInt();
                if (readformat != FORMAT)
                    throw new UnknownFormatException("Format " + readformat.ToString() + " is not supported");

                versionPlayer = reader.ReadStringToNull();
                versionEngine = reader.ReadStringToNull();

                // Read Header6
                bundleSize = reader.ReadLong();
                compressedSize = reader.ReadInt();
                uncompressedSize = reader.ReadInt();
                flag = reader.ReadInt();
            }

            public void Write(UnityBinaryWriter writer) {
                var utf8 = Encoding.UTF8;
                writer.WriteStringToNull("UnityFS");
                writer.WriteInt(FORMAT);
                writer.WriteStringToNull(versionPlayer);
                writer.WriteStringToNull(versionEngine);

                // Writer Header6
                writer.WriteLong(bundleSize);
                writer.WriteInt(compressedSize);
                writer.WriteInt(uncompressedSize);
                writer.WriteInt(flag);
            }
        }
    }
}
