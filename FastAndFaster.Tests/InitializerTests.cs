using FastAndFaster.Tests.DummyClasses;
using FluentAssertions;
using Xunit;

namespace FastAndFaster.Tests
{
    public class InitializerTests
    {
        [Fact]
        public void ShouldCreateObject_OneParameterlessConstructor()
        {
            // Arrange
            var assemblyQualifiedName = "FastAndFaster.Tests.DummyClasses.OneParameterlessConstructor, " +
                "FastAndFaster.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            // Act
            var cls = Initializer.Create(assemblyQualifiedName) as OneParameterlessConstructor;

            // Assert
            cls.Should().NotBeNull();
            cls.DummyField.Should().Be(17);
        }
    }
}
