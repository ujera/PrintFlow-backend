using AutoMapper;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Notifications;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResult<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
        return ApiResult<List<NotificationDto>>.Ok(_mapper.Map<List<NotificationDto>>(notifications));
    }

    public async Task<ApiResult<bool>> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new ForbiddenException();

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<bool>.Ok(true, "Notification marked as read.");
    }

    public async Task<ApiResult<UnreadCountDto>> GetUnreadCountAsync(Guid userId)
    {
        var count = await _unitOfWork.Notifications.CountUnreadAsync(userId);
        return ApiResult<UnreadCountDto>.Ok(new UnreadCountDto { Count = count });
    }
}