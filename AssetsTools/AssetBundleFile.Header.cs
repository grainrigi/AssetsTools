using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools
{
    public partial class AssetBundleFile
    {
        public const int FORMAT = 6;

        public struct HeaderType : ISerializable {
            public string signature;
            
            public int format;
            public string versionPlayer;
            public string versionEngine;

            public long bundleSize;

            public long CalcSize() {
                return signature.Length + 1
                    + 4 //format
                    + versionPlayer.Length + 1
                    + versionEngine.Length + 1
                    + 8; // bundleSize
            }

            public void Read(UnityBinaryReader reader) {
                signature = reader.ReadStringToNull();
                format = reader.ReadIntBE();

                versionPlayer = reader.ReadStringToNull();
                versionEngine = reader.ReadStringToNull();
                bundleSize = reader.ReadLongBE();
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteStringToNull(signature);
                writer.WriteIntBE(format);

                writer.WriteStringToNull(versionPlayer);
                writer.WriteStringToNull(versionEngine);
                writer.WriteLongBE(bundleSize);
            }
        }

        private void initHeader() {
            Header.signature = "UnityFS";
            Header.format = FORMAT;
            Header.versionPlayer = "5.x.x";
            Header.versionEngine = "2017.3.1f1";
            Header.bundleSize = 0;
        }
    }
}
