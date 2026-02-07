using System;
using Kivancalp.Core.DI;
using NUnit.Framework;

namespace Kivancalp.Core.Tests
{
    [TestFixture]
    public sealed class DiContainerTests
    {
        [Test]
        public void RegisterSingleton_Resolve_ReturnsSameInstance()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ISimpleService, SimpleService>();

            var first = container.Resolve<ISimpleService>();
            var second = container.Resolve<ISimpleService>();

            Assert.AreSame(first, second);
        }

        [Test]
        public void RegisterSingleton_WithFactory_ReturnsSameInstance()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ISimpleService>(_ => new SimpleService());

            var first = container.Resolve<ISimpleService>();
            var second = container.Resolve<ISimpleService>();

            Assert.AreSame(first, second);
        }

        [Test]
        public void RegisterScoped_SameScope_ReturnsSameInstance()
        {
            using var container = new DiContainer();
            container.RegisterScoped<ISimpleService, SimpleService>();

            using var scope = container.CreateScope();
            var first = scope.Resolve<ISimpleService>();
            var second = scope.Resolve<ISimpleService>();

            Assert.AreSame(first, second);
        }

        [Test]
        public void RegisterScoped_DifferentScopes_ReturnsDifferentInstances()
        {
            using var container = new DiContainer();
            container.RegisterScoped<ISimpleService, SimpleService>();

            using var scope1 = container.CreateScope();
            using var scope2 = container.CreateScope();

            var first = scope1.Resolve<ISimpleService>();
            var second = scope2.Resolve<ISimpleService>();

            Assert.AreNotSame(first, second);
        }

        [Test]
        public void RegisterTransient_ReturnsNewInstanceEachTime()
        {
            using var container = new DiContainer();
            container.RegisterTransient<ISimpleService, SimpleService>();

            var first = container.Resolve<ISimpleService>();
            var second = container.Resolve<ISimpleService>();

            Assert.AreNotSame(first, second);
        }

        [Test]
        public void RegisterInstance_ReturnsExactInstance()
        {
            using var container = new DiContainer();
            var instance = new SimpleService();
            container.RegisterInstance<ISimpleService>(instance);

            var resolved = container.Resolve<ISimpleService>();

            Assert.AreSame(instance, resolved);
        }

        [Test]
        public void ConstructorInjection_SelectsConstructorWithMostParameters()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ISimpleService, SimpleService>();
            container.RegisterSingleton<IDependentService, DependentService>();

            var resolved = container.Resolve<IDependentService>() as DependentService;

            Assert.IsNotNull(resolved);
            Assert.IsNotNull(resolved.Dependency);
        }

        [Test]
        public void CircularDependency_ThrowsInvalidOperationException()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ICircularA, CircularA>();
            container.RegisterSingleton<ICircularB, CircularB>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<ICircularA>());
        }

        [Test]
        public void ScopedServiceInSingletonGraph_ThrowsInvalidOperationException()
        {
            using var container = new DiContainer();
            container.RegisterScoped<ISimpleService, SimpleService>();
            container.RegisterSingleton<IDependentService>(r => new DependentService(r.Resolve<ISimpleService>()));

            using var scope = container.CreateScope();

            Assert.Throws<InvalidOperationException>(() => scope.Resolve<IDependentService>());
        }

        [Test]
        public void DuplicateRegistration_ThrowsInvalidOperationException()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ISimpleService, SimpleService>();

            Assert.Throws<InvalidOperationException>(() =>
                container.RegisterSingleton<ISimpleService, SimpleService>());
        }

        [Test]
        public void Dispose_DisposesAllRegisteredDisposables()
        {
            var disposable = new DisposableService();

            var container = new DiContainer();
            container.RegisterInstance<IDisposableService>(disposable);

            Assert.IsFalse(disposable.IsDisposed);

            container.Dispose();

            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void Dispose_ScopedDisposablesDisposedInReverseOrder()
        {
            var disposeLog = new System.Collections.Generic.List<string>();
            var first = new TrackableDisposable(() => disposeLog.Add("first"));
            var second = new TrackableDisposable(() => disposeLog.Add("second"));

            var container = new DiContainer();
            container.RegisterScoped<IFirstService>(_ => first);
            container.RegisterScoped<ISecondService>(_ => second);

            var scope = container.CreateScope();
            scope.Resolve<IFirstService>();
            scope.Resolve<ISecondService>();

            scope.Dispose();

            Assert.AreEqual(2, disposeLog.Count);
            Assert.AreEqual("second", disposeLog[0], "Last registered should be disposed first (reverse order)");
            Assert.AreEqual("first", disposeLog[1], "First registered should be disposed last (reverse order)");
            container.Dispose();
        }

        [Test]
        public void TryResolve_NotRegistered_ReturnsFalse()
        {
            using var container = new DiContainer();

            bool found = container.TryResolve<ISimpleService>(out var service);

            Assert.IsFalse(found);
            Assert.IsNull(service);
        }

        [Test]
        public void TryResolve_Registered_ReturnsTrueWithInstance()
        {
            using var container = new DiContainer();
            container.RegisterSingleton<ISimpleService, SimpleService>();

            bool found = container.TryResolve<ISimpleService>(out var service);

            Assert.IsTrue(found);
            Assert.IsNotNull(service);
        }

        [Test]
        public void Resolve_IResolver_ReturnsSelf()
        {
            using var container = new DiContainer();

            var resolver = container.Resolve<IResolver>();

            Assert.AreSame(container, resolver);
        }

        [Test]
        public void Resolve_IDiContainer_ReturnsSelf()
        {
            using var container = new DiContainer();

            var resolved = container.Resolve<IDiContainer>();

            Assert.AreSame(container, resolved);
        }

        [Test]
        public void Resolve_NotRegistered_ThrowsInvalidOperationException()
        {
            using var container = new DiContainer();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<ISimpleService>());
        }

        [Test]
        public void Resolve_AfterDispose_ThrowsObjectDisposedException()
        {
            var container = new DiContainer();
            container.RegisterSingleton<ISimpleService, SimpleService>();
            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() => container.Resolve<ISimpleService>());
        }

        #region Test Doubles

        private interface ISimpleService { }

        private interface IDependentService { }

        private interface IDisposableService { }

        private interface ICircularA { }

        private interface ICircularB { }

        private interface IFirstService { }

        private interface ISecondService { }

        private sealed class SimpleService : ISimpleService { }

        private sealed class DependentService : IDependentService
        {
            public ISimpleService Dependency { get; }

            public DependentService(ISimpleService dependency)
            {
                Dependency = dependency;
            }
        }

        private sealed class CircularA : ICircularA
        {
            public CircularA(ICircularB b) { }
        }

        private sealed class CircularB : ICircularB
        {
            public CircularB(ICircularA a) { }
        }

        private sealed class DisposableService : IDisposableService, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private sealed class TrackableDisposable : IFirstService, ISecondService, IDisposable
        {
            private readonly Action _onDispose;

            public TrackableDisposable(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose?.Invoke();
            }
        }

        #endregion
    }
}
