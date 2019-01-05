#define SAFETY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;



namespace IOLibGen {
    public static class UnityBinaryReaderBuilder {
        public static void Builder(ClassHelper helper) {
            type = helper.Type;

            file = helper.CreateField("file", typeof(byte[]));
            offset = helper.CreateField("offset", typeof(int));
            bound = helper.CreateField("bound", typeof(int));

            Position = helper.CreateProperty("Position", typeof(int), get_PositionEmitter, set_PositionEmitter);

            defctor = helper.CreatePrivateCtor(null, CtorEmitter);
            helper.CreateCtor(new[] { (typeof(string), "filename") }, CtorFromFileEmitter);
            helper.CreateCtor(new[] { (typeof(byte[]), "bin") }, CtorFromBinaryEmitter);

            helper.CreateMethod("ReadByte", typeof(byte), ReadByteEmitter);
            helper.CreateMethod("ReadSByte", typeof(sbyte), ReadSByteEmitter);
            helper.CreateMethod("ReadShortLE", typeof(short), ReadShortLEEmitter);
            ReadIntLE = helper.CreateMethod("ReadIntLE", typeof(int), ReadIntLEEmitter);
            helper.CreateMethod("ReadLongLE", typeof(long), ReadLongLEEmitter);
            helper.CreateMethod("ReadFloatLE", typeof(float), ReadFloatLEEmitter);
            helper.CreateMethod("ReadDoubleLE", typeof(double), ReadDoubleLEEmitter);
            helper.CreateMethod("ReadUShortLE", typeof(ushort), ReadUShortLEEmitter);
            helper.CreateMethod("ReadUIntLE", typeof(uint), ReadUIntLEEmitter);
            helper.CreateMethod("ReadULongLE", typeof(ulong), ReadULongLEEmitter);

            helper.CreateMethod("ReadShortBE", typeof(short), ReadShortBEEmitter);
            helper.CreateMethod("ReadIntBE", typeof(int), ReadIntBEEmitter);
            helper.CreateMethod("ReadLongBE", typeof(long), ReadLongBEEmitter);
            helper.CreateMethod("ReadFloatBE", typeof(float), ReadFloatBEEmitter);
            helper.CreateMethod("ReadDoubleBE", typeof(double), ReadDoubleBEEmitter);
            helper.CreateMethod("ReadUShortBE", typeof(ushort), ReadUShortBEEmitter);
            helper.CreateMethod("ReadUIntBE", typeof(uint), ReadUIntBEEmitter);
            helper.CreateMethod("ReadULongBE", typeof(ulong), ReadULongBEEmitter);


            helper.CreateMethod("ReadStringToNull", typeof(string), ReadStringToNullEmitter);
            helper.CreateGenericMethodWithArrayReturn("ReadValueArray", ReadArrayEmitter);
        }

        private static FieldInfo file = default(FieldInfo);
        private static FieldInfo offset = default(FieldInfo);
        private static FieldInfo bound = default(FieldInfo);
        private static PropertyInfo Position = default(PropertyInfo);
        private static ConstructorInfo defctor = default(ConstructorInfo);
        private static MethodInfo ReadIntLE = default(MethodInfo);
        private static Type type = default(Type);

        #region Property
        private static void get_PositionEmitter(ILGenerator il) {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ret);
        }

