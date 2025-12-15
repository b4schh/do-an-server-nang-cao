# NotificationManagement Module - Complete Documentation

## 📋 Tổng Quan

Module **NotificationManagement** cung cấp hệ thống thông báo real-time cho ứng dụng đặt sân bóng, sử dụng **Server-Sent Events (SSE)** để push notifications ngay lập tức đến client mà không cần polling.

### ✨ Tính Năng Chính

- ✅ **Real-time notifications** qua SSE (Server-Sent Events)
- ✅ **Automatic reconnection** với replay missed notifications
- ✅ **Multiple notification types**: System, Booking, Payment, Review
- ✅ **Cursor-based pagination** cho hiệu năng tốt
- ✅ **Bulk operations**: Mark multiple/all as read
- ✅ **Comprehensive logging** và error handling
- ✅ **Connection monitoring** và auto-cleanup
- ✅ **Security**: JWT authentication, ownership validation

---

## 🏗️ Kiến Trúc

```
NotificationManagement/
├── Controllers/
│   ├── NotificationsController.cs    # REST API cho CRUD operations
│   └── SseController.cs               # SSE streaming endpoint
├── Services/
│   ├── INotificationService.cs        # Service interface
│   └── NotificationService.cs         # Business logic implementation
├── Repositories/
│   ├── ISseRepository.cs              # SSE connection interface
│   └── SseRepository.cs               # In-memory SSE storage
├── Entities/
│   └── Notification.cs                # Database entity
├── Dtos/
│   └── NotificationDto.cs             # Data transfer objects
├── Helpers/
│   └── NotificationHelper.cs          # Helper methods
└── NotificationModule.cs              # DI registration
```

---

## 📊 Database Schema

### Table: NOTIFICATION

```sql
CREATE TABLE NOTIFICATION (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    sender_id INT NULL,
    title NVARCHAR(255) NULL,
    message NVARCHAR(MAX) NULL,
    type TINYINT NOT NULL DEFAULT 0,
    related_table VARCHAR(100) NULL,
    related_id INT NULL,
    is_read BIT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT DATEADD(HOUR, 7, GETUTCDATE()),
    read_at DATETIME NULL,
    
    CONSTRAINT FK_Notification_User FOREIGN KEY (user_id) 
        REFERENCES [USER](id) ON DELETE CASCADE,
    CONSTRAINT FK_Notification_Sender FOREIGN KEY (sender_id) 
        REFERENCES [USER](id) ON DELETE NO ACTION
);

CREATE INDEX IX_Notification_UserId_IsRead ON NOTIFICATION(user_id, is_read);
```

### Enum: NotificationType

```csharp
public enum NotificationType : byte
{
    System = 0,    // Thông báo hệ thống
    Booking = 1,   // Liên quan đến booking
    Payment = 2,   // Liên quan đến thanh toán
    Review = 3,    // Liên quan đến đánh giá
    Other = 4      // Khác
}
```

---

## 🔌 API Endpoints

### 1. REST API - NotificationsController

#### GET /api/notifications
Lấy danh sách notifications của user hiện tại

**Query Parameters:**
- `sinceId` (int, optional): Lấy notifications sau ID này (cursor pagination)
- `limit` (int, optional): Số lượng notifications (default: 50, max: 200)

**Response:**
```json
{
  "success": true,
  "message": "Lấy danh sách thông báo thành công",
  "statusCode": 200,
  "data": {
    "notifications": [
      {
        "id": 123,
        "userId": 456,
        "senderId": 789,
        "senderName": "John Doe",
        "title": "Booking Confirmed",
        "message": "Your booking has been confirmed",
        "type": 1,
        "typeName": "Booking",
        "relatedTable": "BOOKING",
        "relatedId": 999,
        "isRead": false,
        "createdAt": "2025-12-05T10:30:00Z",
        "readAt": null
      }
    ],
    "unreadCount": 5,
    "hasMore": true,
    "lastId": 100
  }
}
```

