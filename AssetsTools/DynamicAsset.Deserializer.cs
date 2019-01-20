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

            private string GetFQN() {
                return types.Aggregate((a, b) => a + "." + b);
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

            private object GenReadObject(NodeTree node) {
                PushType(node.Type);

                // Init Prototype
                string FQN = GetFQN();
                Dictionary<string, object> protodic = new Dictionary<string, object>();


                // Init Dictionary
                il.Emit(OpCodes.Ldc_I4, (int)node.Children.Count);
                il.Emit(OpCodes.Newobj, DicStrObjCtor);

                var members = node.Children;

                for(int i = 0; i < members.Count; i++) {
                    string membername = PrettifyName(members[i].Name);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldstr, PrettifyName(membername));

                    protodic.Add(membername, GenReadUnknownType(members[i], requireBoxing: true));

                    il.Emit(OpCodes.Callvirt, DicStrObjAdd);
                }

                // asset = new DynamicAsset(Dic, protoname);
                il.Emit(OpCodes.Ldstr, FQN);
                il.Emit(OpCodes.Newobj, DynamicAssetCtor);

                PopType();

                // Generate Prototype
                var proto = new DynamicAsset(protodic, FQN);
                PrototypeDic[FQN] = proto;
                return proto;
            }

            private object GenReadUnknownType(NodeTree node, bool requireBoxing) {
                object proto;
                // Try Known Type
                if (TryGenKnownType(node, requireBoxing, out proto)) { }
                else if (node.Type == "TypelessData") {
                    // Assert node.Children[0].Type == "int" && node.Children[0].Name == "size"
                    // Assert node.Children[1].Type == "UInt8" && node.Children[1].Name == "data"
                    var readfunc = ReadValueArray.MakeGenericMethod(typeof(byte));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, readfunc);

                    proto = new byte[0];
                }
                // Map
                else if (node.Type == "map") {
                    proto = GenReadDic(node);
                }
                // Array
                else if (node.HasChildren && node.Children[0].Type == "Array") {
                    proto = GenReadArray(node.Children[0]);
                }
                else
                    proto = GenReadObject(node);

                if (node.IsAligned)
                    GenAlign();

                return proto;
            }

            private bool TryGenKnownType(NodeTree node, bool requireBoxing, out object prototype) {
                // Try Primitive Type
                Type type;

                if (PrimitiveTypeDic.TryGetValue(node.Type, out type)) {
                    // reader.Read<T>()
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, PrimitiveReaderDic[type]);
                    if (requireBoxing)
                        il.Emit(OpCodes.Box, type);

                    prototype = Activator.CreateInstance(type);

                    return true;
                }
                // Try String
                else if (node.Type == "string") {
                    GenReadString();

                    prototype = "";

                    return true;
                }
                else {
                    prototype = null;
                    return false;
                }
            }

            private void GenReadString() {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, ReadAlignedString);
            }
            
            private object GenReadArray(NodeTree node) {
                NodeTree elem = node.Children[1];
                object proto;

                // Try Primitive Type
                Type elemtype;
                if (PrimitiveTypeDic.TryGetValue(elem.Type, out elemtype)) {
                    // reader.ReadValueArray<T>()
                    var readfunc = ReadValueArray.MakeGenericMethod(elemtype);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, readfunc);

                    proto = Activator.CreateInstance(elemtype.MakeArrayType(), new object[] { 0 });
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

                    proto = new string[0];
                }
                else if (elem.Type == "map")
                    throw new NotImplementedException("Array of map is not supported");
                // Object
                else {
                    string FQN = GetFQN(elem.Type);

                    // vec = new DynamicAssetArray(reader.ReadInt(), protoname)
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, ReadInt);

                    il.Emit(OpCodes.Ldstr, FQN);

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

                            il.Emit(OpCodes.Stelem_Ref);
                        }
                    );

                    il.Emit(OpCodes.Pop);

                    proto = new DynamicAssetArray(0, FQN);
                }
                if (node.IsAligned)
                    GenAlign();

                return proto;
            }

            private object GenReadDic(NodeTree node) {
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

                string keyFQN = (keytype == typeof(object)) ? GetFQN(pair.Children[0].Type) : keytype.GetCSharpName();
                string valueFQN = (valuetype == typeof(object)) ? GetFQN(pair.Children[1].Type) : valuetype.GetCSharpName();

                // int cnt = reader.ReadInt();
                int cnt = AllocLocal(typeof(int));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, ReadInt);
                il.EmitStloc(cnt);

                // Create Dictionary<keytype, valuetype>
                Type dictype = typeof(DynamicAssetDictionary<,>).MakeGenericType(keytype, valuetype);
                MethodInfo add = dictype.GetMethod("Add", new Type[] { keytype, valuetype });
                il.EmitLdloc(cnt);
                il.Emit(OpCodes.Ldstr, keyFQN);
                il.Emit(OpCodes.Ldstr, valueFQN);
                il.Emit(OpCodes.Newobj, dictype.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string) }));

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

                // Make prototype
                return Activator.CreateInstance(dictype, new object[] { 0, keyFQN, valueFQN });
            }

            private void GenAlign() {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_4);
                il.Emit(OpCodes.Call, AlignReader);
            }
        }
    }
}
