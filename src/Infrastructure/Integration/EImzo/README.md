## E-IMZO Integration

This module has three responsibilities:

- `Certificate`: load certificate metadata from `.pfx` without exposing secrets.
- `Sign`: choose an `ICertificateSigner` strategy from the certificate algorithm OID.
- `Verify`: call `e-imzo-server` for attached/detached PKCS#7 verification.

### Extending signing

To add a new signer such as `SdkSigner` or `CspSigner`:

1. Implement `ICertificateSigner`
2. Match the target algorithm OID in `CanSign(...)`
3. Register it in `AddEImzoIntegration(...)`

`EImzoSigner` does not need to change for new signing providers.
