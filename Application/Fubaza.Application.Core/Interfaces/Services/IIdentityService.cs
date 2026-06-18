using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.DTO.Contracts
{
    public interface IIdentityService
    {
        Task<IResult<UserDto>> SignUpAsync(SignUpRequest request);
        Task<IResult<UserDto>> SignInAsync(SignInRequest request);
        Task<IResult<UserDto>> SocialLoginAsync(ExternalAuthRequest request);
        Task<IResult<UserDto>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<IResult> ResendOTPAsync(ResendOTPRequest request);
        Task<IResult> VerifyOTPAsync(VerifyOTPRequest request);
        Task<IResult<UserDto>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<IResult<UserDto>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId);
        Task<Result<FacebookPagesResponseDTO>> GetUserPagesAsync(FacebookPageRequest request);
        Task<Result<UserDto>> RemoveUserPagesAsync(RemoveUserPageRequest request);
        Task<Result<InstagramResponseDTO>> GetInstagramCode(FacebookPageRequest request);
        Task<Result<bool>> UpdateLanguageAsync(Guid userId);
        Task<IResult<List<UserListItemDto>>> GetUsersAsync(PaginationRequest request);
        Task<Result<bool>> DeleteUserAsync(Guid userId);
        Task<Result<bool>> AddOrUpdateUserAsync(AddOrUpdateUserRequest request);
        Task<IResult<UserListItemDto>> GetUserByIdAsync(Guid userId);
        Task<IResult<bool>> ResetUserPasswordAsync(ResetPasswordRequest request);
        Task<IResult<List<UserListItemDto>>> GetGraphicDesignerUsersAsync();
    }
}