        private static void set_PositionEmitter(ILGenerator il) {
            var l_Br = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, bound);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Bgt_S, l_Br);

            il.Emit(OpCodes.Newobj, typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_Br);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, offset);
            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Ctor
        private static void CtorEmitter(ILGenerator il) {
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stfld, file);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, bound);

            il.Emit(OpCodes.Ret);
        }

        private static void CtorFromBinaryEmitter(ILGenerator il) {
            // Check for endianness

            var l_main = il.DefineLabel();
            il.Emit(OpCodes.Ldsfld,
                typeof(BitConverter).GetField("IsLittleEndian"));
            il.Emit(OpCodes.Brtrue_S, l_main);
            il.Emit(OpCodes.Ldstr, "BigEndian platform is not supported");
            il.Emit(OpCodes.Newobj,
                typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_main);

            var l_cont = il.DefineLabel();



            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Brtrue_S, l_cont);

            il.Emit(OpCodes.Newobj,
                typeof(NullReferenceException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_cont);

            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, file);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Stfld, bound);

            il.Emit(OpCodes.Ret);
        }

        private static void CtorFromFileEmitter(ILGenerator il) {
            // Check for endianness

            var l_main = il.DefineLabel();
            il.Emit(OpCodes.Ldsfld,
                typeof(BitConverter).GetField("IsLittleEndian"));
            il.Emit(OpCodes.Brtrue_S, l_main);
            il.Emit(OpCodes.Ldstr, "BigEndian platform is not supported");
            il.Emit(OpCodes.Newobj,
                typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_main);

            // using(FileStream fs = new FileStream(name, FileMode.Open)) {
            il.DeclareLocal(typeof(FileStream)).SetLocalSymInfo("fs");
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Newobj,
                typeof(FileStream).GetConstructor(
                    new Type[] { typeof(string), typeof(FileMode) }));
            il.Emit(OpCodes.Stloc_0);

            il.BeginExceptionBlock();

            // ret.file = new byte[new FileInfo(filename).Length];
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj,
                typeof(FileInfo).GetConstructor(
                    new Type[] { typeof(string) }));
            il.Emit(OpCodes.Call, typeof(FileInfo).GetProperty("Length", typeof(long)).GetMethod);
            il.Emit(OpCodes.Conv_Ovf_I);
            il.Emit(OpCodes.Newarr, typeof(byte));

            il.Emit(OpCodes.Stfld, file);

            // ret.bound = fs.Read(ret.file, 0, ret.file.Length);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldloc_0);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);

            il.Emit(OpCodes.Ldc_I4_0);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);

            il.Emit(OpCodes.Callvirt,
                typeof(Stream).GetMethod("Read",
                    new Type[] { typeof(byte[]), typeof(int), typeof(int) }));

            il.Emit(OpCodes.Stfld, bound);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, offset);

            il.BeginFinallyBlock();

            var l_EndFinally = il.DefineLabel();

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Brfalse_S, l_EndFinally);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt,
                typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes));

            il.MarkLabel(l_EndFinally);

            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
        }
        #endregion Ctor

        #region Byte
        private static void ReadByteEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_1);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelem_U1);
            ForwardEmitter(il, OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadSByteEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_1);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelem_I1);
            ForwardEmitter(il, OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Little Endians
        private static void ReadShortLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_I2);
            ForwardEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadIntLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_I4);
            ForwardEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadLongLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_I8);
            ForwardEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadFloatLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_R4);
            ForwardEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadDoubleLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_R8);
            ForwardEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadUShortLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_U2);
            ForwardEmitter(il, OpCodes.Ldc_I4_2);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadUIntLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_U4);
            ForwardEmitter(il, OpCodes.Ldc_I4_4);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }

        private static void ReadULongLEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_1);
#endif
            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Ldind_I8);
            ForwardEmitter(il, OpCodes.Ldc_I4_8);
#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_1);
#endif
            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Big Endians
        private static void ReadShortBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);

            il.DeclareLocal(typeof(short)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy2Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif

            ForwardEmitter(il, OpCodes.Ldc_I4_2);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadIntBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);

            il.DeclareLocal(typeof(int)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy4Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_4);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadLongBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);

            il.DeclareLocal(typeof(long)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy8Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_8);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadFloatBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);

            il.DeclareLocal(typeof(float)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy4Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_4);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadDoubleBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);

            il.DeclareLocal(typeof(double)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy8Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_8);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadUShortBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_2);

            il.DeclareLocal(typeof(ushort)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy2Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif

            ForwardEmitter(il, OpCodes.Ldc_I4_2);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadUIntBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_4);

            il.DeclareLocal(typeof(uint)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy4Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_4);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void ReadULongBEEmitter(ILGenerator il) {
            BoundaryCheckEmitter(il, OpCodes.Ldc_I4_8);

            il.DeclareLocal(typeof(ulong)).SetLocalSymInfo("ret");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
#if SAFETY
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_3);
#endif
            il.Emit(OpCodes.Ldloca_S, 1);
            il.Emit(OpCodes.Conv_U);

            LoadCurrentAddrEmitter(il);
            il.Emit(OpCodes.Stloc_2);

            ReverseCopy8Emitter(il);