#### GET /api/notifications/unread-count
Lấy số lượng notifications chưa đọc

**Response:**
```json
{
  "success": true,
  "message": "Lấy số lượng thông báo chưa đọc thành công",
  "statusCode": 200,
  "data": {
    "count": 5
  }
}
```

#### POST /api/notifications/mark-read
Đánh dấu một notification là đã đọc

**Request Body:**
```json
{
  "notificationId": 123
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đánh dấu đã đọc thành công",
  "statusCode": 200,
  "data": null
}
```

#### POST /api/notifications/mark-multiple-read
Đánh dấu nhiều notifications là đã đọc

**Request Body:**
```json
{
  "notificationIds": [123, 124, 125]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đã đánh dấu 3 thông báo là đã đọc",
  "statusCode": 200,
  "data": {
    "markedCount": 3
  }
}
```

#### POST /api/notifications/mark-all-read
Đánh dấu tất cả notifications là đã đọc

**Response:**
```json
{
  "success": true,
  "message": "Đã đánh dấu tất cả 10 thông báo là đã đọc",
  "statusCode": 200,
  "data": {
    "markedCount": 10
  }
}
```

#### DELETE /api/notifications/{id}
Xóa một notification

**Response:**
```json
{
  "success": true,
  "message": "Xóa thông báo thành công",
  "statusCode": 200,
  "data": null
}
```

---

### 2. SSE API - SseController

#### GET /sse/stream
Thiết lập SSE connection để nhận real-time notifications

**Headers:**
- `Authorization: Bearer {token}` (required)
- `Last-Event-ID: {lastNotificationId}` (optional, for reconnection)

**SSE Events:**
```
event: notification
id: 123
data: {"id":123,"userId":456,"title":"New Booking",...}

event: notification
data: {"msg":"connected","at":"2025-12-05T10:30:00","userId":456}
```

**Client Example (JavaScript):**
```javascript
const eventSource = new EventSource('/sse/stream', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

eventSource.addEventListener('notification', (event) => {
  const notification = JSON.parse(event.data);
  console.log('New notification:', notification);
  
  // Update UI
  if (notification.id) {
    showNotificationBadge(notification);
  }
});

eventSource.onerror = (error) => {
  console.error('SSE error:', error);
  // Browser will auto-reconnect with Last-Event-ID
};
```

#### GET /sse/stats
Lấy thống kê SSE connections (Admin only)

**Response:**
```json
{
  "success": true,
  "message": "Lấy thống kê kết nối SSE thành công",
  "statusCode": 200,
  "data": {
    "TotalUsers": 25,
    "TotalConnections": 30
  }
}
```

---

## 💻 Code Usage Examples

### 1. Tạo và Gửi Notification (Trong Service)

```csharp
public class BookingService
{
    private readonly INotificationService _notificationService;
    
    public async Task ConfirmBookingAsync(int bookingId, int userId)
    {
        // ... business logic ...
        
        // Gửi notification
        var notification = new Notification
        {
            UserId = userId,
            SenderId = null, // System notification
            Title = "Booking Confirmed",
            Message = $"Your booking #{bookingId} has been confirmed",
            Type = NotificationType.Booking,
            RelatedTable = "BOOKING",
            RelatedId = bookingId
        };
        
        await _notificationService.CreateAndPushAsync(notification);
    }
}
```

### 2. Sử dụng NotificationHelper (Recommended)

```csharp
public class BookingService
{
    private readonly NotificationHelper _notificationHelper;
    
    public async Task ConfirmBookingAsync(int bookingId, int userId)
    {
        // ... business logic ...
        
        // Gửi notification dễ dàng hơn
        await _notificationHelper.SendBookingNotificationAsync(
            userId: userId,
            senderId: null,
            title: "Booking Confirmed",
            message: $"Your booking #{bookingId} has been confirmed",
            bookingId: bookingId
        );
    }
    
    public async Task ProcessPaymentAsync(int bookingId, int userId)
    {
        // ... business logic ...
        
        await _notificationHelper.SendPaymentNotificationAsync(
            userId: userId,
            senderId: null,
            title: "Payment Successful",
            message: "Your payment has been processed",
            bookingId: bookingId
        );
    }
}
```

