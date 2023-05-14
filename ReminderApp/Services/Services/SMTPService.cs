﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ReminderApp.Contracts.Email;
using ReminderApp.Options;
using ReminderApp.Services.Concretes;

namespace ReminderApp.Services.Services;

public class SMTPService : IEmailService
{
    private EmailConfigOptions _emailConfig;

    public SMTPService(IOptions<EmailConfigOptions> emailConfigOptions)
    {
        _emailConfig = emailConfigOptions.Value;
    }

    public void Send(MessageDto message)
    {
        var emailMessage = CreateEmailMessage(message);
        Send(emailMessage);
    }

    private MimeMessage CreateEmailMessage(MessageDto message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(string.Empty, _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
        {
            Text = message.Content
        };
        return emailMessage;
    }

    private void Send(MimeMessage emailMessage)
    {
        using (var client = new SmtpClient())
        {
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.From, _emailConfig.Password);

                client.Send(emailMessage);
            }
            catch
            {

                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }

}