#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_3);
#endif
            ForwardEmitter(il, OpCodes.Ldc_I4_8);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }
        #endregion Big Endians

        #region VarLength Data
        private static void ReadStringToNullEmitter(ILGenerator il) {
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("curptr");
            il.DeclareLocal(typeof(byte*)).SetLocalSymInfo("endptr");

//#if SAFETY <- Pinning is needed
            il.DeclareLocal(typeof(byte[]), true);
            FixFileEmitter(il, OpCodes.Stloc_2);
//#endif
            // Loop preparation
            // curptr = &file[0] + offset;
            // endptr = &file[0] + file.Length;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_0);


            // Loop
            // i = curptr;
            // while(file[i] != 0x00 && ++i < endptr);
            var l_LoopStart = il.DefineLabel();
            var l_LoopBreak = il.DefineLabel();

            il.Emit(OpCodes.Ldloc_0);
            il.MarkLabel(l_LoopStart);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Brfalse, l_LoopBreak);

            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Blt, l_LoopStart);

            il.Emit(OpCodes.Newobj,
                typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
            
            il.MarkLabel(l_LoopBreak);

            // Conversion
            // len = i - curptr;
            // ret = Encoding.UTF8.GetString(file, offset, len);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Call,
                typeof(Encoding).GetProperty("UTF8").GetMethod);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Conv_I4);

            il.Emit(OpCodes.Callvirt,
                typeof(Encoding).GetMethod("GetString",
                    new Type[] { typeof(byte[]), typeof(int), typeof(int) }));

//#if SAFETY
            UnfixFileEmitter(il, OpCodes.Stloc_2);
//#endif

            // Forwarding
            // offset += len + 1;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ret);
        }

        private static void ReadArrayEmitter(ILGenerator il, TypeInfo T) {
            il.DeclareLocal(T.MakeArrayType(), true);
            il.DeclareLocal(typeof(long)).SetLocalSymInfo("copysize");

            // Read index
            // cnt = ReadIntLE();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReadIntLE);

            // Boundary Check
            // Check(cnt + offset <= bound);
            var l_cont = il.DefineLabel();

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, bound);
            il.Emit(OpCodes.Ble_S, l_cont);

            il.Emit(OpCodes.Newobj, typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(l_cont);

            // PrepareBuffer
            // buf = new T[cnt];(fixed)
            il.Emit(OpCodes.Newarr, T);
            il.Emit(OpCodes.Stloc_0);

            // Calc copy size
            // copysize = (&buf[1] - &buf[0]) * buf.Length;
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

            // Copy
            // Buffer.MemoryCopy(&file[offset], &buf[0], copysize, copysize);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, file);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, T);
            il.Emit(OpCodes.Conv_U);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_1);

            il.Emit(OpCodes.Call,
                typeof(Buffer).GetMethod("MemoryCopy",
                    new Type[] { typeof(void*), typeof(void*), typeof(long), typeof(long) }));

            // Unfix
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_0);

            // Forward
            // offset += copysize;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, offset);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, offset);

            il.Emit(OpCodes.Ret);
        }
        #endregion

        #region Helpers
        // load current address as native int
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
            il.Emit(OpCodes.Ldfld, bound);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(step);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Bge_S, cont);
            il.Emit(OpCodes.Newobj, typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

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
            // 2.src address(native int) is in loc2
            // Note: this consumes 1 value on eval-stack.

            // ret[1] = src[0]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[1]
            il.Emit(OpCodes.Ldloc_2);
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

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[2] = src[1]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[1] = src[2]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[3]
            il.Emit(OpCodes.Ldloc_2);
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

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[6] = src[1]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_6);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[5] = src[2]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_5);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[4] = src[3]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[3] = src[4]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_3);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_4);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[2] = src[5]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_5);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[1] = src[6]
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_6);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);

            // ret[0] = src[7]
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_7);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            il.Emit(OpCodes.Stind_I1);
        }

        private static void DebugLogEmitter(ILGenerator il, string message) {
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Call, typeof(System.Diagnostics.Debug).GetMethod("WriteLine", new Type[] { typeof(string) }));
        }
        #endregion
    }
}
