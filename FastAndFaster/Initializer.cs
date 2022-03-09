using System;
using System.Reflection.Emit;

namespace FastAndFaster
{
    public class Initializer
    {
        public static object Create(string typeName)
        {
            var type = Type.GetType(typeName);

            var dynCtor = new DynamicMethod($"{type.FullName}_ctor", type, Type.EmptyTypes, true);
            var il = dynCtor.GetILGenerator();
            var ctorInfo = type.GetConstructor(Type.EmptyTypes);

            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            var ctorDelegate = (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>));

            var instance = ctorDelegate();
            return instance;
        }
    }
}
