using AutoMapper;
using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Persistence;
using Google.Apis.Auth;
using LinqKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Fubaza.Application.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMailService _mailService;
        private readonly ILogger<IdentityService> _logger;
        private readonly IMapper _mapper;
        private readonly IJobService _jobService;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _db;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly FacebookAuthSettings _facebookAuthSettings;
        private readonly InstagramAuthSettings _instagramAuthSettings;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly HttpClient _httpClient;
        private readonly ICurrentUser _currentUser;
        public IdentityService(
             SignInManager<User> signInManager,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<IdentityService> logger,
            IMailService mailService,
            IMapper mapper,
            IFileService fileService,
             IJobService jobService,
             ApplicationDbContext db,
             IOptions<GoogleAuthSettings> googleAuthSettings,
             IOptions<FacebookAuthSettings> facebookAuthSettings,
             IOptions<InstagramAuthSettings> instagramAuthSettings,
             IJwtTokenService jwtTokenService,
             HttpClient httpClient,
            ICurrentUser  currentUser
           )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
            _logger = logger;
            _mapper = mapper;
            _fileService = fileService;
            _jobService = jobService;
            _jwtTokenService = jwtTokenService;
            _db = db;
            _googleAuthSettings = googleAuthSettings.Value;
            _facebookAuthSettings = facebookAuthSettings.Value;
            _instagramAuthSettings = instagramAuthSettings.Value;
            _httpClient = httpClient;
            _currentUser = currentUser;
        }

        public async Task<IResult<UserDto>> SignInAsync(SignInRequest request)
        {
            try
            {
                var user = await _db.Users
                    .Include(x=>x.Player)
                    .ThenInclude(x=>x.Sport)
                    .Include(x => x.Club)
                    .ThenInclude(x => x.Sport)
                    .Where(x=>x.Email == request.Email && x.IsActive && x.IsAdminPanel == request.IsAdminPanel).FirstOrDefaultAsync();

                if (user is null)
                    return await Result<UserDto>.FailAsync("IdentityService.SignInAsync.EmailNotRegistered");

                if (!user.IsActive || !user.CanLogin)
                    return await Result<UserDto>.FailAsync("IdentityService.SignInAsync.AccountDeleted");
                

                if (!await _userManager.CheckPasswordAsync(user, request.Password))
                    return await Result<UserDto>.FailAsync("IdentityService.SignInAsync.InvalidPassword");

                if (!user.EmailConfirmed)
                {
                    var verificationCode = GenerateVerificationCode();
                    user.EmailVerificationCode = verificationCode;
                    user.CodeGeneratedAt = DateTime.UtcNow;

                    var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                    user.RefreshToken = jwtToken;

                    await _userManager.UpdateAsync(user);

                    await SendVerificationEmail(user);

                    var UserDto = _mapper.Map<UserDto>(user);

                    return await Result<UserDto>.SuccessAsync(UserDto, "IdentityService.SignInAsync.EmailNotVerified");
                }


                if (!await _signInManager.CanSignInAsync(user))
                    return await Result<UserDto>.FailAsync("IdentityService.SignInAsync.CannotSignIn");


                var result = await _signInManager.PasswordSignInAsync(user, request.Password, true, lockoutOnFailure: true);
                

                if (result.Succeeded)
                {
                    var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                    user.RefreshToken = jwtToken;

                    user.FcmToken = request.FcmToken;

                    var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

                    // ✅ Update LanguageCode if it’s empty or different
                    if (string.IsNullOrWhiteSpace(user.LanguageCode) || user.LanguageCode != currentLang)
                    {
                        user.LanguageCode = currentLang;
                    }

                    await _userManager.UpdateAsync(user);

                    var UserDto = _mapper.Map<UserDto>(user);

                    return await Result<UserDto>.SuccessAsync(UserDto);
                }
                else
                {
                    string LoginFailedmessage = "IdentityService.SignInAsync.InvalidEmailOrPassword";

                    return await Result<UserDto>.FailAsync(LoginFailedmessage);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetMessage());
                return await Result<UserDto>.FailAsync();
            }
        }
        public async Task<IResult<UserDto>> SignUpAsync(SignUpRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return await Result<UserDto>.FailAsync("IdentityService.SignUpAsync.EmailEmpty");

                // ✅ Only block ACTIVE users
                var activeUser = await _db.Users
                    .FirstOrDefaultAsync(x =>
                        x.Email == request.Email &&
                        x.IsActive && !x.IsAdminPanel);

                if (activeUser != null)
                    return await Result<UserDto>.FailAsync(
                        "IdentityService.SignUpAsync.ActiveAccountExists");

                var user = new User
                {
                    Email = request.Email,
                    UserName = $"{request.Email}_{Guid.NewGuid():N}",
                    IsActive = true,
                    CanLogin = true,
                    Created = DateTime.UtcNow,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    IsPasswordRequired = true,
                };

                var password = request.Password ?? UserConstants.DefaultPassword;

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    return await Result<UserDto>.FailAsync(
                        result.Errors.Select(e => e.Description).ToList());

                // 🔐 Email verification setup
                user.EmailVerificationCode = GenerateVerificationCode();
                user.CodeGeneratedAt = DateTime.UtcNow;

                user.FcmToken = request.FcmToken;

                var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

                // ✅ Update LanguageCode if it’s empty or different
                if (string.IsNullOrWhiteSpace(user.LanguageCode) || user.LanguageCode != currentLang)
                {
                    user.LanguageCode = currentLang;
                }

                // 🔑 Generate SIGNUP token (limited usage)
                var signupToken = await _jwtTokenService.GenerateTokenAsync(user);
                user.RefreshToken = signupToken;

                await _userManager.UpdateAsync(user);

                await SendVerificationEmail(user);

                var userDto = _mapper.Map<UserDto>(user);

                return await Result<UserDto>.SuccessAsync(
                    userDto,
                    "IdentityService.SignUpAsync.RegistrationSuccessful"

                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<UserDto>.FailAsync("IdentityService.SignUpAsync.RegistrationFailed");
            }
        }

        public async Task<IResult<UserDto>> SocialLoginAsync(ExternalAuthRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Provider))
                    return await Result<UserDto>.FailAsync("Authentication provider is required.");

                var payload = request.Provider.ToLowerInvariant() switch
                {
                    "google" => await VerifyGoogleTokenAsync(request),
                    "facebook" => await VerifyFacebookTokenAsync(request),
                    "apple" => await VerifyAppleTokenAsync(request),
                    _ => null
                };

                if (payload == null || string.IsNullOrWhiteSpace(payload.Subject))
                    return await Result<UserDto>.FailAsync("Invalid authentication payload.");

                User? user = null;

                // 1️⃣ Find user by external login (Identity)
                var identityUser = await _userManager.FindByLoginAsync(
                    request.Provider, payload.Subject);

                // 2️⃣ Reload with Includes if found
                if (identityUser != null)
                {
                    user = await _db.Users
                        .Include(x => x.Player)
                            .ThenInclude(x => x.Sport)
                        .Include(x => x.Club)
                            .ThenInclude(x => x.Sport)
                        .FirstOrDefaultAsync(x => x.Id == identityUser.Id && !x.IsAdminPanel);
                }

                // 🚫 Ignore inactive / blocked users
                if (user != null && (!user.IsActive || !user.CanLogin))
                    user = null;

                // 3️⃣ If not linked, find ACTIVE user by email
                if (user == null && !string.IsNullOrWhiteSpace(payload.Email))
                {
                    user = await _db.Users
                        .Include(x => x.Player)
                            .ThenInclude(x => x.Sport)
                        .Include(x => x.Club)
                            .ThenInclude(x => x.Sport)
                        .FirstOrDefaultAsync(x =>
                            x.Email == payload.Email &&
                            x.IsActive && !x.IsAdminPanel);
                }

                // 4️⃣ Existing user logic
                if (user != null)
                {
                    if (!user.IsActive || !user.CanLogin)
                        return await Result<UserDto>.FailAsync("This account has been deactivated.");

                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }

                    var logins = await _userManager.GetLoginsAsync(user);
                    if (!logins.Any(l =>
                        l.LoginProvider == request.Provider &&
                        l.ProviderKey == payload.Subject))
                    {
                        var linkResult = await _userManager.AddLoginAsync(
                            user,
                            new UserLoginInfo(
                                request.Provider,
                                payload.Subject,
                                request.Provider));

                        if (!linkResult.Succeeded)
                            return await Result<UserDto>.FailAsync("Failed to link social account.");
                    }
                }
                else
                {
                    // 5️⃣ Create new user
                    user = new User
                    {
                        FullName = request.FullName?? null,
                        Email = payload.Email,
                        UserName = $"{payload.Email}_{Guid.NewGuid():N}",
                        EmailConfirmed = true,
                        IsActive = true,
                        CanLogin = true,
                        Created = DateTime.UtcNow,
                        IsAdminPanel = false,
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return await Result<UserDto>.FailAsync("Failed to create user.");

                    var loginResult = await _userManager.AddLoginAsync(
                        user,
                        new UserLoginInfo(
                            request.Provider,
                            payload.Subject,
                            request.Provider));

                    if (!loginResult.Succeeded)
                        return await Result<UserDto>.FailAsync(
                            "Failed to link social account.");

                    // 🔁 Reload new user with Includes
                    user = await _db.Users
                        .Include(x => x.Player)
                            .ThenInclude(x => x.Sport)
                        .Include(x => x.Club)
                            .ThenInclude(x => x.Sport)
                        .FirstAsync(x => x.Id == user.Id);
                }

                // 6️⃣ Generate token & finalize login
                user.RefreshToken = await _jwtTokenService.GenerateTokenAsync(user);
                user.FcmToken = request.FcmToken;

                var currentLang = CultureInfo.CurrentUICulture
                    .TwoLetterISOLanguageName.ToLower();

                if (string.IsNullOrWhiteSpace(user.LanguageCode))
                    user.LanguageCode = currentLang;

                await _userManager.UpdateAsync(user);

                var userDto = _mapper.Map<UserDto>(user);
                return await Result<UserDto>.SuccessAsync(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<UserDto>.FailAsync("Social login failed.");
            }
        }

        public async Task<IResult<UserDto>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                var user = await _db.Users
                    .Include(x => x.Player)
                    .ThenInclude(x => x.Sport)
                    .Include(x => x.Club)
                    .ThenInclude(x => x.Sport)
                    .Where(x => x.Email == request.Email && x.IsActive).FirstOrDefaultAsync();

                if (user is null)
                    return await Result<UserDto>.FailAsync("User not found");

                if (user.EmailVerificationCode != request.Code)
                    return await Result<UserDto>.FailAsync("Invalid verification code");

                if (user.CodeGeneratedAt.HasValue &&
                    DateTime.UtcNow > user.CodeGeneratedAt.Value.AddMinutes(30))
                {
                    return await Result<UserDto>.FailAsync("Verification code has expired");
                }
                
                user.EmailConfirmed = true;
                user.EmailVerificationCode = null;
                user.CodeGeneratedAt = null;

                
                var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                user.RefreshToken = jwtToken;

                await _userManager.UpdateAsync(user);

                var UserDto = _mapper.Map<UserDto>(user);

                return await Result<UserDto>.SuccessAsync(UserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetMessage());
                return await Result<UserDto>.FailAsync();
            }

        }

        public async Task<IResult> ResendOTPAsync(ResendOTPRequest request)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Email == request.Email && x.IsActive).FirstOrDefaultAsync();
                if (user is null)
                    return await Result.FailAsync("Your  Email Is Not  Register");

                if (!user.IsActive)
                    return await Result.FailAsync("This account has been deleted. To recover your account");

                var verificationCode = GenerateVerificationCode();
                user.EmailVerificationCode = verificationCode;
                user.CodeGeneratedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                // Send verification email
                await SendVerificationEmail(user);

                return await Result.SuccessAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetMessage());
                return await Result.FailAsync();
            }
        }

        public async Task<IResult> VerifyOTPAsync(VerifyOTPRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                    return await Result.FailAsync("User not found");

                if (user.EmailVerificationCode != request.Code)
                    return await Result.FailAsync("Invalid verification code");
                
                if (user.CodeGeneratedAt.HasValue &&
                    DateTime.UtcNow > user.CodeGeneratedAt.Value.AddMinutes(30))
                {
                    return await Result.FailAsync("Verification code has expired");
                }

                user.EmailVerificationCode = null;
                user.CodeGeneratedAt = null;

                await _userManager.UpdateAsync(user);

                return await Result.SuccessAsync("OTP has been Verified");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetMessage());
                return await Result<UserDto>.FailAsync();
            }
        }

        public async Task<IResult<UserDto>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var user = await _db.Users
                    .Include(x => x.Player)
                    .Include(x => x.Club)
                    .Where(x => x.Email == request.Email && x.IsActive).FirstOrDefaultAsync();

                if (user is null)
                    return await Result<UserDto>.FailAsync("Your  Email Is Not  Register");

                if (!user.IsActive)
                    return await Result<UserDto>.FailAsync("This account has been deleted. To recover your account");

                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    var errorMessage = string.Join("; ", removePasswordResult.Errors.Select(x => x.Description));
                    return await Result<UserDto>.FailAsync(errorMessage);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (addPasswordResult.Succeeded)
                {

                    user.IsPasswordRequired = true;

                    user.EmailConfirmed = true;

                    var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                    user.RefreshToken = jwtToken;

                    await _userManager.UpdateAsync(user);

                    var userDTO = _mapper.Map<UserDto>(user);
                    return await Result<UserDto>.SuccessAsync(userDTO);
                }

                var addPasswordErrorMessage = string.Join("; ", addPasswordResult.Errors.Select(e => e.Description));
                return await Result<UserDto>.FailAsync(addPasswordErrorMessage);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetMessage());
                return await Result<UserDto>.FailAsync();
            }
        }

        public async Task<IResult<UserDto>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId)
        {
            const string errorMessage = "Unable to update password";

            if (request == null)
                return await Result<UserDto>.FailAsync(new List<string> { "Request cannot be null" });

            if (string.IsNullOrEmpty(request.NewPassword))
                return await Result<UserDto>.FailAsync(new List<string> { "New password must be provided" });

            try
            {
                var user = await _db.Users.Where(x => x.Id == userId && x.IsActive).FirstOrDefaultAsync();
                if (user is null)
                {
                    return await Result<UserDto>.FailAsync(new List<string> { "User not found" });
                }

                var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

                // ✅ Update LanguageCode if it’s empty or different
                if (string.IsNullOrWhiteSpace(user.LanguageCode) || user.LanguageCode != currentLang)
                {
                    user.LanguageCode = currentLang;
                }

                // Check if password is required
                if (user.IsPasswordRequired)
                {
                    if (string.IsNullOrEmpty(request.OldPassword))
                    {
                        return await Result<UserDto>.FailAsync(new List<string> { "Old password must be provided" });
                    }

                    var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                    if (result.Succeeded)
                    {
                        var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                        user.RefreshToken = jwtToken;

                        await _userManager.UpdateAsync(user);

                        var userDTO = _mapper.Map<UserDto>(user);

                        return await Result<UserDto>.SuccessAsync(userDTO);

                    }

                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return await Result<UserDto>.FailAsync(errors);
                }
                else
                {
                    // For social login users or first-time password setup
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                    if (result.Succeeded)
                    {
                        user.IsPasswordRequired = true;

                        var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                        user.RefreshToken = jwtToken;

                        await _userManager.UpdateAsync(user);

                        var userDTO = _mapper.Map<UserDto>(user);

                        return await Result<UserDto>.SuccessAsync(userDTO);
                    }


                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return await Result<UserDto>.FailAsync(errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<UserDto>.FailAsync(errorMessage);
            }
        }

        public async Task<Result<UserDto>> RemoveUserPagesAsync(RemoveUserPageRequest request)
        {
            try
            {
                if (!request.RemoveFacebook && !request.RemoveInstagram)
                    return await Result<UserDto>.FailAsync("No pages specified for removal.");
                if (request.RemoveFacebook)
                {
                    await _db.Users
                        .Where(x => x.Id == request.UserId)
                        .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.IsConnectedFacebook, false)
                            .SetProperty(x => x.FacebookLongLivedToken, (string?)null)
                            .SetProperty(x => x.FacebookTokenExpiresAt, (DateTime?)null));
                }

                if (request.RemoveInstagram)
                {
                    await _db.Users
                        .Where(x => x.Id == request.UserId)
                        .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.IsConnectedInstagram, false)
                            .SetProperty(x => x.InstagramLongLivedToken, (string?)null)
                            .SetProperty(x => x.InstagramTokenExpiresAt, (DateTime?)null)
                            .SetProperty(x => x.InstagramBusinessId, (string?)null));
                }

                var user = await _db.Users
                    .Include(x => x.Player)
                    .ThenInclude(x => x.Sport)
                    .Include(x => x.Club)
                    .ThenInclude(x => x.Sport)
                    .Where(x => x.Id == request.UserId).FirstOrDefaultAsync();
                if (user == null)
                    return await Result<UserDto>.FailAsync("User not found.");

                user.CodeGeneratedAt = DateTime.UtcNow;

                var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);

                user.RefreshToken = jwtToken;
                await _userManager.UpdateAsync(user);
                var UserDto = _mapper.Map<UserDto>(user);

                return await Result<UserDto>.SuccessAsync(UserDto,"Page disconnected successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting pages for user {UserId}", request.UserId);
                return await Result<UserDto>.FailAsync("An unexpected error occurred while disconnecting pages.");
            }
        }

        public async Task<Result<FacebookPagesResponseDTO>> GetUserPagesAsync(FacebookPageRequest request)
        {
            try
            {
                var user = await _db.Users.FindAsync(request.UserId);
                if (user == null)
                    return await Result<FacebookPagesResponseDTO>.FailAsync("User not found.");

                string tokenToUse;
                DateTime? tokenExpiry = user.FacebookTokenExpiresAt;

                // ✅ Use saved long-lived token if still valid
                if (user.IsConnectedFacebook
                    && !string.IsNullOrEmpty(user.FacebookLongLivedToken)
                    && tokenExpiry.HasValue
                    && tokenExpiry.Value > DateTime.UtcNow)
                {
                    tokenToUse = user.FacebookLongLivedToken;
                }
                else
                {
                    // Token expired or not connected → use client-provided short-lived token
                    if (string.IsNullOrEmpty(request.AccessToken))
                        return await Result<FacebookPagesResponseDTO>.FailAsync("Access token is required.");

                    // Exchange short-lived → long-lived
                    var (longLivedToken, expiration) = await ExchangeForLongLivedTokenAsync(request.AccessToken);
                    if (string.IsNullOrEmpty(longLivedToken))
                        return await Result<FacebookPagesResponseDTO>.FailAsync("Failed to exchange token.");

                    // Save new token + expiry
                    user.IsConnectedFacebook = true;
                    user.FacebookLongLivedToken = longLivedToken;
                    user.FacebookTokenExpiresAt = expiration;
                    await _db.SaveChangesAsync();

                    tokenToUse = longLivedToken;
                    tokenExpiry = expiration;
                }

                // ✅ Fetch pages with valid token (incl. picture)
                var url = $"https://graph.facebook.com/v19.0/me/accounts?fields=id,name,access_token,picture{{url}}&access_token={tokenToUse}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch Facebook pages for user {UserId}. Status: {StatusCode}. Error: {Error}",
                        request.UserId, response.StatusCode, error);

                    return await Result<FacebookPagesResponseDTO>.FailAsync("Failed to fetch Facebook pages.");
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(json);

                var pages = new List<FacebookPageDTO>();

                // ✅ Loop through all pages and fetch Facebook + Instagram profile data
                foreach (var page in result["data"] ?? new JArray())
                {
                    var pageId = page["id"]?.ToString();
                    var pageAccessToken = page["access_token"]?.ToString();
                    var pagePictureUrl = page["picture"]?["data"]?["url"]?.ToString();
                    string? instagramBusinessId = null;
                    InstagramProfileDTO? instagramProfile = null;
                    FacebookPageProfileDTO? facebookProfile = null;

                    if (!string.IsNullOrEmpty(pageId) && !string.IsNullOrEmpty(pageAccessToken))
                    {
                        // ✅ Fetch full Facebook page profile
                        try
                        {
                            var fbProfileUrl = $"https://graph.facebook.com/v19.0/{pageId}" +
                                               "?fields=id,name,username,about,category,link,website,picture{url},fan_count,followers_count" +
                                               $"&access_token={pageAccessToken}";
                            var fbProfileResponse = await _httpClient.GetAsync(fbProfileUrl);

                            if (fbProfileResponse.IsSuccessStatusCode)
                            {
                                var fbProfileJson = await fbProfileResponse.Content.ReadAsStringAsync();
                                var fbProfileResult = JObject.Parse(fbProfileJson);

                                facebookProfile = new FacebookPageProfileDTO
                                {
                                    Id = fbProfileResult["id"]?.ToString(),
                                    Name = fbProfileResult["name"]?.ToString(),
                                    Username = fbProfileResult["username"]?.ToString(),
                                    About = fbProfileResult["about"]?.ToString(),
                                    Category = fbProfileResult["category"]?.ToString(),
                                    Link = fbProfileResult["link"]?.ToString(),
                                    Website = fbProfileResult["website"]?.ToString(),
                                    PictureUrl = fbProfileResult["picture"]?["data"]?["url"]?.ToString(),
                                    FanCount = (int?)fbProfileResult["fan_count"],
                                    FollowersCount = (int?)fbProfileResult["followers_count"]
                                };

                                // fall back to detailed picture if list call had none
                                if (string.IsNullOrEmpty(pagePictureUrl))
                                    pagePictureUrl = facebookProfile.PictureUrl;
                            }
                            else
                            {
                                var fbProfileError = await fbProfileResponse.Content.ReadAsStringAsync();
                                _logger.LogWarning("Failed to fetch Facebook page profile for Page {PageId}. Status: {StatusCode}. Error: {Error}",
                                    pageId, fbProfileResponse.StatusCode, fbProfileError);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to fetch Facebook page profile for Page {PageId}", pageId);
                        }

                        try
                        {
                            var igUrl = $"https://graph.facebook.com/v19.0/{pageId}?fields=instagram_business_account&access_token={pageAccessToken}";
                            var igResponse = await _httpClient.GetAsync(igUrl);

                            if (igResponse.IsSuccessStatusCode)
                            {
                                var igJson = await igResponse.Content.ReadAsStringAsync();
                                var igResult = JObject.Parse(igJson);
                                instagramBusinessId = igResult["instagram_business_account"]?["id"]?.ToString();

                                // ✅ Fetch Instagram profile info (username, profile pic, etc.)
                                if (!string.IsNullOrEmpty(instagramBusinessId))
                                {
                                    try
                                    {
                                        var profileUrl = $"https://graph.facebook.com/v19.0/{instagramBusinessId}" +
                                                         "?fields=id,username,name,profile_picture_url,biography,followers_count,follows_count,media_count,website" +
                                                         $"&access_token={pageAccessToken}";
                                        var profileResponse = await _httpClient.GetAsync(profileUrl);

                                        if (profileResponse.IsSuccessStatusCode)
                                        {
                                            var profileJson = await profileResponse.Content.ReadAsStringAsync();
                                            var profileResult = JObject.Parse(profileJson);

                                            instagramProfile = new InstagramProfileDTO
                                            {
                                                Id = profileResult["id"]?.ToString(),
                                                Username = profileResult["username"]?.ToString(),
                                                Name = profileResult["name"]?.ToString(),
                                                ProfilePictureUrl = profileResult["profile_picture_url"]?.ToString(),
                                                Biography = profileResult["biography"]?.ToString(),
                                                FollowersCount = (int?)profileResult["followers_count"],
                                                FollowsCount = (int?)profileResult["follows_count"],
                                                MediaCount = (int?)profileResult["media_count"],
                                                Website = profileResult["website"]?.ToString()
                                            };
                                        }
                                        else
                                        {
                                            var profileError = await profileResponse.Content.ReadAsStringAsync();
                                            _logger.LogWarning("Failed to fetch Instagram profile for IG {InstagramBusinessId}. Status: {StatusCode}. Error: {Error}",
                                                instagramBusinessId, profileResponse.StatusCode, profileError);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to fetch Instagram profile for IG {InstagramBusinessId}", instagramBusinessId);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to fetch Instagram Business Account for Page {PageId}", pageId);
                        }
                    }

                    pages.Add(new FacebookPageDTO
                    {
                        Id = pageId,
                        Name = page["name"]?.ToString(),
                        AccessToken = pageAccessToken,
                        PictureUrl = pagePictureUrl,
                        FacebookProfile = facebookProfile,
                        InstagramBusinessId = instagramBusinessId,
                        InstagramProfile = instagramProfile
                    });
                }

                var responseDto = new FacebookPagesResponseDTO
                {
                    IsFacebookConnected = user.IsConnectedFacebook,
                    FacebookTokenExpiresAt = tokenExpiry,
                    Pages = pages
                };

                return await Result<FacebookPagesResponseDTO>.SuccessAsync(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Facebook pages for user {UserId}", request.UserId);
                return await Result<FacebookPagesResponseDTO>.FailAsync("An unexpected error occurred while fetching pages.");
            }
        }

        public async Task<Result<InstagramResponseDTO>> GetInstagramCode(FacebookPageRequest request)
        {
            try
            {
                var user = await _db.Users.FindAsync(request.UserId);
                if (user == null)
                    return await Result<InstagramResponseDTO>.FailAsync("User not found.");

                string tokenToUse;
                DateTime? tokenExpiry = user.InstagramTokenExpiresAt;

                // ✅ Use saved long-lived token if still valid
                if (user.IsConnectedInstagram
                    && !string.IsNullOrEmpty(user.InstagramLongLivedToken)
                    && tokenExpiry.HasValue
                    && tokenExpiry.Value > DateTime.UtcNow)
                {
                    tokenToUse = user.InstagramLongLivedToken;
                }
                else
                {
                    // Token expired or not connected → use client-provided short-lived token
                    if (string.IsNullOrEmpty(request.AccessToken))
                        return await Result<InstagramResponseDTO>.FailAsync("Access token is required.");

                    // Exchange short-lived → long-lived
                    var (longLivedToken, expiration, instagramBusinessId) = await ExchangeForLongLivedTokenInstagramAsync(request.AccessToken);
                    if (string.IsNullOrEmpty(longLivedToken))
                        return await Result<InstagramResponseDTO>.FailAsync("Failed to exchange token.");

                    // Save new token + expiry
                    user.IsConnectedInstagram = true;
                    user.InstagramLongLivedToken = longLivedToken;
                    user.InstagramTokenExpiresAt = expiration;
                    user.InstagramBusinessId= instagramBusinessId;
                    await _db.SaveChangesAsync();

                    tokenToUse = longLivedToken;
                    tokenExpiry = expiration;
                }

                var responseDto = new InstagramResponseDTO
                {
                    IsInstagramConnected = user.IsConnectedInstagram,
                    InstagramTokenExpiresAt = tokenExpiry,
                    AccessToken = tokenToUse
                };

                return await Result<InstagramResponseDTO>.SuccessAsync(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Instagram code for user {UserId}", request.UserId);
                return await Result<InstagramResponseDTO>.FailAsync("An unexpected error occurred while fetching Instagram code.");
            }
        }

        public async Task<Result<bool>> UpdateLanguageAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    return await Result<bool>.FailAsync(new List<string> { "User not found" });
                }

                var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

                // ✅ Update LanguageCode if it’s empty or different
                if (string.IsNullOrWhiteSpace(user.LanguageCode) || user.LanguageCode != currentLang)
                {
                    user.LanguageCode = currentLang;
                    await _userManager.UpdateAsync(user);
                }

                return await Result<bool>.SuccessAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<bool>.FailAsync(new List<string> { "Failed to update language." });
            }
        }

        public async Task<IResult<List<UserListItemDto>>> GetUsersAsync(PaginationRequest request)
        {
            try
            {
                var predicate = PredicateBuilder.New<User>(true);
                predicate = predicate.And(u => u.IsAdminPanel);
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string term = request.SearchTerm.ToLower();

                    predicate = predicate.And(u =>
                        u.Email.ToLower().Contains(term) ||
                        u.FullName.ToLower().Contains(term)
                    );
                }

                var query =
                    from u in _db.Users.AsExpandable().Where(predicate)
                    join ur in _db.UserRoles on u.Id equals ur.UserId
                    join r in _db.Roles on ur.RoleId equals r.Id
                    where r.Name != "Player" && r.Name != "Club"
                    select new UserListItemDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FullName = u.FullName,
                        CreatedAt = u.Created,
                        Role = r.Name,
                        RoleId = r.Id,
                        IsActive = u.IsActive,
                        PhoneNumber = u.PhoneNumber,
                    };

                if (request.RoleId.HasValue)
                {
                    var roleName = await _db.Roles
                        .Where(r => r.Id == request.RoleId)
                        .Select(r => r.Name)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(roleName))
                    {
                        query = query.Where(x => x.Role == roleName);
                    }
                }

                query = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);

                var list = query.ToList();

                return await Result<List<UserListItemDto>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<List<UserListItemDto>>.FailAsync(
                    new List<string> { "Failed to load users." }
                );
            }
        }

        public async Task<Result<bool>> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    return await Result<bool>.FailAsync(
                        new List<string> { "User not found" });
                }

                // Soft delete flags
                user.IsActive = false;
                user.CanLogin = false;
                user.DeletedAt = DateTime.UtcNow;
                user.LastModified = DateTime.UtcNow;

                // Force logout & block login
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _db.UserLogins.Where(x => x.UserId == user.Id).ExecuteDeleteAsync();
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return await Result<bool>.FailAsync(
                        result.Errors.Select(e => e.Description).ToList());
                }

                return await Result<bool>.SuccessAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Result<bool>.FailAsync(
                    new List<string> { "Failed to delete user account." });
            }
        }

        public async Task<IResult<List<UserListItemDto>>> GetGraphicDesignerUsersAsync()
        {
            try
            {
                // Correct role name (fix spelling if needed)
                var roleName = "Graphic Designer";

                // Get users assigned to this role
                var users = await _userManager.GetUsersInRoleAsync(roleName);

                if (users == null || !users.Any())
                {
                    return await Result<List<UserListItemDto>>.FailAsync("No users found with this role.");
                }

                // Fetch role details (RoleId, RoleName)
                var role = await _roleManager.FindByNameAsync(roleName);

                if (role == null)
                {
                    return await Result<List<UserListItemDto>>.FailAsync("Role not found.");
                }

                // Convert user entities into DTO format
                var list = users.Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    CreatedAt = u.Created,
                    RoleId = role.Id,
                    Role = role.Name, // Or Role = role.Name if your DTO uses 'Role'
                    IsActive = u.IsActive,
                    PhoneNumber = u.PhoneNumber
                }).ToList();

                return await Result<List<UserListItemDto>>.SuccessAsync(list, "Graphic designers fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<List<UserListItemDto>>.FailAsync("Failed to load graphic designers.");
            }
        }

        public async Task<Result<bool>> AddOrUpdateUserAsync(AddOrUpdateUserRequest request)
        {
            try
            {
                if (request.UserId == Guid.Empty)
                {
                    if (request.RoleId == null || request.RoleId == Guid.Empty)
                        return await Result<bool>.FailAsync("RoleId is required.");

                    var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                    if (role == null)
                        return await Result<bool>.FailAsync("Role not found.");
                   
                    var exists = await _userManager.FindByEmailAsync(request.Email);
                    if (exists != null && exists.IsAdminPanel)
                        return await Result<bool>.FailAsync("Email already registered.");

                    // Create user
                    var user = new User
                    {
                        FullName = request.FullName,
                        UserName = request.Email,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        IsAddedFromPortal = true,
                        IsPasswordRequired = true,
                        Created = DateTime.UtcNow,
                        CreatedBy = _currentUser.GetFullName(),
                        LastModified = DateTime.UtcNow,
                    };

                    var createResult = await _userManager.CreateAsync(user, request.Password);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        return await Result<bool>.FailAsync($"User creation failed: {errors}");
                    }
                    
                    await _userManager.AddToRoleAsync(user, role.Name);

                    return await Result<bool>.SuccessAsync(true, "User created successfully.");
                }

                var existingUser = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (existingUser == null)
                    return await Result<bool>.FailAsync("User not found.");
                
                existingUser.FullName = request.FullName;
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.LastModified = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.Email) && existingUser.Email != request.Email)
                {
                    var emailOwner = await _userManager.FindByEmailAsync(request.Email);
                    if (emailOwner != null && emailOwner.Id != existingUser.Id)
                        return await Result<bool>.FailAsync("Email already used by another user.");

                    existingUser.Email = request.Email;
                    existingUser.UserName = request.Email;
                }

                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return await Result<bool>.FailAsync($"User update failed: {errors}");
                }
                
                if (request.RoleId != null && request.RoleId != Guid.Empty)
                {
                    var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                    if (role != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        if (currentRoles.Any())
                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);

                        await _userManager.AddToRoleAsync(existingUser, role.Name);
                    }
                }

                return await Result<bool>.SuccessAsync(true, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<IResult<UserListItemDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var query =
                    from u in _db.Users
                    join ur in _db.UserRoles on u.Id equals ur.UserId
                    join r in _db.Roles on ur.RoleId equals r.Id
                    where u.Id == userId   // ← ONLY filter by UserId
                    select new UserListItemDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FullName = u.FullName,
                        CreatedAt = u.Created,
                        Role = r.Name,
                        RoleId = r.Id,
                        IsActive = u.IsActive,
                        PhoneNumber = u.PhoneNumber,
                    };

                var user = await query.FirstOrDefaultAsync();

                if (user == null)
                    return await Result<UserListItemDto>.FailAsync("User not found.");

                return await Result<UserListItemDto>.SuccessAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<UserListItemDto>.FailAsync("Failed to load user details.");
            }
        }

        public async Task<IResult<bool>> ResetUserPasswordAsync(ResetPasswordRequest request)
        {
            try
            {

                var user = await _userManager.FindByIdAsync(request.UserId.ToString());

                if (user is null)
                {
                    return await Result<bool>.FailAsync(new List<string> { "User not found" });
                }

                // Generate reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Reset password using token
                var identityResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                if (!identityResult.Succeeded)
                {
                    var errors = identityResult.Errors
                                                .Select(e => e.Description)
                                                .ToList();

                    return await Result<bool>.FailAsync(errors);
                }

                return await Result<bool>.SuccessAsync(true, "Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<bool>.FailAsync(new List<string> { "Failed to reset password." });
            }
        }

        #region Methods
        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString(); // 4-digit code
        }
        
        private async Task SendVerificationEmail(User user)
        {
            var placeholders = GetPlaceholders(user);

            var mailRequest = new MailRequest
            {
                To = user.Email,
                Body = await _fileService.GetTemplateContent(EmailConstants.Templates.Email.OTPEmail, placeholders),
                Subject = EmailConstants.Subjects.EmailVerification
            };

            _jobService.Enqueue(() => _mailService.SendAsync(mailRequest));
        }
        private Dictionary<string, string> GetPlaceholders(User user)
        {
            var placeholders = new Dictionary<string, string>()
            {
                //{ "{#FullName#}", user.FullName ?? string.Empty},
            };

            if (user.EmailVerificationCode != null)
            {
                //int[] otpDigits = Array.ConvertAll(user.EmailVerificationCode.ToCharArray(), digit => (int)char.GetNumericValue(digit));
                //string htmlOTP = "";
                //foreach (int digit in otpDigits)
                //{
                //    htmlOTP += $"<td class=\"otp-box\" style=\"width:45px; height:45px; border:1px solid #ccc; text-align:center; font-size:18px; font-weight:bold; line-height:45px; border-radius:8px;\">{digit}</td>";
                //    //htmlOTP += $"<div class=\"otp-digit\">{digit}</div>";
                //}
                string htmlOTP = $"<td align=\"center\" class=\"otp-code\" style=\"padding: 20px; font-size: 32px; font-weight: bold; letter-spacing: 4px; color: #000000;\">\r\n   {user.EmailVerificationCode}\r\n  </td>"; ; 
                placeholders.Add("{#EmailVerificationCode#}", htmlOTP);
            }

            return placeholders;
        }
        public async Task<SocialDTO?> VerifyGoogleTokenAsync(ExternalAuthRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
                {
                    _logger.LogError("Google ClientId is not configured properly.");
                    return null;
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleAuthSettings.ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);

                return new SocialDTO
                {
                    FacebookTokenExpiresAt = null,
                    FacebookLongLivedToken = null,
                    IsConnectedFacebook = false,
                    Email = payload.Email,
                    Subject = payload.Subject
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null; // Explicitly returning null
            }
        }
        public async Task<SocialDTO?> VerifyFacebookTokenAsync(ExternalAuthRequest request)
        {
            try
            {
                // Step 1: Get App Token
                var appToken = await GetAppTokenAsync();
                if (string.IsNullOrEmpty(appToken))
                    return null;

                // Step 2: Validate User Token
                var validationResult = await GetDebugTokenAsync(request.Token, appToken);
                if (validationResult == null || !(validationResult["is_valid"]?.Value<bool>() ?? false))
                {
                    _logger.LogError("Facebook token is invalid.");
                    return null;
                }

                var userId = validationResult["user_id"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("Unable to retrieve Facebook user ID.");
                    return null;
                }

                // Step 3: Get User Info
                var userInfo = await GetFacebookUserInfoAsync(userId, request.Token);
                if (userInfo == null)
                {
                    _logger.LogError("Invalid or incomplete Facebook user data.");
                    return null;
                }

                if (string.IsNullOrEmpty(userInfo.Email))
                {
                    _logger.LogWarning("Facebook user does not have an email registered.");
                    userInfo.Email = $"facebook_{userInfo.Id}@facebook.com"; // fallback
                }

                // Step 4: Exchange for Long-Lived Token
                var longLivedToken = await ExchangeForLongLivedTokenAsync(request.Token);
                if (string.IsNullOrEmpty(longLivedToken.AccessToken))
                {
                    _logger.LogError("Failed to exchange for long-lived token.");
                    return null;
                }

                // Step 5: Return Social Payload
                return new SocialDTO
                {
                    
                    IsConnectedFacebook = true,
                    FacebookLongLivedToken = longLivedToken.AccessToken,
                    FacebookTokenExpiresAt = longLivedToken.Expiration,
                    Email = userInfo.Email,
                    Subject = userInfo.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in VerifyFacebookTokenAsync");
                return null;
            }
        }
        public async Task<SocialDTO> VerifyAppleTokenAsync(ExternalAuthRequest request)
        {
            try
            {
                var social = new SocialDTO();

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(request.Token);

                var appleUserId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Facebook user does not have an email registered.");
                    social.Email = $"apple_{appleUserId}@apple.com"; // fallback
                }

                if (string.IsNullOrEmpty(appleUserId))
                {
                    _logger.LogError("Apple token does not contain a valid user ID.");
                    return null;
                }

               
                social.FacebookTokenExpiresAt = null;
                social.FacebookLongLivedToken = null;
                social.IsConnectedFacebook = false;
                social.Email = email;
                social.Subject = appleUserId;

                return social;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Apple token.");
                return null;
            }
        }
        private async Task<string?> GetAppTokenAsync()
        {
            try
            {
                var url = $"https://graph.facebook.com/oauth/access_token" +
                          $"?client_id={_facebookAuthSettings.AppId}" +
                          $"&client_secret={_facebookAuthSettings.AppSecret}" +
                          $"&grant_type=client_credentials";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json)["access_token"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook App token.");
                return null;
            }
        }
        private async Task<JObject?> GetDebugTokenAsync(string userToken, string appToken)
        {
            try
            {
                var url = $"https://graph.facebook.com/debug_token" +
                          $"?input_token={userToken}" +
                          $"&access_token={appToken}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json)["data"] as JObject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Facebook user token.");
                return null;
            }
        }
        private async Task<FacebookUserInfoDTO?> GetFacebookUserInfoAsync(string userId, string accessToken)
        {
            try
            {
                var fields = "id,email,name,picture.width(300).height(300)";
                var url = $"https://graph.facebook.com/{userId}?fields={fields}&access_token={accessToken}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FacebookUserInfoDTO>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook user info.");
                return null;
            }
        }
        public async Task<(string? AccessToken, DateTime? Expiration)> ExchangeForLongLivedTokenAsync(string shortLivedToken)
        {
            try
            {
                // Step 1: Exchange short-lived token for long-lived token
                var url = $"https://graph.facebook.com/v19.0/oauth/access_token" +
                          $"?grant_type=fb_exchange_token" +
                          $"&client_id={_facebookAuthSettings.AppId}" +
                          $"&client_secret={_facebookAuthSettings.AppSecret}" +
                          $"&fb_exchange_token={shortLivedToken}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return (null, null);

                var json = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                var accessToken = obj["access_token"]?.ToString();
                if (string.IsNullOrEmpty(accessToken))
                    return (null, null);

                // Step 2: Debug the token to check expiration
                var debugUrl = $"https://graph.facebook.com/debug_token" +
                               $"?input_token={accessToken}" +
                               $"&access_token={_facebookAuthSettings.AppId}|{_facebookAuthSettings.AppSecret}";

                var debugResponse = await _httpClient.GetAsync(debugUrl);
                if (!debugResponse.IsSuccessStatusCode) return (accessToken, null);

                var debugJson = await debugResponse.Content.ReadAsStringAsync();
                var debugObj = JObject.Parse(debugJson);

                // Get both values
                var expiresAtUnix = debugObj["data"]?["expires_at"]?.ToObject<long?>();
                var dataAccessExpiresAtUnix = debugObj["data"]?["data_access_expires_at"]?.ToObject<long?>();

                // Treat 0 as "not set"
                if (expiresAtUnix.HasValue && expiresAtUnix.Value == 0)
                {
                    expiresAtUnix = null;
                }

                // Prefer expires_at if valid, otherwise fall back
                long? expiryUnix = expiresAtUnix ?? dataAccessExpiresAtUnix;

                DateTime? expirationDate = null;
                if (expiryUnix.HasValue)
                {
                    expirationDate = DateTimeOffset.FromUnixTimeSeconds(expiryUnix.Value).UtcDateTime;
                }

                return (accessToken, expirationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging Facebook token for long-lived token.");
                return (null, null);
            }
        }

        public async Task<(string? AccessToken, DateTime? Expiration, string? InstagramBusinessId)> ExchangeForLongLivedTokenInstagramAsync(string shortLivedToken)
        {
            try
            {
                // Step 1: Get short-lived token for long-lived token
                var url = "https://api.instagram.com/oauth/access_token";
               
                var formData = new Dictionary<string, string>
                {
                    { "client_id", $"{_instagramAuthSettings.AppId}" },
                    { "client_secret", $"{_instagramAuthSettings.AppSecret}" },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", "https://fubaza.de/auth/instagram/callback" }, // must match exactly with Instagram app settings
                    { "code", $"{shortLivedToken}" }
                };

                var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(formData));
                if (!response.IsSuccessStatusCode) return (null, null,null);

                var json = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                var accessToken = obj["access_token"]?.ToString();
                if (string.IsNullOrEmpty(accessToken))
                    return (null, null,null);

                // Step 2: Get the long lived token's and expiration.
                var longlivedUrl = $"https://graph.instagram.com/access_token" +
                               $"?grant_type=ig_exchange_token" +
                               $"&client_secret={_instagramAuthSettings.AppSecret}" +
                               $"&access_token={accessToken}";

                var longlivedResponse = await _httpClient.GetAsync(longlivedUrl);
                if (!longlivedResponse.IsSuccessStatusCode) return (null, null,null);

                var longlivedJson = await longlivedResponse.Content.ReadAsStringAsync();
                var longlivedObj = JObject.Parse(longlivedJson);
                // Get both values

                 accessToken = longlivedObj["access_token"]?.ToString();
                 var expiresInSeconds = longlivedObj["expires_in"]?.ToObject<long?>();

                //Step 3: Get InstagramBuissnessId
                var IdUrl = $"https://graph.instagram.com/v25.0/me" +
                               $"?fields=user_id,username" +
                               $"&access_token={accessToken}";
                var IdResponse = await _httpClient.GetAsync(IdUrl);
               if (!IdResponse.IsSuccessStatusCode) return (null, null,null);

                var IdJson = await IdResponse.Content.ReadAsStringAsync();
                                var IdObj = JObject.Parse(IdJson);

                var instagrambuissnessId = IdObj["user_id"]?.ToString();

                // Treat 0 as "not set"
                if (expiresInSeconds.HasValue && expiresInSeconds.Value == 0)
                {
                    expiresInSeconds = null;
                }
                // Prefer expires_at if valid, otherwise fall back
                long? expiryUnix = expiresInSeconds;

                DateTime? expirationDate = null;
                if (expiryUnix.HasValue)
                {
                    expirationDate = DateTime.UtcNow.AddSeconds((double)expiryUnix);
                }

                return (accessToken, expirationDate, instagrambuissnessId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging Instagram token for long-lived token.");
                return (null, null,null);
            }
        }

        

        #endregion
    }
}
