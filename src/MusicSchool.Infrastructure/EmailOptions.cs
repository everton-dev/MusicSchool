namespace MusicSchool.Infrastructure;

public sealed class EmailOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 25;

    public string From { get; set; } = "noreply@musicschool.local";

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool EnableSsl { get; set; }
}
