# Production Environment Variables

The backend no longer stores production secrets in repository files.

Configure these values with environment variables, user secrets, or a secure configuration provider before startup.

## Connection strings

- `ConnectionStrings__Default`

## JWT

- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__AccessTokenExpirationMinutes`
- `Jwt__RefreshTokenExpirationDays`
- `Jwt__ValidateIssuer`
- `Jwt__ValidateAudience`
- `Jwt__ValidateLifetime`
- `Jwt__ValidateIssuerSigningKey`
- `Jwt__ClockSkewSeconds`

## Backup job

- `BackupJob__EnableEmailSend`
- `BackupJob__PgDumpPath`
- `BackupJob__Database__Host`
- `BackupJob__Database__Name`
- `BackupJob__Database__User`
- `BackupJob__Database__Password`

## Google Drive

- `GoogleDrive__CredentialsPath`
- `GoogleDrive__BackupFolderId`

## Faktura

- `FakturaAuthSettings__GrantType`
- `FakturaAuthSettings__Username`
- `FakturaAuthSettings__Password`
- `FakturaAuthSettings__ClientId`
- `FakturaAuthSettings__ClientSecret`

## Notes

- Development can use .NET user secrets.
- Production should use environment variables or a secure secret store.
- The repository `credentials.json` file is a placeholder only and must be replaced by a secure runtime file path.
