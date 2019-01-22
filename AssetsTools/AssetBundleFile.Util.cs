using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools {
    public partial class AssetBundleFile {
        public static AssetBundleFile LoadFromFile(string filename) {
            AssetBundleFile bundle = new AssetBundleFile();
            bundle.Read(new UnityBinaryReader(filename));
            return bundle;
        }

        public static AssetBundleFile LoadFromMemory(byte[] bin) {
            AssetBundleFile bundle = new AssetBundleFile();
            bundle.Read(new UnityBinaryReader(bin));
            return bundle;
        }
    }
}
