using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MusicSchool.Application.Abstractions;

namespace MusicSchool.Infrastructure;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value;
        using var message = new MailMessage(emailOptions.From, to, subject, body);
        using var client = new SmtpClient(emailOptions.Host, emailOptions.Port)
        {
            EnableSsl = emailOptions.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(emailOptions.UserName))
        {
            client.Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password);
        }

        await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
