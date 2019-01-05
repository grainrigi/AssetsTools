using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace IOLibGen {
    public class AssemblyHelper {
        private AssemblyName _name;
        private AssemblyBuilder _assembly;
        private ModuleBuilder _module;

        public AssemblyHelper(string name) {
            this._name = new AssemblyName { Name = name };
            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(this._name, AssemblyBuilderAccess.RunAndSave);
            _module = _assembly.DefineDynamicModule(name, name + ".dll", true);
        }

        public Type CreateClass(string name, Action<ClassHelper> builder) {
            var helper = new ClassHelper(_module, name);
            builder(helper);
            return helper.CompleteClass();
        }

        public void Save() {
            _assembly.Save(_name.Name + ".dll");
        }
    }
}
