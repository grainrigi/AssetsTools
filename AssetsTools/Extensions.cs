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

        /*** AssetsFile ***/

        /// <summary>
        /// Convert the object into DynamicAsset.
        /// </summary>
        /// <returns>Converted DynamicAsset.</returns>
        public static DynamicAsset ToDynamicAsset(this AssetsFile.ObjectType obj) {
            AssetsFile parent;
            if (!obj.parent.TryGetTarget(out parent))
                throw new NullReferenceException("parent");
            return (DynamicAsset.GetDeserializer(parent.Types[obj.TypeID]))(new UnityBinaryReader(obj.Data));
        }

        /// <summary>
        /// Update the content with the specified DynamicAsset.
        /// </summary>
        /// <param name="asset">DynamicAsset to load.</param>
        public static void LoadDynamicAsset(this AssetsFile.ObjectType obj, DynamicAsset asset) {
            AssetsFile parent;
            if (!obj.parent.TryGetTarget(out parent))
                throw new NullReferenceException("parent");
            UnityBinaryWriter w = new UnityBinaryWriter();
            DynamicAsset.GetSerializer(parent.Types[obj.TypeID])(w, asset);
            obj.Data = w.ToBytes();
        }
    }
}
