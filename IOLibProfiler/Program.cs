using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IOLibProfiler {
    class Program {
        static void Main(string[] args) {
            byte[] src = new byte[10000000];

            UnityBinaryReader r = new UnityBinaryReader(src);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < 2500000; i++) {
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");

            sw.Restart();

            for(int i = 0; i < 2500000; i++) {
                r.ReadIntLE();
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");

            r = new UnityBinaryReader(src);

            sw.Restart();

            for (int i = 0; i < 2500000; i++) {
                r.ReadIntBE();
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");

            using(MemoryStream ms = new MemoryStream(src))
            using (BinaryReader br = new BinaryReader(ms)) {
                sw.Restart();

                for (int i = 0; i < 2500000; i++) {
                    br.ReadInt32();
                }

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            }

        }
    }
}
