using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace IOLibGen {
    class Program {
        static void Main(string[] args) {
            AssemblyHelper assem = new AssemblyHelper("IOLib");
            assem.CreateClass("UnityBinaryReader", UnityBinaryReaderBuilder.Builder);
            assem.Save();
        }
    }
}
