using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetsTools;
using System.IO;

namespace AssetsToolsRunner {
    class Program {
        static void Main(string[] args) {
            UnityBinaryReader r = new UnityBinaryReader("test.unity3d");
            AssetBundleFile bundle = new AssetBundleFile();
            bundle.Read(r);
            UnityBinaryWriter w = new UnityBinaryWriter();
            bundle.EnableCompression = true;
            bundle.Write(w);
            byte[] raw = w.ToBytes();
            using (FileStream fs = new FileStream("test.unity3d.comp", FileMode.Create)) {
                fs.Write(raw, 0, raw.Length);
            }
        }
    }
}
