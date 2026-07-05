using Application.Abstractions.Integration;
using Integration.EImzo.Configs;
using Integration.EImzo.Services.Signers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Pkcs;
using SharedKernel.Results;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Integration.EImzo.Services;

internal sealed class EImzoSigner : IEImzoSigner
{
    private readonly EImzoSettings _settings;
    private readonly ILogger<EImzoSigner> _logger;
    private readonly IReadOnlyList<ICertificateSigner> _certificateSigners;

    public EImzoSigner(
        IOptions<EImzoSettings> options,
        ILogger<EImzoSigner> logger,
        IEnumerable<ICertificateSigner> certificateSigners)
    {
        _settings = options.Value;
        _logger = logger;
        _certificateSigners = certificateSigners.ToList();
    }

    public Result<CertificateInfo> ReadCertificateInfo(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var settingsValidationResult = ValidateSettings();
        if (!settingsValidationResult.IsSuccess)
            return Result.Failure<CertificateInfo>(settingsValidationResult.Error);

        // Prefer .NET's native reader first, then fall back to BouncyCastle for broader PKCS#12 compatibility.
        var x509Result = TryReadWithX509(ct);
        if (x509Result.IsSuccess)
            return x509Result;

        var bouncyCastleResult = TryReadWithBouncyCastle(ct);
        if (bouncyCastleResult.IsSuccess)
            return bouncyCastleResult;

        _logger.LogError(
            "E-IMZO certificate could not be read from {CertificatePath}. Native reader failed with {X509Code}; BouncyCastle failed with {BouncyCode}",
            _settings.CertificatePath,
            x509Result.Error.Code,
            bouncyCastleResult.Error.Code);

        return Result.Failure<CertificateInfo>(Error.Problem(
            "EImzo.CertificateReadFailed",
            $"EImzo certificate could not be read. Native reader: {x509Result.Error.Description} BouncyCastle: {bouncyCastleResult.Error.Description}"));
    }

    public Result<byte[]> SignPkcs7(byte[] data, Pkcs7SignMode mode = Pkcs7SignMode.Attached, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (data is null || data.Length == 0)
            return Result.Failure<byte[]>(Error.Problem("EImzo.SignDataMissing", "PKCS#7 signing payload is empty."));

        var certificateInfoResult = ReadCertificateInfo(ct);
        if (!certificateInfoResult.IsSuccess)
            return Result.Failure<byte[]>(certificateInfoResult.Error);

        var certificateContextResult = TryLoadCertificateContext(certificateInfoResult.Value, ct);
        if (!certificateContextResult.IsSuccess)
            return Result.Failure<byte[]>(certificateContextResult.Error);

        var algorithm = EImzoSignatureAlgorithm.Parse(certificateContextResult.Value.CertificateInfo.SignatureAlgorithm);
        var signer = _certificateSigners.FirstOrDefault(x => x.CanSign(algorithm));

        if (signer is null)
            return Result.Failure<byte[]>(Error.Problem("EImzo.UnsupportedAlgorithm", $"Qo'llab-quvvatlanmaydigan algoritm: {algorithm.DisplayValue}"));

        _logger.LogInformation(
            "E-IMZO PKCS#7 signing is using {SignerType} for algorithm {SignatureAlgorithm} in {Mode} mode",
            signer.GetType().Name,
            algorithm.DisplayValue,
            mode);

        return signer.Sign(data, certificateContextResult.Value, mode, ct);
    }

