namespace Infrastructure.Options;

public sealed class HrFileStorageOptions
{
    public string LocalRoot { get; set; } = "appdata/hr/absences";
    public int MaxFileSizeMb { get; set; } = 20;
    public string[] AllowedExtensions { get; set; } =
        [".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx", ".xls", ".xlsx"];
}

public sealed class TelegramFileStorageOptions
{
    public string BotToken { get; set; } = string.Empty;
    public long ChatId { get; set; }
}
