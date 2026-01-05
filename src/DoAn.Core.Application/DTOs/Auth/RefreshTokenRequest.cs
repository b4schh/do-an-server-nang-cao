using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token không được để trống")]
    public string RefreshToken { get; set; } = string.Empty;
}

