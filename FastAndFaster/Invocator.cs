using FastAndFaster.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Reflection;
using System.Reflection.Emit;

// TODO: add comment everywhere
namespace FastAndFaster
{
    public static class Invocator
    {
        public static int SlidingExpirationInSecs { get; set; } = 12 * 3600;

        private const byte METHOD_LOAD_INDEX = 1;

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static Action<object, object[]> CreateAction(
            string typeName, string methodName, Type[] parameterTypes = null)
        {
            if (parameterTypes is null)
            {
                parameterTypes = Type.EmptyTypes;
            }
            var key = (typeName, methodName, TypeHelper.GetTypesIdentity(parameterTypes));

            return _cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(SlidingExpirationInSecs);

                var type = TypeHelper.GetTypeByName(typeName);
                var methodInfo = TypeHelper.GetMethodInfoByName(type, methodName, parameterTypes);

                var delegateParameterTypes = new[] { typeof(object), typeof(object[]) };
                var dynInvoc = new DynamicMethod(
                    $"{type.FullName}_{methodInfo.Name}_{TypeHelper.GetTypesIdentity(parameterTypes)}_Invoc",
                    null, delegateParameterTypes, true);

                GenerateIL(dynInvoc, type, methodInfo, parameterTypes);

                return (Action<object, object[]>)dynInvoc
                    .CreateDelegate(typeof(Action<object, object[]>));
            });
        }

        public static Func<object, object[], object> CreateFunc(
            string typeName, string methodName, Type[] parameterTypes = null)
        {
            if (parameterTypes is null)
            {
                parameterTypes = Type.EmptyTypes;
            }
            var key = (typeName, methodName, TypeHelper.GetTypesIdentity(parameterTypes));

            return _cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(SlidingExpirationInSecs);

                var type = TypeHelper.GetTypeByName(typeName);
                var methodInfo = TypeHelper.GetMethodInfoByName(type, methodName, parameterTypes);

                var delegateParameterTypes = new[] { typeof(object), typeof(object[]) };
                var dynInvoc = new DynamicMethod(
                    $"{type.FullName}_{methodInfo.Name}_{TypeHelper.GetTypesIdentity(parameterTypes)}_Invoc",
                    typeof(object), delegateParameterTypes, true);

                GenerateIL(dynInvoc, type, methodInfo, parameterTypes);

                return (Func<object, object[], object>)dynInvoc
                    .CreateDelegate(typeof(Func<object, object[], object>));
            });
        }

        private static void GenerateIL(
            DynamicMethod dynInvoc, Type type, MethodInfo methodInfo, Type[] parameterTypes)
        {
            var il = dynInvoc.GetILGenerator();

            if (methodInfo.IsStatic)
            {
                IlHelper.LoadArguments(il, METHOD_LOAD_INDEX, parameterTypes);
                IlHelper.ExecuteMethod(il, methodInfo, false);
            }
            else
            {
                IlHelper.LoadTarget(il, type);
                IlHelper.LoadArguments(il, METHOD_LOAD_INDEX, parameterTypes);
                IlHelper.ExecuteMethod(il, methodInfo);
            }
        }
    }
}
