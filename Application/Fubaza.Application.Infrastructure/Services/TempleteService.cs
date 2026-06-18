using Azure.Core;
using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;

using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;
using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Infrastructure.Services
{
    public class TempleteService : ITempleteService
    {
        private readonly ILogger<TempleteService> _logger;
        private readonly ITempleteRepository _repository;
        private readonly IFileService _fileService;
        public TempleteService(ILogger<TempleteService> logger,
            
            IFileService fileService,
            ITempleteRepository repository
            )
        {
            _logger = logger;
            _repository = repository;
            _fileService = fileService;
        }

        public async Task<IResult<bool>> AddOrUpdatedTempleteAsync(Templete templete)
        {
            const string message = "Unable to add the Templete";
            try
            {
                if (templete.Documents != null)
                {
                    // Save documents if any.
                    foreach (var document in templete.Documents)
                    {
                        if (document.FileUrl == null)
                        {
                            document.FileUrl = await _fileService.UploadAsync(new UploadRequest()
                            {
                                FileContent = document.FileContent,
                                Extension = document.Extension,
                                FileName = document.FileName,
                                UploadType = UploadType.TempleteRequest
                            });
                        }
                    }
                }

                var response = await _repository.AddOrUpdatedTempleteAsync(templete);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<PaginatedResponse<TempleteDto>>> GetTempletesAsync(PaginationRequest request)
        {
            try
            {
                var result = await _repository.GetTempletesAsync(request);

                if (result != null)
                {
                    return await Result<PaginatedResponse<TempleteDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Templetes";

                _logger.LogError(message);
                return await Result<PaginatedResponse<TempleteDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PaginatedResponse<TempleteDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<TempleteDto>> GetTempleteAsync(Guid templeteId)
        {
            try
            {
                var result = await _repository.GetTempleteAsync(templeteId);

                if (result != null)
                {
                    return await Result<TempleteDto>.SuccessAsync(result);
                }

                const string message = "Unable to get the Templete";

                _logger.LogError(message);
                return await Result<TempleteDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<TempleteDto>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> BulkApproveTemplatesAsync(TempleteApprovalRequest request)
        {
            const string message = "Unable to updated approved  Templetes";
            try
            {

                var response = await _repository.BulkApproveTemplatesAsync(request);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> DeleteTempleteAsync(Guid templeteId)
        {
            const string message = "Unable to delete  Templete";
            try
            {

                var response = await _repository.DeleteTempleteAsync(templeteId);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<TempleteImageDto>> UplaodTempleteImageAsync(TempleteImage templeteImage)
        {
            const string message = "Unable to add the Template";

            try
            {
                if (templeteImage == null)
                {
                    _logger.LogError("Template image is null");
                    return await Result<TempleteImageDto>.FailAsync("Template image cannot be null");
                }

                // Save documents if any.
                if (templeteImage.FileUrl == null)
                {
                    templeteImage.FileUrl = await _fileService.UploadAsync(new UploadRequest()
                    {
                        FileContent = templeteImage.FileContent,
                        Extension = templeteImage.Extension,
                        FileName = templeteImage.FileName,
                        UploadType = UploadType.TempleteImageRequest
                    });
                }

                var response = await _repository.UplaodTempleteImageAsync(templeteImage);

                if (response != null)
                {
                    return await Result<TempleteImageDto>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<TempleteImageDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                 return await Result<TempleteImageDto>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<List<TempleteImageDto>>> GetTempleteImageAsync(Guid userId)
        {
            const string message = "Unable to get the Templete Image";
            try
            {
                var response = await _repository.GetTempleteImageAsync(userId);

                if (response != null)
                {
                    return await Result<List<TempleteImageDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<TempleteImageDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<TempleteImageDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> DeleteTempleteImageAsync(Guid TempleteImageId)
        {
            const string message = "Unable to Delete Templete Image";
            try
            {

                var response = await _repository.DeleteTempleteImageAsync(TempleteImageId);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
    }
}
