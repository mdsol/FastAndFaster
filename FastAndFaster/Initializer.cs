using FastAndFaster.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FastAndFaster
{
    public static class Initializer
    {
        /// <summary>
        /// Sliding cache expiration time for delegates to call a public constructor of a class.
        /// The unit is second and the default value is 43,200 (12 hours).
        /// </summary>
        public static int SlidingExpirationInSecs { get; set; } = 12 * 3600;

        private const byte CONSTRUCTOR_LOAD_INDEX = 0;

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// Create a delegate to call a public constructor of a class.
        /// </summary>
        /// <param name="typeName">
        /// The assembly qualified name of the target class.
        /// </param>
        /// <param name="parameterTypes">
        /// The list of the method's parameter types. For a parameterless method, this value can be omitted.
        /// </param>
        /// <returns>
        /// A delegate to invoke the target method. The delegate type is Func<object[], object>.
        /// </returns>
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