### 3. Frontend Integration (React Example)

```typescript
// hooks/useNotifications.ts
import { useEffect, useState } from 'react';

export function useNotifications(token: string) {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    const eventSource = new EventSource('/sse/stream', {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    eventSource.addEventListener('notification', (event) => {
      const data = JSON.parse(event.data);
      
      if (data.id) {
        setNotifications(prev => [data, ...prev]);
        setUnreadCount(prev => prev + 1);
        
        // Show toast notification
        showToast(data.title, data.message);
      }
    });

    return () => eventSource.close();
  }, [token]);

  const markAsRead = async (notificationId: number) => {
    await fetch('/api/notifications/mark-read', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ notificationId })
    });
    
    setUnreadCount(prev => Math.max(0, prev - 1));
  };

  return { notifications, unreadCount, markAsRead };
}
```

---

## ⚠️ Deployment Considerations

### Single-Instance Deployment (Current Implementation)

✅ **Hoạt động hoàn hảo** với:
- Single server/container
- No load balancer
- Vertical scaling only

### Multi-Instance Deployment (Production)

🔴 **KHÔNG HOẠT ĐỘNG** với implementation hiện tại vì:
- SseRepository lưu connections trong memory của mỗi instance
- User connect đến instance A nhưng notification được tạo ở instance B → **KHÔNG NHẬN ĐƯỢC**

**Giải pháp cho Multi-Instance:**

#### Option 1: Redis Pub/Sub (Recommended)

```csharp
// Repositories/RedisSseRepository.cs
public class RedisSseRepository : ISseRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ConcurrentDictionary<int, List<Channel<string>>> _localConnections;
    
    public RedisSseRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _localConnections = new();
        
        // Subscribe to Redis channel
        var subscriber = _redis.GetSubscriber();
        subscriber.Subscribe("notifications:*", (channel, message) =>
        {
            var userId = ExtractUserIdFromChannel(channel);
            PushToLocalConnections(userId, message);
        });
    }
    
    public void PushToUser(int userId, string jsonPayload)
    {
        // Publish to Redis - all instances will receive
        var subscriber = _redis.GetSubscriber();
        subscriber.Publish($"notifications:{userId}", jsonPayload);
    }
    
    private void PushToLocalConnections(int userId, string jsonPayload)
    {
        // Push to local SSE connections only
        if (_localConnections.TryGetValue(userId, out var connections))
        {
            foreach (var conn in connections)
            {
                conn.Writer.TryWrite(jsonPayload);
            }
        }
    }
}
```

**Update NotificationModule.cs:**
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

services.AddSingleton<ISseRepository, RedisSseRepository>();
```

#### Option 2: SignalR (Alternative)

Replace SSE with SignalR Hub - natively supports Redis backplane:

```csharp
services.AddSignalR()
    .AddStackExchangeRedis(configuration.GetConnectionString("Redis"));
