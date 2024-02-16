using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IdentityServer.Services;

public class EmailSender : IEmailSender
{
    private readonly ISendGridClient _sendGridClient;

    public EmailSender(ISendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(Environment.GetEnvironmentVariable("FROM_EMAIL")),
            Subject = subject,
            HtmlContent = htmlMessage
        };
        msg.AddTo(email);

        await _sendGridClient.SendEmailAsync(msg);
    }
}