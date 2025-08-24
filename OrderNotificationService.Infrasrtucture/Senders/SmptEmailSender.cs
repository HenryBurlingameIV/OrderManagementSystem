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

namespace OrderNotificationService.Infrastructure.Senders
{
    public class SmptEmailSender : IEmailMessageSender
    {
        public async Task SendAsync(string message, string recipientEmail, CancellationToken ct)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("OrderService", "mr.ivliev@gmail.com"));
            mimeMessage.To.Add(new MailboxAddress("Reciever", recipientEmail));
            mimeMessage.Subject = "OrderStatusChanged";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = message;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync("apikey", "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", ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
