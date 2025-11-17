using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using OrderNotificationService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderNotificationService.Infrastructure.Senders
{
    public class SmptEmailSender : IEmailMessageSender
    {
        private readonly EmailSettings _settings;

        public SmptEmailSender(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task SendAsync(string message, string recipientEmail, CancellationToken ct)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mimeMessage.To.Add(new MailboxAddress("Reciever", recipientEmail));
            mimeMessage.Subject = "OrderStatusChanged";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = message;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
