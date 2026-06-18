using Fubaza.API.Extensions;
using Fubaza.API.Resources;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Fubaza.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IIdentityService _identityService;
        private readonly ICurrentUser _currentUser;
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public AuthController(
            ILogger<AuthController> logger,
            IIdentityService IdentityService,
            ICurrentUser currentUser,
            HttpClient httpClient,
            IStringLocalizer<SharedResource> localizer
           )
        {
            _logger = logger;
            _identityService = IdentityService;
            _currentUser = currentUser;
            _httpClient = httpClient;
            _localizer = localizer;
        }

        [AllowAnonymous]
        [HttpPost("SocialLogin")]
        public async Task<IActionResult> SocialLogin([FromBody] ExternalAuthRequest request)
        {
            try
            {
                var result = await _identityService.SocialLoginAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.SocialLogin"].Value, Data = result.Data, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.SocialLogin"].Value, Data = result.Data, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });

        }

        [AllowAnonymous]
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInRequest request)
        {
            var result = await _identityService.SignInAsync(request);


            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.SignInAsync"].Value, Data = result.Data,  Errors = result.Messages.GetCulturedList<SharedResource>(_localizer) });
            }

            return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.SignInAsync"].Value, Data = result.Data,  Errors = result.Messages.GetCulturedList<SharedResource>(_localizer) });
        }

        [AllowAnonymous]
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpRequest request)
        {
            try
            {
                var result = await _identityService.SignUpAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.SignUpAsync"].Value, Data = result.Data, Error = result.Messages.GetCulturedList<SharedResource>(_localizer) });

                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.SignUpAsync"].Value, Data = result.Data, Errors = result.Messages.GetCulturedList<SharedResource>(_localizer) });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }

        [AllowAnonymous]
        [HttpPost("ResendOTP")]
        public async Task<IActionResult> ResendOTPAsync([FromBody] ResendOTPRequest request)
        {
            try
            {
                var result = await _identityService.ResendOTPAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.ResendOTPAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.ResendOTPAsync"].Value, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }

        [AllowAnonymous]
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTPAsync([FromBody] VerifyOTPRequest request)
        {
            try
            {
                var result = await _identityService.VerifyOTPAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.VerifyOTPAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.VerifyOTPAsync"].Value, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            try
            {

                var result = await _identityService.ResetPasswordAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.ResetPasswordAsync"].Value, Data = result.Data, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.ResetPasswordAsync"].Value, Data = result.Data, Error = result.Messages });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }
        
        [HttpPost("Verify-Email")]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
        {
            try
            {
                var email = _currentUser.GetUserEmail();

                if (email != null)
                    request.Email = email;

                var result = await _identityService.VerifyEmailAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.VerifyEmailAsync"].Value, Data = result.Data, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.VerifyEmailAsync"].Value, Data = result.Data, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
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
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
                }

                var result = await _identityService.ChangePasswordAsync(request, userid);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.ChangePasswordAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.ChangePasswordAsync"].Value, Error = result.Messages });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }
            return Ok(new { success = false, Error = "" });
        }

        [HttpPost("GetUserPages")]
        public async Task<IActionResult> GetUserPages([FromForm] FacebookPageRequest request)
        {
            var userId = _currentUser.GetUserId();

            if (!Guid.TryParse(userId.ToString(), out Guid userid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
            }

            request.UserId = userId;

            var result = await _identityService.GetUserPagesAsync(request);

            var sports = new FacebookPagesResponseDTO();

            if (result.Succeeded)
            {
                sports = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.GetUserPages"].Value, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.GetUserPages"].Value,Data= sports, Error = result.Messages });
        }

        [HttpPost("GetInstagramCode")]
        public async Task<IActionResult> GetInstagramCode([FromForm] FacebookPageRequest request)
        {
            var userId = _currentUser.GetUserId();

            if (!Guid.TryParse(userId.ToString(), out Guid userid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
            }

            request.UserId = userId;

            var result = await _identityService.GetInstagramCode(request);

            var response = new InstagramResponseDTO();

            if (result.Succeeded)
            {
                response = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.GetUserPages"].Value, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.GetUserPages"].Value, Data = response, Error = result.Messages });
        }

        [HttpPost("RemoveUserPages")]
        public async Task<IActionResult> RemoveUserPages([FromForm] RemoveUserPageRequest request)
        {
            var userId = _currentUser.GetUserId();

            request.UserId = userId;

            var result = await _identityService.RemoveUserPagesAsync(request);

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Auth.Message.Failed.RemoveUserPages"].Value, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.RemoveUserPages"].Value, Data = result.Data, Error = "" });
        }
        [HttpPut("UpdateLanguage")]
        public async Task<IActionResult> UpdateLanguageAsync()
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
                       message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                       Error = _localizer["Common.Error.InvalidToken"].Value
                    });
                }

                var result = await _identityService.UpdateLanguageAsync(userid);

                if (result.Succeeded)
                    return Ok(new { success = true, message = _localizer["Controller.Auth.Message.Succeeded.UpdateLanguageAsync"].Value , errors = result.Messages });

                return BadRequest(new { success = false, message = _localizer["Controller.Auth.Failed.Succeeded.UpdateLanguageAsync"].Value, errors = result.Messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
            }
        }

        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteUserAsync()
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
                        message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                        Error = _localizer["Common.Error.InvalidToken"].Value
                    });
                }

                var result = await _identityService.DeleteUserAsync(userid);


                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizer["Controller.Auth.Failed.Succeeded.DeleteUserAsync"].Value,
                        errors = result.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Auth.Message.Succeeded.DeleteUserAsync"].Value,
                    errors = result.Messages
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
            }
        }


    }
}
