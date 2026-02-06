using System;
using Kivancalp.Core.DI;
using NUnit.Framework;

namespace Kivancalp.Tests.EditMode
{
    public sealed class DiContainerTests
    {
        [Test]
        public void Singleton_ShouldBeSharedAcrossScopes()
        {
            var container = new DiContainer();
            container.RegisterSingleton<SingletonService, SingletonService>();

            IDiContainer scopeA = container.CreateScope();
            IDiContainer scopeB = container.CreateScope();

            SingletonService fromA = scopeA.Resolve<SingletonService>();
            SingletonService fromB = scopeB.Resolve<SingletonService>();

            Assert.That(fromA, Is.SameAs(fromB));

            scopeA.Dispose();
            scopeB.Dispose();
            container.Dispose();
        }

        [Test]
        public void Scoped_ShouldBeUniquePerScope()
        {
            var container = new DiContainer();
            container.RegisterScoped<ScopedService, ScopedService>();

            IDiContainer scopeA = container.CreateScope();
            IDiContainer scopeB = container.CreateScope();

            ScopedService fromA = scopeA.Resolve<ScopedService>();
            ScopedService fromB = scopeB.Resolve<ScopedService>();

            Assert.That(fromA, Is.Not.SameAs(fromB));

            scopeA.Dispose();
            scopeB.Dispose();
            container.Dispose();
        }

        [Test]
        public void ScopedDisposable_ShouldDisposeWithScope()
        {
            var container = new DiContainer();
            container.RegisterScoped<DisposableScopedService, DisposableScopedService>();

            IDiContainer scope = container.CreateScope();
            DisposableScopedService scopedService = scope.Resolve<DisposableScopedService>();

            Assert.That(scopedService.IsDisposed, Is.False);

            scope.Dispose();

            Assert.That(scopedService.IsDisposed, Is.True);

            container.Dispose();
        }

        private sealed class SingletonService
        {
        }

        private sealed class ScopedService
        {
        }

        private sealed class DisposableScopedService : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
