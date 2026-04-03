using Microsoft.Extensions.Logging;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace PrintFlow.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IUnitOfWork unitOfWork, ILogger<InvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId)
                ?? throw new Exception($"Order {orderId} not found.");

            QuestPDF.Settings.License = LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("PrintFlow").Bold().FontSize(24).FontColor("#1E3A5F");
                                left.Item().Text("Custom Print Shop").FontSize(10).FontColor("#666666");
                            });

                            row.RelativeItem().AlignRight().Column(right =>
                            {
                                right.Item().Text("INVOICE").Bold().FontSize(20).FontColor("#1E3A5F");
                                right.Item().Text($"#{order.Id.ToString()[..8].ToUpper()}").FontSize(12);
                                right.Item().Text($"Date: {order.CreatedAt:yyyy-MM-dd}").FontSize(10);
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#CCCCCC");
                    });

                    // Content
                    page.Content().Column(col =>
                    {
                        // Customer info
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Bill To:").Bold().FontSize(11);
                                c.Item().Text(order.User?.Name ?? "N/A");
                                c.Item().Text(order.User?.Email ?? "N/A");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text($"Status: {order.Status}").Bold();
                                c.Item().Text($"Payment: {order.PaymentMethod}");
                                c.Item().Text($"Payment Status: {order.PaymentStatus}");
                            });
                        });

                        // Items table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Product
                                columns.RelativeColumn(1); // Qty
                                columns.RelativeColumn(1); // Unit Price
                                columns.RelativeColumn(1); // Subtotal
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Background("#1E3A5F").Padding(6)
                                    .Text("Product").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E3A5F").Padding(6)
                                    .Text("Qty").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E3A5F").Padding(6)
                                    .Text("Unit Price").FontColor("#FFFFFF").Bold().AlignRight();
                                header.Cell().Background("#1E3A5F").Padding(6)
                                    .Text("Subtotal").FontColor("#FFFFFF").Bold().AlignRight();
                            });

                            // Item rows
                            var alternate = false;
                            foreach (var item in order.Items)
                            {
                                var bg = alternate ? "#F5F5F5" : "#FFFFFF";

                                table.Cell().Background(bg).Padding(6)
                                    .Text(item.Product?.Name ?? "Product");
                                table.Cell().Background(bg).Padding(6)
                                    .Text(item.Quantity.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text($"${item.UnitPrice:F2}").AlignRight();
                                table.Cell().Background(bg).Padding(6)
                                    .Text($"${item.Subtotal:F2}").AlignRight();

                                alternate = !alternate;
                            }
                        });

                        // Total
                        col.Item().PaddingTop(10).AlignRight().Row(row =>
                        {
                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().BorderTop(1).BorderColor("#1E3A5F").PaddingTop(8)
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text("Total:").Bold().FontSize(14);
                                        r.RelativeItem().AlignRight()
                                            .Text($"${order.TotalAmount:F2}").Bold().FontSize(14).FontColor("#1E3A5F");
                                    });
                            });
                        });

                        // Notes
                        if (!string.IsNullOrWhiteSpace(order.Notes))
                        {
                            col.Item().PaddingTop(20).Column(c =>
                            {
                                c.Item().Text("Notes:").Bold();
                                c.Item().Text(order.Notes);
                            });
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("PrintFlow — Custom Print Shop | ");
                        text.Span("Thank you for your business!").Italic();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            _logger.LogInformation("Invoice PDF generated for order {OrderId}", orderId);
            return pdfBytes;
        }
    }
}
