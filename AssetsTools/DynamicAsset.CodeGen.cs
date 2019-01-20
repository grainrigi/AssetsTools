using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace AssetsTools {
    public partial class DynamicAsset {
        private static Type DicStrObjType = typeof(Dictionary<string, object>);
        private static ConstructorInfo DicStrObjCtor = typeof(Dictionary<string, object>).GetConstructor(new Type[] { typeof(int) });
        private static MethodInfo DicStrObjAdd = typeof(Dictionary<string, object>).GetMethod("Add", new Type[] { typeof(string), typeof(object) });

        private static ConstructorInfo DynamicAssetCtor = typeof(DynamicAsset).GetConstructor(BindingFlags.InvokeMethod |
#if DEBUG
            BindingFlags.Public
#else
            BindingFlags.NonPublic
#endif
            | BindingFlags.Instance, null, new Type[] { typeof(Dictionary<string, object>), typeof(string) }, null);

        private static ConstructorInfo DynamicAssetArrayCtor = typeof(DynamicAssetArray).GetConstructor(BindingFlags.InvokeMethod |
#if DEBUG
            BindingFlags.Public
#else
            BindingFlags.NonPublic
#endif
            | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(string) }, null);
        private static FieldInfo DynamicAssetArrayelems = typeof(DynamicAssetArray).GetField("elems", BindingFlags.InvokeMethod |
#if DEBUG
            BindingFlags.Public
#else
            BindingFlags.NonPublic
#endif
            | BindingFlags.Instance);

        private static MethodInfo ReadInt = typeof(UnityBinaryReader).GetMethod("ReadInt", Type.EmptyTypes);
        private static MethodInfo ReadString = typeof(UnityBinaryReader).GetMethod("ReadString", new Type[] { typeof(int) });
        private static MethodInfo ReadAlignedString = typeof(IOLibExtensions).GetMethod("ReadAlignedString");
        private static MethodInfo ReadValueArray = typeof(UnityBinaryReader).GetMethod("ReadValueArray", Type.EmptyTypes);
        private static MethodInfo AlignReader = typeof(IOLibExtensions).GetMethod("Align", new Type[] { typeof(UnityBinaryReader), typeof(int) });

        private static Dictionary<string, Type> PrimitiveTypeDic = new Dictionary<string, Type> {
            { "SInt8", typeof(sbyte) },
            { "UInt8", typeof(byte) },
            { "short", typeof(short) },
            { "SInt16", typeof(short) },
            { "UInt16", typeof(ushort) },
            { "unsigned short", typeof(ushort) },
            { "int", typeof(int) },
            { "SInt32", typeof(int) },
            { "UInt32", typeof(uint) },
            { "unsigned int", typeof(uint) },
            { "Type*", typeof(uint) },
            { "long long", typeof(long) },
            { "SInt64", typeof(long) },
            { "UInt64", typeof(ulong) },
            { "unsigned long long", typeof(long) },
            { "float", typeof(float) },
            { "double", typeof(double) },
            { "bool", typeof(bool) },
        };

        private static Dictionary<Type, MethodInfo> PrimitiveReaderDic = new Dictionary<Type, MethodInfo> {
            { typeof(sbyte), typeof(UnityBinaryReader).GetMethod("ReadSByte", Type.EmptyTypes) },
            { typeof(byte), typeof(UnityBinaryReader).GetMethod("ReadByte", Type.EmptyTypes) },
            { typeof(short), typeof(UnityBinaryReader).GetMethod("ReadShort", Type.EmptyTypes) },
            { typeof(ushort), typeof(UnityBinaryReader).GetMethod("ReadUShort", Type.EmptyTypes) },
            { typeof(int), typeof(UnityBinaryReader).GetMethod("ReadInt", Type.EmptyTypes) },
            { typeof(uint), typeof(UnityBinaryReader).GetMethod("ReadUInt", Type.EmptyTypes) },
            { typeof(long), typeof(UnityBinaryReader).GetMethod("ReadLong", Type.EmptyTypes) },
            { typeof(ulong), typeof(UnityBinaryReader).GetMethod("ReadULong", Type.EmptyTypes) },
            { typeof(bool), typeof(UnityBinaryReader).GetMethod("ReadBool", Type.EmptyTypes) },
            { typeof(float), typeof(UnityBinaryReader).GetMethod("ReadFloat", Type.EmptyTypes) },
            { typeof(double), typeof(UnityBinaryReader).GetMethod("ReadDouble", Type.EmptyTypes) },
        };

        private static Dictionary<string, Type> KnownTypeDic = new Dictionary<string, Type> {
        };

        private static string PrettifyName(string name) {
            return name.Replace(' ', '_').Replace("[", "").Replace("]","");
        }


        private struct NodeTree {
            public string Name;
            public string Type;
            public bool IsAligned;

            public List<NodeTree> Children;

            public bool HasChildren => Children != null;

            public static NodeTree FromNodes(TypeTree.Node[] nodes) {
                int i = 0;
                return readNodes(nodes, ref i);
            }

            private static NodeTree readNodes(TypeTree.Node[] nodes, ref int i) {
                NodeTree ret;
                ret.Name = nodes[i].Name;
                ret.Type = nodes[i].Type;
                ret.IsAligned = (nodes[i].MetaFlag & 0x4000) != 0;
                ret.Children = null;

                if (i < nodes.Length - 1) { // May have children
                    if (nodes[i].Level < nodes[i + 1].Level) { // Has children
                        var list = ret.Children = new List<NodeTree>();
                        i++;
                        var mindepth = nodes[i].Level;

                        while (i < nodes.Length && nodes[i].Level >= mindepth) {
                            list.Add(readNodes(nodes, ref i));
                        }
                    }
                    else // Leaf node
                        i++;
                }
                else // Leaf node
                    i++;

                return ret;
            }
        }

        private class LocalManager {
            ILGenerator il;

            Dictionary<Type, List<int>> local_table = new Dictionary<Type, List<int>>();
            byte local_count;
            int ret_local = -1;

            public LocalManager(ILGenerator il) {
                this.il = il;
            }

            public int AllocLocal(Type type) {
                List<int> list;
                if(!local_table.TryGetValue(type, out list)) {
                    list = new List<int>(16);
                    local_table[type] = list;
                    list.Add(0);
                }

                int usedCount = list[0];
                if(usedCount < list.Count - 1) {
                    list[0] = usedCount + 1;
                    return list[usedCount];
                }
                else {
                    il.DeclareLocal(type);
                    list.Add(local_count);
                    list[0] = usedCount + 1;
                    return local_count++;
                }
            }

            public void ReleaseLocal(Type type) {
                local_table[type][0]--;
            }

            public void ReturnLocal(Type type) {
                List<int> list = local_table[type];
                ret_local = list[list[0]];
                list[0]--;
            }

            public int GetRetLocal() {
                return ret_local;
            }
        }

        private class ProtoNameManager {
            List<string> types = new List<string>(32);

            public void PushType(string name) {
                types.Add(name);
            }

            public void PopType() {
                types.RemoveAt(types.Count - 1);
            }

            public string GetFQN(string name) {
                return types.Aggregate((a, b) => a + "." + b) + "." + name;
            }

            public string GetFQN() {
                return types.Aggregate((a, b) => a + "." + b);
            }
        }
    }
}
