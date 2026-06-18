using AutoMapper;
using AutoMapper.QueryableExtensions;

using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Persistence;
using LinqKit;
using Microsoft.EntityFrameworkCore;


namespace Fubaza.Application.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public NotificationRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Notifications.AddAsync(notification, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken); // ✅ Save inside Add

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Notification due to: {e.GetMessage()}");
            }
        }

        public async Task<PaginatedResponse<UserNotificationsDto>> GetNotificationsAsync(PaginationRequest request)
        {
            try
            {
                var predicate = PredicateBuilder.New<Notification>(true);

                predicate = predicate.And(n => n.UserId == request.UserId);
                
                var query = _db.Notifications
                    .AsExpandable()
                    .Where(predicate);
                
                var totalCount =  query.Count();
                
                var unreadCount = await _db.Notifications
                    .CountAsync(n => n.UserId == request.UserId && !n.IsRead);
                
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ThenByDescending(n => n.Id)
                    .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var item = new UserNotificationsDto
                {
                    UnreadCount = unreadCount,
                    Notifications = _mapper.Map<List<NotificationDto>>(notifications),
                };


                return new PaginatedResponse<UserNotificationsDto>
                {
                    Pagination = new PaginationInfo
                    {
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    Item = item
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch notifications. Details: {ex.GetMessage()}");
            }
        }

        public async Task<bool> MarkAllReadAsync(Guid userId)
        {

            try
            {
                var affected = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
                     .ExecuteUpdateAsync(set => set.SetProperty(n => n.IsRead, true));

                return affected > 0; // true if anything changed

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the player: {e.GetMessage()}");
            }

        }

        public async Task<bool> UpdateNotificationStatus(NotificationPreferenceRequest request)
        {
            try
            {
                var notificationStatus = await _db.Users.Where(n => n.Id == request.UserId)
                     .ExecuteUpdateAsync(set => set.SetProperty(n => n.IsNotificationEnabled, request.IsNotificationEnabled));

                return notificationStatus > 0; // true if anything changed

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the player: {e.GetMessage()}");
            }
        }
    }
}
