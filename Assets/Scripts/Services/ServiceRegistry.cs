using System;
using System.Collections.Generic;

namespace GameServices
{
    /// <summary>
    /// A simple service locator for managing instances of game services.
    /// </summary>
    public class ServiceRegistry
    {
        /// <summary>
        /// Dictionary to hold registered service instances.
        /// </summary>
        private readonly Dictionary<string, IGameService> serviceDictionary = new Dictionary<string, IGameService>();

        public static ServiceRegistry Instance { get; private set; }

        public static void Initialize()
        {
            Instance = new ServiceRegistry();
        }

        /// <summary>
        /// Retrieves a service instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>The instance of the requested service.</returns>
        public T GetService<T>() where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (!serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is not registered.");
            }

            return (T)serviceDictionary[serviceName];
        }

        /// <summary>
        /// Registers a service instance with the service registry.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public void RegisterService<T>(T service) where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is already registered.");
            }

            serviceDictionary.Add(serviceName, service);
        }

        /// <summary>
        /// Unregisters a service instance from the service registry.
        /// </summary>
        /// <typeparam name="T">The type of service to unregister.</typeparam>
        public void UnregisterService<T>() where T : IGameService
        {
            string serviceName = typeof(T).Name;
            if (!serviceDictionary.ContainsKey(serviceName))
            {
                throw new InvalidOperationException($"Service of type {serviceName} is not registered.");
            }

            serviceDictionary.Remove(serviceName);
        }
    }
}
