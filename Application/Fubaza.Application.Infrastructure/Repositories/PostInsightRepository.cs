using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fubaza.Application.Infrastructure.Repositories
{
    public class PostInsightRepository : IPostInsightRepository
    {
        private readonly ApplicationDbContext _db;

        public PostInsightRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PostInsightCollectionTarget>> GetActiveTargetsForCollectionAsync(CancellationToken ct = default)
        {
            try
            {
                // Facebook: alles wo FacebookPostId gesetzt ist
                var fb = await _db.PostTarget
                    .AsNoTracking()
                    .Where(t => t.IsPublished
                                && t.IsFacebook
                                && !string.IsNullOrEmpty(t.FacebookPostId)
                                && !string.IsNullOrEmpty(t.AccessToken))
                    .Select(t => new PostInsightCollectionTarget
                    {
                        PostTargetId = t.Id,
                        Platform = SocialPlatform.Facebook,
                        ExternalPostId = t.FacebookPostId!,
                        AccessToken = t.AccessToken!,
                    })
                    .ToListAsync(ct);

                // Instagram: alles wo InstagramPostId gesetzt ist
                var ig = await _db.PostTarget
                    .AsNoTracking()
                    .Where(t => t.IsPublished
                                && !string.IsNullOrEmpty(t.InstagramPostId)
                                && !string.IsNullOrEmpty(t.AccessToken))
                    .Select(t => new PostInsightCollectionTarget
                    {
                        PostTargetId = t.Id,
                        Platform = SocialPlatform.Instagram,
                        ExternalPostId = t.InstagramPostId!,
                        AccessToken = t.AccessToken!,
                    })
                    .ToListAsync(ct);

                return fb.Concat(ig).ToList();
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to load post-targets for insight collection due to: {e.GetMessage()}");
            }
        }

        public async Task AddSnapshotAsync(PostInsightSnapshot snapshot, CancellationToken ct = default)
        {
            try
            {
                await _db.PostInsightSnapshot.AddAsync(snapshot, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to add insight snapshot due to: {e.GetMessage()}");
            }
        }

        public async Task<List<PlatformAggregateResult>> GetLatestAggregatedByUserAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                // "Neuester Snapshot pro PostTargetId" — Sub-Query als Lookup.
                var latestPerTarget = _db.PostInsightSnapshot
                    .GroupBy(s => s.PostTargetId)
                    .Select(g => new { PostTargetId = g.Key, FetchedAt = g.Max(x => x.FetchedAt) });

                // Join: nur Snapshots die zum jeweils neuesten Eintrag passen, scoped auf den User.
                var query =
                    from snap in _db.PostInsightSnapshot.AsNoTracking()
                    join latest in latestPerTarget
                        on new { snap.PostTargetId, snap.FetchedAt }
                        equals new { latest.PostTargetId, latest.FetchedAt }
                    join target in _db.PostTarget.AsNoTracking() on snap.PostTargetId equals target.Id
                    join post in _db.Post.AsNoTracking() on target.PostId equals post.Id
                    where post.UserId == userId && target.IsPublished
                    group snap by snap.Platform into pg
                    select new PlatformAggregateResult
                    {
                        Platform = pg.Key,
                        TotalLikes = pg.Sum(x => x.Likes),
                        TotalImpressions = pg.Sum(x => x.Impressions),
                        TotalShares = pg.Sum(x => x.Shares),
                    };

                return await query.ToListAsync(ct);
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to aggregate post insights due to: {e.GetMessage()}");
            }
        }
    }
}
