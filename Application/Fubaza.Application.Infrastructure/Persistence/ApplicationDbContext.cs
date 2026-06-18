using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Fubaza.Application.Infrastructure.Extensions;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Contracts.Serialization;
using Fubaza.Application.Core.Contracts.Services.Identity;


namespace Fubaza.Application.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, RoleClaim, IdentityUserToken<Guid>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly PersistenceSettings? _persistenceOptions;
        private readonly IJsonSerializer _jsonSerializer;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ICurrentUser currentUser,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer jsonSerializer)
            : base(options)
        {
            _persistenceOptions = persistenceOptions.Value;
            _currentUser = currentUser;
            _jsonSerializer = jsonSerializer;
        }
        public DbSet<AuditableEntity> AuditableEntity { get; set; }
        public DbSet<PlayerDocument> PlayerDocument { get; set; }
        public DbSet<Player> Player { get; set; }
        public DbSet<Club> Club { get; set; }
        public DbSet<ClubDocument> ClubDocument { get; set; }
        public DbSet<PlayerClubHistory> PlayerClubHistory { get; set; }
        public DbSet<Sport> Sport { get; set; }
        public DbSet<PlayingPosition> PlayingPosition { get; set; }
        public DbSet<SubscriptionPeriod> SubscriptionPeriod { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
        public DbSet<PlanFeature> PlanFeature { get; set; }
        public DbSet<ClubOfficial> ClubOfficial { get; set; }
        public DbSet<ClubOfficialDocument> ClubOfficialDocument { get; set; }
        public DbSet<Templete> Templete { get; set; }
        public DbSet<TempleteDocument> TempleteDocument { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Matchday> Matchday { get; set; }
        public DbSet<SponsorDocument> SponsorDocument { get; set; }
        public DbSet<MatchSummary> MatchSummary { get; set; }
        public DbSet<MatchLineUp> MatchLineUp { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<PostTarget> PostTarget { get; set; }
        public DbSet<PostInsightSnapshot> PostInsightSnapshot { get; set; }
        public DbSet<PostDocument> PostDocument { get; set; }
        public DbSet<TempleteImage> TempleteImage { get; set; }
        public DbSet<EventType> EventType { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChangesAsync(auditEntries);
            return result;
        }

        private List<AuditableEntity> OnBeforeSaveChanges()
        {
            var auditEntries = new List<AuditableEntity>();


            foreach (var entry in ChangeTracker.Entries())
            {

                if (entry.Entity is AuditableEntity || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var auditEntry = new AuditableEntity
                    {
                        TableName = entry.Entity.GetType().Name,
                        Action = entry.State.ToString(),
                        Timestamp = DateTime.UtcNow,
                        UserId = _currentUser.GetUserId(),
                        Changes = _jsonSerializer.Serialize(
                                      entry.Properties.ToDictionary(p => p.Metadata.Name, p => new
                                      {
                                          IsModified = p.IsModified,                       // Indicates if the property was modified
                                          OriginalValue = entry.State == EntityState.Added ? null : p.OriginalValue, // Old value (null if added)
                                          CurrentValue = p.CurrentValue                    // New value
                                      }))
                    };
                    auditEntries.Add(auditEntry);
                }
            }
            return auditEntries;
        }

        private async Task OnAfterSaveChangesAsync(List<AuditableEntity> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                AuditableEntity.Add(auditEntry);
            }

            await base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            if (_persistenceOptions != null)
            {
                modelBuilder.ApplicationConfiguration(_persistenceOptions);
            }
        }
    }

}
