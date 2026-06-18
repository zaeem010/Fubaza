using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.DTO.Contracts;
using Fubaza.Application.Infrastructure.Extensions;
using Fubaza.Application.Infrastructure.Factories;
using Fubaza.Application.Infrastructure.Middlewares;
using Fubaza.Application.Infrastructure.Permissions;
using Fubaza.Application.Infrastructure.Persistence;
using Fubaza.Application.Infrastructure.Repositories;
using Fubaza.Application.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddPersistenceSettings(config)
            .AddOptions()
            .AddMemoryCache()
            .AddHttpClient()
            .AddHttpContextAccessor()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddSession()
            .AddHangfireServer()
            .AddSingleton<GlobalExceptionHandler>();
        
        services.AddDatabaseContext<ApplicationDbContext>();

        services.AddLocalizationSettings();

        services
            .Configure<JwtSettings>(config.GetSection("JwtSettings"))
            .AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddJwtAuthentication(config);

        services.AddGoogleAuthentication();

       

        services
            .AddPermissions(config)
            .AddScoped<IUserClaimsPrincipalFactory<User>, AdditionalUserClaimsPrincipalFactory>()
            .AddTransient<IDatabaseSeeder, IdentityDbSeeder>()
                .AddTransient<IDatabaseSeeder, MetaDataDbSeeder>();

        services.AddServiceInfrastructure(config);
        services.AddAddRepositoriesInfrastructure(config);
        services.AddAutoMapper();

        return services;
    }

    public static IServiceCollection AddServiceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<ILookUpService, LookUpService>()
            .AddScoped<IRoleService, RoleService>()
            .AddScoped<IJwtTokenService, JwtTokenService>()
            .AddScoped<IPlayerService, PlayerService>()
            .AddScoped<IClubService, ClubService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddScoped<IEventService, EventService>()
            .AddScoped<IDashboardService, DashboardService>()
            .AddScoped<IRoleClaimService, RoleClaimService>();

        // Social Insights — pro Plattform ein Provider. Werden im SocialInsightsCollectorJob
        // als IEnumerable<ISocialInsightsProvider> injiziert und per Platform-Lookup gemappt.
        services
            .AddScoped<ISocialInsightsProvider, Fubaza.Application.Infrastructure.Services.SocialInsights.FacebookInsightsProvider>()
            .AddScoped<ISocialInsightsProvider, Fubaza.Application.Infrastructure.Services.SocialInsights.InstagramInsightsProvider>();

        // ✅ Add background worker
        services.AddHostedService<NotificationWorker>();
        //services.AddHostedService<ScheduledPostPublisherService>();


        //Admin Services
        services
            .AddScoped<IPlayerOverviewService, PlayerOverviewService>()
            .AddScoped<IClubOverviewService, ClubOverviewService>()
            .AddScoped<ITempleteService, TempleteService>();

        return services;
    }
    public static IServiceCollection AddAddRepositoriesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<ILookUpRepository, LookUpRepository>()
            .AddScoped<IPlayerRepository, PlayerRepository>()
            .AddScoped<IClubRepository, ClubRepository>()
            .AddScoped<INotificationRepository, NotificationRepository>()
            .AddScoped<IEventRepository, EventRepository>()
            .AddScoped<IPostInsightRepository, PostInsightRepository>();
            

        //Admin repositories
        services
          .AddScoped<IPlayerOverviewRepository, PlayerOverviewRepository>()
          .AddScoped<ITempleteRepository, TempleteRepository>()
          .AddScoped<IClubOverviewRepository, ClubOverviewRepository>();
          



        return services;
    }
    private static IServiceCollection AddLocalizationSettings(this IServiceCollection services)
    {
        services.AddLocalization();

        var supportedCultures = new[]
        {
                new CultureInfo("en"),
                new CultureInfo("de")
            };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
        });

        return services;
    }
    private static IServiceCollection AddPersistenceSettings(this IServiceCollection services, IConfiguration config)
    {
        return services.Configure<PersistenceSettings>(config.GetSection(nameof(PersistenceSettings)));
    }

    private static IServiceCollection AddPermissions(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = services.GetOptions<JwtSettings>(nameof(JwtSettings));
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key ?? throw new InvalidOperationException("JWT Key cannot be null or empty."));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.RequireHttpsMetadata = false;
           options.SaveToken = true;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = false,
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtSettings.Issuer,
               ValidAudience = jwtSettings.Audience,
               ClockSkew = System.TimeSpan.Zero,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
           };
       });
        return services;
    }

    internal static IServiceCollection AddGoogleAuthentication(this IServiceCollection services)
    {
        var googleAuthSettings = services.GetOptions<GoogleAuthSettings>(nameof(GoogleAuthSettings));
        if (!string.IsNullOrWhiteSpace(googleAuthSettings.ClientId) && !string.IsNullOrWhiteSpace(googleAuthSettings.ClientSecret))
        {
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = googleAuthSettings.ClientId;
                googleOptions.ClientSecret = googleAuthSettings.ClientSecret;
                googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }
        return services;
    }
}
