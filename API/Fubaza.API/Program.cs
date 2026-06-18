using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Infrastructure.Middlewares;
using Fubaza.Application.Utilities;
using Fubaza.Application.Utilities.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Fubaza.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = long.MaxValue;
                });

                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = long.MaxValue;
                });

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                builder.Host.UseSerilog();

                builder.Services
                    .AddDistributedMemoryCache()
                    .AddSerialization(builder.Configuration)
                    .AddApplicationUtilities(builder.Configuration)
                    .AddApplicationInfrastructure(builder.Configuration)
                    .AddApplicationCore(builder.Configuration);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // ✅ Allow ALL domains (temporary)
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                var app = builder.Build();

                app.Use(async (context, next) =>
                {
                    var maxFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                    if (maxFeature != null && !maxFeature.IsReadOnly)
                        maxFeature.MaxRequestBodySize = long.MaxValue;

                    await next.Invoke();
                });

                app.UseSwagger();
                app.UseSwaggerUI();

                var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
                app.UseRequestLocalization(locOptions.Value);

                // Default static files
                app.UseStaticFiles();

                // Static files with CORS (Attachments folder)
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "Attachments")),
                    RequestPath = "/Attachments",
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
                        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
                    }
                });

                app.UseMiddleware<GlobalExceptionHandler>();

                UseSerilogRequestLogging(app);

                // ✅ Must be BEFORE Authentication
                app.UseCors("AllowAll");

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    DashboardTitle = "Fubaza Jobs",
                });

                Initialize(app);

                RegisterRecurringJobs();

                app.MapControllers();

                Log.Information("Starting up Fubaza");

                if (app.Environment.IsDevelopment())
                {
                    app.Run();
                }
                else
                {
                    app.Run("http://0.0.0.0:5000");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception");
            }
            finally
            {
                Log.Information("Shut down complete");
                Log.CloseAndFlush();
            }
        }

        private static void Initialize(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers)
            {
                initializer.Initialize();
            }
        }

        private static void RegisterRecurringJobs()
        {
            // Täglich 03:00 UTC — Social-Media-Insights für publizierte Posts einsammeln.
            // Idempotent: AddOrUpdate erlaubt, den Cron später ohne Migration zu ändern.
            RecurringJob.AddOrUpdate<SocialInsightsCollectorJob>(
                SocialInsightsCollectorJob.JobId,
                job => job.RunAsync(CancellationToken.None),
                "0 3 * * *",
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }

        public static void UseSerilogRequestLogging(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });
        }
    }
}