```

---

## 🔒 Security Features

### 1. Authentication
- Tất cả endpoints yêu cầu `[Authorize]` attribute
- JWT token validation
- User ID extraction từ Claims

### 2. Authorization
- Users chỉ có thể xem/sửa notifications của chính mình
- Ownership validation ở service layer
- Admin-only endpoints cho monitoring

### 3. Input Validation
- DTO validation cho tất cả requests
- NotificationId > 0 checks
- Limit capping (max 200)

---

## 📈 Performance Optimizations

### 1. Database Indexing
```sql
CREATE INDEX IX_Notification_UserId_IsRead 
ON NOTIFICATION(user_id, is_read);
```
- Tối ưu query `WHERE user_id = ? AND is_read = 0`
- Tăng tốc GetUnreadCount

### 2. Cursor Pagination
- Sử dụng `sinceId` thay vì offset
- Hiệu quả hơn với dataset lớn
- Tránh "page drift" problem

### 3. Connection Cleanup
- Auto cleanup mỗi 5 phút
- Remove stale connections (inactive > 30 min)
- Memory leak prevention

### 4. Best-Effort Push
- `TryWrite` thay vì `WriteAsync`
- Non-blocking operations
- Graceful handling of dead connections

---

## 🐛 Error Handling

### ExceptionMiddleware Integration

NotificationsController sử dụng **throw exceptions** thay vì manual error handling. `ExceptionMiddleware` tự động xử lý và trả về responses nhất quán:

**Validation Errors (400 Bad Request):**
```csharp
if (dto.NotificationId <= 0)
    throw new ArgumentException("Invalid notification ID");
```
Response:
```json
{
  "success": false,
  "message": "Invalid notification ID",
  "statusCode": 400,
  "data": null
}
```

**Authorization Errors (403 Forbidden):**
```csharp
if (userIdClaim == null)
    throw new UnauthorizedAccessException("Invalid user ID in token");
```
Response:
```json
{
  "success": false,
  "message": "Invalid user ID in token",
  "statusCode": 403,
  "data": null
}
```

**Not Found Errors (404):**
```csharp
if (!success)
    throw new KeyNotFoundException("Notification not found");
