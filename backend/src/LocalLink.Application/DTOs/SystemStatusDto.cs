namespace LocalLink.Application.DTOs;

public class SystemStatusDto
{
    public string Application { get; set; } = "LocalLink";
    public string Status { get; set; } = "running";
    public string Environment { get; set; } = string.Empty;
    public string Database { get; set; } = "unknown";
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
}
