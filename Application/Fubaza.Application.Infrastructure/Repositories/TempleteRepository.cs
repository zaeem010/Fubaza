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
    public class TempleteRepository : ITempleteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        public TempleteRepository(ApplicationDbContext db, IMapper mapper, ICurrentUser currentUser)
        {
            _db = db;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<bool> AddOrUpdatedTempleteAsync(Templete templete)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (templete.Id == Guid.Empty)
                    {
                        // New record
                        templete.Id = Guid.NewGuid();
                        _db.Templete.Add(templete);
                    }
                    else
                    {
                        // Load existing template with documents
                        var existingTemplete = await _db.Templete
                            .Include(x => x.Documents)
                            .FirstOrDefaultAsync(c => c.Id == templete.Id);

                        if (existingTemplete != null)
                        {
                            existingTemplete.Title = templete.Title;
                            existingTemplete.SportId = templete.SportId;
                            existingTemplete.TempleteType = templete.TempleteType;

                            // 1. REMOVE all old documents
                            if (existingTemplete.Documents.Any())
                            {
                                _db.TempleteDocument.RemoveRange(existingTemplete.Documents);
                            }

                            // 2. ADD new documents list
                            if (templete.Documents != null && templete.Documents.Any())
                            {
                                foreach (var doc in templete.Documents)
                                {
                                    doc.Id = Guid.NewGuid();   // Ensure new IDs
                                    doc.TempleteId = existingTemplete.Id;
                                }

                                await _db.TempleteDocument.AddRangeAsync(templete.Documents);
                            }
                        }
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to process the Templete: {e.GetMessage()}");
                }
            }
        }
        public async Task<PaginatedResponse<TempleteDto>> GetTempletesAsync(PaginationRequest request)
        {
            var predicate = PredicateBuilder.New<Templete>(true);

            // Filter: SportId
            if (request.SportId.HasValue)
                predicate = predicate.And(p => p.SportId == request.SportId.Value);

            // Filter: TempleteType
            if (request.TempleteType.HasValue)
                predicate = predicate.And(p => p.TempleteType == request.TempleteType.Value);

            // Filter: Search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                string term = request.SearchTerm.ToLower();
                predicate = predicate.And(p => p.Title!.ToLower().Contains(term));
            }

            // Filter: IsApproved
            if (request.IsApproved.HasValue)
                predicate = predicate.And(p => p.IsApproved == request.IsApproved);

            // ------------------------------------
            // PERMISSION CHECK: SUPERADMIN LOGIC
            // ------------------------------------
            var permissions = _currentUser.GetPermissions().ToList();
            bool isSuperAdmin = permissions.Contains("Permissions.UserTemepletes.View");

            if (isSuperAdmin)
            {
                // SuperAdmin can filter by user if provided
                if (request.UserId.HasValue)
                {
                    predicate = predicate.And(p => p.UserId == request.UserId.Value);
                }
            }
            else
            {
                // Normal users: only their own templates
                predicate = predicate.And(p => p.UserId == request.UserId);
            }

            // ------------------------------------
            // QUERY
            // ------------------------------------
            var query = _db.Templete
                .Where(x => !x.IsDeleted)
                .AsExpandable()
                .Where(predicate)
                .Include(p => p.Documents)
                .Include(p => p.Sport);

            var total = query.Count();

            var items = await query
                .OrderBy(p => p.Title)
                .ProjectTo<TempleteDto>(_mapper.ConfigurationProvider)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<TempleteDto>
            {
                Pagination = new PaginationInfo
                {
                    TotalCount = total,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                },
                Items = items
            };
        }

        public async Task<TempleteDto> GetTempleteAsync(Guid templeteId)
        {
            try
            {
                var templete = await _db.Templete
                    .Include(x=>x.Sport)
                    .Include(x => x.Documents)
                    .AsNoTracking().FirstOrDefaultAsync(x => x.Id == templeteId);

                var templeteDto = _mapper.Map<TempleteDto>(templete);

                return templeteDto;


            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch player sport counts. Details: {ex.Message}");
            }
        }

        public async Task<bool> BulkApproveTemplatesAsync(TempleteApprovalRequest request)
        {
            try
            {
                await _db.Templete
                    .Where(t => request.TemplateIds.Contains(t.Id))
                    .ExecuteUpdateAsync(t => t
                        .SetProperty(x => x.IsApproved, request.IsApproved)
                    );

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the Templete: {e.GetMessage()}");
            }
        }
        public async Task<bool> DeleteTempleteAsync(Guid templeteId)
        {
            try
            {
                await _db.Templete
                    .Where(t => t.Id == templeteId)
                    .ExecuteUpdateAsync(t => t
                        .SetProperty(x => x.IsDeleted, true)
                    );

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the Templete: {e.GetMessage()}");
            }
        }

        public async Task<TempleteImageDto> UplaodTempleteImageAsync(TempleteImage templateImage)
        {
            try
            {
                templateImage.Id = Guid.NewGuid();

                await _db.TempleteImage.AddAsync(templateImage);
                await _db.SaveChangesAsync();

                var templateImages = await _db.TempleteImage
                    .Where(x => !x.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x=>x.Id == templateImage.Id);

                return _mapper.Map<TempleteImageDto>(templateImages);
            }
            catch (Exception ex)
            {
                throw new CustomException($"Failed to remove Templete Image: {ex.GetMessage()}");
            }
        }
        public async Task<bool> DeleteTempleteImageAsync(Guid TempleteImageId)
        {
            try
            {
                var templeteImage = await _db.TempleteImage
                    .FirstOrDefaultAsync(o => o.Id == TempleteImageId);

                if (templeteImage == null)
                    return false;

                templeteImage.IsDeleted = true;
                _db.TempleteImage.Update(templeteImage);

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Failed to remove Templete Image: {ex.GetMessage()}");
            }
        }
        public async Task<List<TempleteImageDto>> GetTempleteImageAsync(Guid userId)
        {
            try
            {
                var templeteImage = await _db.TempleteImage.Where(x=> !x.IsDeleted & x.UserId == userId && x.IsUserUpload).
                    ToListAsync();

                var templeteDto = _mapper.Map<List<TempleteImageDto>>(templeteImage);

                return templeteDto;

            }
            catch (Exception ex)
            {
                throw new CustomException($"Unable to fetch Templete Image. Details: {ex.Message}");
            }
        }

       
    }
}
