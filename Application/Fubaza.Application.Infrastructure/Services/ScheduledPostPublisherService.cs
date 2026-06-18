using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Fubaza.Application.Infrastructure.Services
{
    public class ScheduledPostPublisherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledPostPublisherService> _logger;
        private readonly AppSettings _appSettings;

        public ScheduledPostPublisherService(IServiceScopeFactory scopeFactory,
            ILogger<ScheduledPostPublisherService> logger,
             IOptions<AppSettings> appSettings
             )
        {
            _appSettings = appSettings.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🕒 ScheduledPostPublisherService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                //try
                //{
                //    using var scope = _scopeFactory.CreateScope();
                //    var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                //    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                //    var origin = _appSettings.BaseUrl;

                //    var duePosts = await repository.GetDueScheduledPostsAsync(DateTime.UtcNow);

                //    foreach (var post in duePosts)
                //    {
                //        _logger.LogInformation($"🚀 Publishing post {post.Id} scheduled for {post.ScheduleDateTime}");

                //        foreach (var target in post.Targets)
                //        {
                //            IResult<string> result;

                //            if (target.IsFacebook)
                //            {
                //                result = await eventService.PublishToFacebookAsync(post, target, origin);
                //                if (result.Succeeded && !string.IsNullOrEmpty(result.Data))
                //                {
                //                    target.FacebookPostId = result.Data;
                //                    _logger.LogInformation($" Facebook post ID saved: {result.Data}");
                //                }
                //            }

                //            if (target.IsInstagram)
                //            {
                //                result = await eventService.PublishToInstagramAsync(post, target, origin);
                //                if (result.Succeeded && !string.IsNullOrEmpty(result.Data))
                //                {
                //                    target.InstagramPostId = result.Data;
                //                    _logger.LogInformation($" Instagram post ID saved: {result.Data}");
                //                }
                //            }

                //            target.IsPublished = true;

                //            await repository.UpdatePostIdAsync(target);

                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Error while running scheduled post publisher job.");
                //}

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every 1 min
            }
        }
    }

}
