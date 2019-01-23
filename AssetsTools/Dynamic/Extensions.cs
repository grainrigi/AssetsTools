using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.Dynamic {
    public static class Extensions {
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

        public static IEnumerable<AssetsFile.ObjectType> ObjectsWithClass(this AssetsFile assets, ClassIDType id) {
            // Determine TypeID
            int typeid;
            for(typeid = 0; typeid < assets.Types.Length; typeid++) {
                if (assets.Types[typeid].ClassID == (int)id)
                    break;
            }

            if (typeid == assets.Types.Length)
                throw new ClassNotFoundException(id);

            foreach (var obj in assets.Objects)
                if (obj.TypeID == typeid)
                    yield return obj;
        }

        public class ClassNotFoundException : Exception {
            public ClassNotFoundException(ClassIDType id)
                : base($"Class {id.ToString()} is not available in this file.") { }
        }
    }
}
