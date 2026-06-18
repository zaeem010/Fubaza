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
    public class PlayerOverviewService : IPlayerOverviewService
    {
        private readonly ILogger<PlayerOverviewService> _logger;
        private readonly IPlayerOverviewRepository _repository;

        public PlayerOverviewService(ILogger<PlayerOverviewService> logger,
            IPlayerOverviewRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        public async Task<IResult<List<PlayerCountBySportDto>>> GetPlayerCountBySportAsync()
        {
            try
            {
                var result = await _repository.GetPlayerCountBySportAsync();

                if (result != null)
                {
                    return await Result<List<PlayerCountBySportDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport count";

                _logger.LogError(message);
                return await Result<List<PlayerCountBySportDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<PlayerCountBySportDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<PaginatedResponse<PaginatedPlayersDto>>> GetPaginatedPlayersAsync(PaginationRequest request)
        {
            try
            {
                var result = await _repository.GetPaginatedPlayersAsync(request);

                if (result != null)
                {
                    return await Result<PaginatedResponse<PaginatedPlayersDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport count";

                _logger.LogError(message);
                return await Result<PaginatedResponse<PaginatedPlayersDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PaginatedResponse<PaginatedPlayersDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<PlayerInfoDto>> GetPlayerInfoAsync(Guid playerId)
        {
            try
            {
                var result = await _repository.GetPlayerInfoAsync(playerId);

                if (result != null)
                {
                    return await Result<PlayerInfoDto>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player Info";

                _logger.LogError(message);
                return await Result<PlayerInfoDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PlayerInfoDto>.FailAsync(e.Message);
            }
        }
    }
}
