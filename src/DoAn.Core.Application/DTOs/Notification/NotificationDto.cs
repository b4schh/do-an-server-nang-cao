namespace DoAn.Core.Application.DTOs.Notification;

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

public class MarkReadDto
{
    public int NotificationId { get; set; }
}

public class MarkMultipleReadDto
{
    public List<int> NotificationIds { get; set; } = new();
}

public class NotificationListDto
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public bool HasMore { get; set; }
    public int? LastId { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
