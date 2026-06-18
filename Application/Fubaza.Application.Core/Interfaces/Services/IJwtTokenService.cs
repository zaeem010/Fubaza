using Fubaza.Application.Core.Entities;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
