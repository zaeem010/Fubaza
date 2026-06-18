using AutoMapper;

using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;


namespace Fubaza.Application.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public PlayerRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> AddOrUpdatePlayerAsync(Player player, Notification notification)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var club = await _db.Club.FirstOrDefaultAsync(s => s.Id == player.CurrentClubId);
                    player.SportId = club?.SportId;

                    if (player.IsCaption && player.CurrentClubId != null)
                    {
                        var currentCaptain = await _db.Player
                            .Where(p => p.CurrentClubId == player.CurrentClubId && p.IsCaption)
                            .FirstOrDefaultAsync();

                        if (currentCaptain != null)
                        {
                            currentCaptain.IsCaption = false;
                            _db.Player.Update(currentCaptain);
                        }
                    }

                    if (player.Id == Guid.Empty)
                    {
                        player.Id = Guid.NewGuid();

                        if (player.Documents != null)
                        {
                            foreach (var doc in player.Documents)
                            {
                                doc.Id = Guid.NewGuid();
                                doc.PlayerId = player.Id;
                            }
                        }
                        _db.Player.Add(player);
                    }
                    else
                    {
                        var existingPlayer = await _db.Player
                            .Include(p => p.Documents)
                            .FirstOrDefaultAsync(p => p.Id == player.Id);

                        if (existingPlayer != null)
                        {
                            existingPlayer.FullName = player.FullName;
                            existingPlayer.DateOfBirth = player.DateOfBirth;
                            existingPlayer.Gender = player.Gender;
                            existingPlayer.IsCaption = player.IsCaption;
                            existingPlayer.JerseyNumber = player.JerseyNumber;
                            existingPlayer.PlayingPositionId = player.PlayingPositionId;
                            existingPlayer.Nationality = player.Nationality;
                            existingPlayer.StrongFoot = player.StrongFoot;
                            existingPlayer.ThrowingHand = player.ThrowingHand;

                            var existingDocs = existingPlayer.Documents.ToList();
                            var newDocs = player.Documents ?? new List<PlayerDocument>();

                            foreach (var newDoc in newDocs)
                            {
                                var match = existingDocs.FirstOrDefault(d => d.DocumentType == newDoc.DocumentType);
                                if (match != null)
                                {
                                    match.FileContent = newDoc.FileContent;
                                    match.FileName = newDoc.FileName;
                                    match.Extension = newDoc.Extension;
                                    match.FileUrl = newDoc.FileUrl;
                                    _db.PlayerDocument.Update(match);
                                }
                                else
                                {
                                    newDoc.Id = Guid.NewGuid();
                                    newDoc.PlayerId = player.Id;
                                    _db.PlayerDocument.Add(newDoc);
                                }
                            }
                        }
                        else
                        {
                            player.Id = Guid.NewGuid();

                            if (player.Documents != null)
                            {
                                foreach (var doc in player.Documents)
                                {
                                    doc.Id = Guid.NewGuid();
                                    doc.PlayerId = player.Id;
                                }
                            }
                            _db.Player.Add(player);
                        }
                    }
                   
                    await _db.Notifications.AddAsync(notification);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to process the player: {e.GetMessage()}");
                }
            }
        }
        public async Task<User?> UpdatePlayerProfileAsync(Player player)
        {
            try
            {
                var ouser = await _db.Users
                    .Include(u => u.Player)
                        .ThenInclude(p => p.CurrentClub)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.ClubHistory)
                            .ThenInclude(ch => ch.Club)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Documents) 
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == player.UserId);

                var profileDoc = player.Documents?.FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);

                if (ouser == null) return null;

                var existingPlayer = ouser.Player;

                if (existingPlayer == null)
                {
                    player.Id = player.Id != Guid.Empty ? player.Id : Guid.NewGuid();
                    ouser.Player = player;

                    if (profileDoc != null)
                        profileDoc.PlayerId = player.Id;

                    _db.Player.Add(player);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(player.FullName)) existingPlayer.FullName = player.FullName;
                    if (player.Gender != null) existingPlayer.Gender = player.Gender;
                    if (player.DateOfBirth != null) existingPlayer.DateOfBirth = player.DateOfBirth;
                    if (player.SportId.HasValue) existingPlayer.SportId = player.SportId;
                    if (player.PlayingPositionId.HasValue) existingPlayer.PlayingPositionId = player.PlayingPositionId;
                    if (player.WeightKg.HasValue) existingPlayer.WeightKg = player.WeightKg;
                    if (player.HeightCm.HasValue) existingPlayer.HeightCm = player.HeightCm;
                    if (player.StrongFoot.HasValue) existingPlayer.StrongFoot = player.StrongFoot;
                    if (player.ThrowingHand.HasValue) existingPlayer.ThrowingHand = player.ThrowingHand;
                    if (!string.IsNullOrWhiteSpace(player.Nationality)) existingPlayer.Nationality = player.Nationality;


                    if (player.ClubHistory != null && player.ClubHistory.Any())
                    {
                        var existingHistories = _db.PlayerClubHistory.Where(ch => ch.PlayerId == existingPlayer.Id);
                        _db.PlayerClubHistory.RemoveRange(existingHistories);

                        foreach (var history in player.ClubHistory)
                        {
                            var newHistory = new PlayerClubHistory
                            {
                                PlayerId = existingPlayer.Id,
                                ClubId = history.ClubId,
                                StartYear = history.StartYear,
                                EndYear = history.EndYear,
                                IsCurrentClub = history.IsCurrentClub
                            };

                            _db.PlayerClubHistory.Add(newHistory);

                            if (history.IsCurrentClub)
                            {
                                existingPlayer.CurrentClubId = history.ClubId;
                                existingPlayer.CurrentClub = await _db.Club.FindAsync(history.ClubId);
                            }
                        }
                    }
                    
                    if (profileDoc is { FileName: not null })
                    {
                        var existingProfileDoc = existingPlayer.Documents
                            .FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);

                        if (existingProfileDoc != null)
                        {
                            existingProfileDoc.FileName = profileDoc.FileName;
                            existingProfileDoc.FileUrl = profileDoc.FileUrl;
                            existingProfileDoc.Extension = profileDoc.Extension;
                        }
                        else
                        {
                            profileDoc.PlayerId = existingPlayer.Id;
                            existingPlayer.Documents.Add(profileDoc);
                        }
                    }
                }
               
                await _db.SaveChangesAsync();
               
                return ouser;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Sports due to: {e.GetMessage()}");
            }
        }
        public async Task<PlayerProfileDTO?> EditPlayerProfileAsync(Player updatedPlayer)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.Player)
                        .ThenInclude(p => p.CurrentClub)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.ClubHistory)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Documents)
                    .FirstOrDefaultAsync(u => u.Id == updatedPlayer.UserId);

                if (user == null || user.Player == null)
                    return null;

                var player = user.Player;

                var profileDoc = updatedPlayer.Documents?
                    .FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);

                // Update basic info
                player.FullName = updatedPlayer.FullName;
                player.Gender = updatedPlayer.Gender;
                player.DateOfBirth = updatedPlayer.DateOfBirth;
                player.WeightKg = updatedPlayer.WeightKg;
                player.HeightCm = updatedPlayer.HeightCm;
                player.StrongFoot = updatedPlayer.StrongFoot;
                player.ThrowingHand = updatedPlayer.ThrowingHand;
                player.PlayingPositionId = updatedPlayer.PlayingPositionId;

                if (!string.IsNullOrWhiteSpace(updatedPlayer.Nationality) &&
                !string.Equals(player.Nationality, updatedPlayer.Nationality, StringComparison.OrdinalIgnoreCase))
                {
                    player.Nationality = updatedPlayer.Nationality;
                }

                // Update club history
                if (updatedPlayer.ClubHistory != null && updatedPlayer.ClubHistory.Any())
                {
                    var existingHistories = _db.PlayerClubHistory.Where(ch => ch.PlayerId == player.Id);
                    _db.PlayerClubHistory.RemoveRange(existingHistories);

                    foreach (var history in updatedPlayer.ClubHistory)
                    {
                        var newHistory = new PlayerClubHistory
                        {
                            PlayerId = player.Id,
                            ClubId = history.ClubId,
                            StartYear = history.StartYear,
                            EndYear = history.EndYear,
                            IsCurrentClub = history.IsCurrentClub,
                        };

                        _db.PlayerClubHistory.Add(newHistory);

                        if (history.IsCurrentClub)
                        {
                            player.CurrentClubId = history.ClubId;
                            player.CurrentClub = await _db.Club.FindAsync(history.ClubId);
                        }
                    }
                }

                // Update or add profile document
                if (profileDoc != null && !string.IsNullOrEmpty(profileDoc.FileName))
                {
                    var existingProfileDoc = player.Documents?
                        .FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);

                    if (existingProfileDoc != null)
                    {
                        existingProfileDoc.FileName = profileDoc.FileName;
                        existingProfileDoc.FileUrl = profileDoc.FileUrl;
                        existingProfileDoc.Extension = profileDoc.Extension;
                    }
                    else
                    {
                        profileDoc.PlayerId = player.Id;
                        _db.PlayerDocument.Add(profileDoc);
                        player.Documents.Add(profileDoc);
                    }
                }

                await _db.SaveChangesAsync();

                // Re-fetch with all related data
                var updatedUser = await _db.Users
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Sport)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.PlayingPosition)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Documents)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.ClubHistory)
                            .ThenInclude(ch => ch.Club)
                    .FirstOrDefaultAsync(u => u.Id == updatedPlayer.UserId);

                if (updatedUser?.Player?.Documents != null)
                {
                    updatedUser.Player.Documents = updatedUser.Player.Documents
                        .Where(d => d.DocumentType == PlayerDocumentType.Profile)
                        .ToList();
                }

                var userDTO = _mapper.Map<PlayerProfileDTO>(updatedUser);

                
                return userDTO;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to update player profile due to: {e.GetMessage()}");
            }
        }
        public async Task<PlayerProfileDTO?> GetPlayerProfileAsync(Guid userId)
        {
            try
            {
                var userprofile = await _db.Users
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Sport)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.PlayingPosition)
                    .Include(u => u.Player)
                        .ThenInclude(p => p.Documents) 
                    .Include(u => u.Player)
                        .ThenInclude(p => p.ClubHistory)
                            .ThenInclude(ch => ch.Club)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (userprofile == null || userprofile.Player == null)
                    return null;

                if (userprofile.Player?.Documents != null && userprofile.Player.Documents.Any())
                {
                    userprofile.Player.Documents = userprofile.Player.Documents
                        .Where(d => d.DocumentType == PlayerDocumentType.Profile)
                        .ToList();
                }

                var userDTO = _mapper.Map<PlayerProfileDTO>(userprofile);

                
                return userDTO;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Players due to: {e.GetMessage()}");
            }
        }
        public async Task<Dictionary<string, CategoryPlayerDto>> GetPlayersByCategoryAsync(Guid clubId)
        {
            try
            {
                var players = await _db.Player
                    .Include(p => p.PlayingPosition)
                    .Include(p => p.Documents)
                    .Include(p => p.User)
                    .Where(p => p.CurrentClubId == clubId && p.User.CanLogin == false)
                    .ToListAsync();

                if (players == null || players.Count == 0)
                    return new Dictionary<string, CategoryPlayerDto>();

                var groupedResult = players
                    .GroupBy(p =>
                        p.PlayingPosition?.Category ?? PositionCategory.Other
                    )
                    .OrderBy(g => (int)g.Key) // enum order
                    .ToDictionary(
                        g => g.Key.GetLocalizedEnum(), // 🌍 localized parent key
                        g => new CategoryPlayerDto
                        {
                            Count = g.Count(),
                            Players = g.Select(p => new PlayerDto
                            {
                                Id = p.Id,
                                FullName = p.FullName,
                                PlayingPosition = p.PlayingPosition?.Name.Localize(p.PlayingPosition?.NameDe),
                                PlayingPositionId = p.PlayingPosition?.Id,
                                IsCaption = p.IsCaption,
                                DateOfBirth = p.DateOfBirth,
                                StrongFoot = p.StrongFoot,
                                ThrowingHand = p.ThrowingHand,
                                JerseyNumber = p.JerseyNumber,
                                Gender = p.Gender,
                                Documents = Enum.GetValues<PlayerDocumentType>()
                                    .ToDictionary(
                                        docType => docType,
                                        docType => p.Documents
                                            .FirstOrDefault(d => d.DocumentType == docType)?.FileUrl)
                            }).ToList()
                        });

                return groupedResult;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the players due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> RemovePlayerAsync(Guid playerId , Notification notification)
        {

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var player = await _db.Player
                    .Include(p => p.ClubHistory)
                    .FirstOrDefaultAsync(p => p.Id == playerId);

                    if (player == null)
                    {
                        return false;
                    }

                    if (player.CurrentClubId == null)
                    {
                        return false;
                    }

                    var clubId = player.CurrentClubId;
                    player.CurrentClubId = null;

                    // Update club history
                    var currentHistory = player.ClubHistory
                        .FirstOrDefault(h => h.ClubId == clubId && h.IsCurrentClub);

                    if (currentHistory != null)
                    {
                        currentHistory.EndYear = DateTime.UtcNow.Year;
                        currentHistory.IsCurrentClub = false;
                        _db.PlayerClubHistory.Update(currentHistory);
                    }

                    _db.Player.Update(player);

                    await _db.Notifications.AddAsync(notification);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;


                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to process the player: {e.GetMessage()}");
                }
            }

        }
        public async Task<Player?> GetPlayerAsync(Guid playerId)
        {
            try
            {
                var player = await _db.Player.FindAsync(playerId);

                return player;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Player due to: {e.GetMessage()}");
            }
        }

        public async Task<Dictionary<object, DocumentTypeGroupDto>> GetMediaGalleryAsync(Guid clubid)
        {
            try
            {
                var club = await _db.Club
            .Include(c => c.Document)                // club logo or document
            .Include(c => c.Players)
                .ThenInclude(p => p.Documents)       // player images
            .Include(c => c.Players)
                .ThenInclude(p => p.User)            // for player names
            .FirstOrDefaultAsync(c => c.Id == clubid);

                if (club == null)
                    return new Dictionary<object, DocumentTypeGroupDto>();


                var players = club.Players
                    .Where(p => p.User?.CanLogin == false && p.CurrentClubId != null)
                .ToList();


                var result = players
                    .SelectMany(p => p.Documents.Select(doc => new { Player = p, Document = doc }))
                    .Where(x => !x.Document.IsDeleted)
                    .GroupBy(x => (object)x.Document.DocumentType)
                    .ToDictionary(
                        group => group.Key,
                        group => new DocumentTypeGroupDto
                        {
                            Count = group.Count(),
                            Photos = group.Select(x => new DocumentPhotoDto
                            {
                                Name = x.Player.FullName ?? "Unknown",
                                Url = x.Document.FileUrl ?? ""
                            }).ToList()
                        });

                var clubs = await _db.Club
                            .Include(c => c.Document)
                            .Include(c => c.User)
                            .Where(c => c.Id == clubid && c.Document != null &&
                        !c.Document.IsDeleted &&
                        !string.IsNullOrEmpty(c.Document.FileUrl)).ToListAsync();

                if (clubs.Any())
                {
                    var clubLogos = clubs.Select(c => new DocumentPhotoDto
                    {
                        Name = c.FullName ?? "Unnamed Club",
                        Url = c.Document?.FileUrl ?? ""
                    }).ToList();

                    // Add club logos to the dictionary under ClubLogo enum
                    result["ClubLogo"] = new DocumentTypeGroupDto
                    {
                        Count = clubLogos.Count,
                        Photos = clubLogos
                    };
                }


                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch player documents due to: {e.GetMessage()}");
            }
        }

        public async Task<bool> CheckPlayerJerseyAsync(Player player)
        {
            try
            {
                var jerseycheck = await _db.Player
                         .Where(p => p.CurrentClubId == player.CurrentClubId && p.JerseyNumber == player.JerseyNumber && p.Id != player.Id)
                         .AnyAsync();
                if (jerseycheck)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to check jersey number due to: {e.GetMessage()}");
            }
        }
    }
}
