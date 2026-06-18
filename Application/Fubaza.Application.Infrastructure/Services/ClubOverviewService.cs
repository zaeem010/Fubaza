using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

using Microsoft.Extensions.Logging;


namespace Fubaza.Application.Infrastructure.Services
{
    public class ClubOverviewService : IClubOverviewService
    {
        private readonly ILogger<ClubOverviewService> _logger;
        private readonly IClubOverviewRepository _repository;

        public ClubOverviewService(ILogger<ClubOverviewService> logger,
            IClubOverviewRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IResult<List<ClubCountBySportDto>>> GetClubCountBySportAsync()
        {
            try
            {
                var result = await _repository.GetClubCountBySportAsync();

                if (result != null)
                {
                    return await Result<List<ClubCountBySportDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport count";

                _logger.LogError(message);
                return await Result<List<ClubCountBySportDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<ClubCountBySportDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<PaginatedResponse<PaginatedClubsDto>>> GetPaginatedClubsAsync(PaginationRequest request)
        {
            try
            {
                var result = await _repository.GetPaginatedClubsAsync(request);

                if (result != null)
                {
                    return await Result<PaginatedResponse<PaginatedClubsDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport count";

                _logger.LogError(message);
                return await Result<PaginatedResponse<PaginatedClubsDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PaginatedResponse<PaginatedClubsDto>>.FailAsync(e.Message);
            }
        }



        public async Task<IResult<ClubInfoDto>> GetClubInfoAsync(Guid clubId)
        {
            try
            {
                var result = await _repository.GetClubInfoAsync(clubId);

                if (result != null)
                {
                    return await Result<ClubInfoDto>.SuccessAsync(result);
                }

                const string message = "Unable to get the Club Info";

                _logger.LogError(message);
                return await Result<ClubInfoDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<ClubInfoDto>.FailAsync(e.Message);
            }
        }


    }
}
