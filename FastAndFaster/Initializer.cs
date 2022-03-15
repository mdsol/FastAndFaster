using FastAndFaster.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FastAndFaster
{
    public static class Initializer
    {
        public static int SlidingExpirationInSecs { get; set; } = 12 * 3600;

        private const byte CONSTRUCTOR_LOAD_INDEX = 0;

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static Func<object[], object> Create(string typeName, Type[] parameterTypes = null)
        {
            if (parameterTypes is null)
            {
                parameterTypes = Type.EmptyTypes;
            }
            var key = (typeName, TypeHelper.GetTypesIdentity(parameterTypes));

            return _cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(SlidingExpirationInSecs);

                var type = TypeHelper.GetTypeByName(typeName);
                var delegateParameterTypes = new[] { typeof(object[]) };
                var dynCtor = new DynamicMethod(
                    $"{type.FullName}_{TypeHelper.GetTypesIdentity(parameterTypes)}_ctor",
                    type, delegateParameterTypes, true);
                var ctorInfo = TypeHelper.GetConstructorInfoByType(type, parameterTypes);

                GenerateIL(dynCtor, ctorInfo, parameterTypes);

                return (Func<object[], object>)dynCtor.CreateDelegate(typeof(Func<object[], object>));
            });
        }

        private static void GenerateIL(
            DynamicMethod dynCtor, ConstructorInfo ctorInfo, Type[] parameterTypes)
        {
            var il = dynCtor.GetILGenerator();
            IlHelper.LoadArguments(il, CONSTRUCTOR_LOAD_INDEX, parameterTypes);
            IlHelper.CreateInstance(il, ctorInfo);
        }
    }
}
