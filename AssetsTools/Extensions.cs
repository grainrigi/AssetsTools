using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public static class Extensions {
        /*** AssetBundleFile ***/
        /// <summary>
        /// Convert this file into AssetsFile.
        /// </summary>
        /// <returns>Converted AssetsFile.</returns>
        public static AssetsFile ToAssetsFile(this AssetBundleFile.FileType file) {
            AssetsFile assets = new AssetsFile();
            assets.Read(new UnityBinaryReader(file.Data));
            return assets;
        }

        /// <summary>
        /// Update the content with the specified AssetsFile.
        /// </summary>
        /// <param name="assets">AssetsFile to load.</param>
        public static void LoadAssetsFile(this AssetBundleFile.FileType file, AssetsFile assets) {
            UnityBinaryWriter w = new UnityBinaryWriter();
            assets.Write(w);
            file.Data = w.ToBytes();
        }
    }
}
