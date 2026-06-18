using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Fubaza.API.Controllers.Admin
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class TempleteController : ControllerBase
    {
        private readonly ILogger<TempleteController> _logger;
        private readonly ITempleteService _templeteService;
        private readonly ICurrentUser _currentUser;
        public TempleteController(
             ILogger<TempleteController> logger,
             ITempleteService templeteService,
            ICurrentUser currentUser
            )
        {
            _logger = logger;
            _templeteService = templeteService;
            _currentUser = currentUser;

        }

        [HttpPost("AddOrUpdatedTemplete")]
        public async Task<IActionResult> AddOrUpdatedTempleteAsync([FromForm] AddOrUpdateTempleteRequest request)
        {
            try
            {
                var userId = _currentUser.GetUserId();

                if (!Guid.TryParse(userId.ToString(), out Guid userid))
                {
                    return BadRequest(
                            new
                            {
                                success = false,
                                message = "Invalid or missing UserId.",
                                Error = "Invalid token."
                            });
                }

                var templete = new Templete
                {
                    UserId = userId,
                    Id = request.TempleteId ?? Guid.Empty,
                    Title = request.Title,
                    SportId = request.SportId,
                    TempleteType = request.TempleteType,
                };

                if (request.Files != null && request.DocumentTypes != null)
                {
                    for (int i = 0; i < request.Files.Count; i++)
                    {
                        var file = request.Files[i];
                        var docType = request.DocumentTypes[i];  // FIXED

                        if (file == null || file.Length == 0)
                            continue;

                        await using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);

                        templete.Documents.Add(new TempleteDocument
                        {
                            TempleteId = templete.Id,
                            DocumentType = docType,
                            FileName = file.FileName,
                            Extension = Path.GetExtension(file.FileName),
                            FileContent = ms.ToArray()
                        });
                    }
                }

                var result = await _templeteService.AddOrUpdatedTempleteAsync(templete);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = "Templete Not added successfully", Error = result.Messages });
                }

                return Ok(new { success = true, message = "Templete added successfully", Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = "An error occurred while updating the profile and role." });
        }
        
        [HttpPost("Templetes")]
        public async Task<IActionResult> GetTempletesAsync([FromBody] PaginationRequest request)
        {

            var result = await _templeteService.GetTempletesAsync(request);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Templetes fetched successfully",
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = "Failed to fetch Templetes",
                Error = result.Messages
            });
        }
        [HttpGet("GetTemplete/{templeteId}")]
        public async Task<IActionResult> GetTempleteAsync(Guid templeteId)
        {
            var result = await _templeteService.GetTempleteAsync(templeteId);

            var templete = new TempleteDto();

            if (result.Succeeded)
            {
                templete = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Templete Not fetched successfully", Data = templete, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Templete  fetched successfully", Data = templete, Error = result.Messages });
        }

        [HttpDelete("DeleteTemplate/{templeteId}")]
        public async Task<IActionResult> DeleteTemplateAsync([FromRoute] Guid templeteId)
        {
            var result = await _templeteService.DeleteTempleteAsync(templeteId);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = "Template could not be deleted.",
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = "Template deleted successfully.",
                errors = result.Messages
            });
        }

        [HttpPost("BulkApproveTemplates")]
        public async Task<IActionResult> BulkApproveTemplatesAsync([FromBody] TempleteApprovalRequest request)
        {
            var result = await _templeteService.BulkApproveTemplatesAsync(request);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = "Templates were not approved successfully.",
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = "Templates approved successfully.",
                errors = result.Messages
            });
        }

        [HttpGet("GetTempleteImage")]
        public async Task<IActionResult> GetTempleteImageAsync()
        {

            var userId = _currentUser.GetUserId();

            if (!Guid.TryParse(userId.ToString(), out Guid userid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = "Invalid or missing UserId.",
                            Error = "Invalid token."
                        });
            }

            var result = await _templeteService.GetTempleteImageAsync(userid);

            var templeteImage = new List<TempleteImageDto>();

            if (result.Succeeded)
            {
                templeteImage = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Templete Image Not fetched successfully", Data = templeteImage, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Templete Image  fetched successfully", Data = templeteImage, Error = result.Messages });
        }

        [HttpPatch("TempleteImage/{id}/delete")]
        public async Task<IActionResult> DeleteTempleteImageAsync(Guid id)
        {
            try
            {
                var userId = _currentUser.GetUserId();

                if (!Guid.TryParse(userId.ToString(), out Guid userid))
                {
                    return BadRequest(
                            new
                            {
                                success = false,
                                message = "Invalid or missing UserId.",
                                Error = "Invalid token."
                            });
                }

                var status = await _templeteService.DeleteTempleteImageAsync(id);

                if (!status.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete the template image.",
                        errors = status.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Template image deleted successfully.",
                    errors = (string[]?)null
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deleting the template image.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    error = e.Message
                });
            }
        }

        [HttpPost("UplaodTempleteImage")]
        public async Task<IActionResult> UplaodTempleteImageAsync(UplaodTempleteImageRequest request)
        {
            try
            {
                var userId = _currentUser.GetUserId();

                if (!Guid.TryParse(userId.ToString(), out Guid userid))
                {
                    return BadRequest(
                            new
                            {
                                success = false,
                                message = "Invalid or missing UserId.",
                                Error = "Invalid token."
                            });
                }

                if (request.file == null || request.file.Length == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No file uploaded.",
                        Error = "File cannot be null or empty."
                    });
                }

                var ms = new MemoryStream();
                await request.file.CopyToAsync(ms);

                var templeteImage = new TempleteImage
                {
                    UserId = userId,
                    FileName = request.file.FileName,
                    Extension = Path.GetExtension(request.file.FileName),
                    FileContent = ms.ToArray(),
                    IsUserUpload = request.IsUserUpload
                };

                var result = await _templeteService.UplaodTempleteImageAsync(templeteImage);

                var templeteImageresponse = new TempleteImageDto();

                if (result.Succeeded)
                {
                    templeteImageresponse = result.Data;
                }

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = "Templete Image Not fetched successfully", Data = templeteImageresponse, Error = result.Messages });
                }

                return Ok(new { success = true, message = "Templete Image  fetched successfully", Data = templeteImageresponse, Error = result.Messages });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while uploading the template image.",
                    Error = e.Message
                });
            }
        }

    }
}
