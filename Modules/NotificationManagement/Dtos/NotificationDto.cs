using FootballField.API.Modules.NotificationManagement.Entities;

namespace FootballField.API.Modules.NotificationManagement.Dtos;

/// <summary>
/// Response DTO for notification details
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public byte Type { get; set; }
    public string? TypeName { get; set; }
    public string? RelatedTable { get; set; }
    public int? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

/// <summary>
/// Request DTO for creating a notification
/// </summary>
public class CreateNotificationDto
{
    public int UserId { get; set; }
    public int? SenderId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public NotificationType Type { get; set; } = NotificationType.System;
    public string? RelatedTable { get; set; }
    public int? RelatedId { get; set; }
}

/// <summary>
/// Request DTO for marking notification as read
/// </summary>
public class MarkReadDto
{
    public int NotificationId { get; set; }
}

/// <summary>
/// Request DTO for marking multiple notifications as read
/// </summary>
public class MarkMultipleReadDto
{
    public List<int> NotificationIds { get; set; } = new();
}

/// <summary>
/// Response DTO for notifications list with pagination info
/// </summary>
public class NotificationListDto
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public bool HasMore { get; set; }
    public int? LastId { get; set; }
}

/// <summary>
/// Response DTO for unread count
/// </summary>
public class UnreadCountDto
{
    public int Count { get; set; }
}
