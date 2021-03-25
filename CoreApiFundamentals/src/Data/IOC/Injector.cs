using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data.IOC
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        /// <summary>
        /// Indicates Service lifetime
        /// </summary>
        public readonly ServiceLifetime ServiceLifetime;

        /// <summary>
        /// Indicates which type is going to be created/instatiated
        /// </summary>
        public readonly Type[] ServiceTypes;

        /// <summary>
        /// Indicates if type can be instatiated multiple time
        /// </summary>
        public bool AllowDuplicate { get; set; } = false;

        public AutoRegisterAttribute(
            ServiceLifetime serviceLifetime,
            params Type[] serviceTypes
            )
        {
            ServiceLifetime = serviceLifetime;
            ServiceTypes = serviceTypes;
        }

    }

    /// <summary>
    /// Indicates if interface can be instatiated multiple times
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class AutoRegisterUsageAttribute : Attribute
    {

        public AutoRegisterUsageAttribute()
        {

        }

        public bool AllowMultiple { get; set; } = false;
    }

    public static class Injector
    {
        public static void AutoRegisterServices(this IServiceCollection services, Assembly[] assemblies)
        {

            var items = GetTypesToRegister(assemblies);

            foreach (var item in items)
                services.RegisterType(item.typeInfo, item.attribute);

        }

        public static Type[] GetInterfaces(TypeInfo implementationType, AutoRegisterAttribute attribute)
        {

            var allInterfaces = implementationType.GetInterfaces();

            if (attribute.ServiceTypes != default && attribute.ServiceTypes.Length > 0)
            {
                ValidateInterfaces(implementationType, attribute);
                return attribute.ServiceTypes;
            }

            return allInterfaces.Count() == 0
                ? new[] { implementationType }
                : allInterfaces
                ;
        }

        public static void ValidateInterfaces(TypeInfo implementationType, AutoRegisterAttribute attribute)
        {

            var invalidInterfaces = attribute.ServiceTypes.Where(x => !x.IsAssignableFrom(implementationType));

            if (invalidInterfaces.Count() > 0)
                throw new InvalidOperationException(
                    string.Concat(
                        Environment.NewLine,
                        $"[{implementationType.FullName}] does not implement the following types:",
                        string.Concat(
                            invalidInterfaces.Select(x => string.Concat(Environment.NewLine, $" - {x.FullName}"))
                            ),
                        Environment.NewLine
                        ));

        }

        public static IEnumerable<(TypeInfo typeInfo, AutoRegisterAttribute attribute)> GetTypesToRegister(Assembly[] assemblies)
            => from assembly in assemblies
               from type in assembly.DefinedTypes
               let attribute = type.GetCustomAttribute<AutoRegisterAttribute>()
               where attribute != null
               select (type, attribute);

        public static void RegisterType(this IServiceCollection services, TypeInfo implementationType, AutoRegisterAttribute attribute)
        {

            var interfaces = GetInterfaces(implementationType, attribute);

            foreach (var serviceType in interfaces)
            {

                if (!attribute.AllowDuplicate)
                {
                    // check for duplicates
                    var usageAttribute = serviceType.GetCustomAttribute<AutoRegisterUsageAttribute>();
                    var allowMultiple = !(usageAttribute?.AllowMultiple).IsDefault();
                    if (!allowMultiple && services.Any(x => x.ServiceType == serviceType))
                        throw new InvalidOperationException($"Multiple registrations of type [{serviceType.FullName}] not allowed.");
                }

                services.Add(new ServiceDescriptor(
                    serviceType,
                    implementationType,
                    attribute.ServiceLifetime
                    ));

            }
        }
    }

    public static class NullableExtensions
    {

        public static bool IsDefault<T>(this T? nullable)
            where T : struct
        {

            return !nullable.HasValue || nullable.Value.Equals(default(T));

        }
    }

    public static class AutoRegisterHostBuilderExtensions
    {

        public static IHostBuilder AutoRegisterServices(this IHostBuilder hostBuilder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return hostBuilder.AutoRegisterServices(assemblies);
        }

        public static IHostBuilder AutoRegisterServices(this IHostBuilder hostBuilder, params Assembly[] assemblies)
        {

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AutoRegisterServices(assemblies);
            });

            return hostBuilder;

        }
    }
}


