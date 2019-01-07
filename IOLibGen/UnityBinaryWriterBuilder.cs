#define SAFETY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace IOLibGen {
    public class UnityBinaryWriterBuilder {
        public static void Builder(ClassHelper helper) {
            type = helper.Type;

            file = helper.CreateField("file", typeof(byte[]));
            offset = helper.CreateField("offset", typeof(int));
            capacity = helper.CreateField("capacity", typeof(int));

            defctor = helper.CreateCtor(null, CtorEmitter);

            EnsureCapacity = helper.CreateMethod("EnsureCapacity", typeof(void), new[] { (typeof(int), "value") }, EnsureCapacityEmitter);
            helper.CreateMethod("ToBytes", typeof(byte[]), ToBytesEmitter);

            helper.CreateMethod("WriteByte", typeof(void), new[] { (typeof(byte), "value") }, WriteByteEmitter);
            helper.CreateMethod("WriteSByte", typeof(void), new[] { (typeof(sbyte), "value") }, WriteSByteEmitter);

            helper.CreateMethod("WriteShort", typeof(void), new[] { (typeof(short), "value") }, WriteShortEmitter);
            helper.CreateMethod("WriteInt", typeof(void), new[] { (typeof(int), "value") }, WriteIntEmitter);
            helper.CreateMethod("WriteLong", typeof(void), new[] { (typeof(long), "value") }, WriteLongEmitter);
            helper.CreateMethod("WriteFloat", typeof(void), new[] { (typeof(float), "value") }, WriteFloatEmitter);
            helper.CreateMethod("WriteDouble", typeof(void), new[] { (typeof(double), "value") }, WriteDoubleEmitter);
            helper.CreateMethod("WriteUShort", typeof(void), new[] { (typeof(ushort), "value") }, WriteShortEmitter);
            helper.CreateMethod("WriteUInt", typeof(void), new[] { (typeof(uint), "value") }, WriteIntEmitter);
            helper.CreateMethod("WriteULong", typeof(void), new[] { (typeof(ulong), "value") }, WriteLongEmitter);

            helper.CreateMethod("WriteShortBE", typeof(void), new[] { (typeof(short), "value") }, WriteShortBEEmitter);
            helper.CreateMethod("WriteIntBE", typeof(void), new[] { (typeof(int), "value") }, WriteIntBEEmitter);
            helper.CreateMethod("WriteLongBE", typeof(void), new[] { (typeof(long), "value") }, WriteLongBEEmitter);
            helper.CreateMethod("WriteFloatBE", typeof(void), new[] { (typeof(float), "value") }, WriteFloatBEEmitter);
            helper.CreateMethod("WriteDoubleBE", typeof(void), new[] { (typeof(double), "value") }, WriteDoubleBEEmitter);
            helper.CreateMethod("WriteUShortBE", typeof(void), new[] { (typeof(ushort), "value") }, WriteShortBEEmitter);
            helper.CreateMethod("WriteUIntBE", typeof(void), new[] { (typeof(uint), "value") }, WriteIntBEEmitter);
            helper.CreateMethod("WriteULongBE", typeof(void), new[] { (typeof(ulong), "value") }, WriteLongBEEmitter);

            helper.CreateMethod("WriteStringToNull", typeof(void), new[] { (typeof(string), "value") }, WriteStringToNullEmitter);
            helper.CreateGenericMethodWithArrayParam("WriteValueArray", WriteArrayEmitter);
            WriteLZ4Data = helper.CreateMethod("WriteLZ4Data", typeof(int),
                new[] { (typeof(byte[]), "bin"),
                    (typeof(int), "offset"),
                    (typeof(int), "length") }, WriteLZ4DataEmitter);
            helper.CreateMethod("WriteLZ4Data", typeof(int),
                new[] { (typeof(byte[]), "bin") }, WriteLZ4Data_1ArgEmitter);
        }

        private static FieldInfo file = default(FieldInfo);
        private static FieldInfo offset = default(FieldInfo);
        private static FieldInfo capacity = default(FieldInfo);
        private static PropertyInfo Position = default(PropertyInfo);
        private static ConstructorInfo defctor = default(ConstructorInfo);
        private static MethodInfo EnsureCapacity = default(MethodInfo);
        private static MethodInfo WriteLZ4Data = default(MethodInfo);
        private static Type type = default(Type);

        #region Ctor
        private static void CtorEmitter(ILGenerator il) {
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, 256);
            il.Emit(OpCodes.Newarr, typeof(byte));
            il.Emit(OpCodes.Stfld, file);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ldc_I4, 256);
            il.Emit(OpCodes.Stfld, capacity);

            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Byte
        private static void WriteByteEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_1);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stelem_I1);
            ForwardEmitter(il, OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }

        private static void WriteSByteEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_1);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stelem_I1);
            ForwardEmitter(il, OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region LittleEndian
        private static void WriteShortEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_I2);
            ForwardEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void WriteIntEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_I4);
            ForwardEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void WriteLongEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_I8);
            ForwardEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void WriteFloatEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_R4);
            ForwardEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void WriteDoubleEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_R8);
            ForwardEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Big Endian
        private static void WriteShortBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);

            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
