namespace Application.Common.Settings;

public class BackupJobSettings
{
    /// <summary>true bo'lsa backup Google Drive ga ham yuboriladi</summary>
    public bool    EnableEmailSend { get; set; }

    /// <summary>Backup ishga tushadigan soat (Toshkent vaqti).</summary>
    public int ScheduleHour { get; set; } = 4;

    /// <summary>Backup ishga tushadigan daqiqa.</summary>
    public int ScheduleMinute { get; set; } = 5;

    /// <summary>
    /// pg_dump ning to'liq yo'li. Bo'sh bo'lsa avtomatik qidiradi.
    /// Misol (Windows): "C:\\Program Files\\PostgreSQL\\18\\bin\\pg_dump.exe"
    /// </summary>
    public string? PgDumpPath { get; set; }

    public DbSettings Database { get; set; } = null!;
}

public class DbSettings
{
    public string Host     { get; set; } = null!;
    public string Name     { get; set; } = null!;
    public string User     { get; set; } = null!;
    public string Password { get; set; } = null!;
}
