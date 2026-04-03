using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Exceptions;
using PrintFlow.Domain.Enums;

namespace PrintFlow.UnitTests.Unit.Helpers;

public class ExceptionTests
{
    [Fact]
    public void NotFoundException_HasCorrectStatusAndMessage()
    {
        var ex = new NotFoundException("Product", Guid.NewGuid());

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Product", ex.Message);
        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public void BadRequestException_SingleMessage()
    {
        var ex = new BadRequestException("Something went wrong");

        Assert.Equal(400, ex.StatusCode);
        Assert.Single(ex.Errors);
        Assert.Equal("Something went wrong", ex.Errors[0]);
    }

    [Fact]
    public void BadRequestException_MultipleErrors()
    {
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var ex = new BadRequestException(errors);

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal(3, ex.Errors.Count);
        Assert.Equal("Error 1", ex.Message);
    }

    [Fact]
    public void UnauthorizedException_DefaultMessage()
    {
        var ex = new UnauthorizedException();

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("You are not authenticated.", ex.Message);
    }

    [Fact]
    public void UnauthorizedException_CustomMessage()
    {
        var ex = new UnauthorizedException("Token expired");

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Token expired", ex.Message);
    }

    [Fact]
    public void ForbiddenException_DefaultMessage()
    {
        var ex = new ForbiddenException();

        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("permission", ex.Message);
    }

    [Fact]
    public void ConflictException_HasCorrectMessage()
    {
        var ex = new ConflictException("Product", "slug", "business-cards");

        Assert.Equal(409, ex.StatusCode);
        Assert.Contains("Product", ex.Message);
        Assert.Contains("slug", ex.Message);
        Assert.Contains("business-cards", ex.Message);
    }

    [Fact]
    public void PaymentException_WithStripeCode()
    {
        var ex = new PaymentException("Card declined", "card_declined");

        Assert.Equal(402, ex.StatusCode);
        Assert.Equal("card_declined", ex.StripeErrorCode);
    }

    [Fact]
    public void PaymentException_WithoutStripeCode()
    {
        var ex = new PaymentException("Payment failed");

        Assert.Equal(402, ex.StatusCode);
        Assert.Null(ex.StripeErrorCode);
    }

    [Fact]
    public void InvalidOrderStateException_HasStatusInfo()
    {
        var ex = new InvalidOrderStateException(OrderStatus.Completed, OrderStatus.InProduction);

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal(OrderStatus.Completed, ex.CurrentStatus);
        Assert.Equal(OrderStatus.InProduction, ex.AttemptedStatus);
        Assert.Contains("Completed", ex.Message);
        Assert.Contains("InProduction", ex.Message);
    }
}

public class ApiResultTests
{
    [Fact]
    public void Ok_ReturnsSuccess()
    {
        var result = ApiResult<string>.Ok("data", "message");

        Assert.True(result.Success);
        Assert.Equal("data", result.Data);
        Assert.Equal("message", result.Message);
    }

    [Fact]
    public void Fail_String_ReturnsFailed()
    {
        var result = ApiResult<string>.Fail("error");

        Assert.False(result.Success);
        Assert.Equal("error", result.Message);
    }

    [Fact]
    public void Fail_List_ReturnsFailed()
    {
        var errors = new List<string> { "error1", "error2" };
        var result = ApiResult<string>.Fail(errors);

        Assert.False(result.Success);
        Assert.Equal(2, result.Errors!.Count);
        Assert.Equal("error1", result.Message);
    }
}

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var paged = new PagedResponse<string>
        {
            Items = new List<string>(),
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25
        };

        Assert.Equal(3, paged.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_FirstPage_ReturnsFalse()
    {
        var paged = new PagedResponse<string> { PageNumber = 1, PageSize = 10, TotalCount = 50 };
        Assert.False(paged.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_SecondPage_ReturnsTrue()
    {
        var paged = new PagedResponse<string> { PageNumber = 2, PageSize = 10, TotalCount = 50 };
        Assert.True(paged.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_LastPage_ReturnsFalse()
    {
        var paged = new PagedResponse<string> { PageNumber = 5, PageSize = 10, TotalCount = 50 };
        Assert.False(paged.HasNextPage);
    }

    [Fact]
    public void HasNextPage_MiddlePage_ReturnsTrue()
    {
        var paged = new PagedResponse<string> { PageNumber = 2, PageSize = 10, TotalCount = 50 };
        Assert.True(paged.HasNextPage);
    }
}