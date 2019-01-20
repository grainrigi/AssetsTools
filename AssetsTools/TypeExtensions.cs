using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    internal static class TypeExtensions {
        public static string GetCSharpName(this Type type) {
            string name;
            if (PrimitiveNames.TryGetValue(type, out name))
                return name;
            if (type.GenericTypeArguments.Length > 0) {
                string genericparam = type.GenericTypeArguments[0].GetCSharpName();
                for (int i = 1; i < type.GenericTypeArguments.Length; i++)
                    genericparam += ", " + type.GenericTypeArguments[i].GetCSharpName();

                return type.Name.Substring(0, type.Name.LastIndexOf('`')) + "<" + genericparam + ">";
            }
            else
                return type.Name;
        }

        private static Dictionary<Type, string> PrimitiveNames = new Dictionary<Type, string> {
            { typeof(sbyte), "sbyte" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(bool), "bool" },
            { typeof(string), "string" },
        };
    }
}
