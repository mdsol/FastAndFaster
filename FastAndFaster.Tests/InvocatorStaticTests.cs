using FastAndFaster.Tests.DummyClasses;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace FastAndFaster.Tests
{
    public class InvocatorStaticTests
    {
        [Fact]
        public void ShouldCallParameterlessAction()
        {
            // Arrange
            InvocationStaticTarget.ActionParameterlessCalled = false;
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationStaticTarget.ActionParameterless);
            var invocator = Invocator.CreateAction(typeName, methodName);

            // Act
            invocator(null, null);

            // Assert
            InvocationStaticTarget.ActionParameterlessCalled.Should().BeTrue();
        }

        [Fact]
        public void ShouldCallAction_ValueTypeParameter()
        {
            // Arrange
            InvocationStaticTarget.ActionHandleValData = 0;
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationStaticTarget.ActionHandleVal);
            var parameterTypes = new[] { typeof(int), typeof(int) };
            var first = 17;
            var second = 89;
            var parameters = new object[] { first, second };
            var expectedResult = 17 + 89;
            var invocator = Invocator.CreateAction(typeName, methodName, parameterTypes);

            // Act
            invocator(null, parameters);

            // Assert
            InvocationStaticTarget.ActionHandleValData.Should().Be(expectedResult);
        }

        [Fact]
        public void ShouldCallAction_ReferenceTypeParameter()
        {
            // Arrange
            InvocationStaticTarget.ActionHandleRefData = null;
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationStaticTarget.ActionHandleRef);
            var parameterTypes = new[] { typeof(List<string>), typeof(string) };
            var parameters = new object[]
            {
                new List<string> { "Duong", "Luy" },
                "Nana"
            };
            var expectedResult = new List<string> { "Duong", "Luy", "Nana" };
            var invocator = Invocator.CreateAction(typeName, methodName, parameterTypes);

            // Act
            invocator(null, parameters);

            // Assert
            InvocationStaticTarget.ActionHandleRefData.Should().BeEquivalentTo(expectedResult);
        }

        public static IEnumerable<object[]> StaticFuncData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    nameof(InvocationStaticTarget.FuncHandleVal),
                    new[] { typeof(int), typeof(int) },
                    new object[] { 17, 89 },
                    17 + 89
                },
                new object[]
                {
                    nameof(InvocationStaticTarget.FuncHandleRef),
                    new[] { typeof(List<string>), typeof(string) },
                    new object[]
                    {
                        new List<string> { "Duong", "Luy" },
                        "Nana"
                    },
                    new List<string> { "Duong", "Luy", "Nana" }
                },
                new object[]
                {
                    nameof(InvocationStaticTarget.FuncParameterless),
                    Type.EmptyTypes,
                    null,
                    InvocationStaticTarget.FuncParameterlessData
                }
            };
        }

        [Theory]
        [MemberData(nameof(StaticFuncData))]
        public void ShouldCallStaticFunc(
            string methodName, Type[] parameterTypes, object[] parameters, object expectedResult)
        {
            // Arrange
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var invocator = Invocator.CreateFunc(typeName, methodName, parameterTypes);

            // Act
            var rs = invocator(null, parameters);

            // Assert
            rs.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void ShouldCallStaticFunc_Interface()
        {
            // Arrange
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationStaticTarget.FuncWithInterface);
            var argumentTypes = new[] { typeof(IEnumerable<string>) };
            var names = new List<string> { "First", "Second", "Third", "Fourth" };
            var arguments = new object[] { names };
            var expectedRs = names.Count;
            var invocator = Invocator.CreateFunc(typeName, methodName, argumentTypes);

            // Act
            var rs = invocator(null, arguments);

            // Assert
            rs.Should().BeEquivalentTo(expectedRs);
        }

        [Fact]
        public void ShouldCallStaticAction_Interface()
        {
            // Arrange
            var typeName = typeof(InvocationStaticTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationStaticTarget.ActionWithInterface);
            var argumentTypes = new[] { typeof(int) };
            var arguments = new object[] { 5 };
            var expectedRs = new List<int> { 0, 1, 2, 3, 4 };
            var invocator = Invocator.CreateAction(typeName, methodName, argumentTypes);

            // Act
            invocator(null, arguments);

            // Assert
            InvocationStaticTarget.InterfaceTestData.Should().BeEquivalentTo(expectedRs);
        }
    }
}
