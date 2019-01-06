using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace IOLibGen {
    public class ClassHelper {
        TypeBuilder _type;

        public Type Type => _type;

        public ClassHelper(ModuleBuilder mod, string name) {
            _type = mod.DefineType(name, System.Reflection.TypeAttributes.Public);
        }

        public MethodInfo CreateMethod(string name, Type ret, Action<ILGenerator> emitter) {
            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public,
                ret, new Type[] { });
            emitter(method.GetILGenerator());
            return method;
        }

        public MethodInfo CreateMethod(string name, Type ret, (Type, string)[] args, Action<ILGenerator> emitter) {
            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public,
                ret, args.Select(tup => tup.Item1).ToArray());
            for (int i = 0; i < args.Length; i++)
                method.DefineParameter(i+1, ParameterAttributes.None, args[i].Item2);
            emitter(method.GetILGenerator());
            return method;
        }

        public MethodInfo CreateGenericMethodWithArrayReturn(string name, Action<ILGenerator, TypeInfo> emitter) {
            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public);
            GenericTypeParameterBuilder T =
                method.DefineGenericParameters("T")[0];
            T.SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);
            method.SetReturnType(T.MakeArrayType());
            emitter(method.GetILGenerator(), T);
            return method;
        }

        public MethodInfo CreateGenericMethodWithArrayParam(string name, Action<ILGenerator, TypeInfo> emitter) {
            MethodBuilder method = _type.DefineMethod(
                name,
                MethodAttributes.Public);
            GenericTypeParameterBuilder T =
                method.DefineGenericParameters("T")[0];
            T.SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);
            method.SetParameters(T.MakeArrayType());
            method.DefineParameter(1, ParameterAttributes.None, "array");
            emitter(method.GetILGenerator(), T);
            return method;
        }

        public ConstructorInfo CreateCtor((Type, string)[] args, Action<ILGenerator> emitter) {
            ConstructorBuilder ctor = _type.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                args == null ? Type.EmptyTypes :
                args.Select(tup => tup.Item1).ToArray());
            if (args != null)
                for (int i = 0; i < args.Length; i++)
                    ctor.DefineParameter(i+1, ParameterAttributes.None, args[i].Item2);
            emitter(ctor.GetILGenerator());
            return ctor;
        }

        public ConstructorInfo CreatePrivateCtor((Type, string)[] args, Action<ILGenerator> emitter) {
            ConstructorBuilder ctor = _type.DefineConstructor(
                MethodAttributes.Private,
                CallingConventions.Standard,
                args == null ? Type.EmptyTypes :
                args.Select(tup => tup.Item1).ToArray());
            if (args != null)
                for (int i = 0; i < args.Length; i++)
                    ctor.DefineParameter(i+1, ParameterAttributes.None, args[i].Item2);
            emitter(ctor.GetILGenerator());
            return ctor;
        }

        public FieldInfo CreateField(string name, Type type) {
            FieldBuilder field = _type.DefineField(name, type, FieldAttributes.Private);
            return field;
        }

        public PropertyInfo CreateProperty(string name, Type type,  Action<ILGenerator> getemitter, Action<ILGenerator> setemitter) {
            PropertyBuilder prop = _type.DefineProperty(name, PropertyAttributes.None, type, null);

            MethodAttributes attr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            MethodBuilder getter = _type.DefineMethod("get_" + name, attr, type, Type.EmptyTypes);
            getemitter(getter.GetILGenerator());

            MethodBuilder setter = _type.DefineMethod("set_" + name, attr, null, new Type[] { type });
            setemitter(setter.GetILGenerator());

            prop.SetGetMethod(getter);
            prop.SetSetMethod(setter);

            return prop;
        }

        public Type CompleteClass() {
            return _type.CreateType();
        }
    }
}
