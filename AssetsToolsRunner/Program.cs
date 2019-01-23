using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetsTools;
using AssetsTools.Dynamic;
using System.IO;

namespace AssetsToolsRunner {
    class Program {
        static void Main(string[] args) {
            var currentDir = new DirectoryInfo(@".");
            var files = currentDir.GetFiles("*.unity3d");

            foreach (var file in files) {
                AssetBundleFile bundle = new AssetBundleFile();
                bundle.Read(new UnityBinaryReader(file.FullName));

                Console.WriteLine("Loading " + file.Name);

                AssetsFile assets = new AssetsFile();
                assets.Read(new UnityBinaryReader(bundle.Files[0].Data));

                for (int typeid = 0; typeid < assets.Types.Length; typeid++) {
                    if (assets.Types[typeid].ClassID == (int)ClassIDType.AssetBundle)
                        continue;

                    var des = DynamicAsset.GetDeserializer(assets.Types[typeid]);
                    var ser = DynamicAsset.GetSerializer(assets.Types[typeid]);

                    Console.WriteLine("Checking " + assets.Types[typeid].TypeTree.Nodes[0].Type);

                    foreach (var obj in assets.Objects.Where(obj => obj.TypeID == typeid)) {
                        byte[] org = obj.Data;
                        var asset = des(new UnityBinaryReader(org));
                        UnityBinaryWriter w = new UnityBinaryWriter();
                        ser(w, asset);
                        byte[] result = w.ToBytes();

                        for (int i = 0; i < result.Length; i++) {
                            if (result[i] != org[i])
                                throw new Exception();
                        }
                        string name = asset.HasMember("m_Name") ? asset.AsDynamic().m_Name : "(unnamed asset)";
                        Console.WriteLine(name + " Passed for check (" + result.Length + "bytes)");
                    }
                }
            }

            Console.WriteLine("Check has done.");
            System.Console.ReadLine();
        }
    }
}
