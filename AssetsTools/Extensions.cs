using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public static class Extensions {
        /*** AssetBundleFile ***/
        
        public static AssetsFile ToAssetsFile(this AssetBundleFile.FileType file) {
            AssetsFile assets = new AssetsFile();
            assets.Read(new UnityBinaryReader(file.Data));
            return assets;
        }

        public static void LoadAssetsFile(this AssetBundleFile.FileType file, AssetsFile assets) {
            UnityBinaryWriter w = new UnityBinaryWriter();
            assets.Write(w);
            file.Data = w.ToBytes();
        }

        /*** AssetsFile ***/

        public static DynamicAsset ToDynamicAsset(this AssetsFile.ObjectType obj) {
            AssetsFile parent;
            if (!obj.parent.TryGetTarget(out parent))
                throw new NullReferenceException("parent");
            return (DynamicAsset.GetDeserializer(parent.Types[obj.TypeID]))(new UnityBinaryReader(obj.Data));
        }

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
