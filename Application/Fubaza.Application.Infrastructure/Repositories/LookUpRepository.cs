using AutoMapper;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Fubaza.Application.Infrastructure.Repositories
{
    public class LookUpRepository : ILookUpRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public LookUpRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<SportDto>> GetSportsAsync()
        {
            try
            {
                var sportOrder = new List<string>
                {
                    "Football", "Basketball", "American Football", "Ice Hockey",  "Handball", "Volleyball"
                };

                var sports = await _db.Sport
                       .Where(x => !x.IsDeleted)
                       .Select(s => new SportDto
                       {
                           Id = s.Id,
                           Name = s.Name.Localize(s.NameDe),
                           IsDeleted = s.IsDeleted,
                           NormalizedName = s.NormalizedName,
                       }).ToListAsync();

                 var orderedSports = sportOrder
                .Join(sports, orderName => orderName, s => s.Name, (orderName, s) => s).Concat(sports.Where(s => !sportOrder.Contains(s.Name ?? string.Empty))).ToList(); // Add remaining items not in order list

                return orderedSports;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Sports due to: {e.GetMessage()}");
            }
        }

        public async Task<List<PlayingPositionDto>> GetPlayingPositionsAsync(Guid sportId)
        {
            try
            {
                var playingPosition = await _db.PlayingPosition
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted & x.SportId == sportId)
                    .OrderBy(x => x.OrderId)
                    .Select(d => new PlayingPositionDto
                    {
                        Id = d.Id,
                        Name = d.Name.Localize(d.NameDe),
                        Category = d.Category,
                        IsDeleted = d.IsDeleted,
                        SportId = d.SportId
                    })
                    .ToListAsync();

                return playingPosition;

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Playing Position due to: {e.GetMessage()}");
            }
        }

        public async Task<List<EventTypeDTO>> GetEventTypeAsync(Guid sportId)
        {
            try
            {
                var eventType = await _db.EventType
                    .Where(x => !x.IsDeleted & x.SportId == sportId)
                    .AsNoTracking()
                    .Select(d => new EventTypeDTO
                    {
                        Id = d.Id,
                        Name = d.Name.Localize(d.NameDe),
                        EvenTypeName=d.EventTypeName,
                    })
                    .ToListAsync();

                return eventType;

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Event Type due to: {e.GetMessage()}");
            }
        }

        public async Task<List<DesignationDto>> GetDesignationsAsync()
        {
            try
            {
                var designations = await _db.Designation.AsNoTracking().OrderBy(d => d.Title)
                    .Select(d => new DesignationDto
                    {
                        Id = d.Id,
                        Title = d.Title.Localize(d.TitleDe),
                        Department = d.Department
                    })
                    .ToListAsync();

                return designations;

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Designation due to: {e.GetMessage()}");
            }
        }

        public async Task<Designation> GetDesignationAsync(Guid designationId)
        {
            try
            {
                var designation = await _db.Designation.FindAsync(designationId);

                if (designation == null)
                    throw new CustomException($"Designation with ID {designationId} was not found.");

                return designation;

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Playing Position due to: {e.GetMessage()}");
            }
        }

        public async Task<ClubOfficial> GetClubOfficialAsync(Guid clubOfficialId)
        {
            try
            {
                var clubOfficial = await _db.ClubOfficial.Include(x=>x.Designation).FirstOrDefaultAsync(x=>x.Id == clubOfficialId);

                if (clubOfficial == null)
                    throw new CustomException($"Designation with ID {clubOfficialId} was not found.");

                return clubOfficial;

            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Playing Position due to: {e.GetMessage()}");
            }
        }

        public async Task<List<TempleteDto>> GetTempletesAsync(TempleteRequest request)
        {
            try
            {
                var query = _db.Templete
                    .Include(x => x.Sport)
                    .Include(x => x.Documents)
                    .Where(x => !x.IsDeleted && x.IsApproved && x.SportId == request.SportId)
                    .AsNoTracking();

                // ✅ Apply category filter if provided
                if (request.TempleteType.HasValue)
                {
                    query = query.Where(x => x.TempleteType == request.TempleteType.Value);
                }

                var templetes = await query.ToListAsync();

                var templeteDtos = _mapper.Map<List<TempleteDto>>(templetes);

                return templeteDtos;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the Templates due to: {e.GetMessage()}");
            }
        }

    }
}
