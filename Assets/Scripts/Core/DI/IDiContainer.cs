using System;

namespace Kivancalp.Core.DI
{
    public interface IDiContainer : IResolver, IDisposable
    {
        void RegisterSingleton<TService>(Func<IResolver, TService> factory)
            where TService : class;

        void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterScoped<TService>(Func<IResolver, TService> factory)
            where TService : class;

        void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterTransient<TService>(Func<IResolver, TService> factory)
            where TService : class;

        void RegisterTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterInstance<TService>(TService instance)
            where TService : class;

        IDiContainer CreateScope();
    }
}