#endif
            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_1);

            LoadCurrentAddrEmitter(il);


            ReverseCopy2Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
#endif

            ForwardEmitter(il, OpCodes.Ldc_I4_2);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteIntBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);

            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
#endif
            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_1);

            LoadCurrentAddrEmitter(il);

            ReverseCopy4Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_4);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteLongBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);

            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
#endif
            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_1);

            LoadCurrentAddrEmitter(il);

            ReverseCopy8Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_8);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteFloatBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);

            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
#endif
            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_1);

            LoadCurrentAddrEmitter(il);

            ReverseCopy4Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_4);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteDoubleBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);

            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
#endif
            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_1);

            LoadCurrentAddrEmitter(il);

            ReverseCopy8Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_8);

            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region VarLength Data
        private static void WriteStringToNullEmitter(ILGenerator il) {
            il.DeclareLocal(typeof(int)).SetLocalSymInfo("noffset");
            il.DeclareLocal(typeof(int)).SetLocalSymInfo("offset");

            // if(value?.Length == 0) return;
            var l_cont = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, l_cont);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(l_cont);

            l_cont = il.DefineLabel();

            il.Emit(OpCodes.Call, typeof(string).GetProperty("Length").GetMethod);
            il.Emit(OpCodes.Brtrue_S, l_cont);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(l_cont);

            // noffset = offset + GetMaxByteCount(value.Length + 1);
            il.Emit(OpCodes.Call, typeof(Encoding).GetProperty("UTF8").GetMethod);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(string).GetProperty("Length").GetMethod);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Callvirt,
                typeof(Encoding).GetMethod("GetMaxByteCount",
                    new Type[] { typeof(int) }));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_0);

            // if(noffset > capacity) EnsureCapacity(noffset);
            l_cont = il.DefineLabel();

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Ble, l_cont);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, EnsureCapacity);

            il.MarkLabel(l_cont);

            // offset += UTF8.GetBytes(value, 0, value.Length, file, offset);
            il.Emit(OpCodes.Call, typeof(Encoding).GetProperty("UTF8").GetMethod);

            il.Emit(OpCodes.Ldarg_1);

            il.Emit(OpCodes.Ldc_I4_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(string).GetProperty("Length").GetMethod);
            il.Emit(OpCodes.Conv_I4);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);

            il.Emit(OpCodes.Ldloc_1);

            il.Emit(OpCodes.Callvirt,
                typeof(Encoding).GetMethod("GetBytes",
                    new Type[] { typeof(string), typeof(int), typeof(int), typeof(byte[]), typeof(int) }));

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);

            // file[offset] = 0x00;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stelem_I1);

            // offset = offset + 1;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteArrayEmitter(ILGenerator il, TypeInfo T) {
            il.DeclareLocal(T.MakeArrayType(), true);
            il.DeclareLocal(typeof(long)).SetLocalSymInfo("copysize");
            il.DeclareLocal(typeof(byte[]), true);
            il.DeclareLocal(typeof(int)).SetLocalSymInfo("offset");

            // if(value?.Length == 0) return;
            var l_cont = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, l_cont);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(l_cont);

            l_cont = il.DefineLabel();

            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Brtrue_S, l_cont);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(l_cont);

            // fix
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc_0);
            FixFileEmitter(il, OpCodes.Stloc_2);

            // copysize = (&value[1] - &value[0]) * value.Length;
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldelema, T);
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, T);
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Conv_I8);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Mul);

            il.Emit(OpCodes.Stloc_1);

            // Boundary Check
            // Check(copysize + offset + 4  <= capacity);
            l_cont = il.DefineLabel();

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ble_S, l_cont);

            il.Emit(OpCodes.Newobj, typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_cont);

            // Write Size
            // *(uint32_t*)(&file[offset]) = value.Length;
            // offset += 4;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Stind_I4);

            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_3);

            // Copy
            // Buffer.MemoryCopy(&buf[0], &file[offset], copysize, copysize);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, T);
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_1);

            il.Emit(OpCodes.Call,
                typeof(Buffer).GetMethod("MemoryCopy",
                    new Type[] { typeof(void*), typeof(void*), typeof(long), typeof(long) }));

            // Unfix
            UnfixFileEmitter(il, OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_0);

            // Forward
            // offset += copysize;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ret);
        }

        private static void WriteLZ4DataEmitter(ILGenerator il) {
            il.DeclareLocal(typeof(int)).SetLocalSymInfo("written");

            var l_cont = il.DefineLabel();

            // Boundary Check
            // offset + length <= capacity
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Ble_S, l_cont);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Call, EnsureCapacity);

            il.MarkLabel(l_cont);

            // read = MessagePack.LZ4.LZ4Codec.Encode64Unsafe(
            //  bin, offset, length, file, offset, capacity
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);

            il.Emit(OpCodes.Call,
                typeof(LZ4.LZ4Codec).GetMethod("Encode64Unsafe"));

            il.Emit(OpCodes.Stloc_0);

            // offset += written;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }

        private static void WriteLZ4Data_1ArgEmitter(ILGenerator il) {
            // WriteLZ4Data(bin, 0, bin.Length);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldarg_1);

            il.Emit(OpCodes.Ldc_I4_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);

            il.Emit(OpCodes.Call, WriteLZ4Data);

            il.Emit(OpCodes.Ret);
        }
        #endregion

        private static void EnsureCapacityEmitter(ILGenerator il) {
            il.DeclareLocal(typeof(byte[]), true).SetLocalSymInfo("old");
            il.DeclareLocal(typeof(byte[]), true).SetLocalSymInfo("new");
            il.DeclareLocal(typeof(int)).SetLocalSymInfo("newCapacity");

            var l_cont = il.DefineLabel();

            // newCapacity = value;
            // if(newCapacity < capacity * 2) newCapacity = capacity * 2;
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldloc_2);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Mul);            
            il.Emit(OpCodes.Bge, l_cont);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Stloc_2);

            il.MarkLabel(l_cont);

            // old = file;
            // new = new byte[newCapacity];
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Newarr, typeof(byte));
            il.Emit(OpCodes.Stloc_1);

            // Buffer.MemoryCopy(&old[0], &new[0], new.Length, old.Length);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I8);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I8);

            il.Emit(OpCodes.Call,
                typeof(Buffer).GetMethod("MemoryCopy",
                    new Type[] { typeof(void*), typeof(void*), typeof(long), typeof(long) }));

            // file = new;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Stfld, file);

            // Unfix
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_0);

            // capacity = newCapacity
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Stfld, capacity);

            il.Emit(OpCodes.Ret);
        }

        private static void ToBytesEmitter(ILGenerator il) {
            il.DeclareLocal(typeof(byte[]), true);
            il.DeclareLocal(typeof(byte[]), true);

            // ret = new byte[offset];
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Newarr, typeof(byte));
            il.Emit(OpCodes.Stloc_0);

            // Fix
            FixFileEmitter(il, OpCodes.Stloc_1);

            // Copy
            // Buffer.MemoryCopy(&file[0], &ret[0], offset, offset);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Conv_I8);

            il.Emit(OpCodes.Dup);

            il.Emit(OpCodes.Call,
                typeof(Buffer).GetMethod("MemoryCopy",
                    new Type[] { typeof(void*), typeof(void*), typeof(long), typeof(long) }));

            //Unfix
            UnfixFileEmitter(il, OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ret);
        }

        #region Helpers
        private static void LoadCurrentAddrEmitter(ILGenerator il) {
            // Prerequisit
            // 1.arg0 must be the this-Pointer of UnityBinaryReader
            // 2.loc0 must be offset

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);
        }

        private static void ForwardEmitter(ILGenerator il, OpCode step) {
            // Prerequisit
            // 1.arg0 must be the this-Pointer of UnityBinaryReader
            // 2.loc0 must be the offset

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(step);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);
        }

        private static void BoundaryCheckEmitter(ILGenerator il, OpCode step) {
            // Prerequisit
            // 1.arg0 must be the this-Pointer of UnityBinaryReader
            // 2.No locals have been declared

            il.DeclareLocal(typeof(int)).SetLocalSymInfo("offset");
            var cont = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldfld, capacity);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(step);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Bge_S, cont);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(step);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Call, EnsureCapacity);

            il.MarkLabel(cont);
        }

        private static void FixFileEmitter(ILGenerator il, OpCode st_loc) {
            // Prerequisit
            // 1.arg0 must be the this-Pointer of UnityBinaryReader
            // 2.a pinning local variable must be defined (byte[])

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(st_loc);
        }

        private static void UnfixFileEmitter(ILGenerator il, OpCode st_loc) {
            // Prerequisit
            // 1.a pinning local variable must be defined (byte[])

            il.Emit(OpCodes.Ldnull);
            il.Emit(st_loc);
        }

        private static void ReverseCopy2Emitter(ILGenerator il) {
            // Prerequisit
            // 1.dest address(native int) is pushed on the top of eval-stack
            // 2.src address(native int) is in loc1
            // Note: this consumes 1 value on eval-stack.

            // ret[1] = src[0]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[1]
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);
        }

        private static void ReverseCopy4Emitter(ILGenerator il) {
            // Prerequisit
            // 1.dest address(native int) is pushed on the top of eval-stack
            // 2.src address(native int) is in loc2
            // Note: this consumes 1 value on eval-stack.

            // ret[3] = src[0]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[2] = src[1]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[1] = src[2]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[3]
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);
        }

        private static void ReverseCopy8Emitter(ILGenerator il) {
            // Prerequisit
            // 1.dest address(native int) is pushed on the top of eval-stack
            // 2.src address(native int) is in loc2
            // Note: this consumes 1 value on eval-stack.

            // ret[7] = src[0]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_7);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[6] = src[1]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_6);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[5] = src[2]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_5);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[4] = src[3]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[3] = src[4]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[2] = src[5]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_5);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[1] = src[6]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_6);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[7]
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_7);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);
        }
        #endregion
    }
}
