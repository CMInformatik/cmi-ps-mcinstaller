using System;
using cmi.mc.config.AspectDependencies;
using cmi.mc.config.SchemaComponents;
using Moq;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
    public class SimpleAspectDependencyTests
    {
        private static Mock<ISimpleAspect> GetAspectMock()
        {
            var aspect = new Mock<ISimpleAspect>();
            aspect.Setup(m => m.GetAspectPath()).Returns("mock");
            aspect.Setup(m => m.Name).Returns("mock");
            return aspect;
        }

        [Test]
        public void Should_TestValue_When_ConstructorIsCalled()
        {
            var mock = GetAspectMock();
            var valueToTest = new object();

            // ReSharper disable once ObjectCreationAsStatement
            new SimpleAspectDependency(App.Common, mock.Object, valueToTest);

            mock.Verify(a => a.TestValue(valueToTest), Times.Once);
        }

        [Test]
        public void Should_Throw_When_AspectIsNull()
        {
            void D() => new SimpleAspectDependency(App.Common, null, new object());
            Assert.Throws(typeof(ArgumentNullException), D);
        }

        [Test]
        public void Should_Throw_When_TenantIsNull()
        {
            var dep = new SimpleAspectDependency(App.Common, (new Mock<ISimpleAspect>().Object), new object());
            void D1() => dep.Verify(null);
            void D2() => dep.Ensure(null);
            Assert.Throws(typeof(ArgumentNullException), D1);
            Assert.Throws(typeof(ArgumentNullException), D2);
        }

        [Test]
        public void Should_Throw_When_ConfigurationPropertyIsNull()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, new object());
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns(null);

            void D1() => dep.Verify(mock.Object);

            Assert.Throws(typeof(AspectDependencyNotFulfilled), D1);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_Throw_When_DesiredAndConfigurationIsNotEqual()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("not some string");

            void D1() => dep.Verify(mock.Object);

            Assert.Throws(typeof(AspectDependencyNotFulfilled), D1);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_ReturnVoid_When_DesiredAndConfigurationPropertyIsNull()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, null);
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns(null);

            dep.Verify(mock.Object);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_ReturnVoid_When_DesiredAndConfigurationPropertyIsEqual()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("some string");

            dep.Verify(mock.Object);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_NotSetProperty_When_DesiredAndConfigurationPropertyIsEqual()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("some string");

            dep.Ensure(mock.Object);
            mock.Verify(m => m.SetConfigurationProperty(It.IsAny<App>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void Should_SetProperty_When_DesiredAndConfigurationIsNotEqual()
        {
            var dep = new SimpleAspectDependency(App.Common, GetAspectMock().Object, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("not some string");

            dep.Ensure(mock.Object);
            mock.Verify(m => m.SetConfigurationProperty(App.Common, "mock", It.IsAny<object>(), true), Times.Once);
        }
    }
}
