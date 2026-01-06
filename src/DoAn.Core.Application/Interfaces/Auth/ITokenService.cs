using System.Security.Claims;

namespace DoAn.Core.Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateToken(DoAn.Core.Domain.Entities.User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}

