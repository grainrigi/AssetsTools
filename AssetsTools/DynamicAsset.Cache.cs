using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class DynamicAsset {
        internal static Dictionary<string, DynamicAsset> PrototypeDic = new Dictionary<string, DynamicAsset>();

        private static Dictionary<int, Func<UnityBinaryReader, DynamicAsset>> _deserializerCache = new Dictionary<int, Func<UnityBinaryReader, DynamicAsset>>();
        internal static Func<UnityBinaryReader, DynamicAsset> GetDeserializer(SerializedType type) {
            if (_deserializerCache.TryGetValue(type.ClassID, out var func))
                return func;
            else {
                var des = GenDeserializer(type.TypeTree.Nodes);
                _deserializerCache.Add(type.ClassID, des);
                return des;
            }
        }

        private static Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>> _serializerCache = new Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>>();
        internal static Action<UnityBinaryWriter, DynamicAsset> GetSerializer(SerializedType type) {
            if (_serializerCache.TryGetValue(type.ClassID, out var func))
                return func;
            else {
                var ser = GenSerializer(type.TypeTree.Nodes);
                _serializerCache.Add(type.ClassID, ser);
                return ser;
            }
        }
    }
}
