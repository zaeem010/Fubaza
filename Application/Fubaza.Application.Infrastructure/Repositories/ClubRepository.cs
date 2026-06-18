using AutoMapper;
using AutoMapper.QueryableExtensions;

using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts.Services.Identity;
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
    public class ClubRepository : IClubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        public ClubRepository(ApplicationDbContext db, IMapper mapper, ICurrentUser currentUser)
        {
            _db = db;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<PaginatedResponse<ClubDTO>> GetClubAsync(PaginationRequest request)
        {
            try
            {
                var predicate = PredicateBuilder.New<Club>(true);

                predicate = predicate.And(p => p.SportId == request.SportId);

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();

                    predicate = predicate.And(p =>
                        p.FullName!.ToLower().Contains(searchTerm)

                    );
                }
                if (request.ClubId.HasValue)
                {
                    predicate = predicate.And(p => p.CreatorClubId == request.ClubId.Value);
                }
                else if (request.UserId.HasValue)
                {
                    predicate = predicate.And(p => p.CreatedByUserId == request.UserId.Value);
                }

                var query = _db.Club.AsExpandable()
                             .Where(predicate)
                             .Include(p => p.User)
                             .Include(p => p.Document);

                var totalCount = query.Count();

                var items = await query
                    .OrderBy(p => p.FullName)
                    .ProjectTo<ClubDTO>(_mapper.ConfigurationProvider)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PaginatedResponse<ClubDTO>
                {
                    Pagination = new PaginationInfo
                    {
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    Items = items
                };
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Sports due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> AddClubAsync(Club club)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    _db.Club.Add(club);

                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to fetch the Club due to: {e.GetMessage()}");
                }
            }
        }
        public async Task<ClubProfileDTO?> GetClubProfileAsync(Guid userId)
        {
            try
            {
                var clubprofile = await _db.Users
                    .Include(u => u.Club)
                    .ThenInclude(u => u.Document)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (clubprofile == null || clubprofile.Club == null)
                    return null;

                var clubProfileDTO = _mapper.Map<ClubProfileDTO>(clubprofile);

                return clubProfileDTO;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Clubs due to: {e.GetMessage()}");
            }
        }
        public async Task<ClubProfileDTO> EditClubProfileAsync(Club updatedClub)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.Club)
                        .ThenInclude(c => c.Document)
                    .FirstOrDefaultAsync(u => u.Id == updatedClub.UserId);

                if (user?.Club == null)
                    return null;

                var club = user.Club;

                // Update club details
                club.FullName = updatedClub.FullName;
                club.Address = updatedClub.Address;
                club.Nationality = updatedClub.Nationality;
                club.League = updatedClub.League;
                club.Division = updatedClub.Division;
                club.Description = updatedClub.Description;

                // Update user's phone number if provided
                if (!string.IsNullOrWhiteSpace(updatedClub.PhoneNumber))
                {
                    user.PhoneNumber = updatedClub.PhoneNumber;
                }

                // Update or add document
                if (updatedClub.Document is { FileName: not null })
                {
                    if (club.Document != null)
                    {
                        club.Document.FileName = updatedClub.Document.FileName;
                        club.Document.FileUrl = updatedClub.Document.FileUrl;
                        club.Document.Extension = updatedClub.Document.Extension;
                    }
                    else
                    {
                        var newDoc = new ClubDocument
                        {
                            ClubId = club.Id,
                            FileName = updatedClub.Document.FileName,
                            FileUrl = updatedClub.Document.FileUrl,
                            Extension = updatedClub.Document.Extension
                        };
                        _db.ClubDocument.Add(newDoc);
                        club.Document = newDoc;
                    }
                }

                await _db.SaveChangesAsync();

                // Re-fetch the updated user including club + document
                var updatedUser = await _db.Users
                    .Include(u => u.Club)
                        .ThenInclude(c => c.Document)
                    .FirstOrDefaultAsync(u => u.Id == updatedClub.UserId);

                // Map and return the updated profile
                var clubProfileDTO = _mapper.Map<ClubProfileDTO>(updatedUser);
                return clubProfileDTO;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to edit club profile: {e.GetMessage()}");
            }
        }
        public async Task<User> UpdateClubProfileAsync(Club club)
        {
            try
            {
                var ouser = await _db.Users
                    .Include(u => u.Club)
                    .ThenInclude(x => x.Sport)
                     .Include(u => u.Club).
                    ThenInclude(c => c.Document)
                    .FirstOrDefaultAsync(u => u.Id == club.UserId);

                if (ouser == null) return null;

                var existingClub = ouser.Club;

                if (existingClub == null)
                {
                    _db.Club.Add(club);
                    existingClub = club;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(club.FullName)) existingClub.FullName = club.FullName;
                    if (club.SportId.HasValue) existingClub.SportId = club.SportId;
                    if (!string.IsNullOrWhiteSpace(club.Address)) existingClub.Address = club.Address;
                    if (!string.IsNullOrWhiteSpace(club.Nationality)) existingClub.Nationality = club.Nationality;
                    if (!string.IsNullOrWhiteSpace(club.League)) existingClub.League = club.League;
                    if (!string.IsNullOrWhiteSpace(club.Division)) existingClub.Division = club.Division;
                    if (!string.IsNullOrWhiteSpace(club.Description)) existingClub.Description = club.Description;

                    if (club.Document is { FileName: not null })
                    {
                        if (existingClub.Document != null)
                        {
                            existingClub.Document.FileName = club.Document.FileName;
                            existingClub.Document.FileUrl = club.Document.FileUrl;
                            existingClub.Document.Extension = club.Document.Extension;
                        }
                        else
                        {
                            var newDoc = new ClubDocument
                            {
                                ClubId = existingClub.Id,
                                FileName = club.Document.FileName,
                                FileUrl = club.Document.FileUrl,
                                Extension = club.Document.Extension
                            };
                            _db.ClubDocument.Add(newDoc);
                            existingClub.Document = newDoc;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(club.PhoneNumber) && ouser.PhoneNumber != club.PhoneNumber)
                {
                    ouser.PhoneNumber = club.PhoneNumber;
                    ouser.PhoneNumberConfirmed = true;
                }

                await _db.SaveChangesAsync();

                if (existingClub?.SportId != null)
                {
                    await _db.Entry(existingClub)
                        .Reference(c => c.Sport)
                        .LoadAsync();
                }
                return ouser;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Sports due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> AddOrUpdateClubOfficialAsync(ClubOfficial clubOfficial , Notification notification)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (clubOfficial.Id == Guid.Empty)
                    {
                        clubOfficial.Id = Guid.NewGuid();
                        _db.ClubOfficial.Add(clubOfficial);
                    }
                    else
                    {
                        var existingOfficial = await _db.ClubOfficial.Include(x=>x.Document)
                            .FirstOrDefaultAsync(c => c.Id == clubOfficial.Id);

                        if (existingOfficial != null)
                        {
                            existingOfficial.Name = clubOfficial.Name;
                            existingOfficial.DesignationId = clubOfficial.DesignationId;
                            existingOfficial.DateOfBirth = clubOfficial.DateOfBirth;
                            existingOfficial.JoiningDate = clubOfficial.JoiningDate;
                            existingOfficial.Gender = clubOfficial.Gender;
                            existingOfficial.ClubId = clubOfficial.ClubId;

                            if (clubOfficial.Document != null)
                            {
                                if (existingOfficial.Document == null)
                                {
                                    clubOfficial.Document.Id = Guid.NewGuid();
                                    clubOfficial.Document.ClubOfficialId = existingOfficial.Id;
                                    _db.ClubOfficialDocument.Add(clubOfficial.Document);
                                }
                                else
                                {
                                    existingOfficial.Document.FileName = clubOfficial.Document.FileName;
                                    existingOfficial.Document.FileUrl = clubOfficial.Document.FileUrl;
                                    existingOfficial.Document.Extension = clubOfficial.Document.Extension;
                                    _db.ClubOfficialDocument.Update(existingOfficial.Document);
                                }
                            }
                        }

                    }

                    _db.Notifications.Add(notification);

                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to process the Club Official: {e.GetMessage()}");
                }
            }
        }
        public async Task<Dictionary<string, List<DepartmentOfficialDto>>> GetClubOfficialsByDepartmentAsync(Guid userId)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.Club)
                        .ThenInclude(c => c.Officials)
                            .ThenInclude(o => o.Designation)
                    .Include(u => u.Club)
                        .ThenInclude(c => c.Officials)
                            .ThenInclude(o => o.Document)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.Club?.Officials == null)
                    return new Dictionary<string, List<DepartmentOfficialDto>>();

                var officials = user.Club.Officials
                    .Where(o => !o.IsDeleted && o.Designation?.Department != null)
                    .ToList();

                var result = officials
                    .GroupBy(o => o.Designation!.Department!.Value)
                    .ToDictionary(
                        group => EnumExtensions.GetLocalizedEnum(group.Key), // 🔹 Get description instead of enum name
                        group => group.Select(o => new DepartmentOfficialDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            DesignationTitle = o.Designation?.Title.Localize(o.Designation?.TitleDe),
                            DesignationId = o.DesignationId,
                            Gender = o.Gender,
                            DateOfBirth = o.DateOfBirth,
                            JoiningDate = o.JoiningDate,
                            DocumentUrl = o.Document?.FileUrl
                        }).ToList()
                    );

                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch Club Officials due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> RemoveClubOfficialAsync(Guid clubOfficialId , Notification notification)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            try
            {
                    var official = await _db.ClubOfficial
                        .FirstOrDefaultAsync(o => o.Id == clubOfficialId);

                    if (official == null)
                        return false;

                    official.IsDeleted = true;
                    _db.ClubOfficial.Update(official);

                    await _db.Notifications.AddAsync(notification);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
            catch (Exception ex)
            {
                 await transaction.RollbackAsync();
                 throw new CustomException($"Failed to remove Club Official: {ex.GetMessage()}");
            }
        }

        public async Task<List<LineupPlayerInfoDto>> GetMatchLinupbyCludAsync(Guid matchdayId,Guid organizerClubId)
        {
            try
            {
                var result = await _db.Matchday
                    .AsNoTracking()
                    .Where(x => x.Id == matchdayId && x.OrganizerClubId == organizerClubId)
                    .SelectMany(x => x.MatchLineUp)
                    .Select(p => new LineupPlayerInfoDto
                    {
                        PlayerId = p.PlayerId,
                        FullName = p.Player.FullName,
                        FileUrl = p.Player.Documents.Select(d => d.FileUrl).FirstOrDefault()
                    })
                    .ToListAsync();
                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the line up due to: {e.GetMessage()}");
            }
        }

        public async Task<ClubStatDto> GetClubStatsAsync(Guid clubId)
        {
            var result = new ClubStatDto();
            try
            {
                var scoringTypes = new List<string>
                {
                    "Field Goal",
                    "Goal",
                    "Point"
                };
                var eventId = await _db.EventType
                    .Where(x => x.SportId == _currentUser.GetSportId() && scoringTypes.Contains(x.Name ?? string.Empty))
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                var matches= await _db.Matchday
                    .AsNoTracking()
                    .Where(m => m.OrganizerClubId == clubId && m.DisappearDateTime < DateTime.UtcNow)
                    .ToListAsync();

                int totalGoals = 0, totalMatches = 0, totalwins = 0, totallose = 0, totaldraw = 0;
                totalMatches = matches.Count();
                foreach (var item in matches)
                {
                    int mygoal=0, oppgoal = 0;
                    var matchsummary = await _db.MatchSummary.Where(x => x.MatchdayId == item.Id && x.EventTypeId == eventId).ToListAsync();
                    foreach (var goal in matchsummary)
                    {
                        if (goal.ClubId == item.OrganizerClubId)
                        {
                            mygoal++;
                        }
                        if (goal.ClubId == item.OpponentClubId)
                        {
                            oppgoal++;
                        }
                    }
                    if (mygoal > oppgoal)
                    {
                        totalwins++;
                    }
                    if (mygoal < oppgoal)
                    {
                        totallose++;
                    }
                    if (mygoal == oppgoal)
                    {
                        totaldraw++;
                    }

                    totalGoals += mygoal;
                }
                result.Matches = totalMatches;
                result.Goals = totalGoals;
                result.Wins = totalwins;
                result.Loses = totallose;
                result.Draws = totaldraw;
                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the line up due to: {e.GetMessage()}");
            }  
        }
    }
}
