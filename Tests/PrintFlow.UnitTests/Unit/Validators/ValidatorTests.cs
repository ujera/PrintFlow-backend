using FluentValidation.TestHelper;
using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Validators.Auth;
using PrintFlow.Application.Validators.Cart;
using PrintFlow.Application.Validators.Catalog;
using PrintFlow.Application.Validators.Orders;

namespace PrintFlow.UnitTests.Unit.Validators;

public class AuthValidatorTests
{
    private readonly AdminLoginRequestValidator _loginValidator = new();

    [Fact]
    public void Login_EmptyEmail_Fails()
    {
        var result = _loginValidator.TestValidate(new AdminLoginRequest { Email = "", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_InvalidEmail_Fails()
    {
        var result = _loginValidator.TestValidate(new AdminLoginRequest { Email = "notanemail", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_EmptyPassword_Fails()
    {
        var result = _loginValidator.TestValidate(new AdminLoginRequest { Email = "test@test.com", Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_ValidRequest_Passes()
    {
        var result = _loginValidator.TestValidate(new AdminLoginRequest { Email = "test@test.com", Password = "pass" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CartValidatorTests
{
    private readonly AddCartItemRequestValidator _addValidator = new();
    private readonly UpdateCartItemRequestValidator _updateValidator = new();

    [Fact]
    public void AddItem_EmptyProductId_Fails()
    {
        var result = _addValidator.TestValidate(new AddCartItemRequest { ProductId = Guid.Empty, Quantity = 1 });
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void AddItem_ZeroQuantity_Fails()
    {
        var result = _addValidator.TestValidate(new AddCartItemRequest { ProductId = Guid.NewGuid(), Quantity = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void AddItem_ValidRequest_Passes()
    {
        var result = _addValidator.TestValidate(new AddCartItemRequest { ProductId = Guid.NewGuid(), Quantity = 5 });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateItem_ZeroQuantity_Fails()
    {
        var result = _updateValidator.TestValidate(new UpdateCartItemRequest { Quantity = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
}

public class CatalogValidatorTests
{
    private readonly CreateCategoryRequestValidator _categoryValidator = new();
    private readonly CreateProductRequestValidator _productValidator = new();
    private readonly CreatePricingTierRequestValidator _tierValidator = new();

    [Fact]
    public void CreateCategory_EmptyName_Fails()
    {
        var result = _categoryValidator.TestValidate(new CreateCategoryRequest { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateCategory_NameTooLong_Fails()
    {
        var result = _categoryValidator.TestValidate(new CreateCategoryRequest { Name = new string('x', 151) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateProduct_ZeroBasePrice_Fails()
    {
        var result = _productValidator.TestValidate(new CreateProductRequest
        {
            CategoryId = Guid.NewGuid(),
            Name = "Test",
            BasePrice = 0
        });
        result.ShouldHaveValidationErrorFor(x => x.BasePrice);
    }

    [Fact]
    public void CreateProduct_EmptyCategoryId_Fails()
    {
        var result = _productValidator.TestValidate(new CreateProductRequest
        {
            CategoryId = Guid.Empty,
            Name = "Test",
            BasePrice = 10
        });
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void PricingTier_MaxLessThanMin_Fails()
    {
        var result = _tierValidator.TestValidate(new CreatePricingTierRequest
        {
            MinQuantity = 500,
            MaxQuantity = 100,
            UnitPrice = 1
        });
        result.ShouldHaveValidationErrorFor(x => x.MaxQuantity);
    }

    [Fact]
    public void PricingTier_ValidTier_Passes()
    {
        var result = _tierValidator.TestValidate(new CreatePricingTierRequest
        {
            MinQuantity = 100,
            MaxQuantity = 500,
            UnitPrice = 0.10m
        });
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class OrderValidatorTests
{
    private readonly CreateOrderRequestValidator _createValidator = new();
    private readonly UpdateOrderStatusRequestValidator _statusValidator = new();

    [Fact]
    public void CreateOrder_EmptyPaymentMethod_Fails()
    {
        var result = _createValidator.TestValidate(new CreateOrderRequest { PaymentMethod = "" });
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    }

    [Fact]
    public void CreateOrder_InvalidPaymentMethod_Fails()
    {
        var result = _createValidator.TestValidate(new CreateOrderRequest { PaymentMethod = "Bitcoin" });
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    }

    [Theory]
    [InlineData("Card")]
    [InlineData("BankTransfer")]
    public void CreateOrder_ValidPaymentMethod_Passes(string method)
    {
        var result = _createValidator.TestValidate(new CreateOrderRequest { PaymentMethod = method });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateStatus_InvalidStatus_Fails()
    {
        var result = _statusValidator.TestValidate(new UpdateOrderStatusRequest { Status = "Dancing" });
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData("Paid")]
    [InlineData("InProduction")]
    [InlineData("QualityCheck")]
    [InlineData("ReadyForPickup")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void UpdateStatus_ValidStatus_Passes(string status)
    {
        var result = _statusValidator.TestValidate(new UpdateOrderStatusRequest { Status = status });
        result.ShouldNotHaveAnyValidationErrors();
    }
}