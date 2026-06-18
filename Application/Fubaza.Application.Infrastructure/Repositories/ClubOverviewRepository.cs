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
    public class ClubOverviewRepository : IClubOverviewRepository
    { 
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public ClubOverviewRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<ClubCountBySportDto>> GetClubCountBySportAsync()
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
                        _db.Club.Where(c => c.SportId != null && c.Sport!.Name != null),
                        sport => sport.Id,
                        club => club.SportId,
                        (sport, clubs) => new { Sport = sport, ClubCount = clubs.Count() }
                    )
                    .Select(g => new ClubCountBySportDto
                    {
                        SportId = g.Sport.Id,
                        SportName = g.Sport.Name ?? string.Empty,
                        ClubCount = g.ClubCount
                    })
                    .ToListAsync();
                
                var orderedResult = sportOrder
                    .Join(result, order => order, dto => dto.SportName, (order, dto) => dto)
                    .ToList();

                return orderedResult;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch Club sport counts. Details: {ex.Message}");
            }
        }

        public async Task<PaginatedResponse<PaginatedClubsDto>> GetPaginatedClubsAsync(PaginationRequest request)
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

                var query = _db.Club.AsExpandable()
                             .Where(predicate)
                             .Include(p => p.User)
                             .Include(p => p.Document);

                var totalCount = query.Count();

                var items = await query
                    .OrderBy(p => p.FullName)
                    .ProjectTo<PaginatedClubsDto>(_mapper.ConfigurationProvider)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PaginatedResponse<PaginatedClubsDto>
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
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch player sport counts. Details: {ex.Message}");
            }
        }


        public async Task<ClubInfoDto> GetClubInfoAsync(Guid clubId)
        {
            try
            {

                var club = await _db.Club.Where(c => c.Id == clubId).Include(c => c.Sport)
                                  .Include(c => c.Document)
                                  .Include(c => c.Players)
                                  .ThenInclude(p => p.PlayingPosition)
                                  .Include(c => c.Players)
                                  .ThenInclude(p => p.Documents)
                                  .Include(c => c.Officials)
                                  .ThenInclude(o => o.Designation)
                                  .Include(c => c.Officials)
                                  .ThenInclude(o => o.Document)
                                  .ProjectTo<ClubInfoDto>(_mapper.ConfigurationProvider)
                                  .FirstOrDefaultAsync();

                if (club == null)
                    throw new CustomException("Club not found");

                return club;

            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch Club sport counts. Details: {ex.Message}");
            }
        }
    }
}
