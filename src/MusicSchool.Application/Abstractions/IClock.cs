namespace MusicSchool.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
