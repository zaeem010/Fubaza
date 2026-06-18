using AutoMapper;
using Azure.Core;
using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Infrastructure.Services
{
    public class ClubService : IClubService
    {
        private readonly ILogger<ClubService> _logger;
        private readonly IClubRepository _repository;
        private readonly IFileService _fileService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IFirbaseService _firbaseService;
        private readonly IJobService _jobService;
        private readonly ILookUpRepository _lookUpRepository;

        public ClubService(ILogger<ClubService> logger,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IFileService fileService,
            IClubRepository repository,
            IMapper mapper,
            IJwtTokenService jwtTokenService,
            IFirbaseService firbaseService,
            IJobService jobService,
            ILookUpRepository lookUpRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _fileService = fileService;
            _jwtTokenService = jwtTokenService;
            _jobService = jobService;
            _firbaseService = firbaseService;
            _lookUpRepository = lookUpRepository;
        }

        public async Task<IResult<PaginatedResponse<ClubDTO>>> GetClubAsync(PaginationRequest request)
        {
            try
            {
                var result = await _repository.GetClubAsync(request);

                if (result != null)
                {
                    return await Result<PaginatedResponse<ClubDTO>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Clubs";

                _logger.LogError(message);
                return await Result<PaginatedResponse<ClubDTO>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PaginatedResponse<ClubDTO>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> AddClubAsync(Club club)
        {
            const string message = "Unable to add the club";
            try
            {
                if (club.Document?.FileName != null)
                {
                    club.Document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = club.Document.FileContent,
                        Extension = club.Document.Extension,
                        FileName = club.Document.FileName,
                        UploadType = UploadType.ClubRequest
                    });
                }

                var user = new User
                {
                    Email = $"club_{Guid.NewGuid()}@club.com",
                    IsActive = false,
                    UserName = $"club_{Guid.NewGuid()}@club.com",
                    Created = DateTime.Now,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    CanLogin = false
                };

                user.Password ??= UserConstants.DefaultPassword;

                var result = await _userManager.CreateAsync(user, user.Password!);

                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return await Result<bool>.FailAsync("An error occurred while creating the user.");
                }

                var roleAssignResult = await _userManager.AddToRoleAsync(user, RoleConstants.Club);
                if (!roleAssignResult.Succeeded)
                {
                    _logger.LogError("Role assignment failed: {Errors}", string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
                    return await Result<bool>.FailAsync("User created but role assignment failed.");
                }

                club.UserId = user.Id;

                var response = await _repository.AddClubAsync(club);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await  Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await  Result<bool>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<ClubProfileDTO>> GetClubProfileAsync(Guid userId)
        {
            try
            {
                var result = await _repository.GetClubProfileAsync(userId);

                if (result != null)
                {
                    return await Result<ClubProfileDTO>.SuccessAsync(result);
                }

                const string message = "Unable to get the Club";

                _logger.LogError(message);
                return await Result<ClubProfileDTO>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<ClubProfileDTO>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<ClubProfileDTO>> EditClubProfileAsync(Club updatedClub)
        {
            const string message = "Unable to add the club";
            try
            {

                if (updatedClub.Document is { FileName: not null })
                {
                    updatedClub.Document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = updatedClub.Document.FileContent,
                        Extension = updatedClub.Document.Extension,
                        FileName = updatedClub.Document.FileName,
                        UploadType = UploadType.ClubRequest
                    });
                }

                var response = await _repository.EditClubProfileAsync(updatedClub);

                if (response != null)
                {
                    return await Result<ClubProfileDTO>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<ClubProfileDTO>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<ClubProfileDTO>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<UserDto>> UpdateClubProfileAsync(Club club)
        {
            const string message = "Unable to add the club";
            try
            {
                if (club.Document is { FileName: not null })
                {
                    club.Document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = club.Document.FileContent,
                        Extension = club.Document.Extension,
                        FileName = club.Document.FileName,
                        UploadType = UploadType.ClubRequest
                    });
                }

                var ouser = await _repository.UpdateClubProfileAsync(club);
                if (ouser == null) return await Result<UserDto>.FailAsync("User not found");

                if (club.RoleId.HasValue)
                {
                    var role = await _roleManager.FindByIdAsync(club.RoleId.ToString());
                    if (role == null || string.IsNullOrWhiteSpace(role.Name))
                        return await Result<UserDto>.FailAsync("Role not found or invalid");

                    var roleName = role.Name.Trim();
                    var currentRoles = await _userManager.GetRolesAsync(ouser);

                    if (!currentRoles.Contains(roleName))
                    {
                        await _userManager.RemoveFromRolesAsync(ouser, currentRoles);
                        await _userManager.AddToRoleAsync(ouser, roleName);
                    }
                }

                var jwtToken = await _jwtTokenService.GenerateTokenAsync(ouser);

                ouser.RefreshToken = jwtToken;
                await _userManager.UpdateAsync(ouser);

                var userDto = _mapper.Map<UserDto>(ouser);

                return await Result<UserDto>.SuccessAsync(userDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<UserDto>.FailAsync(message);
            }
        }

        public async Task<IResult<bool>> AddOrUpdateClubOfficialAsync(ClubOfficial clubOfficial , Guid userId)
        {
            const string message = "Unable to add the Club Official";
            try
            {

                var desination = await _lookUpRepository.GetDesignationAsync(clubOfficial.DesignationId);

                string title = clubOfficial.Id == Guid.Empty
                    ? NotificationConstants.En.Titles.OfficialAdded
                    : NotificationConstants.En.Titles.OfficialUpdated;

                string body = clubOfficial.Id == Guid.Empty
                ? string.Format(NotificationConstants.En.Bodies.OfficialAdded, clubOfficial.Name, desination.Title)
                : string.Format(NotificationConstants.En.Bodies.OfficialUpdated, clubOfficial.Name, desination.Title);


                string titleDe = clubOfficial.Id == Guid.Empty
                   ? NotificationConstants.De.Titles.OfficialAdded
                   : NotificationConstants.De.Titles.OfficialUpdated;

                string bodyDe = clubOfficial.Id == Guid.Empty
                ? string.Format(NotificationConstants.De.Bodies.OfficialAdded, clubOfficial.Name, desination.TitleDe)
                : string.Format(NotificationConstants.De.Bodies.OfficialUpdated, clubOfficial.Name, desination.TitleDe);

                if (clubOfficial.Document is { FileName: not null })
                {
                    clubOfficial.Document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = clubOfficial.Document.FileContent,
                        Extension = clubOfficial.Document.Extension,
                        FileName = clubOfficial.Document.FileName,
                        UploadType = UploadType.ClubOfficialRequest
                    });
                }

                var notification = new Notification
                {
                    Title = title,
                    Body = body,
                    TitleDe = titleDe,
                    BodyDe = bodyDe,
                    IsRead = false,
                    UserId = userId,
                };

                var response = await _repository.AddOrUpdateClubOfficialAsync(clubOfficial , notification);

                if (response)
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());

                    if (user != null)
                    {
                        if (user.IsNotificationEnabled)
                        {
                            var notificationRequest = new NotificationRequest
                            {
                                Token = user.FcmToken,
                                Title = LocalizationExtensions.Localize(title, titleDe),
                                Body = LocalizationExtensions.Localize(body, bodyDe),
                            };

                            _jobService.Enqueue(() => _firbaseService.SendAsync(notificationRequest));
                        }
                            
                    }

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

        public async Task<IResult<Dictionary<string, List<DepartmentOfficialDto>>>> GetClubOfficialsByDepartmentAsync(Guid userId)
        {
            try
            {
                var result = await _repository.GetClubOfficialsByDepartmentAsync(userId);

                if (result != null)
                {
                    return await Result<Dictionary<string, List<DepartmentOfficialDto>>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Official";

                _logger.LogError(message);
                return await Result<Dictionary<string, List<DepartmentOfficialDto>>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<Dictionary<string, List<DepartmentOfficialDto>>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<bool>> RemoveClubOfficialAsync(Guid clubOfficialId, Guid userId)
        {
            const string message = "Unable to remove the Club Official";
            try
            {

                var clubOfficial = await _lookUpRepository.GetClubOfficialAsync(clubOfficialId);

                if (clubOfficial != null)
                {
                    string title = NotificationConstants.En.Titles.OfficialRemoved;

                    string body = string.Format(NotificationConstants.En.Bodies.OfficialRemoved, clubOfficial.Name, clubOfficial.Designation?.TitleDe);

                    string titleDe = NotificationConstants.De.Titles.OfficialRemoved;

                    string bodyDe = string.Format(NotificationConstants.De.Bodies.OfficialRemoved, clubOfficial.Name, clubOfficial.Designation?.TitleDe);

                    var notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        TitleDe = titleDe,
                        BodyDe = bodyDe,
                        IsRead = false,
                        UserId = userId,
                    };

                    var response = await _repository.RemoveClubOfficialAsync(clubOfficialId, notification);

                    if (response)
                    {
                        var user = await _userManager.FindByIdAsync(userId.ToString());

                        if (user != null)
                        {
                            if (user.IsNotificationEnabled)
                            {
                                var notificationRequest = new NotificationRequest
                                {
                                    Token = user.FcmToken,
                                    Title = LocalizationExtensions.Localize(title, titleDe),
                                    Body = LocalizationExtensions.Localize(body, bodyDe),
                                };

                                _jobService.Enqueue(() => _firbaseService.SendAsync(notificationRequest));
                            }
                            
                        }

                        return await Result<bool>.SuccessAsync(response);
                    }

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

        public async Task<IResult<List<LineupPlayerInfoDto>>> GetMatchLinupbyCludAsync(Guid clubId, Guid organizerClubId)
        {
            try
            {
                var result = await _repository.GetMatchLinupbyCludAsync(clubId,organizerClubId);

                if (result != null)
                {
                    return await Result<List<LineupPlayerInfoDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the match line Up";

                _logger.LogError(message);
                return await Result<List<LineupPlayerInfoDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<LineupPlayerInfoDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<ClubStatDto>> GetClubStatsAsync(Guid clubId)
        {
            try
            {   
                var result = await _repository.GetClubStatsAsync(clubId);

                if (result != null)
                {
                    return await Result<ClubStatDto>.SuccessAsync(result);
                }

                const string message = "Unable to get the club stats";

                _logger.LogError(message);
                return await Result<ClubStatDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<ClubStatDto>.FailAsync(e.Message);
            }
        }
    }
}
