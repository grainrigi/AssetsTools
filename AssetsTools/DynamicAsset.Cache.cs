using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class DynamicAsset {
        internal static Dictionary<string, DynamicAsset> PrototypeDic = new Dictionary<string, DynamicAsset>();

        private static Dictionary<int, Func<UnityBinaryReader, DynamicAsset>> _deserializerCache = new Dictionary<int, Func<UnityBinaryReader, DynamicAsset>>();
        private static Dictionary<int, Func<UnityBinaryReader, DynamicAsset>> _monodeserializerCache = new Dictionary<int, Func<UnityBinaryReader, DynamicAsset>>();

        /// <summary>
        /// Get deserializer for the specified type.
        /// </summary>
        /// <param name="type">Type to deserialize.</param>
        /// <returns>Deserializer for the type.</returns>
        public static Func<UnityBinaryReader, DynamicAsset> GetDeserializer(SerializedType type) {
            var dic = _deserializerCache;
            int id = type.ClassID;
            if(type.ClassID == (int)ClassIDType.MonoBehaviour) {
                dic = _monodeserializerCache;
                id = GetHashOfMonoBehaviour(type.ScriptID);
            }

            if (dic.TryGetValue(id, out var func))
                return func;
            else {
                var des = GenDeserializer(type.TypeTree.Nodes);
                dic.Add(id, des);
                return des;
            }
        }

        private static Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>> _serializerCache = new Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>>();
        private static Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>> _monoserializerCache = new Dictionary<int, Action<UnityBinaryWriter, DynamicAsset>>();

        /// <summary>
        /// Get serializer for the specified type.
        /// </summary>
        /// <param name="type">Type to serialize.</param>
        /// <returns>Serializer for the type.</returns>
        public static Action<UnityBinaryWriter, DynamicAsset> GetSerializer(SerializedType type) {
            var dic = _serializerCache;
            int id = type.ClassID;
            if(type.ClassID == (int)ClassIDType.MonoBehaviour) {
                dic = _monoserializerCache;
                id = GetHashOfMonoBehaviour(type.ScriptID);
            }

            if (dic.TryGetValue(id, out var func))
                return func;
            else {
                var ser = GenSerializer(type.TypeTree.Nodes);
                dic.Add(id, ser);
                return ser;
            }
        }

        private static int GetHashOfMonoBehaviour(byte[] scriptID) {
            int hash = BitConverter.ToInt32(scriptID, 0);
            hash ^= BitConverter.ToInt32(scriptID, 4);
            hash ^= BitConverter.ToInt32(scriptID, 8);
            hash ^= BitConverter.ToInt32(scriptID, 12);
            return hash;
        }
    }
}
