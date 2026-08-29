using Application.Abstractions.Barcode;
using Application.Abstractions.Authentication;
using Application.Features.Barcode;
using Microsoft.Extensions.Logging;
using QRCoder;
using SharedKernel.Results;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using ZXing.Common;

namespace Infrastructure.Services.Barcode;

/// <summary>
/// Lokal barcode/QR generatori. Tashqi API ishlatmaydi (HttpClient yo'q), holatsiz.
/// QR uchun QRCoder (o'z PNG encoder'i bilan), chiziqli barcode uchun ZXing.Net
/// (pixel data) + ichki minimal PNG encoder (qo'shimcha/litsenziyali paketsiz).
/// </summary>
public sealed class BarcodeGenerator : IBarcodeGenerator
{
    private readonly ILogger<BarcodeGenerator> _logger;
    private readonly IUserContext _userContext;

    public BarcodeGenerator(ILogger<BarcodeGenerator> logger, IUserContext userContext)
    {
        _logger = logger;
        _userContext = userContext;
    }

    public Result<byte[]> GenerateQr(string content, int pixelsPerModule = 20)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<byte[]>(BarcodeErrors.EmptyContent(_userContext.LanguageId));

        var ppm = pixelsPerModule < 1 ? 1 : pixelsPerModule;

        try
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            return Result.Success(qr.GetGraphic(ppm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR generatsiyasi muvaffaqiyatsiz");
            return Result.Failure<byte[]>(BarcodeErrors.GenerationFailed(_userContext.LanguageId));
        }
    }

    public Result<byte[]> GenerateBarcode(string content, BarcodeFormat format = BarcodeFormat.Code128)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<byte[]>(BarcodeErrors.EmptyContent(_userContext.LanguageId));

        ZXing.BarcodeFormat zxingFormat;
        switch (format)
        {
            case BarcodeFormat.Code128:
                zxingFormat = ZXing.BarcodeFormat.CODE_128;
                break;
            case BarcodeFormat.Ean13:
                if (!IsValidEan13(content))
                    return Result.Failure<byte[]>(BarcodeErrors.InvalidEan13(_userContext.LanguageId));
                zxingFormat = ZXing.BarcodeFormat.EAN_13;
                break;
            case BarcodeFormat.QrCode:
                return Result.Failure<byte[]>(BarcodeErrors.UseQrMethod(_userContext.LanguageId));
            default:
                return Result.Failure<byte[]>(BarcodeErrors.UnsupportedFormat(format.ToString(), _userContext.LanguageId));
        }

        try
        {
            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = zxingFormat,
                Options = new EncodingOptions
                {
                    Width = 400,
                    Height = 150,
                    Margin = 4
                }
            };

            var pixelData = writer.Write(content);
            return Result.Success(EncodePng(pixelData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Barcode generatsiyasi muvaffaqiyatsiz (format: {Format})", format);
            return Result.Failure<byte[]>(BarcodeErrors.GenerationFailed(_userContext.LanguageId));
        }
    }

    /// <summary>
    /// ZXing BGRA pixel data'sini PNG (color type 6 = RGBA, 8-bit) formatiga o'giradi.
    /// Tashqi grafik kutubxona ishlatilmaydi — zlib IDAT uchun System.IO.Compression.ZLibStream.
    /// </summary>
    private static byte[] EncodePng(ZXing.Rendering.PixelData pixelData)
    {
        var width = pixelData.Width;
        var height = pixelData.Height;
        var bgra = pixelData.Pixels;

        // Har qatorga filter bayti (0 = None) + RGBA piksellar.
        var raw = new byte[height * (1 + width * 4)];
        var pos = 0;
        for (var y = 0; y < height; y++)
        {
            raw[pos++] = 0;
            var rowStart = y * width * 4;
            for (var x = 0; x < width; x++)
            {
                var i = rowStart + x * 4;
                raw[pos++] = bgra[i + 2]; // R
                raw[pos++] = bgra[i + 1]; // G
                raw[pos++] = bgra[i];     // B
                raw[pos++] = bgra[i + 3]; // A
            }
        }

        byte[] idat;
        using (var compressed = new MemoryStream())
        {
            using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(raw, 0, raw.Length);
            idat = compressed.ToArray();
        }

        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0, 4), (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4, 4), (uint)height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 6;  // color type: RGBA
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace

        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]); // PNG signature
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", idat);
        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        stream.Write(length);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(crcInput));
        stream.Write(crc);
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFFu;
    }

    private static bool IsValidEan13(string content)
    {
        if (content.Length is not (12 or 13))
            return false;

        foreach (var ch in content)
        {
            if (!char.IsDigit(ch))
                return false;
        }

        return true;
    }
}
