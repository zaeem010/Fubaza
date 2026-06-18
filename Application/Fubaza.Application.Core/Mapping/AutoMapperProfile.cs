using AutoMapper;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => LocalizationExtensions.Localize(src.Name, src.NameDe)));

            CreateMap<User, UserDto>();

            CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => LocalizationExtensions.Localize(src.Title, src.TitleDe)))
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => LocalizationExtensions.Localize(src.Body, src.BodyDe)));

            CreateMap<User, PlayerProfileDTO>()
                  .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Player != null ? src.Player.FullName : null))
                  .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Player != null ? src.Player.Nationality : null))
                  .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Player != null ? src.Player.DateOfBirth : null))
                  .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Player != null ? src.Player.Gender : null))
                  .ForMember(dest => dest.WeightKg, opt => opt.MapFrom(src => src.Player != null ? src.Player.WeightKg : null))
                  .ForMember(dest => dest.HeightCm, opt => opt.MapFrom(src => src.Player != null ? src.Player.HeightCm : null))
                  .ForMember(dest => dest.StrongFoot, opt => opt.MapFrom(src => src.Player != null ? src.Player.StrongFoot : null))
                  .ForMember(dest => dest.ThrowingHand, opt => opt.MapFrom(src => src.Player != null ? src.Player.ThrowingHand : null))
                  .ForMember(dest => dest.SportId, opt => opt.MapFrom(src => (src.Player != null && src.Player.Sport != null) ? (Guid?)src.Player.Sport.Id : null))
                  .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => (src.Player != null && src.Player.Sport != null) ? src.Player.Sport.Name : null))
                  .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Player != null && src.Player.Documents != null ? src.Player.Documents.Where(d => d.DocumentType == PlayerDocumentType.Profile).Select(d => d.FileUrl).FirstOrDefault() : null))
                  .ForMember(dest => dest.PlayingPositionId, opt => opt.MapFrom(src => (src.Player != null && src.Player.PlayingPosition != null) ? (Guid?)src.Player.PlayingPosition.Id : null))
                  .ForMember(dest => dest.PlayingPositionName, opt => opt.MapFrom(src => (src.Player != null && src.Player.PlayingPosition != null) ? LocalizationExtensions.Localize(src.Player.PlayingPosition.Name, src.Player.PlayingPosition.NameDe) : null))
                  .ForMember(dest => dest.ClubHistory, opt => opt.MapFrom(src => src.Player != null ? src.Player.ClubHistory : null));


            CreateMap<User, ClubProfileDTO>()
                  .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Club != null ? src.Club.FullName : null))
                  .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Club != null ? src.Club.Address : null))
                  .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Club != null ? src.Club.Nationality : null))
                  .ForMember(dest => dest.League, opt => opt.MapFrom(src => src.Club != null ? src.Club.League : null))
                  .ForMember(dest => dest.Division, opt => opt.MapFrom(src => src.Club != null ? src.Club.Division : null))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Club != null ? src.Club.Description : null))
                  .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Club != null && src.Club.Document != null ? src.Club.Document.FileUrl : null));



            CreateMap<PlayerClubHistory, PlayerClubHistoryDTO>()
                  .ForMember(dest => dest.ClubId, opt => opt.MapFrom(src => src.ClubId))
                  .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.FullName : null))
                  .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
                  .ForMember(dest => dest.EndYear, opt => opt.MapFrom(src => src.EndYear))
                  .ForMember(dest => dest.IsCurrentClub, opt => opt.MapFrom(src => src.IsCurrentClub));


            CreateMap<Club, ClubDTO>()
                          .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.FullName))
                          .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null));


            CreateMap<Player, PaginatedPlayersDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName ?? null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.PlayingPosition, opt => opt.MapFrom(src => src.PlayingPosition != null ? LocalizationExtensions.Localize(src.PlayingPosition.Name, src.PlayingPosition.NameDe) : null))
            .ForMember(dest => dest.CurrentClub, opt => opt.MapFrom(src => src.CurrentClub != null ? src.CurrentClub.FullName : null))
            .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.Profile).Select(d => d.FileUrl).FirstOrDefault() : null));


            CreateMap<Club, PaginatedClubsDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName ?? null))
           .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null));


            CreateMap<Player, PlayerInfoDto>()
                  .ForMember(dest => dest.SignedAt, opt => opt.MapFrom(src => src.User.Created))
                  .ForMember(dest => dest.PlayingPositionName, opt => opt.MapFrom(src => src.PlayingPosition != null ? LocalizationExtensions.Localize(src.PlayingPosition.Name, src.PlayingPosition.NameDe) : null))
                  .ForMember(dest => dest.CurrentClub, opt => opt.MapFrom(src => src.CurrentClub != null ? src.CurrentClub.FullName : null))
                  .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.ClubHistory.Where(d => d.IsCurrentClub == true).Select(d => d.StartYear).FirstOrDefault()))
                  .ForMember(dest => dest.Career, opt => opt.MapFrom(src => src.ClubHistory != null ? src.ClubHistory : null))
                  .ForMember(dest => dest.Images, opt => opt.MapFrom(src => new PlayerImageDto
                  {
                      ProfileUrl = src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.Profile).Select(d => d.FileUrl).FirstOrDefault() : null,
                      InMotionUrl = src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.InMotion).Select(d => d.FileUrl).FirstOrDefault() : null,
                      CelebrationUrl = src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.Celebration).Select(d => d.FileUrl).FirstOrDefault() : null,
                      FullBodyUrl = src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.FullBody).Select(d => d.FileUrl).FirstOrDefault() : null,
                  }));


            CreateMap<PlayerClubHistory, CareerClubDto>()
                  .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.FullName : null))
                  .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
                  .ForMember(dest => dest.EndYear, opt => opt.MapFrom(src => src.EndYear))
                  .ForMember(dest => dest.IsCurrentClub, opt => opt.MapFrom(src => src.IsCurrentClub))
                  .ForMember(dest => dest.ClubUrl, opt => opt.MapFrom(src => src.Club != null && src.Club.Document != null ? src.Club.Document.FileUrl : null));


            CreateMap<Club, ClubInfoDto>()
            .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport != null ? src.Sport.Name : null))
            .ForMember(dest => dest.ClubUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null))
            .ForMember(dest => dest.TotalPlayers, opt => opt.MapFrom(src => src.Players.Count))
            .ForMember(dest => dest.TotalOfficials, opt => opt.MapFrom(src => src.Officials.Count))
            .ForMember(dest => dest.TotalMembers, opt => opt.MapFrom(src => src.Players.Count + src.Officials.Count));


            CreateMap<Player, ClubPlayerDto>()
            .ForMember(dest => dest.PlayingPositionName, opt => opt.MapFrom(src => src.PlayingPosition != null ? LocalizationExtensions.Localize(src.PlayingPosition.Name, src.PlayingPosition.NameDe) : null))
            .ForMember(dest => dest.PlayerUrl, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Where(d => d.DocumentType == PlayerDocumentType.Profile).Select(d => d.FileUrl).FirstOrDefault() : null));

            CreateMap<ClubOfficial, ClubOfficialDto>()
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Designation != null ? src.Designation.Title : null))
            .ForMember(dest => dest.ClubOfficialUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null));


            CreateMap<Templete, TempleteDto>()
              .ForMember(dest => dest.TempleteUrl, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Where(d => d.DocumentType == TempleteDocumentType.PreviewImage).Select(d => d.FileUrl).FirstOrDefault() : null))
              .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Where(d => d.DocumentType == TempleteDocumentType.Json).Select(d => d.FileUrl).FirstOrDefault() : null))
              .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport != null ? src.Sport.Name : null))
              .ForMember(dest => dest.TempleteTypeId, opt => opt.MapFrom(src => src.TempleteType.HasValue ? (int?)src.TempleteType.Value : null))
             .ForMember(dest => dest.TempleteTypeName, opt => opt.MapFrom(src => src.TempleteType.HasValue ? EnumExtensions.GetLocalizedEnum(src.TempleteType.Value) : string.Empty));

            CreateMap<TempleteImage, TempleteImageDto>();

            CreateMap<Post, UpcomingPostDto>()
            .ForMember(dest => dest.PostUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null));

            CreateMap<Post, DraftPost>()
            .ForMember(dest => dest.PostUrl, opt => opt.MapFrom(src => src.Document != null ? src.Document.FileUrl : null));


            CreateMap<Matchday, UpcomingMatchDto>()
                .ForMember(dest => dest.OrganizerClubName, opt => opt.MapFrom(src => src.OrganizerClub != null ? src.OrganizerClub.FullName : null))
                .ForMember(dest => dest.OpponentClubName, opt => opt.MapFrom(src => src.OpponentClub != null ? src.OpponentClub.FullName : null))
                .ForMember(dest => dest.OrganizerClubUrl, opt => opt.MapFrom(src => src.OrganizerClub != null && src.OrganizerClub.Document != null ? src.OrganizerClub.Document.FileUrl : null))
                .ForMember(dest => dest.OpponentClubUrl, opt => opt.MapFrom(src => src.OpponentClub != null && src.OpponentClub.Document != null ? src.OpponentClub.Document.FileUrl : null))
                .ForMember(dest => dest.CompetitionType, opt => opt.MapFrom(src => src.CompetitionType != null ? EnumExtensions.GetLocalizedEnum(src.CompetitionType.Value) : string.Empty))
                .ForMember(dest => dest.CompetitionTypeId,opt => opt.MapFrom(src => src.CompetitionType.HasValue? (int?)src.CompetitionType.Value: null))
                .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => (int)src.Venue))
                .ForMember(dest => dest.SponsorDocuments, opt =>
                opt.MapFrom(src => src.SponsorDocuments!= null ? src.SponsorDocuments.Select(s => new SponsorDocumentDto
                {
                    Sponsor = s.Sponsor,
                    FileUrl = s.FileUrl
                }).ToList()
                : new List<SponsorDocumentDto>()));

            CreateMap<MatchSummary, MatchSummaryDto>()
            .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.FullName : null))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType != null ? LocalizationExtensions.Localize(src.EventType.Name, src.EventType.NameDe) : null))
            .ForMember(dest => dest.PlayerId, opt => opt.MapFrom(src => src.Player != null ? src.Player.Id : Guid.Empty))
            .ForMember(dest => dest.PlayerFullName, opt =>opt.MapFrom(src => src.Player!= null ? src.Player.FullName : null))
            .ForMember(dest => dest.PlayerFileUrl, opt =>opt.MapFrom(src => src.Player!= null ? src.Player.Documents.Where(x=>x.DocumentType == PlayerDocumentType.Profile).Select(x=>x.FileUrl).FirstOrDefault() : null))
            .ForMember(dest => dest.AssistPlayerId, opt => opt.MapFrom(src => src.AssistPlayer != null ? (Guid?)src.AssistPlayer.Id : null))
            .ForMember(dest => dest.AssistPlayerFullName, opt => opt.MapFrom(src => src.AssistPlayer != null ? src.AssistPlayer.FullName : null))
            .ForMember(dest => dest.AssistPlayerFileUrl, opt => opt.MapFrom(src => src.AssistPlayer != null ? src.AssistPlayer.Documents.Where(x => x.DocumentType == PlayerDocumentType.Profile).Select(x => x.FileUrl).FirstOrDefault() : null))
            ;


            CreateMap<Matchday, MatchHistoryDto>()

                .ForMember(dest => dest.OrganizerClubName, opt => opt.MapFrom(src => src.OrganizerClub != null ? src.OrganizerClub.FullName : null))
                .ForMember(dest => dest.OpponentClubName, opt => opt.MapFrom(src => src.OpponentClub != null ? src.OpponentClub.FullName : null))
                .ForMember(dest => dest.OrganizerClubUrl, opt => opt.MapFrom(src => src.OrganizerClub != null && src.OrganizerClub.Document != null ? src.OrganizerClub.Document.FileUrl : null))
                .ForMember(dest => dest.OpponentClubUrl, opt => opt.MapFrom(src => src.OpponentClub != null && src.OpponentClub.Document != null ? src.OpponentClub.Document.FileUrl : null))
                .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => (int)src.Venue));


        }
    }
}
