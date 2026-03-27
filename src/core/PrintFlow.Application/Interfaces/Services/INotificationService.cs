using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Notifications;

namespace PrintFlow.Application.Interfaces.Services;

public interface INotificationService
{
    Task<ApiResult<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId);
    Task<ApiResult<bool>> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<ApiResult<UnreadCountDto>> GetUnreadCountAsync(Guid userId);
}