using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RazorEngine;
using RazorEngine.Templating;
using SharedUtils.Email;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharedUtils
{
    public interface IEmailSender
    {
        Task SendEmail(List<EmailRecipient> recipients, string body, string subject);
        Task<string> BuildBody(string templateStr, object obj);
    }
    internal class EmailSender: IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendEmail(List<EmailRecipient> recipients,string body,string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            foreach(var rec in recipients)
            {
                message.To.Add(new MailboxAddress(rec.Name, rec.Email));
            }
           
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, false);
                await client.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        public async Task<string> BuildBody(string templateStr,object obj)
        {
            return await Task.Run(() => Engine
                 .Razor
                 .RunCompile(templateStr,
                     Guid.NewGuid().ToString(),
                     null,
                     obj));
        }
    }
    public class EmailRecipient 
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

}
