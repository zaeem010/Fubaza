using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface ITempleteService
    {
        Task<IResult<bool>> AddOrUpdatedTempleteAsync(Templete templete);
        Task<IResult<PaginatedResponse<TempleteDto>>> GetTempletesAsync(PaginationRequest request);
        Task<IResult<TempleteDto>> GetTempleteAsync(Guid templeteId);
        Task<IResult<bool>> BulkApproveTemplatesAsync(TempleteApprovalRequest request);
        Task<IResult<bool>> DeleteTempleteAsync(Guid templeteId);
        Task<IResult<TempleteImageDto>> UplaodTempleteImageAsync(TempleteImage templeteImage);
        Task<IResult<List<TempleteImageDto>>> GetTempleteImageAsync(Guid userId);
        Task<IResult<bool>> DeleteTempleteImageAsync(Guid TempleteImageId);
        
    }
}
