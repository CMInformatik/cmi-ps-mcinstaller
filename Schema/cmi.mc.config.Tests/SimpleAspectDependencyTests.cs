﻿using System;
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

            mock.Verify(a => a.TestValue(valueToTest, null), Times.Once);
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
            var mock = new Mock<ISimpleAspect>().Object;
            var dep = new SimpleAspectDependency(App.Common, mock, new object());
            void D1() => dep.Verify(null, App.Common, mock);
            void D2() => dep.Ensure(null, App.Common, mock);
            Assert.Throws(typeof(ArgumentNullException), D1);
            Assert.Throws(typeof(ArgumentNullException), D2);
        }

        [Test]
        public void Should_Throw_When_ConfigurationPropertyIsNull()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, new object());
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns(null);

            void D1() => dep.Verify(mock.Object, App.Common, aspect);

            Assert.Throws(typeof(AspectDependencyNotFulfilled), D1);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_Throw_When_DesiredAndConfigurationIsNotEqual()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("not some string");

            void D1() => dep.Verify(mock.Object, App.Common, aspect);

            Assert.Throws(typeof(AspectDependencyNotFulfilled), D1);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_ReturnVoid_When_DesiredAndConfigurationPropertyIsNull()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, null);
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns(null);

            dep.Verify(mock.Object, App.Common, aspect);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_ReturnVoid_When_DesiredAndConfigurationPropertyIsEqual()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("some string");

            dep.Verify(mock.Object, App.Common, aspect);
            mock.Verify(m => m.GetConfigurationProperty(App.Common, "mock"));
        }

        [Test]
        public void Should_NotSetProperty_When_DesiredAndConfigurationPropertyIsEqual()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("some string");

            dep.Ensure(mock.Object, App.Common, aspect);
            mock.Verify(m => m.SetConfigurationProperty(It.IsAny<App>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void Should_SetProperty_When_DesiredAndConfigurationIsNotEqual()
        {
            var aspect = GetAspectMock().Object;
            var dep = new SimpleAspectDependency(App.Common, aspect, "some string");
            var mock = new Mock<ITenant>();
            mock.Setup(m => m.GetConfigurationProperty(App.Common, "mock")).Returns("not some string");

            dep.Ensure(mock.Object, App.Common, aspect);
            mock.Verify(m => m.SetConfigurationProperty(App.Common, "mock", It.IsAny<object>(), true), Times.Once);
        }
    }
}