```
Response:
```json
{
  "success": false,
  "message": "Notification not found",
  "statusCode": 404,
  "data": null
}
```

**Database Errors (500):**
Automatically handled by middleware:
```json
{
  "success": false,
  "message": "Lỗi khi thao tác với cơ sở dữ liệu",
  "statusCode": 500,
  "data": null
}
```

### Service Layer (Vẫn giữ try-catch cho logging)
```csharp
try
{
    // Business logic
    await _db.SaveChangesAsync();
    _sseRepo.PushToUser(userId, json);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Failed to create notification");
    throw; // Re-throw để middleware xử lý
}
```

---

## 📊 Logging Strategy

### Log Levels

**Information:**
- SSE connection established/closed
- Notifications created and sent
- Bulk operations completed

**Warning:**
- Invalid tokens
- Notifications not found
- Dead connections removed

**Error:**
- Database errors
- Unexpected exceptions
- SSE stream failures

**Debug:**
- Push success/failure details
- Replay missed notifications count

### Log Examples

```
[INFO] SSE stream established for user 123
[INFO] Notification 456 created and pushed to user 123
[INFO] Marked 5 notifications as read for user 123
[WARN] Removed 2 dead SSE connections for user 123
[ERROR] Failed to create notification for user 123: DbUpdateException
```

---

## 🧪 Testing Guide

### Unit Tests

```csharp
[Fact]
public async Task CreateAndPushAsync_ShouldSaveAndPush()
{
    // Arrange
    var notification = new Notification { UserId = 1, Message = "Test" };
    var mockRepo = new Mock<ISseRepository>();
    var service = new NotificationService(dbContext, mockRepo.Object, logger);
    
    // Act
    await service.CreateAndPushAsync(notification);
    
    // Assert
    Assert.NotEqual(0, notification.Id);
    mockRepo.Verify(r => r.PushToUser(1, It.IsAny<string>()), Times.Once);
}
```

### Integration Tests

```csharp
[Fact]
public async Task SseStream_ShouldReceiveNotifications()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", validToken);
    
    // Act & Assert
    using var response = await client.GetAsync("/sse/stream", 
        HttpCompletionOption.ResponseHeadersRead);
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("text/event-stream", 
        response.Content.Headers.ContentType?.MediaType);
}
```

---

## 🚀 Migration Steps (If Needed)

### From In-Memory to Redis

1. **Install Redis Client:**
```bash
dotnet add package StackExchange.Redis
```

2. **Create RedisSseRepository** (see code above)

3. **Update appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

4. **Update DI Registration:**
```csharp
services.AddSingleton<ISseRepository, RedisSseRepository>();
```

5. **Test thoroughly** before production deployment

---

## 📝 Best Practices

### DO ✅

- Luôn sử dụng `NotificationHelper` cho consistency
- Log mọi notification operations
- Validate ownership trước khi modify
- Handle SSE disconnections gracefully
- Use cursor pagination cho lists
- Set appropriate limit caps
- Monitor connection stats

### DON'T ❌

- Không hardcode user IDs
- Không skip authorization checks
- Không expose sensitive data trong messages
- Không quên dispose connections
- Không deploy multi-instance với in-memory storage
- Không skip error handling

---

## 🔄 Future Enhancements

### Planned Features

1. **Notification Preferences**
   - User settings để bật/tắt từng loại notification
   - Table: `USER_NOTIFICATION_PREFERENCES`

2. **Push Notifications**
   - Firebase Cloud Messaging integration
   - Apple Push Notification Service

3. **Email Notifications**
   - Email digest cho unread notifications
   - Configurable frequency

4. **WebSocket Support**
   - Alternative to SSE cho two-way communication

5. **Notification Templates**
   - Template engine cho messages
   - Localization support

---

## 🤝 Integration Examples

### Booking Module Integration

```csharp
// BookingService.cs
public async Task CreateBookingAsync(CreateBookingDto dto, int userId)
{
    var booking = MapToEntity(dto);
    await _bookingRepository.AddAsync(booking);
    
    // Notify customer
    await _notificationHelper.SendBookingNotificationAsync(
        userId: userId,
        senderId: null,
        title: "Booking Created",
        message: $"Booking #{booking.Id} created successfully",
        bookingId: booking.Id
    );
    
    // Notify owner
    var owner = await _complexRepository.GetOwnerAsync(booking.ComplexId);
    await _notificationHelper.SendBookingNotificationAsync(
        userId: owner.Id,
        senderId: userId,
        title: "New Booking Request",
        message: $"New booking from {customerName}",
        bookingId: booking.Id
    );
}
```

### Payment Module Integration

```csharp
// PaymentService.cs
public async Task ProcessPaymentAsync(int bookingId, PaymentDto dto)
{
    var booking = await _bookingRepository.GetByIdAsync(bookingId);
    
    // Process payment...
    
    await _notificationHelper.SendPaymentNotificationAsync(
        userId: booking.UserId,
        senderId: null,
        title: "Payment Successful",
        message: $"Payment of {booking.TotalAmount:C} confirmed",
        bookingId: bookingId
    );
}
```

---

## 📞 Support & Troubleshooting

### Common Issues

**1. SSE không nhận được notifications**
- ✅ Check JWT token còn valid
- ✅ Check network/firewall cho SSE connections
- ✅ Check browser console cho errors
- ✅ Verify user ID trong token

**2. Memory leak**
- ✅ Ensure connections được dispose properly
- ✅ Check cleanup timer đang chạy
- ✅ Monitor connection stats

**3. Notifications delay**
- ✅ Check database performance
- ✅ Monitor SSE push operations
- ✅ Verify no network throttling

---

## 📚 References

- [Server-Sent Events Specification](https://html.spec.whatwg.org/multipage/server-sent-events.html)
- [ASP.NET Core Real-time](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

## 📄 License & Credits

Module này được phát triển cho hệ thống đặt sân bóng.

**Author:** Football Field Booking Team  
**Last Updated:** December 5, 2025  
**Version:** 2.0.0

---

## ✅ Checklist for Production

- [ ] Update to Redis-based SSE repository
- [ ] Setup monitoring và alerts
- [ ] Configure log retention policies
- [ ] Load test với concurrent connections
- [ ] Setup backup cho notification data
- [ ] Document runbook cho incidents
- [ ] Train team về SSE debugging
- [ ] Setup health checks
