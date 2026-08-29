using Application.Abstractions.Authentication;
using Application.Features.PurchaseDocs;
using Application.Features.Reports.Exports;
using Application.Features.SaleDocs;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace Application.Features.Documents;

public sealed class DocumentPdfService(
    IPurchaseDocService purchaseDocService,
    ISaleDocService saleDocService,
    IPdfExporter pdfExporter,
    IUserContext userContext,
    ILogger<DocumentPdfService> logger) : IDocumentPdfService
{
    private static readonly IReadOnlyCollection<ReportExportColumn> Columns =
    [
        new() { Header = "Document", ValueFactory = x => ((DocumentPdfRow)x).DocumentType },
        new() { Header = "Number", ValueFactory = x => ((DocumentPdfRow)x).DocumentNumber },
        new() { Header = "Date", ValueFactory = x => ((DocumentPdfRow)x).DocumentDate },
        new() { Header = "Counterparty", ValueFactory = x => ((DocumentPdfRow)x).Counterparty },
        new() { Header = "Currency", ValueFactory = x => ((DocumentPdfRow)x).Currency },
        new() { Header = "Line", ValueFactory = x => ((DocumentPdfRow)x).LineNumber },
        new() { Header = "Product", ValueFactory = x => ((DocumentPdfRow)x).ProductName },
        new() { Header = "MXIK", ValueFactory = x => ((DocumentPdfRow)x).ProductMxik },
        new() { Header = "Quantity", ValueFactory = x => ((DocumentPdfRow)x).Quantity },
        new() { Header = "Unit price", ValueFactory = x => ((DocumentPdfRow)x).UnitPrice },
        new() { Header = "Amount", ValueFactory = x => ((DocumentPdfRow)x).Amount },
        new() { Header = "VAT", ValueFactory = x => ((DocumentPdfRow)x).VatAmount },
        new() { Header = "Total", ValueFactory = x => ((DocumentPdfRow)x).TotalAmount },
        new() { Header = "Document total", ValueFactory = x => ((DocumentPdfRow)x).DocumentTotal },
        new() { Header = "Document VAT", ValueFactory = x => ((DocumentPdfRow)x).DocumentVatAmount },
        new() { Header = "Document final", ValueFactory = x => ((DocumentPdfRow)x).DocumentFinalAmount },
        new() { Header = "Marking count", ValueFactory = x => ((DocumentPdfRow)x).MarkingCount }
    ];

    public async Task<Result<ReportExportResult>> GetPurchasePdfAsync(long id, CancellationToken ct = default)
    {
        var result = await purchaseDocService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return Result.Failure<ReportExportResult>(result.Error);

        var document = result.Value;
        var rows = document.Lines.Select((line, index) => new DocumentPdfRow
        {
            DocumentType = "Purchase",
            DocumentNumber = document.DocNumber,
            DocumentDate = document.DocDate,
            Counterparty = document.CounterpartyName,
            Currency = document.CurrencyName,
            LineNumber = index + 1,
            ProductName = line.ProductName,
            ProductMxik = line.ProductMxik,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            Amount = line.Amount,
            VatAmount = line.VatAmount,
            TotalAmount = line.TotalAmount,
            DocumentTotal = document.TotalAmount,
            DocumentVatAmount = document.VatAmount,
            DocumentFinalAmount = document.FinalAmount,
            MarkingCount = line.Items.Sum(item => item.MarkingCount)
        }).ToArray();

        return Render("Purchase document", id, rows);
    }

    public async Task<Result<ReportExportResult>> GetSalePdfAsync(long id, CancellationToken ct = default)
    {
        var result = await saleDocService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return Result.Failure<ReportExportResult>(result.Error);

        var document = result.Value;
        var rows = document.Lines.Select((line, index) => new DocumentPdfRow
        {
            DocumentType = "Sale",
            DocumentNumber = document.DocNumber,
            DocumentDate = document.DocDate,
            Counterparty = document.CounterpartyName,
            Currency = document.CurrencyName,
            LineNumber = index + 1,
            ProductName = line.ProductName,
            ProductMxik = line.ProductMxik,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            Amount = line.Amount,
            VatAmount = line.VatAmount,
            TotalAmount = line.TotalAmount,
            DocumentTotal = document.TotalAmount,
            DocumentVatAmount = document.VatAmount,
            DocumentFinalAmount = document.FinalAmount,
            MarkingCount = line.Items.Sum(item => item.MarkingCount)
        }).ToArray();

        return Render("Sale document", id, rows);
    }

    private Result<ReportExportResult> Render(string title, long id, IReadOnlyCollection<DocumentPdfRow> rows)
    {
        try
        {
            return Result.Success(new ReportExportResult
            {
                FileName = $"document-{id}.pdf",
                ContentType = "application/pdf",
                Content = pdfExporter.Export(title, Columns, rows)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Document PDF rendering failed for document {DocumentId}.", id);
            return Result.Failure<ReportExportResult>(DocumentPdfErrors.RenderFailed(userContext.LanguageId));
        }
    }

    private sealed class DocumentPdfRow
    {
        public string DocumentType { get; init; } = string.Empty;
        public string DocumentNumber { get; init; } = string.Empty;
        public DateTime DocumentDate { get; init; }
        public string Counterparty { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public int LineNumber { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string? ProductMxik { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Amount { get; init; }
        public decimal VatAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal DocumentTotal { get; init; }
        public decimal DocumentVatAmount { get; init; }
        public decimal DocumentFinalAmount { get; init; }
        public int MarkingCount { get; init; }
    }
}
