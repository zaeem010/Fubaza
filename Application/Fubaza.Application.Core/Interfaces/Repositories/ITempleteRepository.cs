
using Fubaza.Application.Core.Common;

using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface ITempleteRepository
    {
        Task<bool> AddOrUpdatedTempleteAsync(Templete templete);
        Task<PaginatedResponse<TempleteDto>> GetTempletesAsync(PaginationRequest request);
        Task<TempleteDto> GetTempleteAsync(Guid templeteId);
        Task<bool> DeleteTempleteAsync(Guid templeteId);
        Task<bool> BulkApproveTemplatesAsync(TempleteApprovalRequest request);
        Task<TempleteImageDto> UplaodTempleteImageAsync(TempleteImage templeteImage);
        Task<List<TempleteImageDto>> GetTempleteImageAsync(Guid userId);
        Task<bool> DeleteTempleteImageAsync(Guid TempleteImageId);
        
    }
}
