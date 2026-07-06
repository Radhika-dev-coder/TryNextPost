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
    }
}
