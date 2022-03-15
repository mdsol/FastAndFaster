using System;
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

        internal static MethodInfo GetMethodInfoByName(Type type, string methodName, Type[] parameterTypes)
        {
            var methodInfo = type.GetMethod(methodName, parameterTypes);
            if (methodInfo is null)
            {
                throw new ArgumentException($"Cannot find method [{methodName}] with the given signature");
            }
            return methodInfo;
        }

        internal static int GetTypesIdentity(Type[] types)
        {
            var sb = new StringBuilder();
            foreach (var type in types)
            {
                sb.Append(type.AssemblyQualifiedName);
            }
            var concatenateName = sb.ToString();
            return concatenateName.GetHashCode();
        }
    }
}
