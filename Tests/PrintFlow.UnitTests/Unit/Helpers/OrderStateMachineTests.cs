using PrintFlow.Application.Services;
using PrintFlow.Domain.Enums;

namespace PrintFlow.UnitTests.Unit.Helpers;

public class OrderStateMachineTests
{
    // ── Valid transitions ──

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.Created, OrderStatus.Paid)]
    [InlineData(OrderStatus.Created, OrderStatus.PaymentFailed)]
    [InlineData(OrderStatus.Created, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.PaymentFailed, OrderStatus.Paid)]
    [InlineData(OrderStatus.PaymentFailed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.AwaitingPayment, OrderStatus.Paid)]
    [InlineData(OrderStatus.AwaitingPayment, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Paid, OrderStatus.InProduction)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.InProduction, OrderStatus.QualityCheck)]
    [InlineData(OrderStatus.InProduction, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.QualityCheck, OrderStatus.ReadyForPickup)]
    [InlineData(OrderStatus.QualityCheck, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.Completed)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.Cancelled)]
    public void CanTransition_ValidTransition_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        Assert.True(OrderStateMachine.CanTransition(from, to));
    }

    // ── Invalid transitions ──

    [Theory]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed, OrderStatus.Paid)]
    [InlineData(OrderStatus.Completed, OrderStatus.InProduction)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Paid)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.InProduction)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Completed)]
    [InlineData(OrderStatus.Created, OrderStatus.InProduction)]
    [InlineData(OrderStatus.Created, OrderStatus.Completed)]
    [InlineData(OrderStatus.Paid, OrderStatus.Created)]
    [InlineData(OrderStatus.Paid, OrderStatus.AwaitingPayment)]
    [InlineData(OrderStatus.InProduction, OrderStatus.Paid)]
    [InlineData(OrderStatus.QualityCheck, OrderStatus.InProduction)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.QualityCheck)]
    [InlineData(OrderStatus.PaymentFailed, OrderStatus.InProduction)]
    public void CanTransition_InvalidTransition_ReturnsFalse(OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStateMachine.CanTransition(from, to));
    }

    [Fact]
    public void Completed_IsTerminalState()
    {
        var allStatuses = Enum.GetValues<OrderStatus>();
        foreach (var status in allStatuses)
        {
            Assert.False(OrderStateMachine.CanTransition(OrderStatus.Completed, status));
        }
    }

    [Fact]
    public void Cancelled_IsTerminalState()
    {
        var allStatuses = Enum.GetValues<OrderStatus>();
        foreach (var status in allStatuses)
        {
            Assert.False(OrderStateMachine.CanTransition(OrderStatus.Cancelled, status));
        }
    }

    [Fact]
    public void EveryNonTerminalStatus_CanBeCancelled()
    {
        var nonTerminal = Enum.GetValues<OrderStatus>()
            .Where(s => s != OrderStatus.Completed && s != OrderStatus.Cancelled);

        foreach (var status in nonTerminal)
        {
            Assert.True(OrderStateMachine.CanTransition(status, OrderStatus.Cancelled),
                $"{status} should be cancellable.");
        }
    }
}