using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Services.Notification;
using DoAn.Core.Application.Interfaces.Notification;

namespace DoAn.Core.Application.Services.Notification.Helpers;

/// <summary>
/// Helper class to easily create and send notifications
/// </summary>
public class NotificationHelper
{
    private readonly INotificationService _notificationService;

    public NotificationHelper(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Send a booking-related notification
    /// </summary>
    public async Task SendBookingNotificationAsync(
        int userId,
        int? senderId,
        string title,
        string message,
        int bookingId)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            SenderId = senderId,
            Title = title,
            Message = message,
            Type = NotificationType.Booking,
            RelatedTable = "BOOKING",
            RelatedId = bookingId
        };

        await _notificationService.CreateAndPushAsync(notification);
    }

    /// <summary>
    /// Send a payment-related notification
    /// </summary>
    public async Task SendPaymentNotificationAsync(
        int userId,
        int? senderId,
        string title,
        string message,
        int bookingId)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            SenderId = senderId,
            Title = title,
            Message = message,
            Type = NotificationType.Payment,
            RelatedTable = "BOOKING",
            RelatedId = bookingId
        };

        await _notificationService.CreateAndPushAsync(notification);
    }

    /// <summary>
    /// Send a review-related notification
    /// </summary>
    public async Task SendReviewNotificationAsync(
        int userId,
        int? senderId,
        string title,
        string message,
        int reviewId)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            SenderId = senderId,
            Title = title,
            Message = message,
            Type = NotificationType.Review,
            RelatedTable = "REVIEW",
            RelatedId = reviewId
        };

        await _notificationService.CreateAndPushAsync(notification);
    }

    /// <summary>
    /// Send a system notification
    /// </summary>
    public async Task SendSystemNotificationAsync(
        int userId,
        string title,
        string message,
        string? relatedTable = null,
        int? relatedId = null)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            SenderId = null,
            Title = title,
            Message = message,
            Type = NotificationType.System,
            RelatedTable = relatedTable,
            RelatedId = relatedId
        };

        await _notificationService.CreateAndPushAsync(notification);
    }

    /// <summary>
    /// Send a generic notification
    /// </summary>
    public async Task SendNotificationAsync(
        int userId,
        int? senderId,
        string title,
        string message,
        NotificationType type = NotificationType.Other,
        string? relatedTable = null,
        int? relatedId = null)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            SenderId = senderId,
            Title = title,
            Message = message,
            Type = type,
            RelatedTable = relatedTable,
            RelatedId = relatedId
        };

        await _notificationService.CreateAndPushAsync(notification);
    }
}
