using MusicSchool.Application.Abstractions;

namespace MusicSchool.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
