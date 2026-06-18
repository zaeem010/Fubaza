using Microsoft.Extensions.DependencyInjection;

using Fubaza.Application.Core.Mapping;

namespace Fubaza.Application.Infrastructure.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile)); // Register the profile
            return services;
        }
    }
}
