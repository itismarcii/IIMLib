using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IIMLib.Core
{
    public static class ServiceLocator
    {
        private static readonly List<(Type Type, IService Service)> ServiceList = new();
        private static readonly Dictionary<Type, IService> ServiceDictionary = new();

        public static void Initialize(IServiceConfig config)
        {
            LoadConfig(config);

            var total = ServiceList.Count;
            for (var i = 0; i < total; i++)
            {
                var (type, service) = ServiceList[i];
                service.Initialize();

                var progress = total == 0 ? 1f : (i + 1f) / total;
                var serviceName = string.IsNullOrEmpty(service.IdentifierName) ? type.Name : service.IdentifierName;
                Debug.Log($"Service: {serviceName} initialized ({progress:P0}).");
            }
        }

        public static async Task InitializeAsync(IServiceConfig config)
        {
            LoadConfig(config);

            var total = ServiceList.Count;
            for (var i = 0; i < total; i++)
            {
                var (type, service) = ServiceList[i];

                // Unity-facing IService implementations must stay on the caller thread.
                // Yielding cooperatively avoids Task.Run/thread-pool violations.
                service.Initialize();

                var progress = total == 0 ? 1f : (i + 1f) / total;
                Debug.Log($"Service: {type.Name} initialized ({progress:P0}).");

                if (i + 1 < total)
                    await Task.Yield();
            }
        }

        public static T Get<T>() where T : class, IService
        {
            if (TryGet<T>(out var service))
                return service;

            throw new KeyNotFoundException($"Service '{typeof(T).FullName}' is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (ServiceDictionary.TryGetValue(typeof(T), out var value))
            {
                service = value as T;
                return service != null;
            }

            service = null;
            return false;
        }

        private static void LoadConfig(IServiceConfig config)
        {
            ServiceList.Clear();
            ServiceDictionary.Clear();

            if (config == null)
            {
                Debug.LogWarning("Service configuration is null. No services were registered.");
                return;
            }

            var services = config.ServiceList;
            if (services == null)
            {
                Debug.LogWarning("Service configuration returned a null service list.");
                return;
            }

            foreach (var (type, service) in services)
            {
                if (type == null)
                    throw new InvalidOperationException("Service configuration contains a null service type.");

                if (service == null)
                    throw new InvalidOperationException($"Service '{type.FullName}' has a null implementation.");

                if (!type.IsInterface)
                    Debug.LogWarning($"Service key '{type.FullName}' is not an interface.");

                if (!type.IsInstanceOfType(service))
                {
                    throw new InvalidOperationException(
                        $"Service implementation '{service.GetType().FullName}' does not implement/derive from '{type.FullName}'.");
                }

                if (ServiceDictionary.ContainsKey(type))
                    throw new InvalidOperationException($"Service '{type.FullName}' is registered more than once.");

                ServiceList.Add((type, service));
                ServiceDictionary.Add(type, service);
            }
        }
    }
}
