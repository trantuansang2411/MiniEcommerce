using ApplicationCore.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Identity;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _host = configuration["EmailSettings:Host"]
            ?? throw new InvalidOperationException("Email SMTP host is not configured.");
        _userName = configuration["EmailSettings:UserName"]
            ?? throw new InvalidOperationException("Email SMTP user name is not configured.");
        _password = configuration["EmailSettings:Password"]
            ?? throw new InvalidOperationException("Email SMTP password is not configured.");
        _fromEmail = configuration["EmailSettings:FromEmail"]
            ?? throw new InvalidOperationException("Email sender address is not configured.");
        _fromName = configuration["EmailSettings:FromName"] ?? "MiniProject";
        _port = configuration.GetValue<int?>("EmailSettings:Port") ?? 587;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_userName, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
