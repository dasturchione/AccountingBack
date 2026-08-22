namespace Integration.GoogleDrive.Configs;

public sealed class GoogleDriveSettings
{
    public const string SectionName = "GoogleDrive";

    public string OAuthClientSecretsPath { get; set; } = string.Empty;
    public string OAuthTokenStorePath { get; set; } = "appdata/drive/oauth-tokens";
    public string OAuthUser { get; set; } = string.Empty;
    public string OAuthRefreshToken { get; set; } = string.Empty;
    public string BackupFolderId { get; set; } = string.Empty;
    public bool UseSharedDrive { get; set; }
}
