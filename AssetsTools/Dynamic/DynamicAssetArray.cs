using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools.Dynamic {
    /// <summary>
    /// Array of DynamicAsset.
    /// </summary>
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

        /// <summary>
        /// Gets the element of this array.
        /// </summary>
        /// <param name="index">Index of the element to operate.</param>
        /// <exception cref="ArrayTypeMismatchException">Given DynamicAsset type does not match the type of element.</exception>
        public IDynamicAssetBase this[int index] {
            get {
                return elems[index];
            }
            set {
                if (value.TypeName != proto_name)
                    throw new ArrayTypeMismatchException("The element type is `" + proto_name + "` but got `" + value.TypeName + "`");
                elems[index] = value;
            }
        }

        /// <summary>
        /// Gets the Prototype of this array.
        /// </summary>
        /// <returns>Prototype of this array.</returns>
        public DynamicAssetArray GetPrototype() {
            return new DynamicAssetArray(0, proto_name);
        }

        /// <summary>
        /// Gets the Prototype of element of this array.
        /// </summary>
        /// <returns>Prototype of element.</returns>
        public DynamicAsset GetElementPrototype() {
            return DynamicAsset.PrototypeDic[proto_name];
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <remarks>If the specified size is less than the current size, the overflown part will be lost.</remarks>
        /// <param name="length">New size of the array.</param>
        public void Resize(int length) {
            DynamicAsset[] newarr = new DynamicAsset[length];
            Array.Copy(elems, newarr, length > elems.Length ? length : elems.Length);
            elems = newarr;
        }
    }
}
