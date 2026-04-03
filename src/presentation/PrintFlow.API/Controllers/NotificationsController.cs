using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Notifications;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// User notifications — email and in-app notification history
/// </summary>
[Authorize]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// List all notifications for the current user
    /// </summary>
    /// <returns>Notifications sorted by newest first</returns>
    /// <response code="200">List of notifications</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<NotificationDto>>), 200)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _notificationService.GetMyNotificationsAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    /// <returns>Number of unread notifications</returns>
    /// <response code="200">Unread count</response>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResult<UnreadCountDto>), 200)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notificationService.GetUnreadCountAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Marked as read</response>
    /// <response code="403">Notification belongs to another user</response>
    /// <response code="404">Notification not found</response>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(GetUserId(), id);
        return Ok(result);
    }
}