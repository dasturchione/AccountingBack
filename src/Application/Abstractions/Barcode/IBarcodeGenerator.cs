using SharedKernel.Results;

namespace Application.Abstractions.Barcode;

public enum BarcodeFormat
{
    QrCode = 0,
    Code128 = 1,
    Ean13 = 2
}

public interface IBarcodeGenerator
{
    /// <summary>QR kod generatsiyasi — PNG bayt massivi qaytaradi.</summary>
    Result<byte[]> GenerateQr(string content, int pixelsPerModule = 20);

    /// <summary>Chiziqli barcode (Code128 / EAN13) — PNG bayt massivi qaytaradi.</summary>
    Result<byte[]> GenerateBarcode(string content, BarcodeFormat format = BarcodeFormat.Code128);
}
