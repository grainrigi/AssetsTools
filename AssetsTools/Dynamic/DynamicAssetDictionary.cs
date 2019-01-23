using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools.Dynamic {
    /// <summary>
    /// Dictionary contained in DynamicAsset.
    /// </summary>
    /// <typeparam name="TKey">Type of Key.</typeparam>
    /// <typeparam name="TValue">Type of Value.</typeparam>
    public class DynamicAssetDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDynamicAssetBase {
        private string key_proto_name;
        private string value_proto_name;

        public string TypeName => "Dictionary<" + key_proto_name + "," + value_proto_name + ">";

        public DynamicAssetDictionary() : base() {
            key_proto_name = typeof(TKey).Name;
            value_proto_name = typeof(TValue).Name;
        }

        public DynamicAssetDictionary(int capacity) : base(capacity) {
            key_proto_name = typeof(TKey).Name;
            value_proto_name = typeof(TValue).Name;
        }

        public DynamicAssetDictionary(Dictionary<TKey, TValue> dic) : base(dic) {
            key_proto_name = typeof(TKey).Name;
            value_proto_name = typeof(TValue).Name;
        }


#if DEBUG
        public
#else
        internal
#endif
            DynamicAssetDictionary(int count, string keytype, string valuetype) {
            key_proto_name = keytype;
            value_proto_name = valuetype;
        }

        public DynamicAsset GetKeyPrototype() {
            return DynamicAsset.PrototypeDic[key_proto_name];
        }

        public DynamicAsset GetValuePrototype() {
            return DynamicAsset.PrototypeDic[value_proto_name];
        }

        public DynamicAssetDictionary<TKey, TValue> GetPrototype() {
            return new DynamicAssetDictionary<TKey, TValue>(0, key_proto_name, value_proto_name);
        }
    }
}
