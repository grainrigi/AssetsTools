using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public class DynamicAssetArray : DynamicAsset {
        public DynamicAsset[] elems;
        string proto_name;
        int ptr;

        public DynamicAssetArray(int count, string protoname) {
            ptr = 0;
            elems = new DynamicAsset[count];
            proto_name = protoname;
        }

        public bool Add(Dictionary<string, object> dic) {
            if (elems.Length <= ptr)
                return false;
            elems[ptr] = new DynamicAsset(dic);
            return true;
        }

        public DynamicAsset this[int index] => elems[index];
    }
}
