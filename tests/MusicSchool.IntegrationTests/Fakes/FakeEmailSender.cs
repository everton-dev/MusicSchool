using MusicSchool.Application.Abstractions;

namespace MusicSchool.IntegrationTests.Fakes;

public sealed class FakeEmailSender : IEmailSender
{
    public List<SentEmail> SentEmails { get; } = [];

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        SentEmails.Add(new SentEmail(to, subject, body));
        return Task.CompletedTask;
    }

    public sealed record SentEmail(string To, string Subject, string Body);
}
