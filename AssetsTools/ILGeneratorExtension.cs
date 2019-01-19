using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace AssetsTools {
    public static class ILGeneratorExtension {
        public static void EmitLdloc(this ILGenerator il, int i) {
            switch (i) {
                default:
                    break;
                case 0:
                    il.Emit(OpCodes.Ldloc_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldloc_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldloc_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldloc_3);
                    return;
            }

            if (i <= 255)
                il.Emit(OpCodes.Ldloc_S, (byte)i);
            else
                il.Emit(OpCodes.Ldloc, i);
        }
        public static void EmitStloc(this ILGenerator il, int i) {
            switch (i) {
                default:
                    break;
                case 0:
                    il.Emit(OpCodes.Stloc_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Stloc_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Stloc_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Stloc_3);
                    return;
            }

            if (i <= 255)
                il.Emit(OpCodes.Stloc_S, (byte)i);
            else
                il.Emit(OpCodes.Stloc, i);
        }

        /// <summary>
        /// Emit for loop
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="i">index of local variable which is used for counter</param>
        /// <param name="cond">condition check emitter (returns OpCodes.B** which is used for breaking loop)</param>
        /// <param name="block">block emitter</param>
        public static void EmitFor(this ILGenerator il, int i, Func<ILGenerator, OpCode> cond, Action<ILGenerator> block) {
            il.Emit(OpCodes.Ldc_I4_0);
            il.EmitStloc(i);

            var l_loopstart = il.DefineLabel();
            var l_loopend = il.DefineLabel();

            il.MarkLabel(l_loopstart);

            // Break loop if condition is not met
            il.Emit(cond(il), l_loopend);

            block(il);

            // Increment
            il.EmitLdloc(i);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.EmitStloc(i);

            il.Emit(OpCodes.Br, l_loopstart);

            il.MarkLabel(l_loopend);
        }
    }
}
