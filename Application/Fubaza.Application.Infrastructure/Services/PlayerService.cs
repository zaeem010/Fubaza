using AutoMapper;

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
    public class PlayerService : IPlayerService
    {
        private readonly ILogger<PlayerService> _logger;
        private readonly IPlayerRepository _repository;
        private readonly IFileService _fileService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IFirbaseService _firbaseService;
        private readonly IJobService _jobService;


        public PlayerService(ILogger<PlayerService> logger,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IFileService fileService,
            IPlayerRepository repository,
            IMapper mapper,
            IJwtTokenService jwtTokenService,
            IFirbaseService firbaseService,
            IJobService jobService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _fileService = fileService;
            _jwtTokenService = jwtTokenService;
            _jobService = jobService;
            _fileService = fileService;
            _firbaseService = firbaseService;
        }

        public async Task<IResult<bool>> AddOrUpdatePlayerAsync(Player player, Guid userid)
        {
            const string message = "Unable to add or update the player";

            try
            {
                var Jerseyresponse = await _repository.CheckPlayerJerseyAsync(player);
                if (!Jerseyresponse)
                {
                    _logger.LogError("The jersey number is already taken for the current club.");
                    return await Result<bool>.FailAsync("Service.Player.Message.Failed.CheckPlayerJerseyAsync");
                }

                string title = player.Id == Guid.Empty
                    ? NotificationConstants.En.Titles.PlayerAdded
                    : NotificationConstants.En.Titles.PlayerUpdated;

                string body = player.Id == Guid.Empty
                ? string.Format(NotificationConstants.En.Bodies.PlayerAdded, player.FullName)
                : string.Format(NotificationConstants.En.Bodies.PlayerUpdated, player.FullName);

                string titleDe = player.Id == Guid.Empty
                    ? NotificationConstants.De.Titles.PlayerAdded
                    : NotificationConstants.De.Titles.PlayerUpdated;

                string bodyDe = player.Id == Guid.Empty
                ? string.Format(NotificationConstants.De.Bodies.PlayerAdded, player.FullName)
                : string.Format(NotificationConstants.De.Bodies.PlayerUpdated, player.FullName);

                if (player.Documents != null)
                {
                    foreach (var document in player.Documents)
                    {
                        if (document.FileUrl == null)
                        {
                            document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                            {
                                FileContent = document.FileContent,
                                Extension = document.Extension,
                                FileName = document.FileName,
                                UploadType = UploadType.ClubRequest
                            });
                        }
                    }
                }
                
                if (player.Id == Guid.Empty)
                {
                    player.ClubHistory.Add(new PlayerClubHistory
                    {
                        ClubId = player.CurrentClubId,
                        IsCurrentClub = true,
                        StartYear = DateTime.Now.Year,
                    });

                    var user = new User
                    {
                        Email = $"player_{Guid.NewGuid()}@player.com",
                        IsActive = false,
                        UserName = $"player{Guid.NewGuid()}@player.com",
                        Created = DateTime.Now,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        CanLogin = false,
                        Password = UserConstants.DefaultPassword
                    };

                    var result = await _userManager.CreateAsync(user, user.Password!);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        return await Result<bool>.FailAsync("An error occurred while creating the user.");
                    }

                    var roleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Player);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Role assignment failed: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        return await Result<bool>.FailAsync("User created but role assignment failed.");
                    }

                    player.UserId = user.Id;
                }

                var notification = new Notification
                {
                    Title = title,
                    Body = body,
                    TitleDe = titleDe,
                    BodyDe = bodyDe,
                    IsRead = false,
                    UserId = userid,
                };

                var response = await _repository.AddOrUpdatePlayerAsync(player,notification);

                if (response)
                {
                    var user = await _userManager.FindByIdAsync(userid.ToString());

                    if (user != null)
                    {
                        if(user.IsNotificationEnabled)
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

        public async Task<IResult<UserDto>> UpdatePlayerProfileAsync(Player player)
        {
            const string message = "Unable to Add/Update User Profile or Role, please check input data and try again.";

            try
            {
                var profileDoc = player.Documents?.FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);
                if (profileDoc is { FileName: not null })
                {
                    profileDoc.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = profileDoc.FileContent,
                        Extension = profileDoc.Extension,
                        FileName = profileDoc.FileName,
                        UploadType = UploadType.PlayerRequest
                    });
                }

                var ouser = await _repository.UpdatePlayerProfileAsync(player);
                if (ouser == null) return await Result<UserDto>.FailAsync("User not found");
               
                
                if (player.RoleId.HasValue)
                {
                    var role = await _roleManager.FindByIdAsync(player.RoleId.ToString());
                    if (role == null)
                        return await Result<UserDto>.FailAsync("Role not found");

                    var roleName = role.Name?.Trim();
                    if (string.IsNullOrWhiteSpace(roleName))
                        return await Result<UserDto>.FailAsync("Role name is invalid");

                    var currentRoles = await _userManager.GetRolesAsync(ouser);

                    if (!currentRoles.Contains(roleName))
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(ouser, currentRoles);
                        if (!removeResult.Succeeded)
                            return await Result<UserDto>.FailAsync("Failed to remove existing roles");

                        var addResult = await _userManager.AddToRoleAsync(ouser, roleName);
                        if (!addResult.Succeeded)
                            return await Result<UserDto>.FailAsync("Failed to assign new role");
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

        public async Task<IResult<PlayerProfileDTO>> EditPlayerProfileAsync(Player updatedPlayer)
        {
            const string message = "Unable to add the club";
            try
            {
                var profileDoc = updatedPlayer.Documents?
                    .FirstOrDefault(d => d.DocumentType == PlayerDocumentType.Profile);

                if (profileDoc != null && !string.IsNullOrEmpty(profileDoc.FileName))
                {
                    profileDoc.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = profileDoc.FileContent,
                        Extension = profileDoc.Extension,
                        FileName = profileDoc.FileName,
                        UploadType = UploadType.PlayerRequest
                    });
                }

                var response = await _repository.EditPlayerProfileAsync(updatedPlayer);

                if (response != null)
                {
                    return await Result<PlayerProfileDTO>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<PlayerProfileDTO>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PlayerProfileDTO>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<PlayerProfileDTO>> GetPlayerProfileAsync(Guid userId)
        {
            try
            {
                var result = await _repository.GetPlayerProfileAsync(userId);

                if (result != null)
                {
                    return await Result<PlayerProfileDTO>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player";

                _logger.LogError(message);
                return await Result<PlayerProfileDTO>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PlayerProfileDTO>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetPlayersByCategoryAsync(Guid clubId)
        {
            try
            {
                var result = await _repository.GetPlayersByCategoryAsync(clubId);

                if (result != null)
                {
                    return await Result<Dictionary<string, CategoryPlayerDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player";

                _logger.LogError(message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<bool>> RemovePlayerAsync(Guid playerId, Guid userid)
        {
            const string message = "Unable to remove the Player";
            try
            {

                var player = await _repository.GetPlayerAsync(playerId);


                if (player == null) {
                    _logger.LogError(message);
                    return await Result<bool>.FailAsync(message);
                }

                if(player != null)
                {
                    string title = NotificationConstants.En.Titles.PlayerRemoved;

                    string body = string.Format(NotificationConstants.En.Bodies.PlayerRemoved, player.FullName);

                    string titleDe = NotificationConstants.De.Titles.PlayerRemoved;

                    string bodyDe = string.Format(NotificationConstants.De.Bodies.PlayerRemoved, player.FullName);


                    var notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        TitleDe = titleDe,
                        BodyDe = bodyDe,
                        IsRead = false,
                        UserId = userid,
                    };

                    var response = await _repository.RemovePlayerAsync(playerId, notification);

                    if (response)
                    {
                        var user = await _userManager.FindByIdAsync(userid.ToString());

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

        public async Task<IResult<Dictionary<object, DocumentTypeGroupDto>>> GetMediaGalleryAsync(Guid clubId)
        {
            try
            {
                var result = await _repository.GetMediaGalleryAsync(clubId);

                if (result != null)
                {
                    return await Result<Dictionary<object, DocumentTypeGroupDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player";

                _logger.LogError(message);
                return await Result<Dictionary<object, DocumentTypeGroupDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<Dictionary<object, DocumentTypeGroupDto>>.FailAsync(e.Message);
            }
        }
    }
}
