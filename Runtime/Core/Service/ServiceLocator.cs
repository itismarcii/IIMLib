using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IIMLib.Core
{
    public static class ServiceLocator
    {
        private static readonly HashSet<(Type, IService)> ServiceList  = new();
        private static readonly ConcurrentDictionary<Type, IService> ServiceDictionary = new();
        
        public static void Initialize(in IServiceConfig config)
        {
            LoadConfig(config);
            
            var total = ServiceList.Count;
            var completed = 0;

            foreach (var service in ServiceList)
            {
                service.Item2.Initialize();
                completed++;
                var serviceName = service.Item1.ToString();
                Debug.Log($"Service: {serviceName[(serviceName.LastIndexOf(".", StringComparison.Ordinal) + 2)..]} Completed. Progress: {completed * 100 / total}%");
            }
        }
        
        public static async Task InitializeAsync(IServiceConfig config, IProgress<(Type, int)> progress = null)
        {
            LoadConfig(config);
            
            var total = ServiceList.Count;
            var completed = 0;

            foreach (var service in ServiceList)
            {
                await Task.Run(service.Item2.Initialize);
                completed++;
                progress?.Report((service.Item1, completed * 100 / total));
            }
        }
        
        private static void LoadConfig(in IServiceConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("No service config found!");
                return;
            }
            
            ServiceList.Clear();
            ServiceDictionary.Clear();

            foreach (var (type, service) in config.ServiceList)
            {
                if(!type.IsInterface) Debug.LogWarning($"The key [{type}] should be an interface");
                if(ServiceList.Add((type, service))) ServiceDictionary[type] = service;
            }
        }
        
        public static T Get<T>() where T : class, IService => (T)ServiceDictionary[typeof(T)];
        
        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (ServiceDictionary.TryGetValue(typeof(T), out var output) && output is T typedService)
            {
                service = typedService;
                return true;
            }
            
            service = null;
            return false;
        }
    }
}