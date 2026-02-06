using System;

namespace Kivancalp.Core.DI
{
    public interface IResolver
    {
        T Resolve<T>();

        object Resolve(Type serviceType);

        bool TryResolve<T>(out T service);
    }
}
