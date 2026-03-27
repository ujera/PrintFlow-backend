using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Services;

public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        [OrderStatus.Created] = new() { OrderStatus.AwaitingPayment, OrderStatus.Paid, OrderStatus.PaymentFailed, OrderStatus.Cancelled },
        [OrderStatus.PaymentFailed] = new() { OrderStatus.Paid, OrderStatus.Cancelled },
        [OrderStatus.AwaitingPayment] = new() { OrderStatus.Paid, OrderStatus.Cancelled },
        [OrderStatus.Paid] = new() { OrderStatus.InProduction, OrderStatus.Cancelled },
        [OrderStatus.InProduction] = new() { OrderStatus.QualityCheck, OrderStatus.Cancelled },
        [OrderStatus.QualityCheck] = new() { OrderStatus.ReadyForPickup, OrderStatus.Cancelled },
        [OrderStatus.ReadyForPickup] = new() { OrderStatus.Completed, OrderStatus.Cancelled },
        [OrderStatus.Completed] = new(),
        [OrderStatus.Cancelled] = new()
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}