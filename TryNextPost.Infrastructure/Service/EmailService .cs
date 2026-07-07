using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.IServices.Interface;
using SendGrid;
using SendGrid.Helpers.Mail;


namespace TryNextPost.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmail(string email, string otp)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var senderEmail = _config["Smtp:SenderEmail"];
            var senderName = _config["Smtp:SenderName"];
            var password = _config["Smtp:Password"];

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Your Login OTP - TryNextPost",
                Body = $"Your OTP is: {otp}\n\nThis OTP is valid for 5 minutes. Do not share it with anyone.",
                IsBodyHtml = false
            };
            mail.To.Add(email);

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        public async Task SendWelcomeEmail(string email, string fullName)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var senderEmail = _config["Smtp:SenderEmail"];
            var senderName = _config["Smtp:SenderName"];
            var password = _config["Smtp:Password"];

            var body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #2E86C1;'>Welcome to TryNextPost, {fullName}! 🎉</h2>
                    <p>Congratulations! Your registration was successful.</p>
                    <p>You can now log in and start using your account.</p>
                    <br/>
                    <p>Thank you for choosing <strong>TryNextPost</strong>.</p>
                    <p style='color: gray; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>";

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Welcome to TryNextPost — Registration Successful!",
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(email);

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
