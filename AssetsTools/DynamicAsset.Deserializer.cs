using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace AssetsTools {
    public partial class DynamicAsset {
        /*
        private struct DynamicAssetBuc.ilder {
            UnityBinaryReader _reader;
            TypeTree.Node[] _nodes;
            int _index;

            public DynamicAssetBuc.ilder(UnityBinaryReader reader, SerializedType type) {
                _reader = reader;
                _nodes = type.TypeTree.Nodes;
                _index = 0;
            }

            public DynamicAsset Buc.ild() {
                // Skip Base
                _index++;

                // Recursively Decode
                DynamicAsset asset = new DynamicAsset();
                var objs = asset.objects;

                int mindepth = _nodes[_index].Level;

                // For general object
                DynamicAssetBuc.ilder buc.ilder;
                buc.ilder._reader = _reader;
                buc.ilder._nodes = _nodes;
                buc.ilder._index = _index;

                whc.ile(_index < _nodes.Length && _nodes[_index].Level >= mindepth) {
                    string name = _nodes[_index].Name;
                    object value;

                    if(TryKnownType(out value)) {
                        objs[name] = value;
                        continue;
                    }


                }
            }

            private bool TryKnownType(out object boxed) {
                switch(_nodes[_index].Type) {
                    default:
                        boxed = null;
                        return false;
                    case "SInt8":
                        boxed = _reader.ReadSByte();
                        break;
                    case "UInt8":
                        boxed = _reader.ReadByte();
                        break;
                    case "short":
                    case "SInt16":
                        boxed = _reader.ReadShort();
                        break;
                    case "UInt16":
                    case "unsigned short":
                        boxed = _reader.ReadUShort();
                        break;
                    case "int":
                    case "SInt32":
                        boxed = _reader.ReadInt();
                        break;
                    case "UInt32":
                    case "unsigned int":
                    case "Type*":
                        boxed = _reader.ReadUInt();
                        break;
                    case "long long":
                    case "SInt64":
                        boxed = _reader.ReadLong();
                        break;
                    case "UInt64":
                    case "unsigned long long":
                        boxed = _reader.ReadULong();
                        break;
                    case "string":
                        boxed = ReadAlignedString();
                        return true;
                    case "vector":
                        boxed = ReadArray();
                        return true;
                    case "TypelessData":
                        boxed = _reader.ReadValueArray<byte>();
                        _index += 3;
                        return true;
                }

                _index++;
                return true;
            }

            private string ReadAlignedString() {
                var length = _reader.ReadInt();

                byte[] utf8 = MiniMemoryPool<DynamicAssetBuc.ilder>.GetBuffer(length);
                _reader.ReadBytes(utf8, 0, length);
                string str = Encoding.UTF8.GetString(utf8);

                _reader.Align(4);
                _index += 4;

                return str;
            }

            private object ReadArray() {
                var elemindex = _index += 3;

                // Try Known Type
                switch (_nodes[_index].Type) {
                    default:
                        break;
                    case "SInt8":
                        return _reader.ReadValueArray<sbyte>();
                    case "UInt8":
                        return _reader.ReadValueArray<byte>();
                    case "short":
                    case "SInt16":
                        return _reader.ReadValueArray<short>();
                    case "UInt16":
                    case "unsigned short":
                        return _reader.ReadValueArray<ushort>();
                    case "int":
                    case "SInt32":
                        return _reader.ReadValueArray<int>();
                    case "UInt32":
                    case "unsigned int":
                    case "Type*":
                        return _reader.ReadValueArray<uint>();
                    case "long long":
                    case "SInt64":
                        return _reader.ReadValueArray<long>();
                    case "UInt64":
                    case "unsigned long long":
                        return _reader.ReadValueArray<ulong>();
                    case "string": 
                        {
                            string[] ret = new string[_reader.ReadInt()];
                            for (int i = 0; i < ret.Length; i++) {
                                _index = elemindex;
                                ret[i] = ReadAlignedString();
                            }
                            return ret;
                        }
                    case "vector":
                        {
                            object[] ret = new object[_reader.ReadInt()];
                            for (int i = 0; i < ret.Length; i++) {
                                _index = elemindex;
                                ret[i] = ReadArray();
                            }
                            return ret;
                        }

                }

                // Buc.ild DynamicAsset array
                object[] darr = new DynamicAsset[_reader.ReadInt()];
                DynamicAssetBuc.ilder buc.ilder;
                buc.ilder._reader = _reader;
                buc.ilder._index = elemindex;
                buc.ilder._nodes = _nodes;

                for(int i = 0; i < darr.Length; i++) {
                    buc.ilder._index = elemindex;
                    darr[i] = buc.ilder.Buc.ild();
                }

                return darr;
            }

            private object ReadDictionary() {
                // First, determine key type
                System.Reflection.Emit.
            }
        }*/

        

        private class GenDeserializerContext {
            public ILGenerator il;
            public TypeTree.Node[] nodes;
            public int i;
            public List<string> types;

            Dictionary<Type, List<int>> local_table = new Dictionary<Type, List<int>>();
            byte local_count;

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
        }

        private static Func<UnityBinaryReader, Dictionary<string, object>> GenDeserializer(TypeTree.Node[] nodes) {
            DynamicMethod method = new DynamicMethod(nodes[0].Type, DicStrObjType, new Type[] { typeof(UnityBinaryReader) }, m: typeof(DynamicAssetArray).Module, skipVisibility: true);

            GenDeserializerContext c = new GenDeserializerContext();
            c.il = method.GetILGenerator();
            c.nodes = nodes;
            c.i = 0;
            c.types = new List<string>(32);

            GenReadObject(c);

            return (Func<UnityBinaryReader, Dictionary<string, object>>)method.CreateDelegate(typeof(Func<UnityBinaryReader, Dictionary<string, object>>));
        }

#if DEBUG
        private static AssemblyName _name;
        private static AssemblyBuilder _assembly = null;
        private static ModuleBuilder _module;
        private static TypeBuilder _type;

        private static void InitAssembly() {
            string name = "Deserializer";

            _name = new AssemblyName { Name = name };
            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
            _module = _assembly.DefineDynamicModule(name, name + ".dll", true);

            
            _type = _module.DefineType(name, System.Reflection.TypeAttributes.Public);
        }

        public static void GenDeserializerAssembly(TypeTree.Node[] nodes) {
            if (_assembly == null)
                InitAssembly();

            string name = nodes[0].Type;

            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(Dictionary<string, object>), new Type[] { typeof(UnityBinaryReader) });

            GenDeserializerContext c = new GenDeserializerContext();
            c.il = method.GetILGenerator();
            c.nodes = nodes;
            c.i = 0;
            c.types = new List<string>(32);

            GenReadObject(c);
        }

        public static void SaveAssembly(string filename) {
            _assembly.Save(filename);
        }
#endif

        private static void GenReadObject(GenDeserializerContext c) {
            c.types.Add(c.nodes[c.i].Type);

            int dic = c.AllocLocal(DicStrObjType);

            int mindepth = c.nodes[c.i].Level;

            // Init Dictionary
            c.il.Emit(OpCodes.Newobj, DicStrObjCtor);
            c.il.Emit(OpCodes.Stloc_S, dic);

            while (c.i < c.nodes.Length && c.nodes[c.i].Level >= mindepth) {
                c.il.Emit(OpCodes.Ldloc_S, dic);
                c.il.Emit(OpCodes.Ldstr, PrettifyName(c.nodes[c.i].Name));

                // Try Known Type
                if (TryGenKnownType(c)) { }
                // Map
                else if (c.nodes[c.i].Type == "map") {
                }
                // Array
                else if (c.i < c.nodes.Length - 1 && c.nodes[c.i + 1].Type == "Array") 
                    GenReadArray(c);

                c.il.Emit(OpCodes.Callvirt, DicStrObjAdd);
            }

            c.ReleaseLocal(DicStrObjType);
            c.types.RemoveAt(c.types.Count - 1);
        }

        private static bool TryGenKnownType(GenDeserializerContext c) {
            // Try Primitive Type
            Type type;

            if (PrimitiveTypeDic.TryGetValue(c.nodes[c.i].Type, out type)) {
                // reader.Read<T>()
                c.il.Emit(OpCodes.Ldarg_0);
                c.il.Emit(OpCodes.Call, PrimitiveReaderDic[type]);
                c.i++;
                return true;
            }
            // Try String
            else if(c.nodes[c.i].Type == "string") {
                GenReadString(c);
                return true;
            }
            else
                return false;
        }

        private static void GenReadString(GenDeserializerContext c) {
            c.il.Emit(OpCodes.Ldarg_0);
            c.il.Emit(OpCodes.Dup);
            c.il.Emit(OpCodes.Call, PrimitiveReaderDic[typeof(int)]);
            c.il.Emit(OpCodes.Call, ReadString);

            // reader.Align(4);
            c.il.Emit(OpCodes.Ldarg_0);
            c.il.Emit(OpCodes.Ldc_I4_4);
            c.il.Emit(OpCodes.Call, AlignReader);

            c.i += 3;
        }

        private static void GenReadArray(GenDeserializerContext c) {
            string name = c.nodes[c.i].Name;
            c.i += 2;
            // Try Primitive Type
            Type elemtype;
            if (PrimitiveTypeDic.TryGetValue(c.nodes[c.i].Type, out elemtype)) {
                // reader.ReadValueArray<T>()

                var readfunc = ReadValueArray.MakeGenericMethod(elemtype);
                c.il.Emit(OpCodes.Ldarg_0);
                c.il.Emit(OpCodes.Call, readfunc);

                c.il.Emit(OpCodes.Callvirt, DicStrObjAdd);
            }
            // Try String
            if (c.nodes[c.i].Type == "string") {
                // var ary = new string[reader.ReadInt()];
                c.il.Emit(OpCodes.Ldarg_0);
                c.il.Emit(OpCodes.Call, ReadInt);
                c.il.Emit(OpCodes.Newarr, typeof(string));

                // for(int i = 0; i < ary.Length; i++) ReadString();
                int i = c.AllocLocal(typeof(int));
                c.il.Emit(OpCodes.Ldc_I4_0);
                c.il.Emit(OpCodes.Stloc_S, i);

                var l_loopstart = c.il.DefineLabel();
                var l_loopend = c.il.DefineLabel();

                c.il.MarkLabel(l_loopstart);

                c.il.Emit(OpCodes.Dup);
                c.il.Emit(OpCodes.Ldlen);
                c.il.Emit(OpCodes.Conv_I4);
                c.il.Emit(OpCodes.Ldloc_S, i);
                c.il.Emit(OpCodes.Ble_S, l_loopend);

                c.il.Emit(OpCodes.Dup);
                c.il.Emit(OpCodes.Ldloc_S, i);
                GenReadString(c);
                c.il.Emit(OpCodes.Stelem, typeof(string));

                c.il.Emit(OpCodes.Ldloc_S, i);
                c.il.Emit(OpCodes.Ldc_I4_1);
                c.il.Emit(OpCodes.Add);
                c.il.Emit(OpCodes.Stloc_S, i);
                c.il.Emit(OpCodes.Br_S, l_loopstart);

                c.il.MarkLabel(l_loopend);

                c.ReleaseLocal(typeof(int));
            }
            // Array in Array
            else if (c.i < c.nodes.Length - 1 && c.nodes[c.i + 1].Type == "Array")
                throw new NotImplementedException("Array in Array is not supported.");
            // Object
            else {
                // vec = DynamicAssetArray(reader.ReadInt(), protoname);
                c.il.Emit(OpCodes.Ldarg_0);
                c.il.Emit(OpCodes.Call, ReadInt);

                c.il.Emit(OpCodes.Ldstr, c.types.Aggregate((a, b) => a + "." + b));

                c.il.Emit(OpCodes.Newobj, DynamicAssetArrayCtor);

                // while(vec.Add(ReadObject());
                var l_loopstart = c.il.DefineLabel();
                c.il.MarkLabel(l_loopstart);

                c.il.Emit(OpCodes.Dup);
                GenReadObject(c);
                c.il.Emit(OpCodes.Ldloc_S, c.local_index + 1);

                c.il.Emit(OpCodes.Call, DynamicAssetArrayAdd);

                c.il.Emit(OpCodes.Brtrue, l_loopstart);
            }
        }

        private static void GenReadDic(GenDeserializerContext c) {
            Type keytype, valuetype;

            // First, scan tree and determine the key/value type
            int scani = c.i + 2;
            int mindepth = c.nodes[scani].Level;

            // Scan KeyType
            if (!PrimitiveTypeDic.TryGetValue(c.nodes[c.i].Type, out keytype)) {
                keytype = typeof(object);
                scani++;
                while (scani < c.nodes.Length && c.nodes[scani].Level > mindepth)
                    scani++;
            }
            else
                scani++;

            // Scan ValueType
            if (!PrimitiveTypeDic.TryGetValue(c.nodes[c.i].Type, out valuetype)) {
                valuetype = typeof(object);
                scani++;
                while (scani < c.nodes.Length && c.nodes[scani].Level >= mindepth)
                    scani++;
            }
            else
                scani++;

            // Create Dictionary<keytype, valuetype>
            Type dictype = typeof(Dictionary<,>).MakeGenericType(keytype, valuetype);
            c.il.Emit(OpCodes.Newobj, dictype.GetConstructor(Type.EmptyTypes));

            // Read KeyValuePairs

            // 
        }
    }
}
