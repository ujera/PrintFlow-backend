using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Exceptions;

public class InvalidOrderStateException : BadRequestException
{
    public OrderStatus CurrentStatus { get; }
    public OrderStatus AttemptedStatus { get; }

    public InvalidOrderStateException(OrderStatus currentStatus, OrderStatus attemptedStatus)
        : base($"Cannot transition order from '{currentStatus}' to '{attemptedStatus}'.")
    {
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }
}