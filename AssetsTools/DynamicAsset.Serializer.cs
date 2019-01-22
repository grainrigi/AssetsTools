using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace AssetsTools {
    public partial class DynamicAsset {
        public static Action<UnityBinaryWriter, DynamicAsset> GenSerializer(TypeTree.Node[] nodes) {
            DynamicMethod method = new DynamicMethod(nodes[0].Type, null, new Type[] { typeof(UnityBinaryWriter), typeof(DynamicAsset) }, m: typeof(DynamicAssetArray).Module, skipVisibility: true);

            SerializerBuilder builder = new SerializerBuilder(nodes);
            builder.Build(method.GetILGenerator());

            return (Action<UnityBinaryWriter, DynamicAsset>)method.CreateDelegate(typeof(Action<UnityBinaryWriter, DynamicAsset>));
        }

#if DEBUG && NET45
        private static AssemblyName _sername;
        private static AssemblyBuilder _serassembly = null;
        private static ModuleBuilder _sermodule;
        public static TypeBuilder _sertype;

        private static void InitSerAssembly() {
            string name = "Serializer";

            _sername = new AssemblyName { Name = name };
            _serassembly = AppDomain.CurrentDomain.DefineDynamicAssembly(_sername, AssemblyBuilderAccess.RunAndSave);
            _sermodule = _serassembly.DefineDynamicModule(name, name + ".dll", true);


            _sertype = _sermodule.DefineType(name, System.Reflection.TypeAttributes.Public);
        }

        public static MethodBuilder GenSerializerAssembly(TypeTree.Node[] nodes) {
            if (_serassembly == null)
                InitSerAssembly();

            string name = nodes[0].Type;

            MethodBuilder method = _sertype.DefineMethod(
                name,
                MethodAttributes.Public | MethodAttributes.Static,
                null, new Type[] { typeof(UnityBinaryWriter), typeof(DynamicAsset) });

            SerializerBuilder builder = new SerializerBuilder(nodes);
            builder.Build(method.GetILGenerator());

            return method;
        }

        public static void SaveSerAssembly(string filename) {
            Type _ser = _sertype.CreateType();
            _serassembly.Save(filename);
        }
#endif


        private class SerializerBuilder {
            ILGenerator il;
            NodeTree root;

            LocalManager locman;
            ProtoNameManager protoman;


            public SerializerBuilder(TypeTree.Node[] nodes) {
                this.il = null;
                root = NodeTree.FromNodes(nodes);
            }

            public void Build(ILGenerator il) {
                this.il = il;
                this.locman = new LocalManager(il);
                this.protoman = new ProtoNameManager();

                // Load root
                il.Emit(OpCodes.Ldarg_1);

                GenWriteObject(root);
                il.Emit(OpCodes.Ret);
            }

            private void GenWriteObject(NodeTree node) {
                var members = node.Children;

                int obj = locman.AllocLocal(DicStrObjType);
                il.Emit(OpCodes.Castclass, typeof(DynamicAsset));
                il.Emit(OpCodes.Ldfld, DynamicAsset_objects);
                il.EmitStloc(obj);

                for (int i = 0; i < members.Count; i++) {
                    string membername = PrettifyName(members[i].Name);

                    GenWriteUnknownType(members[i], requireUnboxing: true, loader: (cil) => {
                        cil.EmitLdloc(obj);
                        cil.Emit(OpCodes.Ldstr, membername);
                        cil.Emit(OpCodes.Callvirt, DicStrObjGetter);
                    });
                }
            }

            private void GenWriteUnknownType(NodeTree node, bool requireUnboxing, Action<ILGenerator> loader) {
                // Try Known Type
                if (TryGenKnownType(node, requireUnboxing, loader)) { }
                else if (node.Type == "TypelessData") {
                    // Assert node.Children[0].Type == "int" && node.Children[0].Name == "size"
                    // Assert node.Children[1].Type == "UInt8" && node.Children[1].Name == "data"
                    var writefunc = WriteValueArray.MakeGenericMethod(typeof(byte));
                    il.Emit(OpCodes.Ldarg_0);
                    loader(il);
                    il.Emit(OpCodes.Castclass, typeof(byte).MakeArrayType());
                    il.Emit(OpCodes.Call, writefunc);
                }
                // Map
                else if (node.Type == "map") {
                    GenWriteDic(node, loader);
                }
                // Array
                else if (node.HasChildren && node.Children[0].Type == "Array") {
                    GenWriteArray(node.Children[0], loader);
                }
                else {
                    loader(il);
                    GenWriteObject(node);
                }

                if (node.IsAligned)
                    GenAlign();
            }

            private bool TryGenKnownType(NodeTree node, bool requireUnboxing, Action<ILGenerator> loader) {
                // Try Primitive Type
                Type type;

                if (PrimitiveTypeDic.TryGetValue(node.Type, out type)) {
                    // writer.Write<T>((type)value)
                    il.Emit(OpCodes.Ldarg_0);
                    loader(il);
                    if (requireUnboxing)
                        il.Emit(OpCodes.Unbox_Any, type);
                    il.Emit(OpCodes.Call, PrimitiveWriterDic[type]);

                    return true;
                }
                // Try String
                else if (node.Type == "string") {
                    il.Emit(OpCodes.Ldarg_0);
                    loader(il);
                    if (requireUnboxing)
                        il.Emit(OpCodes.Castclass, typeof(string));
                    il.Emit(OpCodes.Call, WriteAlignedString);

                    return true;
                }
                else {
                    return false;
                }
            }

            private void GenWriteArray(NodeTree node, Action<ILGenerator> loader) {
                NodeTree elem = node.Children[1];

                // Try Primitive Type
                Type elemtype;
                if (PrimitiveTypeDic.TryGetValue(elem.Type, out elemtype)) {
                    // reader.ReadValueArray<T>()
                    var writefunc = WriteValueArray.MakeGenericMethod(elemtype);
                    il.Emit(OpCodes.Ldarg_0);
                    loader(il);
                    il.Emit(OpCodes.Castclass, elemtype.MakeArrayType());
                    il.Emit(OpCodes.Call, writefunc);
                }
                // Try String
                else if (elem.Type == "string") {
                    Type arytype = typeof(string).MakeArrayType();
                    int ary = locman.AllocLocal(arytype);
                    loader(il);
                    il.Emit(OpCodes.Castclass, arytype);
                    il.EmitStloc(ary);

                    // Write length
                    // writer.WriteInt(ary.Length);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitLdloc(ary);
                    il.Emit(OpCodes.Ldlen);
                    il.Emit(OpCodes.Conv_I4);
                    il.Emit(OpCodes.Call, WriteInt);

                    int i = locman.AllocLocal(typeof(int));
                    // for(int i = 0; i < ary.Length; i++)
                    il.EmitFor(i,
                        cond: (cil) => {
                            il.EmitLdloc(ary);
                            il.Emit(OpCodes.Ldlen);
                            il.Emit(OpCodes.Conv_I4);
                            il.EmitLdloc(i);
                            return OpCodes.Ble_S;
                        },
                        block: (cil) => {
                            // w.WriteAlignedString(ary[i]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.EmitLdloc(ary);
                            il.EmitLdloc(i);
                            il.Emit(OpCodes.Ldelem_Ref);
                            il.Emit(OpCodes.Call, WriteAlignedString);
                        }
                    );
                    locman.ReleaseLocal(typeof(int));
                    locman.ReleaseLocal(arytype);
                }
                else if (elem.Type == "map")
                    throw new NotImplementedException("Array of map is not supported");
                // Object
                else {
                    // Load DynamicAssetArray.elems
                    Type arytype = typeof(IDynamicAssetBase).MakeArrayType();
                    int ary = locman.AllocLocal(arytype);
                    loader(il);
                    il.Emit(OpCodes.Castclass, typeof(DynamicAssetArray));
                    il.Emit(OpCodes.Ldfld, DynamicAssetArrayelems);
                    il.EmitStloc(ary);

                    // Write length
                    // writer.WriteInt(ary.Length);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitLdloc(ary);
                    il.Emit(OpCodes.Ldlen);
                    il.Emit(OpCodes.Conv_I4);
                    il.Emit(OpCodes.Call, WriteInt);

                    // for(int i = 0; i < ary.Length; i++) 
                    int i = locman.AllocLocal(typeof(int));
                    il.EmitFor(i,
                        (cil) => {
                            il.EmitLdloc(ary);
                            il.Emit(OpCodes.Ldlen);
                            il.Emit(OpCodes.Conv_I4);
                            il.EmitLdloc(i);
                            return OpCodes.Ble;
                        },
                        (cil) => {
                            // GenWriteUnknownType(ary[i]);
                            GenWriteUnknownType(elem, requireUnboxing: false, loader: (ccil) => {
                                ccil.EmitLdloc(ary);
                                ccil.EmitLdloc(i);
                                ccil.Emit(OpCodes.Ldelem_Ref);
                            });
                        }
                    );

                    locman.ReleaseLocal(typeof(int));
                    locman.ReleaseLocal(arytype);
                }
                if (node.IsAligned)
                    GenAlign();
            }

            private void GenWriteDic(NodeTree node, Action<ILGenerator> loader) {
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

                // Load Dictionary<keytype, valuetype>
                Type dictype = typeof(DynamicAssetDictionary<,>).MakeGenericType(keytype, valuetype);
                MethodInfo getter = dictype.GetMethod("Item", new Type[] { keytype, valuetype });
                MethodInfo Count = dictype.GetProperty("Count").GetMethod;
                int dic = locman.AllocLocal(dictype);

                loader(il);
                il.Emit(OpCodes.Castclass, dictype);
                il.EmitStloc(dic);

                // Write Count
                il.Emit(OpCodes.Ldarg_0);
                il.EmitLdloc(dic);
                il.Emit(OpCodes.Callvirt, Count);
                il.Emit(OpCodes.Call, WriteInt);

                // Write KeyValuePairs

                // foreach(var kv in dic)
                Type enumtype = typeof(Dictionary<,>.Enumerator).MakeGenericType(keytype, valuetype);
                int enumerator = locman.AllocLocal(enumtype);
                Type kvtype = typeof(KeyValuePair<,>).MakeGenericType(keytype, valuetype);
                int kv = locman.AllocLocal(kvtype);
                MethodInfo getenumerator = dictype.GetMethod("GetEnumerator");
                MethodInfo movenext = enumtype.GetMethod("MoveNext");
                MethodInfo getcurrent = enumtype.GetProperty("Current").GetMethod;
                MethodInfo getkey = kvtype.GetProperty("Key").GetMethod;
                MethodInfo getvalue = kvtype.GetProperty("Value").GetMethod;
                

                var l_foreachstart = il.DefineLabel();
                var l_movenext = il.DefineLabel();

                // Get Enumerator
                loader(il);
                il.Emit(OpCodes.Castclass, dictype);
                il.Emit(OpCodes.Callvirt, getenumerator);
                il.EmitStloc(enumerator);
                il.BeginExceptionBlock();
                il.Emit(OpCodes.Br, l_movenext);

                // Main
                il.MarkLabel(l_foreachstart);
                il.EmitLdloca(enumerator);
                il.Emit(OpCodes.Call, getcurrent);
                il.EmitStloc(kv);

                // Key
                GenWriteUnknownType(pair.Children[0], requireUnboxing: false, loader: (cil) => {
                    cil.EmitLdloca(kv);
                    cil.Emit(OpCodes.Call, getkey);
                });

                // Value
                GenWriteUnknownType(pair.Children[1], requireUnboxing: false, loader: (cil) => {
                    cil.EmitLdloca(kv);
                    cil.Emit(OpCodes.Call, getvalue);
                });

                // MoveNext
                il.MarkLabel(l_movenext);
                il.EmitLdloca(enumerator);
                il.Emit(OpCodes.Call, movenext);
                il.Emit(OpCodes.Brtrue, l_foreachstart);

                // Finally(Dispose)
                il.BeginFinallyBlock();
                il.EmitLdloca(enumerator);
                il.Emit(OpCodes.Constrained, enumtype);
                il.Emit(OpCodes.Callvirt, Dispose);
                il.EndExceptionBlock();


                if (node.Children[0].IsAligned)
                    GenAlign();

                locman.ReleaseLocal(enumtype);
                locman.ReleaseLocal(kvtype);
                locman.ReleaseLocal(dictype);
            }

            private void GenAlign() {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_4);
                il.Emit(OpCodes.Call, AlignWriter);
            }
        }
    }
    
}
