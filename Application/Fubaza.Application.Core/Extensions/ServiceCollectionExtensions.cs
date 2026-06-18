using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Fubaza.Application.Core.Contracts.Serialization;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Serialization;

namespace Fubaza.Application.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationCore(this IServiceCollection services, IConfiguration config)
        {
            // Add additional services if needed
            return services;
        }

        public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            var options = new T();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }

        public static IServiceCollection AddSerialization(this IServiceCollection services, IConfiguration config)
        {
            // Correctly configure and bind the settings
            services.Configure<SerializationSettings>(config.GetSection(nameof(SerializationSettings)).Bind);

            var options = config.GetOptions<SerializationSettings>(nameof(SerializationSettings));

            services.AddSingleton<IJsonSerializerSettingsOptions, JsonSerializerSettingsOptions>();

            if (options.UseSystemTextJson)
            {
                services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>()
                        .Configure<JsonSerializerSettingsOptions>(opts =>
                        {
                            if (!opts.JsonSerializerOptions.Converters.Any(c => c is TimespanJsonConverter))
                            {
                                opts.JsonSerializerOptions.Converters.Add(new TimespanJsonConverter());
                            }
                        });
            }
            else if (options.UseNewtonsoftJson)
            {
                services.AddSingleton<IJsonSerializer, NewtonSoftJsonSerializer>();
            }

            return services;
        }

    }
}
