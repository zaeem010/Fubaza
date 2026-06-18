using AutoMapper;
using AutoMapper.QueryableExtensions;

using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Persistence;

using LinqKit;

using Microsoft.EntityFrameworkCore;

namespace Fubaza.Application.Infrastructure.Repositories
{
    public class PlayerOverviewRepository : IPlayerOverviewRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public PlayerOverviewRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<PlayerCountBySportDto>> GetPlayerCountBySportAsync()
        {
            try
            {
                var sportOrder = new List<string>
                {
                    "Football", "Basketball",  "Ice Hockey", "American Football", "Handball", "Volleyball"
                };

                var result = await _db.Sport
                    .Where(s => !s.IsDeleted)
                    .GroupJoin(
                        _db.Player.Where(p => p.SportId != null && p.Sport!.Name != null),
                        sport => sport.Id,
                        player => player.SportId,
                        (sport, players) => new { Sport = sport, PlayerCount = players.Count() }
                    )
                    .Select(g => new PlayerCountBySportDto
                    {
                        SportId = g.Sport.Id,
                        SportName = g.Sport.Name ?? string.Empty,
                        PlayerCount = g.PlayerCount
                    })
                    .ToListAsync();

                // Apply the custom ordering
                var orderedResult = sportOrder
                    .Join(result, order => order, dto => dto.SportName, (order, dto) => dto)
                    .ToList();

                return orderedResult;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch player sport counts. Details: {ex.Message}");
            }
        }

        public async Task<PaginatedResponse<PaginatedPlayersDto>> GetPaginatedPlayersAsync(PaginationRequest request)
        {
            try
            {
                var predicate = PredicateBuilder.New<Player>(true);

                if (request.SportId != null && request.SportId != Guid.Empty)
                {
                    predicate = predicate.And(p => p.SportId == request.SportId);
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    bool isNumber = int.TryParse(request.SearchTerm, out int number);

                    predicate = predicate.And(p =>
                        p.FullName!.ToLower().Contains(searchTerm) ||
                        (p.CurrentClub != null && p.CurrentClub.FullName!.ToLower().Contains(searchTerm)) ||
                        (p.PlayingPosition != null && p.PlayingPosition.Name!.ToLower().Contains(searchTerm)) ||
                        (
                            p.DateOfBirth.HasValue &&
                            (
                                isNumber &&
                                (
                                    (number >= 1900 && p.DateOfBirth.Value.Year == number) ||
                                    (number >= 1 && number <= 12 && p.DateOfBirth.Value.Month == number) ||
                                    (number >= 1 && number <= 31 && p.DateOfBirth.Value.Day == number)
                                )
                            )
                        )
                    );
                }

                var query = _db.Player
                    .AsExpandable()
                    .Where(predicate)
                    .Include(p => p.User)
                    .Include(p => p.PlayingPosition)
                    .Include(p => p.CurrentClub)
                    .Include(p => p.Documents); // Still required for EF to load in ProjectTo

                var totalCount =  query.Count();

                var players = await query
                    .OrderBy(p => p.FullName)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ProjectTo<PaginatedPlayersDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return new PaginatedResponse<PaginatedPlayersDto>
                {
                    Pagination = new PaginationInfo
                    {
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    Items = players
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch player list. Details: {ex.Message}");
            }
        }

        public async Task<PlayerInfoDto> GetPlayerInfoAsync(Guid playerId)
        {
            try
            {
                var player = await _db.Player
                    .Include(p => p.User)
                    .Include(p => p.PlayingPosition)
                    .Include(p => p.CurrentClub)
                    .Include(p => p.Documents)
                    .Include(p => p.ClubHistory)
                .ThenInclude(ch => ch.Club)
                    .ThenInclude(c => c.Document)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == playerId);

                var playerInfoDto = _mapper.Map<PlayerInfoDto>(player);


                return playerInfoDto;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch player sport counts. Details: {ex.Message}");
            }
        }
    }
}
