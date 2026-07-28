namespace Integration.GoogleDrive.Configs;

public class GoogleDriveOptions
{
    public const string SectionName = "GoogleDrive";

    public string CredentialsPath { get; set; } = null!;
    public string BackupFolderId { get; set; } = null!;
}
