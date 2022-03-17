using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FastAndFaster.Helpers
{
    internal static class TypeHelper
    {
        internal static Type GetTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type is null)
            {
                throw new ArgumentException(
                    $"Cannot find any class with Assembly Qualified Name: {typeName}");
            }
            return type;
        }

        internal static ConstructorInfo GetConstructorInfoByType(Type type, Type[] parameterTypes)
        {
            var constructorInfo = type.GetConstructor(parameterTypes);
            if (constructorInfo is null)
            {
                throw new ArgumentException(
                    $"Cannot find constructor with given signature for class: {type.AssemblyQualifiedName}");
            }
            return constructorInfo;
        }

        internal static MethodInfo GetMethodInfoByName(
            Type type, string methodName, Type[] parameterTypes, GenericInfo genericInfo)
        {
            MethodInfo methodInfo = null;
            if (genericInfo is null)
            {
                methodInfo = type.GetMethod(methodName, parameterTypes);
            }
            else
            {
                var candidates = type.GetMethods().Where(n => n.Name == methodName && n.IsGenericMethod);
                methodInfo = FilterMethodByParameterTypes(candidates, parameterTypes, genericInfo);
                methodInfo = methodInfo?.MakeGenericMethod(genericInfo.GenericType);
            }

            if (methodInfo is null)
            {
                throw new ArgumentException($"Cannot find method [{methodName}] with the given signature");
            }
            return methodInfo;
        }

        /// <summary>
        /// Concatenate the names of all parameter types into a string,
        /// then use the hash code of that string as part of the method's identity.
        /// We cannot use the hashcode of "Type[] types" directly because
        /// 2 different Type arrays with the exact content still hash to 2 different values.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        internal static int GetParameterTypesIdentity(Type[] types, GenericInfo genericInfo = null)
        {
            var sb = new StringBuilder();
            foreach (var type in types)
            {
                sb.Append(type.AssemblyQualifiedName);
            }
            if (genericInfo is object)
            {
                sb.Append("_GenericTypeIndex_");
                foreach (var index in genericInfo.GenericTypeIndex)
                {
                    sb.Append(index);
                    sb.Append(",");
                }
                sb.Append("_GenericType_");
                foreach (var type in genericInfo.GenericType)
                {
                    sb.Append(type.AssemblyQualifiedName);
                }
            }
            var concatenateName = sb.ToString();
            return concatenateName.GetHashCode();
        }

        private static MethodInfo FilterMethodByParameterTypes(
            IEnumerable<MethodInfo> methods, Type[] parameterTypes, GenericInfo genericInfo)
        {
            foreach (var method in methods)
            {
                if (method.GetGenericArguments().Length != genericInfo.GenericType.Length)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != parameterTypes.Length)
                {
                    continue;
                }

                if (IsTypesMatch(parameters, parameterTypes, genericInfo.GenericTypeIndex))
                {
                    return method;
                }
            }

            return null;
        }

        private static bool IsTypesMatch(ParameterInfo[] parameters, Type[] parameterTypes, int[] genericTypeIndex)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var genericParamNotExistInIndex =
                    parameters[i].ParameterType.IsGenericParameter && !genericTypeIndex.Contains(i);
                var nonGenericParamExistsInIndex =
                    !parameters[i].ParameterType.IsGenericParameter && genericTypeIndex.Contains(i);
                var typeOfNoneGenericParamNotMatch =
                    !parameters[i].ParameterType.IsGenericParameter && parameters[i].ParameterType != parameterTypes[i];

                if (genericParamNotExistInIndex || nonGenericParamExistsInIndex || typeOfNoneGenericParamNotMatch)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
