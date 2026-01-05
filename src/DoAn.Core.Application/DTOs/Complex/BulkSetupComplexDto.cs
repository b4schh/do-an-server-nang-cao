using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.Complex;

public class BulkSetupComplexDto
{
    [Required(ErrorMessage = "Thông tin cụm sân là bắt buộc")]
    public CreateComplexByOwnerDto Complex { get; set; } = null!;

    [Required(ErrorMessage = "Danh sách sân là bắt buộc")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sân")]
    public List<BulkFieldDto> Fields { get; set; } = new();

    public TimeSlotTemplateDto? TimeSlotTemplate { get; set; }
    public bool ApplyTemplateToAllFields { get; set; } = true;
}

/// <summary>
/// Field info for bulk creation
/// </summary>
public class BulkFieldDto
{
    [Required(ErrorMessage = "Tên sân là bắt buộc")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Loại sân là bắt buộc")]
    public string FieldType { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Custom timeslots for this specific field (if not using template)
    /// </summary>
    public List<BulkTimeSlotDto>? CustomTimeSlots { get; set; }
}

/// <summary>
/// TimeSlot template to apply to multiple fields
/// </summary>
public class TimeSlotTemplateDto
{
    [Required(ErrorMessage = "Danh sách khung giờ là bắt buộc")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 khung giờ")]
    public List<BulkTimeSlotDto> TimeSlots { get; set; } = new();
}

/// <summary>
/// TimeSlot info for bulk creation
/// </summary>
public class BulkTimeSlotDto
{
    [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
    public decimal Price { get; set; }
}
