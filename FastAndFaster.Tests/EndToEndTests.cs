using FastAndFaster.Tests.DummyClasses;
using FluentAssertions;
using Xunit;

namespace FastAndFaster.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public void ShouldCreateInstanceAndCallMethod()
        {
            // Arrange, Act
            var typeName = typeof(InvocationTarget).AssemblyQualifiedName;
            var methodName = nameof(InvocationTarget.ParameterlessAction);
            var initializer = Initializer.Create(typeName);
            var invocator = Invocator.CreateAction(typeName, methodName);

            // Act
            var target = initializer(null) as InvocationTarget;
            invocator(target, null);

            // Assert
            target.Should().NotBeNull();
            target.ParameterlessActionCalled.Should().BeTrue();
        }
    }
}
