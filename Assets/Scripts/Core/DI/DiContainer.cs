using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kivancalp.Core.DI
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly DiContainer _root;
        private readonly Dictionary<Type, ServiceDescriptor> _registrations;
        private readonly Dictionary<Type, object> _singletonInstances;
        private readonly List<IDisposable> _singletonDisposables;
        private readonly Dictionary<Type, object> _scopedInstances;
        private readonly List<IDisposable> _scopeDisposables;
        private readonly Stack<Type> _resolutionPath;
        private readonly Stack<ServiceLifetime> _resolutionLifetimes;
        private readonly bool _isRoot;
        private bool _disposed;

        public DiContainer()
        {
            _root = this;
            _registrations = new Dictionary<Type, ServiceDescriptor>(64);
            _singletonInstances = new Dictionary<Type, object>(64);
            _singletonDisposables = new List<IDisposable>(64);
            _scopedInstances = new Dictionary<Type, object>(32);
            _scopeDisposables = new List<IDisposable>(32);
            _resolutionPath = new Stack<Type>(16);
            _resolutionLifetimes = new Stack<ServiceLifetime>(16);
            _isRoot = true;
        }

        private DiContainer(DiContainer root)
        {
            _root = root;
            _registrations = root._registrations;
            _singletonInstances = root._singletonInstances;
            _singletonDisposables = root._singletonDisposables;
            _scopedInstances = new Dictionary<Type, object>(32);
            _scopeDisposables = new List<IDisposable>(32);
            _resolutionPath = new Stack<Type>(16);
            _resolutionLifetimes = new Stack<ServiceLifetime>(16);
            _isRoot = false;
        }

        public void RegisterSingleton<TService>(Func<IResolver, TService> factory)
            where TService : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Register(typeof(TService), ServiceLifetime.Singleton, resolver => factory(resolver));
        }

        public void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            RegisterSingleton<TService>(ConstructorFactory<TImplementation>.Create);
        }

        public void RegisterScoped<TService>(Func<IResolver, TService> factory)
            where TService : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Register(typeof(TService), ServiceLifetime.Scoped, resolver => factory(resolver));
        }

        public void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            RegisterScoped<TService>(ConstructorFactory<TImplementation>.Create);
        }

        public void RegisterTransient<TService>(Func<IResolver, TService> factory)
            where TService : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Register(typeof(TService), ServiceLifetime.Transient, resolver => factory(resolver));
        }

        public void RegisterTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            RegisterTransient<TService>(ConstructorFactory<TImplementation>.Create);
        }

        public void RegisterInstance<TService>(TService instance)
            where TService : class
        {
            EnsureRegistrationIsAllowed(typeof(TService));

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var serviceType = typeof(TService);
            _registrations.Add(serviceType, new ServiceDescriptor(serviceType, ServiceLifetime.Singleton, _ => instance));
            _singletonInstances.Add(serviceType, instance);

            if (instance is IDisposable disposable)
            {
                _singletonDisposables.Add(disposable);
            }
        }

        public IDiContainer CreateScope()
        {
            ThrowIfDisposed();
            return new DiContainer(_root);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            ThrowIfDisposed();

            if (serviceType == typeof(IResolver) || serviceType == typeof(IDiContainer))
            {
                return this;
            }

            if (!_root._registrations.TryGetValue(serviceType, out ServiceDescriptor descriptor))
            {
                throw new InvalidOperationException("Service is not registered: " + serviceType.FullName);
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    if (!_root._singletonInstances.TryGetValue(serviceType, out object singleton))
                    {
                        singleton = CreateInstance(serviceType, descriptor);
                        _root._singletonInstances.Add(serviceType, singleton);
                    }

                    return singleton;

                case ServiceLifetime.Scoped:
                    if (IsSingletonResolutionActive())
                    {
                        throw new InvalidOperationException("Cannot resolve scoped service while building a singleton dependency graph: " + serviceType.FullName);
                    }

                    if (!_scopedInstances.TryGetValue(serviceType, out object scoped))
                    {
                        scoped = CreateInstance(serviceType, descriptor);
                        _scopedInstances.Add(serviceType, scoped);
                    }

                    return scoped;

                case ServiceLifetime.Transient:
                    return CreateInstance(serviceType, descriptor);

                default:
                    throw new InvalidOperationException("Unsupported service lifetime: " + descriptor.Lifetime);
            }
        }

        public bool TryResolve<T>(out T service)
        {
            ThrowIfDisposed();
            var serviceType = typeof(T);

            if (serviceType == typeof(IResolver) || serviceType == typeof(IDiContainer))
            {
                service = (T)(object)this;
                return true;
            }

            if (!_root._registrations.ContainsKey(serviceType))
            {
                service = default;
                return false;
            }

            try
            {
                service = Resolve<T>();
                return true;
            }
            catch (InvalidOperationException)
            {
                service = default;
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            DisposeDisposables(_scopeDisposables);
            _scopeDisposables.Clear();
            _scopedInstances.Clear();

            if (_isRoot)
            {
                DisposeDisposables(_singletonDisposables);
                _singletonDisposables.Clear();
                _singletonInstances.Clear();
                _registrations.Clear();
            }
        }

        private void Register(Type serviceType, ServiceLifetime lifetime, Func<IResolver, object> factory)
        {
            EnsureRegistrationIsAllowed(serviceType);
            _registrations.Add(serviceType, new ServiceDescriptor(serviceType, lifetime, factory));
        }

        private void EnsureRegistrationIsAllowed(Type serviceType)
        {
            ThrowIfDisposed();

            if (!_isRoot)
            {
                throw new InvalidOperationException("Registrations are only allowed in the root container.");
            }

            if (_registrations.ContainsKey(serviceType))
            {
                throw new InvalidOperationException("Service is already registered: " + serviceType.FullName);
            }
        }

        private object CreateInstance(Type serviceType, ServiceDescriptor descriptor)
        {
            if (_resolutionPath.Contains(serviceType))
            {
                throw new InvalidOperationException("Circular dependency detected while resolving: " + serviceType.FullName);
            }

            _resolutionPath.Push(serviceType);
            _resolutionLifetimes.Push(descriptor.Lifetime);

            try
            {
                object instance = descriptor.Factory(this);

                if (instance == null)
                {
                    throw new InvalidOperationException("Factory returned null for service: " + serviceType.FullName);
                }

                if (instance is IDisposable disposable)
                {
                    if (descriptor.Lifetime == ServiceLifetime.Singleton)
                    {
                        _root._singletonDisposables.Add(disposable);
                    }
                    else
                    {
                        _scopeDisposables.Add(disposable);
                    }
                }

                return instance;
            }
            finally
            {
                _resolutionLifetimes.Pop();
                _resolutionPath.Pop();
            }
        }

        private bool IsSingletonResolutionActive()
        {
            foreach (ServiceLifetime lifetime in _resolutionLifetimes)
            {
                if (lifetime == ServiceLifetime.Singleton)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DisposeDisposables(List<IDisposable> disposables)
        {
            for (int index = disposables.Count - 1; index >= 0; index -= 1)
            {
                disposables[index].Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DiContainer));
            }
        }

        private static class ConstructorFactory<TImplementation>
            where TImplementation : class
        {
            private static readonly ConstructorInfo CachedConstructor = SelectConstructor();
            private static readonly ParameterInfo[] CachedParameters = CachedConstructor.GetParameters();

            public static TImplementation Create(IResolver resolver)
            {
                if (CachedParameters.Length == 0)
                {
                    return (TImplementation)CachedConstructor.Invoke(Array.Empty<object>());
                }

                object[] arguments = new object[CachedParameters.Length];

                for (int index = 0; index < CachedParameters.Length; index += 1)
                {
                    arguments[index] = resolver.Resolve(CachedParameters[index].ParameterType);
                }

                return (TImplementation)CachedConstructor.Invoke(arguments);
            }

            private static ConstructorInfo SelectConstructor()
            {
                ConstructorInfo[] constructors = typeof(TImplementation).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                if (constructors.Length == 0)
                {
                    throw new InvalidOperationException("No public constructor found for type: " + typeof(TImplementation).FullName);
                }

                ConstructorInfo selectedConstructor = constructors[0];
                int maxParameterCount = selectedConstructor.GetParameters().Length;

                for (int index = 1; index < constructors.Length; index += 1)
                {
                    ConstructorInfo constructor = constructors[index];
                    int parameterCount = constructor.GetParameters().Length;

                    if (parameterCount > maxParameterCount)
                    {
                        selectedConstructor = constructor;
                        maxParameterCount = parameterCount;
                    }
                }

                return selectedConstructor;
            }
        }
    }
}
