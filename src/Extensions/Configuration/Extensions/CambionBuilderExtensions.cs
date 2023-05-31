using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Attributes;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Extensions.Configuration.Extensions
{
    public static class CambionBuilderExtensions
    {
        public static ICambionBuilder ReadSettings(this ICambionBuilder builder, IConfiguration configuration, string configurationKey = "Cambion")
        {
            var config = configuration.GetSection(configurationKey).Get<CambionConfig>();

            if (config.Transport?.Name != null)
            {
                RuntimeLibrary transportLibrary = DependencyContext.Default?.RuntimeLibraries.FirstOrDefault(x => x.Name.Equals(config.Transport.Name, StringComparison.OrdinalIgnoreCase));

                if (transportLibrary == null)
                {
                    throw new Exception($"Transport assembly ({config.Transport.Name}) not found");
                }

                AddTransport(builder.Services, configuration, configurationKey, transportLibrary);
            }

            if (config.Serializer?.Name != null)
            {
                RuntimeLibrary serializerLibrary = DependencyContext.Default?.RuntimeLibraries.FirstOrDefault(x => x.Name.Equals(config.Serializer.Name, StringComparison.OrdinalIgnoreCase));

                if (serializerLibrary != null)
                {
                    AddSerializer(builder.Services, configuration, configurationKey, serializerLibrary);
                }
            }

            return builder;
        }

        public static void AddTransport(IServiceCollection services, IConfiguration configuration, string configurationKey, RuntimeLibrary transportLibrary)
        {
            // ReSharper disable once AssignNullToNotNullAttribute because this is known not to be NULL from line 21 and 24
            IEnumerable<AssemblyName> assemblyNames = transportLibrary.GetDefaultAssemblyNames(DependencyContext.Default);

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);

                // First find the configuration type for the transport (marked with the [CambionConfiguration] attribute)
                Type transportLibraryConfigType = assembly.DefinedTypes.FirstOrDefault(x => x.GetCustomAttribute<CambionConfigurationAttribute>() != null);

                // Can't use generic methods directly, so use reflection to invoke them
                MethodInfo addOptionsMethod = typeof(OptionsServiceCollectionExtensions).GetMethods().First(m => m.Name == "AddOptions" && m.IsStatic && m.ContainsGenericParameters && m.GetParameters().Length == 1);
                MethodInfo addOptionsMethodGeneric = addOptionsMethod.MakeGenericMethod(transportLibraryConfigType);
                object optionsBuilder = addOptionsMethodGeneric.Invoke(null, new object[] { services });

                MethodInfo bindMethod = typeof(OptionsBuilderConfigurationExtensions).GetMethods().First(m => m.Name == "Bind" && m.IsStatic && m.ContainsGenericParameters && m.GetParameters().Length == 2);
                MethodInfo bindMethodGeneric = bindMethod.MakeGenericMethod(transportLibraryConfigType);
                bindMethodGeneric.Invoke(null, new[] { optionsBuilder, configuration.GetSection(configurationKey).GetSection(CambionConfigTransport.Key).GetSection(CambionConfigTransport.ConfigurationKey) });

                // Finally handle the transport itself
                Type transportLibraryTransportType = assembly.DefinedTypes.FirstOrDefault(x => x.ImplementedInterfaces.Contains(typeof(ITransport)));

                // Don't want to use null coalescing here as the line of code would be very long, and it is more readable the way it is.
                if (transportLibraryTransportType == null)
                {
                    throw new InvalidOperationException($"Assembly '{assemblyName}' does not contain a compatible '{nameof(ITransport)}' implementation.");
                }

                services.Replace(new ServiceDescriptor(typeof(ITransport), transportLibraryTransportType, ServiceLifetime.Singleton));
            }
        }

        public static void AddSerializer(IServiceCollection services, IConfiguration configuration, string configurationKey, RuntimeLibrary serializerLibrary)
        {
        }
    }
}
