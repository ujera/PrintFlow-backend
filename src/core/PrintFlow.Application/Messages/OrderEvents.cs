namespace PrintFlow.Application.Messages;

public record OrderCreatedEvent(Guid OrderId);

public record OrderStatusChangedEvent(Guid OrderId, string OldStatus, string NewStatus);

public record PaymentSucceededEvent(Guid OrderId);

public record PaymentFailedEvent(Guid OrderId);

public record OrderCompletedEvent(Guid OrderId);

public record SendPaymentReminderEvent(Guid OrderId);