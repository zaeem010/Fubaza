using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Fubaza.Application.Infrastructure.Services
{
    public class NotificationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationWorker> _logger;

        public NotificationWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var postRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var firebaseService = scope.ServiceProvider.GetRequiredService<IFirbaseService>();
                var jobservice = scope.ServiceProvider.GetRequiredService<IJobService>();

                var now = DateTime.UtcNow;

                // 🔔 1 hour before notifications
                var postsToNotifyBefore = await postRepo.GetPostsToNotifyBeforeAsync(now, stoppingToken);
                foreach (var post in postsToNotifyBefore)
                {
                    if (string.IsNullOrWhiteSpace(post.User?.FcmToken))
                        continue;

                    foreach (var target in post.Targets)
                    {
                        string platformMessageEn;
                        string platformMessageDe;

                        if (target.IsFacebook && target.IsInstagram)
                        {
                            platformMessageEn = "Facebook and Instagram";
                            platformMessageDe = "Facebook und Instagram";
                        }
                        else if (target.IsFacebook)
                        {
                            platformMessageEn = "Facebook";
                            platformMessageDe = "Facebook";
                        }
                        else if (target.IsInstagram)
                        {
                            platformMessageEn = "Instagram";
                            platformMessageDe = "Instagram";
                        }
                        else
                        {
                            continue; // no valid targets found
                        }

                        string title = NotificationConstants.En.Titles.ScheduledPostReminder;
                        string body = string.Format(NotificationConstants.En.Bodies.ScheduledPostReminder, platformMessageEn);

                        string titleDe = NotificationConstants.De.Titles.ScheduledPostReminder;
                        string bodyDe = string.Format(NotificationConstants.De.Bodies.ScheduledPostReminder, platformMessageDe);

                        var lang = string.IsNullOrWhiteSpace(post.User?.LanguageCode) ? "en" : post.User.LanguageCode.Trim().ToLower();

                        string fTitle = lang == "de" ? titleDe : title;
                        string fBody = lang == "de" ? bodyDe : body;

                        if (post.User.IsNotificationEnabled)
                        {
                            var request = new NotificationRequest
                            {
                                Token = post.User.FcmToken,
                                Title = fTitle,
                                Body = fBody
                            };

                            jobservice.Enqueue(() => firebaseService.SendAsync(request));
                        }
                       

                        var notification = new Notification
                        {
                            UserId = post.User.Id,
                            Title = title,
                            Body = body,
                            TitleDe = titleDe,
                            BodyDe = bodyDe
                        };

                        await notificationService.AddNotificationAsync(notification, stoppingToken);

                        _logger.LogInformation($"🔔 Sent {platformMessageEn} reminder to user {post.UserId} for post {post.Id}");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }



    }
}
