using Fubaza.Application.Core.Settings;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Fubaza.Application.Infrastructure.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<T>(this IServiceCollection services) where T : DbContext
        {
            var options = services.GetOptions<PersistenceSettings>(nameof(PersistenceSettings));

            if (options.ConnectionStrings?.MSSQL is null)
                throw new InvalidOperationException("MSSQL connection string is missing.");

            services.AddMSSQL<T>(options.ConnectionStrings.MSSQL);

            return services;
        }

        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : class, new()
        {
            using var provider = services.BuildServiceProvider();
            var config = provider.GetRequiredService<IConfiguration>();

            var options = new T();
            config.GetSection(sectionName).Bind(options);

            return options;
        }

        private static IServiceCollection AddMSSQL<T>(this IServiceCollection services, string connectionString) where T : DbContext
        {
            services.AddDbContext<T>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.MigrationsAssembly(typeof(T).Assembly.FullName)));

            services.AddHangfire(configuration =>
                configuration.UseSqlServerStorage(connectionString));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            dbContext.Database.Migrate();

            return services;
        }
    }
}