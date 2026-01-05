using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.User;

public class UpdateUserStatusDto
{
    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ (0: Inactive, 1: Active, 2: Banned)")]
    public byte Status { get; set; }
}
