using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.DTO.Enums;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



namespace Fubaza.Application.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder ApplicationConfiguration(this ModelBuilder builder, PersistenceSettings persistenceSettings)
        {
            builder.Entity<Sport>(b =>
            {
                b.ToTable("Sport");

                b.Property(e => e.Name).HasMaxLength(128);
                b.Property(e => e.NameDe).HasMaxLength(128);
                b.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            builder.Entity<PlayingPosition>(b =>
            {
                b.ToTable("PlayingPosition");

                b.Property(e => e.Name).HasMaxLength(264);
                b.Property(e => e.NameDe).HasMaxLength(264);
                b.Property(e => e.IsDeleted).HasDefaultValue(false);
                b.Property(e => e.OrderId).HasDefaultValue(0);

            });

            builder.Entity<Fubaza.Application.Core.Entities.EventType>(b =>
            {
                b.ToTable("EventType");

                b.Property(e => e.Name).HasMaxLength(264);
                b.Property(e => e.NameDe).HasMaxLength(264);
                b.Property(e => e.IsDeleted).HasDefaultValue(false);

            });


            builder.Entity<Club>(b =>
            {
                b.ToTable("Club");

                b.Property(e => e.FullName).HasMaxLength(512);
                b.Property(e => e.Address).HasMaxLength(512);
                b.Property(e => e.Nationality).HasMaxLength(512);
                b.Property(e => e.League).HasMaxLength(512);
                b.Property(e => e.Division).HasMaxLength(512);
                b.Property(e => e.Description).HasMaxLength(512);
            });

            builder.Entity<ClubOfficial>(b =>
            {
                b.ToTable("ClubOfficial");
                b.Property(e => e.Name).HasMaxLength(512);
                b.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            builder.Entity<Designation>(b =>
            {
                b.ToTable("Designation");

                b.Property(e => e.Title).HasMaxLength(512);
                b.Property(e => e.TitleDe).HasMaxLength(512);
            });


            builder.Entity<Player>(b =>
            {
                b.ToTable("Player");

                b.Property(e => e.FullName).HasMaxLength(512);
                b.Property(e => e.Nationality).HasMaxLength(512);
                b.Property(e => e.IsCaption).HasDefaultValue(false);
            });

            builder.Entity<PlayerClubHistory>(b =>
            {
                b.ToTable("PlayerClubHistory");

                b.Property(e => e.IsCurrentClub).HasDefaultValue(false);
            });

            builder.Entity<PlayerDocument>(b =>
            {
                b.ToTable("PlayerDocument");

                b.Property(p => p.DocumentType).HasDefaultValue(PlayerDocumentType.Profile);

            });

            builder.Entity<Templete>(b =>
            {
                b.ToTable("Templete");
                b.Property(p => p.Title).IsRequired().HasMaxLength(264);
                b.Property(e => e.IsDeleted).HasDefaultValue(false);
                b.Property(e => e.IsApproved).HasDefaultValue(true);
                b.Property(e => e.IsApproved).HasDefaultValue(false);

            });

            builder.Entity<TempleteImage>(b =>
            {
                b.ToTable("TempleteImage");
                b.Property(e => e.IsUserUpload).HasDefaultValue(true);

            });


            builder.Entity<SubscriptionPeriod>(entity =>
            {
                entity.ToTable("SubscriptionPeriod");
                entity.Property(p => p.PeriodName).IsRequired().HasMaxLength(128);
                entity.Property(p => p.Description).HasMaxLength(512);
                entity.HasMany(p => p.Plans).WithOne(plan => plan.SubscriptionPeriod).HasForeignKey(plan => plan.SubscriptionPeriodId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("SubscriptionPlan");
              
                entity.Property(p => p.PlanName).IsRequired().HasMaxLength(128);
                entity.Property(p => p.MonthlyRate).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(p => p.CurrencyCode).IsRequired().HasMaxLength(10);
                entity.Property(p => p.HasFreeTrial).IsRequired();
                entity.HasMany(p => p.Features).WithOne(f => f.SubscriptionPlan).HasForeignKey(f => f.SubscriptionPlanId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PlanFeature>(entity =>
            {
                entity.ToTable("PlanFeature");
                entity.Property(f => f.FeatureName).IsRequired().HasMaxLength(150);
            });

            builder.Entity<AuditableEntity>(b =>
            {
                b.ToTable("AuditableEntity");
                b.Property(e => e.TableName).HasMaxLength(264);
                b.Property(e => e.Action).HasMaxLength(264);

            });

            builder.Entity<Matchday>(b =>
            {
                b.ToTable("Matchday");
                b.Property(e => e.MatchdayNumber).HasMaxLength(512);
                b.Property(e => e.Referee).HasMaxLength(512);
                b.Property(e => e.AssistantReferee1).HasMaxLength(512);
                b.Property(e => e.AssistantReferee2).HasMaxLength(512);
                b.Property(e => e.Location).HasMaxLength(512);
                b.Property(e => e.IsLineUp).HasDefaultValue(false);
                b.Property(e => e.IsMatchEnd).HasDefaultValue(false);

                b.Property(e => e.MatchDayDateTime)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETUTCDATE()")                 // DB sets UTC on insert
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // read as UTC

                b.Property(e => e.DisappearDateTime)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETUTCDATE()")                 // DB sets UTC on insert
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // read as UTC

               b.Property(e => e.MatchStartDateTime)
               .HasColumnType("datetime2")
               .HasConversion(
                 v => v, // keep as is when saving
                 v => v == null ? (DateTime?)null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) // ensure UTC when reading
               );
                

            });

            builder.Entity<Post>(b =>
            {
                b.ToTable("Post");
                b.Property(e => e.ScheduleDateTime)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETUTCDATE()")                 // DB sets UTC on insert
                   .HasConversion(
                         v => v, // keep as is when saving
                         v => v == null ? (DateTime?)null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) // ensure UTC when reading
                   );
                b.Property(e => e.IsDeleted).HasDefaultValue(false);
                b.Property(e => e.IsFacebookLogin).HasDefaultValue(false);
                b.Property(e => e.IsCancelled).HasDefaultValue(false);
            });

            builder.Entity<PostTarget>(b =>
            {
                b.ToTable("PostTarget");
                b.Property(e => e.IsFacebook).HasDefaultValue(false);
                b.Property(e => e.IsInstagram).HasDefaultValue(false);

                b.Property(e => e.PageId).HasMaxLength(512);
                b.Property(e => e.InstagramBusinessId).HasMaxLength(512);

                b.Property(e => e.AccessToken).HasMaxLength(4000);
                b.Property(e => e.FacebookPostId).HasMaxLength(512);
                b.Property(e => e.InstagramPostId).HasMaxLength(512);
                b.Property(e => e.IsPublished).HasDefaultValue(false);
            });

            builder.Entity<PostInsightSnapshot>(b =>
            {
                b.ToTable("PostInsightSnapshot");

                b.Property(e => e.ExternalPostId).HasMaxLength(512).IsRequired();
                b.Property(e => e.RawResponse).HasColumnType("nvarchar(max)");

                b.Property(e => e.FetchedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                b.HasOne(e => e.PostTarget)
                    .WithMany()
                    .HasForeignKey(e => e.PostTargetId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(e => new { e.PostTargetId, e.FetchedAt });
                b.HasIndex(e => e.Platform);
            });


            builder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.Property(e => e.Title).HasMaxLength(1024);
                b.Property(e => e.Body).HasMaxLength(2000);
                b.Property(e => e.TitleDe).HasMaxLength(1024);
                b.Property(e => e.BodyDe).HasMaxLength(2000);
                b.Property(e => e.IsRead).HasDefaultValue(false);

                b.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETUTCDATE()")                 // DB sets UTC on insert
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // read as UTC
            });

            builder.Entity<MatchSummary>(b =>
            {
                b.ToTable("MatchSummary");
                b.Property(e => e.Minute).HasMaxLength(1024);
                b.Property(e => e.Description).HasMaxLength(2000);

                // New: default value set to current UTC datetime
                b.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Scorer relation (matches Player.MatchSummaries collection)
                b.HasOne(e => e.Player)
                    .WithMany(p => p!.MatchSummaries)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Assist relation (optional, e.g. football). No inverse navigation on Player.
                b.HasOne(e => e.AssistPlayer)
                    .WithMany()
                    .HasForeignKey(e => e.AssistPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(e => e.AssistPlayerId);
            });

            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "Users");
                entity.Property(e => e.Email).HasColumnType("nvarchar(325)");
                entity.Property(e => e.NormalizedEmail).HasColumnType("nvarchar(325)");
                entity.Property(e => e.UserName).HasColumnType("nvarchar(325)");
                entity.Property(e => e.NormalizedUserName).HasColumnType("nvarchar(325)");
                entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(325)");
                entity.Property(e => e.PasswordHash).HasColumnType("nvarchar(256)");
                entity.Property(e => e.RefreshToken).HasColumnType("varchar(max)");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("nvarchar(128)");
                entity.Property(e => e.EmailVerificationCode).HasColumnType("nvarchar(128)");
                entity.Property(e => e.SecurityStamp).HasColumnType("nvarchar(128)");
                entity.Property(e => e.PhoneNumber).HasColumnType("nvarchar(25)");
                entity.Property(e => e.Created).HasColumnType("datetime").HasDefaultValue(new DateTime(2021, 11, 1));
                entity.Property(e => e.LastModified).HasColumnType("datetime");
                entity.Property(e => e.IsPasswordRequired).HasDefaultValue(false);
                entity.Property(e => e.IsConnectedFacebook).HasDefaultValue(false);
                entity.Property(e => e.IsConnectedInstagram).HasDefaultValue(false);
                entity.Property(e => e.IsNotificationEnabled).HasDefaultValue(true);
                entity.Property(e => e.CanLogin).HasDefaultValue(true);
                entity.Property(e => e.IsAdminPanel).HasDefaultValue(false);
                entity.Property(e => e.FcmToken).HasColumnType("varchar(max)");
                entity.Property(e => e.FacebookLongLivedToken).HasColumnType("varchar(max)");
                entity.Property(e => e.InstagramLongLivedToken).HasColumnType("varchar(max)");
                entity.Property(e => e.LanguageCode).HasColumnType("nvarchar(128)");
                entity.Property(e => e.FacebookTokenExpiresAt)
                  .HasColumnType("datetime2")
                  .HasDefaultValueSql("GETUTCDATE()")                 // DB sets UTC on insert
                  .HasConversion(
                        v => v, // keep as is when saving
                        v => v == null ? (DateTime?)null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) // ensure UTC when reading
                  );

                entity.HasIndex(u => u.Email).IsUnique().HasFilter("[IsActive] = 1");


            });

            builder.Entity<Role>(entity =>
            {
                entity.ToTable(name: "Roles");
                entity.Property(e => e.Description).HasColumnType("nvarchar(256)");
                entity.Property(e => e.NameDe).HasColumnType("nvarchar(516)");
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("nvarchar(128)");
            });

            builder.Entity<RoleClaim>(entity =>
            {
                entity.ToTable(name: "RoleClaims");
                entity.Property(e => e.Description).HasColumnType("nvarchar(256)");

                entity.Property(e => e.ClaimType)
                    .HasColumnType("nvarchar(128)")
                    .IsRequired();

                entity.Property(e => e.ClaimValue)
                    .HasColumnType("nvarchar(128)")
                    .IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");

            });

            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims");

                entity.Property(e => e.ClaimType)
                .HasColumnType("nvarchar(128)")
                .IsRequired();

                entity.Property(e => e.ClaimValue)
                    .HasColumnType("nvarchar(128)")
                    .IsRequired();
            });

            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins");

                entity.Property(e => e.ProviderDisplayName).HasColumnType("nvarchar(256)");
            });

            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens");

                entity.Property(e => e.Value).HasColumnType("nvarchar(256)");
            });

            return builder;
        }
    }
}
