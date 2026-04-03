using AutoMapper;
using Moq;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.UnitTests.Unit.Services;

public class NotificationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly NotificationService _service;

    private readonly Guid _userId = Guid.NewGuid();

    public NotificationServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<NotificationProfile>());
        _mapper = config.CreateMapper();
        _service = new NotificationService(_unitOfWork.Object, _mapper);
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsUserNotifications()
    {
        var notifications = new List<Notification>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, Type = NotificationType.Email, Subject = "Order Confirmed", Body = "Your order is confirmed", Status = NotificationStatus.Sent },
            new() { Id = Guid.NewGuid(), UserId = _userId, Type = NotificationType.Email, Subject = "Status Update", Body = "In production", Status = NotificationStatus.Sent }
        };

        _unitOfWork.Setup(u => u.Notifications.GetByUserIdAsync(_userId, default))
            .ReturnsAsync(notifications);

        var result = await _service.GetMyNotificationsAsync(_userId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetMyNotifications_Empty_ReturnsEmptyList()
    {
        _unitOfWork.Setup(u => u.Notifications.GetByUserIdAsync(_userId, default))
            .ReturnsAsync(new List<Notification>());

        var result = await _service.GetMyNotificationsAsync(_userId);

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task MarkAsRead_OwnNotification_Succeeds()
    {
        var notifId = Guid.NewGuid();
        var notification = new Notification { Id = notifId, UserId = _userId, IsRead = false };

        _unitOfWork.Setup(u => u.Notifications.GetByIdAsync(notifId, default)).ReturnsAsync(notification);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.MarkAsReadAsync(_userId, notifId);

        Assert.True(result.Success);
        Assert.True(notification.IsRead);
    }

    [Fact]
    public async Task MarkAsRead_NotFound_ThrowsNotFoundException()
    {
        var notifId = Guid.NewGuid();
        _unitOfWork.Setup(u => u.Notifications.GetByIdAsync(notifId, default))
            .ReturnsAsync((Notification?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.MarkAsReadAsync(_userId, notifId));
    }

    [Fact]
    public async Task MarkAsRead_DifferentUser_ThrowsForbidden()
    {
        var notifId = Guid.NewGuid();
        var notification = new Notification { Id = notifId, UserId = Guid.NewGuid() };

        _unitOfWork.Setup(u => u.Notifications.GetByIdAsync(notifId, default)).ReturnsAsync(notification);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.MarkAsReadAsync(_userId, notifId));
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCount()
    {
        _unitOfWork.Setup(u => u.Notifications.CountUnreadAsync(_userId, default)).ReturnsAsync(5);

        var result = await _service.GetUnreadCountAsync(_userId);

        Assert.True(result.Success);
        Assert.Equal(5, result.Data!.Count);
    }

    [Fact]
    public async Task GetUnreadCount_NoUnread_ReturnsZero()
    {
        _unitOfWork.Setup(u => u.Notifications.CountUnreadAsync(_userId, default)).ReturnsAsync(0);

        var result = await _service.GetUnreadCountAsync(_userId);

        Assert.Equal(0, result.Data!.Count);
    }
}