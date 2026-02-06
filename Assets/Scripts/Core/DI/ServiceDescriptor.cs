using System;

namespace Kivancalp.Core.DI
{
    internal sealed class ServiceDescriptor
    {
        public ServiceDescriptor(Type serviceType, ServiceLifetime lifetime, Func<IResolver, object> factory)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Factory = factory;
        }

        public Type ServiceType { get; }

        public ServiceLifetime Lifetime { get; }

        public Func<IResolver, object> Factory { get; }
    }
}
