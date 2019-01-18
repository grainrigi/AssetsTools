using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace AssetsTools {
    public partial class DynamicAsset : DynamicObject {

        private Dictionary<string, object> objects;

        internal DynamicAsset() {
            objects = new Dictionary<string, object>();
        }

        internal DynamicAsset(Dictionary<string, object> dic) {
            objects = dic;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return objects.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            if (!objects.ContainsKey(binder.Name) || objects[binder.Name].GetType() != value.GetType())
                return false;
            objects[binder.Name] = value;
            return true;
        }
    }
}
