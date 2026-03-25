using FluentValidation;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Validators.Orders;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(value => Enum.TryParse<PaymentMethod>(value, true, out _))
            .WithMessage("Payment method must be either 'Card' or 'BankTransfer'.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly HashSet<string> ValidStatuses = Enum.GetNames<OrderStatus>().ToHashSet();

    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(value => ValidStatuses.Contains(value))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}