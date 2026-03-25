using FluentValidation;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Validators.Catalog;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.")
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater.");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.")
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater.");
    }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0.");

        RuleForEach(x => x.Options).SetValidator(new CreateProductOptionRequestValidator());
        RuleForEach(x => x.PricingTiers).SetValidator(new CreatePricingTierRequestValidator());
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0.");

        RuleForEach(x => x.Options).SetValidator(new CreateProductOptionRequestValidator());
        RuleForEach(x => x.PricingTiers).SetValidator(new CreatePricingTierRequestValidator());
    }
}

public class CreateProductOptionRequestValidator : AbstractValidator<CreateProductOptionRequest>
{
    public CreateProductOptionRequestValidator()
    {
        RuleFor(x => x.OptionType)
            .NotEmpty().WithMessage("Option type is required.")
            .Must(value => Enum.TryParse<OptionType>(value, true, out _))
            .WithMessage("Option type must be one of: Material, Size, Finishing.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Option name is required.")
            .MaximumLength(150).WithMessage("Option name cannot exceed 150 characters.");
    }
}

public class CreatePricingTierRequestValidator : AbstractValidator<CreatePricingTierRequest>
{
    public CreatePricingTierRequestValidator()
    {
        RuleFor(x => x.MinQuantity)
            .GreaterThan(0).WithMessage("Minimum quantity must be greater than 0.");

        RuleFor(x => x.MaxQuantity)
            .GreaterThan(0).WithMessage("Maximum quantity must be greater than 0.")
            .GreaterThanOrEqualTo(x => x.MinQuantity)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.");
    }
}