    private Result<CertificateInfo> TryReadWithX509(CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            using var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                _settings.CertificatePath,
                _settings.CertificatePassword,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

            if (_settings.ValidateCertificateChain && !TryValidateChain(certificate, out var chainError))
                return Result.Failure<CertificateInfo>(Error.Problem("EImzo.CertificateChainInvalid", chainError));

            _logger.LogInformation(
                "E-IMZO certificate metadata was read with native X509 loader. Subject: {Subject}, SignatureAlgorithm: {SignatureAlgorithm}",
                certificate.Subject,
                BuildSignatureAlgorithm(certificate.SignatureAlgorithm?.FriendlyName, certificate.SignatureAlgorithm?.Value));

            return Result.Success(MapFromX509(certificate));
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "Native X509 reader could not open E-IMZO certificate at {CertificatePath}", _settings.CertificatePath);
            return Result.Failure<CertificateInfo>(Error.Problem("EImzo.X509ReadFailed", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Native X509 reader failed unexpectedly for E-IMZO certificate at {CertificatePath}", _settings.CertificatePath);
            return Result.Failure<CertificateInfo>(Error.Problem("EImzo.X509ReadFailed", ex.Message));
        }
    }

    private Result<CertificateInfo> TryReadWithBouncyCastle(CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            using var stream = File.OpenRead(_settings.CertificatePath);
            var store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, _settings.CertificatePassword.ToCharArray());

            var aliases = store.Aliases.Cast<string>().ToList();
            var alias = aliases.FirstOrDefault(store.IsKeyEntry)
                ?? aliases.FirstOrDefault(store.IsCertificateEntry);

            if (string.IsNullOrWhiteSpace(alias))
                return Result.Failure<CertificateInfo>(Error.Problem("EImzo.BouncyCastleAliasMissing", "No certificate entry was found in the PKCS#12 store."));

            var certificateEntry = store.GetCertificate(alias);
            if (certificateEntry?.Certificate is null)
                return Result.Failure<CertificateInfo>(Error.Problem("EImzo.BouncyCastleCertificateMissing", "Certificate entry could not be loaded from the PKCS#12 store."));

            var certificate = certificateEntry.Certificate;

            _logger.LogInformation(
                "E-IMZO certificate metadata was read with BouncyCastle. Subject: {Subject}, SignatureAlgorithm: {SignatureAlgorithm}",
                certificate.SubjectDN,
                BuildSignatureAlgorithm(certificate.SigAlgName, certificate.SigAlgOid));

            return Result.Success(new CertificateInfo
            {
                SubjectName = certificate.SubjectDN.ToString(),
                IssuerName = certificate.IssuerDN.ToString(),
                ValidFrom = certificate.NotBefore,
                ValidTo = certificate.NotAfter,
                IsExpired = certificate.NotAfter <= DateTime.Now,
                SerialNumber = certificate.SerialNumber.ToString(16).ToUpperInvariant(),
                SignatureAlgorithm = BuildSignatureAlgorithm(certificate.SigAlgName, certificate.SigAlgOid),
                Thumbprint = Convert.ToHexString(SHA1.HashData(certificate.GetEncoded()))
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BouncyCastle reader could not open E-IMZO certificate at {CertificatePath}", _settings.CertificatePath);
            return Result.Failure<CertificateInfo>(Error.Problem("EImzo.BouncyCastleReadFailed", ex.Message));
        }
    }

    private static CertificateInfo MapFromX509(X509Certificate2 certificate)
    {
        return new CertificateInfo
        {
            SubjectName = certificate.Subject,
            IssuerName = certificate.Issuer,
            ValidFrom = certificate.NotBefore,
            ValidTo = certificate.NotAfter,
            IsExpired = certificate.NotAfter <= DateTime.Now,
            SerialNumber = certificate.SerialNumber,
            SignatureAlgorithm = BuildSignatureAlgorithm(certificate.SignatureAlgorithm?.FriendlyName, certificate.SignatureAlgorithm?.Value),
            Thumbprint = certificate.Thumbprint
        };
    }

