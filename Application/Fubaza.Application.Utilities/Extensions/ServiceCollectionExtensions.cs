using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Settings;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fubaza.Application.Utilities.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationUtilities(this IServiceCollection services, IConfiguration config)
        {
			// Utilities
			services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddTransient<IMailService, SmtpMailService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddTransient<IFileService, FileService>();
			services.AddScoped<IJobService, HangfireService>();

            // Registriert für Hangfire ActivatorBasedJobActivator — sonst kann der
            // Recurring Job nicht aus dem Container aufgelöst werden.
            services.AddScoped<SocialInsightsCollectorJob>();

            services.AddSingleton<FirebaseApp>(sp =>
            {
                var section = config.GetSection("FirebaseSettings");
                var path = section["CredentialPath"];
                var json = section["CredentialJson"];

                GoogleCredential cred = !string.IsNullOrWhiteSpace(json)
                    ? GoogleCredential.FromJson(json)
                    : GoogleCredential.FromFile(path ?? throw new InvalidOperationException(
                        "Firebase:CredentialPath or CredentialJson is required."));

                // If an instance exists, use it; otherwise create and return a new one.
                return FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions { Credential = cred });
            });

            services.AddSingleton<FirebaseMessaging>(sp =>
            FirebaseMessaging.GetMessaging(sp.GetRequiredService<FirebaseApp>()));

            services.AddSingleton<IFirbaseService, FirbaseService>();

            // Settings
            services.Configure<MailSettings>(config.GetSection(nameof(MailSettings)));

            services.Configure<GoogleAuthSettings>(config.GetSection(nameof(GoogleAuthSettings)));

            services.Configure<FacebookAuthSettings>(config.GetSection(nameof(FacebookAuthSettings)));
            
            services.Configure<InstagramAuthSettings>(config.GetSection(nameof(InstagramAuthSettings)));

            services.Configure<OpenAISettings>(config.GetSection(nameof(OpenAISettings)));

            services.Configure<FirebaseSettings>(config.GetSection(nameof(FirebaseSettings)));

            services.Configure<GeminiSettings>(config.GetSection(nameof(GeminiSettings)));

            services.Configure<CloudStorageSetting>(config.GetSection(nameof(CloudStorageSetting)));

            services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));


            return services;
		}


	}
}
