namespace PrintFlow.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid orderId);
}