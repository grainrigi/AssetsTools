using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    /// <summary>
    /// Base Class(Interface) of DynamicAsset.
    /// </summary>
    public interface IDynamicAssetBase {
        /// <summary>
        /// TypeName(Path) of the object.
        /// </summary>
        string TypeName { get; }
    }
}
