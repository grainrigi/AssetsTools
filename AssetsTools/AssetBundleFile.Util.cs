using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools {
    public partial class AssetBundleFile {
        /// <summary>
        /// Load AssetBundle from file.
        /// </summary>
        /// <param name="filename">Filename of the AssetBundle file.</param>
        /// <returns>Loaded AssetBundle.</returns>
        public static AssetBundleFile LoadFromFile(string filename) {
            AssetBundleFile bundle = new AssetBundleFile();
            bundle.Read(new UnityBinaryReader(filename));
            return bundle;
        }

        /// <summary>
        /// Load AssetBundle from byte array.
        /// </summary>
        /// <param name="bin">Raw data of the AssetBundle.</param>
        /// <returns>Loaded AssetBundle.</returns>
        public static AssetBundleFile LoadFromMemory(byte[] bin) {
            AssetBundleFile bundle = new AssetBundleFile();
            bundle.Read(new UnityBinaryReader(bin));
            return bundle;
        }
    }
}