    private Result<EImzoCertificateContext> TryLoadCertificateContext(CertificateInfo certificateInfo, CancellationToken ct)
    {
        var settingsValidationResult = ValidateSettings();
        if (!settingsValidationResult.IsSuccess)
            return Result.Failure<EImzoCertificateContext>(settingsValidationResult.Error);

        try
        {
            ct.ThrowIfCancellationRequested();

            using var stream = File.OpenRead(_settings.CertificatePath);
            var store = new Pkcs12StoreBuilder().Build();
            store.Load(stream, _settings.CertificatePassword.ToCharArray());

            var aliases = store.Aliases.Cast<string>().ToList();
            var alias = aliases.FirstOrDefault(store.IsKeyEntry);

            if (string.IsNullOrWhiteSpace(alias))
            {
                return Result.Failure<EImzoCertificateContext>(Error.Problem(
                    "EImzo.PrivateKeyMissing",
                    "No private key entry was found in the PKCS#12 store."));
            }

            var keyEntry = store.GetKey(alias);
            if (keyEntry?.Key is null)
            {
                return Result.Failure<EImzoCertificateContext>(Error.Problem(
                    "EImzo.PrivateKeyMissing",
                    "The PKCS#12 store entry does not contain a private key."));
            }

            var certificateEntry = store.GetCertificate(alias);
            if (certificateEntry?.Certificate is null)
            {
                return Result.Failure<EImzoCertificateContext>(Error.Problem(
                    "EImzo.BouncyCastleCertificateMissing",
                    "Certificate entry could not be loaded from the PKCS#12 store."));
            }

            var certificate = certificateEntry.Certificate;
            var certificateChain = store.GetCertificateChain(alias)?
                .Where(x => x?.Certificate is not null)
                .Select(x => x.Certificate)
                .ToList()
                ?? [];

            if (_settings.ValidateCertificateChain)
            {
                using var x509Certificate = X509CertificateLoader.LoadCertificate(certificate.GetEncoded());
                if (!TryValidateChain(x509Certificate, out var chainError))
                    return Result.Failure<EImzoCertificateContext>(Error.Problem("EImzo.CertificateChainInvalid", chainError));
            }

            return Result.Success(new EImzoCertificateContext
            {
                Alias = alias,
                Certificate = certificate,
                PrivateKey = keyEntry.Key,
                CertificateChain = certificateChain,
                CertificateInfo = certificateInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BouncyCastle signer material could not be loaded from {CertificatePath}", _settings.CertificatePath);
            return Result.Failure<EImzoCertificateContext>(Error.Problem("EImzo.SigningMaterialReadFailed", ex.Message));
        }
    }

    private static string BuildSignatureAlgorithm(string? name, string? oid)
    {
        var hasName = !string.IsNullOrWhiteSpace(name);
        var hasOid = !string.IsNullOrWhiteSpace(oid);

        return (hasName, hasOid) switch
        {
            (true, true) => $"{name} ({oid})",
            (true, false) => name!,
            (false, true) => oid!,
            _ => "Unknown"
        };
    }

    private Result ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.CertificatePath))
            return Result.Failure(Error.Problem("EImzo.CertificatePathMissing", "EImzo certificate path is not configured."));

        if (string.IsNullOrWhiteSpace(_settings.CertificatePassword))
            return Result.Failure(Error.Problem("EImzo.CertificatePasswordMissing", "EImzo certificate password is not configured."));

        if (!File.Exists(_settings.CertificatePath))
            return Result.Failure(Error.NotFound("EImzo.CertificateFileNotFound", $"EImzo certificate file was not found: {_settings.CertificatePath}"));

        return Result.Success();
    }

    private static bool TryValidateChain(X509Certificate2 certificate, out string error)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

        if (chain.Build(certificate))
        {
            error = string.Empty;
            return true;
        }

        var details = string.Join(
            "; ",
            chain.ChainStatus
                .Select(status => status.StatusInformation?.Trim())
                .Where(message => !string.IsNullOrWhiteSpace(message)));

        error = string.IsNullOrWhiteSpace(details)
            ? "Certificate chain validation failed."
            : $"Certificate chain validation failed: {details}";

        return false;
    }
}
