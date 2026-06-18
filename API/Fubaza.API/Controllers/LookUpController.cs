using Fubaza.API.Resources;

using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

using System.Diagnostics;


namespace Fubaza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookUpController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ILookUpService _lookUpService;
        private readonly IRoleService _roleService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public LookUpController(
             ILogger<AuthController> logger,
             ILookUpService lookUpService,
                IRoleService roleService,
                IStringLocalizer<SharedResource> localizer
            )
        {
            _logger = logger;
            _lookUpService = lookUpService;
            _roleService = roleService;
            _localizer = localizer;
        }

        [HttpGet("Sports")]
        public async Task<IActionResult> GetSportsAsync()
        {

            var result = await _lookUpService.GetSportsAsync();

            var sports = new List<SportDto>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetSportsAsync"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetSportsAsync"].Value, Data = sports, Error = result.Messages });
        }

        [HttpGet("PlayingPositions/{sportId}")]
        public async Task<IActionResult> GetPlayingPositionsAsync(Guid sportId)
        {
            var result = await _lookUpService.GetPlayingPositionsAsync(sportId);

            var sports = new List<PlayingPositionDto>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetPlayingPositionsAsync"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetPlayingPositionsAsync"].Value, Data = sports, Error = result.Messages });
        }

        [HttpGet("designations")]
        public async Task<IActionResult> GetDesignationsAsync()
        {

            var result = await _lookUpService.GetDesignationsAsync();

            var designations = new List<DesignationDto>();

            if (result.Succeeded)
            {
                designations = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetDesignationsAsync"].Value, Data = designations, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetDesignationsAsync"].Value , Data = designations, Error = result.Messages });
        }

        [HttpGet("StrongFoot")]
        public async Task<IActionResult> GetStrongFootOptionsAsync()
        {
            var result = await _lookUpService.GetStrongFootOptionsAsync();

            var sports = new List<StrongFootDTO>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetStrongFootOptionsAsync"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetStrongFootOptionsAsync"].Value, Data = sports, Error = result.Messages });
        }

        [HttpGet("ThrowingHand")]
        public async Task<IActionResult> GetThrowingHandOptionsAsync()
        {
            var result = await _lookUpService.GetThrowingHandOptionsAsync();

            var sports = new List<ThrowingHandDTO>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetThrowingHandOptionsAsync"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetThrowingHandOptionsAsync"].Value, Data = sports, Error = result.Messages });
        }

        [HttpGet("GetCompetitionTypes")]
        public async Task<IActionResult> GetCompetitionTypesAsync()
        {
            var result = await _lookUpService.GetCompetitionTypesAsync();

            var competitionTypes = new List<CompetitionTypeDto>();

            if (result.Succeeded)
            {
                competitionTypes = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetCompetitionTypesAsync"].Value, Data = competitionTypes, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetCompetitionTypesAsync"].Value, Data = competitionTypes, Error = result.Messages });
        }
        [HttpGet("GetTempleteType")]
        public async Task<IActionResult> GetTempleteTypeAsync()
        {
            var result = await _lookUpService.GetTempleteTypeAsync();

            var sports = new List<TempleteTypeDTO>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetTempleteTypeAsync"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetTempleteTypeAsync"].Value, Data = sports, Error = result.Messages });
        }

        [HttpGet("Events/{sportId}")]
        public async Task<IActionResult> GetEventTypeAsync(Guid sportId)
        {
            var result = await _lookUpService.GetEventTypeAsync(sportId);

            var events = new List<EventTypeDTO>();

            if (result.Succeeded)
            {
                events = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetEventTypeAsync"].Value, Data = events, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetEventTypeAsync"].Value, Data = events, Error = result.Messages });
        }

        [HttpPost("GetTempletes")]
        public async Task<IActionResult> GetTempletesAsync([FromForm] TempleteRequest request)
        {
            var result = await _lookUpService.GetTempletesAsync(request);

            var templetes = new List<TempleteDto>();

            if (result.Succeeded)
            {
                templetes = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetTempletesAsync"].Value, Data = templetes, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetTempletesAsync"].Value, Data = templetes, Error = result.Messages });
        }

        [HttpGet("Roles")]
        public async Task<IActionResult> GetRoleAsync()
        {
            var result = await _roleService.GetRoleAsync();

            var roles = new List<RoleDto>();

            if (result.Succeeded)
            {
                roles = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.GetRoleAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.GetRoleAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpPost("WritePostCaption")]
        public async Task<IActionResult> WritePostCaption(WritePostCaptionRequest request)
        {
            var result = await _lookUpService.WritePostCaptionAsync(request);

            var sports = new WritePostCaptionDTO();

            if (result.Succeeded)
            {
                sports = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.WritePostCaption"].Value, Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.WritePostCaption"].Value, Data = sports, Error = result.Messages });
        }

        [HttpPost("AIImangeEnhancement")]
        public async Task<IActionResult> AIImangeEnhancementAsync([FromForm] TempleteGenerationRequest request)
        {
            var result = await _lookUpService.AIImangeEnhancementAsync(request);

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.LookUp.Message.Failed.AIImangeEnhancementAsync"].Value, Data = result, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.LookUp.Message.Succeeded.AIImangeEnhancementAsync"].Value, Data = result.Data, Error = result.Messages });

        }

        [HttpPost("removebackgroundImage")]
        public async Task<IActionResult> removebackgroundImage(IFormFile image)
        {
            _logger.LogInformation("removebackgroundImage called.");

            if (image == null || image.Length == 0)
            {
                _logger.LogWarning("No image uploaded.");
                return Ok(new
                {
                    success = false,
                    message = "No image uploaded.",
                    Data = (byte[])null,
                    Error = "Missing file input."
                });
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".webp", ".gif" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Unsupported image format: {Extension}", extension);
                return Ok(new
                {
                    success = false,
                    message = "Unsupported image format.",
                    Data = (byte[])null,
                    Error = "Allowed formats: .png, .jpg, .jpeg, .bmp, .tiff, .webp, .gif"
                });
            }

            string tempInputPath = null;
            string tempOutputPath = null;

            try
            {
                _logger.LogInformation("Processing image: {FileName}", image.FileName);

                var originalName = Path.GetFileNameWithoutExtension(image.FileName);
                var downloadFileName = originalName + ".png";

                tempInputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
                tempOutputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

                await using (var stream = System.IO.File.Create(tempInputPath))
                {
                    await image.CopyToAsync(stream);
                }

                var shellScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "remove_bg.sh");

                if (!System.IO.File.Exists(shellScriptPath))
                {
                    _logger.LogError("Shell script not found at {ScriptPath}", shellScriptPath);
                    return Ok(new
                    {
                        success = false,
                        message = "Shell script not found.",
                        Data = (byte[])null,
                        Error = "Missing remove_bg.sh"
                    });
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"\"{shellScriptPath}\" \"{tempInputPath}\" \"{tempOutputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0 || !System.IO.File.Exists(tempOutputPath))
                {
                    _logger.LogError("Background removal failed. ExitCode: {ExitCode}, Error: {Error}", process.ExitCode, stderr);
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.LookUp.Message.Failed.removebackgroundImage"].Value,
                        Data = (byte[])null,
                        Error = stderr
                    });
                }

                using var imageResult = await Image.LoadAsync(tempOutputPath);

                // Optional resize (comment out if not needed)
                int maxWidth = 800;
                if (imageResult.Width > maxWidth)
                {
                    imageResult.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxWidth, 0)
                    }));
                }

                await using var ms = new MemoryStream();

                // Choose JPEG or PNG compression
                imageResult.Save(ms, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.Level6
                });

                var compressedBytes = ms.ToArray();
                var base64Image = Convert.ToBase64String(compressedBytes);

                _logger.LogInformation("Background removed successfully.");
                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.LookUp.Message.Succeeded.removebackgroundImage"].Value,
                    Data = base64Image,
                    Error = (string)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during background removal.");
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.LookUp.InternalMessage.Failed.removebackgroundImage"].Value,
                    Data = (byte[])null,
                    Error = ex.Message
                });
            }
            finally
            {
                try
                {
                    if (tempInputPath != null && System.IO.File.Exists(tempInputPath))
                    {
                        System.IO.File.Delete(tempInputPath);
                        _logger.LogInformation("Deleted temp input file: {Path}", tempInputPath);
                    }

                    if (tempOutputPath != null && System.IO.File.Exists(tempOutputPath))
                    {
                        System.IO.File.Delete(tempOutputPath);
                        _logger.LogInformation("Deleted temp output file: {Path}", tempOutputPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during temp file cleanup.");
                }
            }
        }

    }
}
