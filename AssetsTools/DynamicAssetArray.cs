using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public class DynamicAssetArray : IDynamicAssetBase {
        private string proto_name;

        public string TypeName => "Array<" + proto_name + ">";

#if DEBUG
        public
#else
        internal
#endif
            IDynamicAssetBase[] elems;

#if DEBUG
        public
#else
        internal
#endif
            DynamicAssetArray(int count, string protoname) {
            elems = new IDynamicAssetBase[count];
            proto_name = protoname;
        }

        public IDynamicAssetBase this[int index] {
            get {
                return elems[index];
            }
            set {
                if (value.TypeName != proto_name)
                    throw new ArrayTypeMismatchException();
                elems[index] = value;
            }
        }

        public DynamicAssetArray GetPrototype() {
            return new DynamicAssetArray(0, proto_name);
        }

        public DynamicAsset GetElementPrototype() {
            return DynamicAsset.PrototypeDic[proto_name];
        }

        public void Resize(int length) {
            DynamicAsset[] newarr = new DynamicAsset[length];
            Array.Copy(elems, newarr, length > elems.Length ? length : elems.Length);
            elems = newarr;
        }
    }
}
