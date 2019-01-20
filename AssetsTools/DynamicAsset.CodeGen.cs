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
    }
}
