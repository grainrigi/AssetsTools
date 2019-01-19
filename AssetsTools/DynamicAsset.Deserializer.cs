using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace AssetsTools {
    public partial class DynamicAsset {
        public static Func<UnityBinaryReader, DynamicAsset> GenDeserializer(TypeTree.Node[] nodes) {
            DynamicMethod method = new DynamicMethod(nodes[0].Type, typeof(DynamicAsset), new Type[] { typeof(UnityBinaryReader) }, m: typeof(DynamicAssetArray).Module, skipVisibility: true);

            DeserializerBuilder builder = new DeserializerBuilder(nodes);
            builder.Build(method.GetILGenerator());

            return (Func<UnityBinaryReader, DynamicAsset>)method.CreateDelegate(typeof(Func<UnityBinaryReader, DynamicAsset>));
        }

#if DEBUG
        private static AssemblyName _name;
        private static AssemblyBuilder _assembly = null;
        private static ModuleBuilder _module;
        public static TypeBuilder _type;

        private static void InitAssembly() {
            string name = "Deserializer";

            _name = new AssemblyName { Name = name };
            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
            _module = _assembly.DefineDynamicModule(name, name + ".dll", true);

            
            _type = _module.DefineType(name, System.Reflection.TypeAttributes.Public);
        }

        public static MethodBuilder GenDeserializerAssembly(TypeTree.Node[] nodes) {
            if (_assembly == null)
                InitAssembly();

            string name = nodes[0].Type;

            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(DynamicAsset), new Type[] { typeof(UnityBinaryReader) });

            DeserializerBuilder builder = new DeserializerBuilder(nodes);
            builder.Build(method.GetILGenerator());

            return method;
        }

        public static void SaveAssembly(string filename) {
            Type _des = _type.CreateType();
            _assembly.Save(filename);
        }
#endif
        private class DeserializerBuilder {
            ILGenerator il;
            NodeTree root;

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

            #region LocalManager
            Dictionary<Type, List<int>> local_table = new Dictionary<Type, List<int>>();
            byte local_count;
            int ret_local = -1;

            private int AllocLocal(Type type) {
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
                    if (local_count == 255)
                        throw new IndexOutOfRangeException("No more locals can be allocated");
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
            #endregion

            #region ProtoNameManager
            List<string> types = new List<string>(32);

            private void PushType(string name) {
                types.Add(name);
            }

            private void PopType() {
                types.RemoveAt(types.Count - 1);
            }

            private string GetFQN(string name) {
                return types.Aggregate((a, b) => a + "." + b) + "." + name;
            }
            #endregion

            public DeserializerBuilder(TypeTree.Node[] nodes) {
                this.il = null;
                root = NodeTree.FromNodes(nodes);
            }

            public void Build(ILGenerator il) {
                this.il = il;
                GenReadObject(root);
                il.Emit(OpCodes.Ret);
            }

            private void GenReadObject(NodeTree node) {
                PushType(node.Type);

                // Init Dictionary
                il.Emit(OpCodes.Ldc_I4, (int)node.Children.Count);
                il.Emit(OpCodes.Newobj, DicStrObjCtor);

                var members = node.Children;

                for(int i = 0; i < members.Count; i++) {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldstr, PrettifyName(members[i].Name));

                    GenReadUnknownType(members[i], requireBoxing: true);

                    il.Emit(OpCodes.Callvirt, DicStrObjAdd);
                }

                // asset = new DynamicAsset(Dic);
                il.Emit(OpCodes.Newobj, DynamicAssetCtor);

                PopType();
            }

            private void GenReadUnknownType(NodeTree node, bool requireBoxing) {
                // Try Known Type
                if (TryGenKnownType(node, requireBoxing)) { }
                else if (node.Type == "TypelessData") {
                    // Assert node.Children[0].Type == "int" && node.Children[0].Name == "size"
                    // Assert node.Children[1].Type == "UInt8" && node.Children[1].Name == "data"
                    var readfunc = ReadValueArray.MakeGenericMethod(typeof(byte));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, readfunc);
                }
                // Map
                else if (node.Type == "map") {
                    GenReadDic(node);
                }
                // Array
                else if (node.HasChildren && node.Children[0].Type == "Array") {
                    GenReadArray(node.Children[0]);
                }
                else
                    GenReadObject(node);

                if (node.IsAligned)
                    GenAlign();
            }

            private bool TryGenKnownType(NodeTree node, bool requireBoxing) {
                // Try Primitive Type
                Type type;

                if (PrimitiveTypeDic.TryGetValue(node.Type, out type)) {
                    // reader.Read<T>()
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, PrimitiveReaderDic[type]);
                    if (requireBoxing)
                        il.Emit(OpCodes.Box, type);
                    return true;
                }
                // Try String
                else if (node.Type == "string") {
                    GenReadString();
                    return true;
                }
                else
                    return false;
            }

            private void GenReadString() {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, ReadAlignedString);
            }
            
            private void GenReadArray(NodeTree node) {
                NodeTree elem = node.Children[1];

                // Try Primitive Type
                Type elemtype;
                if (PrimitiveTypeDic.TryGetValue(elem.Type, out elemtype)) {
                    // reader.ReadValueArray<T>()
                    var readfunc = ReadValueArray.MakeGenericMethod(elemtype);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, readfunc);
                }
                // Try String
                else if (elem.Type == "string") {
                    // var ary = new string[reader.ReadInt()];
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, ReadInt);
                    il.Emit(OpCodes.Newarr, typeof(string));

                    int i = AllocLocal(typeof(int));
                    // for(int i = 0; i < ary.Length; i++)
                    il.EmitFor(i,
                        cond: (cil) => {
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldlen);
                            il.Emit(OpCodes.Conv_I4);
                            il.EmitLdloc(i);
                            return OpCodes.Ble_S;
                        },
                        block: (cil) => {
                            // ary[i] = r.ReadAlignedString();
                            il.Emit(OpCodes.Dup);
                            il.EmitLdloc(i);
                            GenReadString();
                            il.Emit(OpCodes.Stelem, typeof(string));
                        }
                    );
                    ReleaseLocal(typeof(int));
                }
                else if (elem.Type == "map")
                    throw new NotImplementedException("Array of map is not supported");
                // Object
                else {
                    // vec = new DynamicAssetArray(reader.ReadInt(), protoname)
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, ReadInt);

                    il.Emit(OpCodes.Ldstr, GetFQN(elem.Type));

                    il.Emit(OpCodes.Newobj, DynamicAssetArrayCtor);

                    // ary = vec.elems;
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldfld, DynamicAssetArrayelems);

                    // for(int i = 0; i < ary.Length; i++) 
                    int i = AllocLocal(typeof(int));
                    il.EmitFor(i,
                        (cil) => {
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldlen);
                            il.Emit(OpCodes.Conv_I4);
                            il.EmitLdloc(i);
                            return OpCodes.Ble;
                        },
                        (cil) => {
                            // ary[i] = new DynamicAsset(GenReadUnkownType()); 
                            il.Emit(OpCodes.Dup);
                            il.EmitLdloc(i);

                            // Fallback
                            GenReadUnknownType(elem, requireBoxing: false);

                            il.Emit(OpCodes.Stelem, typeof(DynamicAsset));
                        }
                    );

                    il.Emit(OpCodes.Pop);
                }
                if (node.IsAligned)
                    GenAlign();
            }

            private void GenReadDic(NodeTree node) {
                Type keytype, valuetype;

                NodeTree pair = node.Children[0].Children[1];

                // Determine Key/Value Type
                if (PrimitiveTypeDic.TryGetValue(pair.Children[0].Type, out keytype)) { }
                else if (pair.Children[0].Type == "string")
                    keytype = typeof(string);
                else
                    keytype = typeof(object);

                if (PrimitiveTypeDic.TryGetValue(pair.Children[1].Type, out valuetype)) { }
                else if (pair.Children[1].Type == "string")
                    valuetype = typeof(string);
                else
                    valuetype = typeof(object);

                // int cnt = reader.ReadInt();
                int cnt = AllocLocal(typeof(int));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, ReadInt);
                il.EmitStloc(cnt);

                // Create Dictionary<keytype, valuetype>
                Type dictype = typeof(Dictionary<,>).MakeGenericType(keytype, valuetype);
                MethodInfo add = dictype.GetMethod("Add", new Type[] { keytype, valuetype });
                il.EmitLdloc(cnt);
                il.Emit(OpCodes.Newobj, dictype.GetConstructor(new Type[] { typeof(int) }));

                // Read KeyValuePairs

                // for(int i = 0; i < cnt; i++)
                int i = AllocLocal(typeof(int));
                il.EmitFor(i,
                    (cil) => {
                        il.EmitLdloc(i);
                        il.EmitLdloc(cnt);
                        return OpCodes.Bge;
                    },
                    (cil) => {
                        // dic.Add(
                        il.Emit(OpCodes.Dup);

                        // Read Key
                        GenReadUnknownType(pair.Children[0], requireBoxing: false);

                        // Read Value
                        GenReadUnknownType(pair.Children[1], requireBoxing: false);

                        // Add KeyValuePair
                        il.Emit(OpCodes.Callvirt, add);
                    }
                );
                ReleaseLocal(typeof(int));
                ReleaseLocal(typeof(int));

                if (node.Children[0].IsAligned) {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_4);
                    il.Emit(OpCodes.Call, AlignReader);
                }
            }

            private void GenAlign() {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_4);
                il.Emit(OpCodes.Call, AlignReader);
            }
        }
    }
}
