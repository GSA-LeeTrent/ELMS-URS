using System;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace lmsextreg.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            if ( String.IsNullOrEmpty(Options.SendGridKey) )
            {
                return ExecuteOnPrem(subject, message, email);
            }
            else
            {
                return ExecuteUsingSendGrid(Options.SendGridKey, subject, message, email);
            }
        }

        public Task ExecuteOnPrem(string subject, string message, string toEmail)
        {
            var client = new SmtpClient
            {
                Host = "smtp.gsa.gov",
                Port = 25
            };
            //client.EnableSsl = true;
              
            MailAddress fromAddress = new MailAddress("ELMSRegistration@gsa.gov", "LMS Registration and Enrollment");
            MailAddress toAddress   = new MailAddress(toEmail);
            
            MailMessage mailMessage     = new MailMessage(fromAddress, toAddress);
            mailMessage.IsBodyHtml      = true;
            mailMessage.Subject         = subject;
            mailMessage.Body            = message;
            //mailMessage.SubjectEncoding =  System.Text.Encoding.UTF8;
            //mailMessage.BodyEncoding    =  System.Text.Encoding.UTF8;

            return client.SendMailAsync(mailMessage);
        }
        public Task ExecuteUsingSendGrid(string apiKey, string subject, string message, string email)
        {
            _logger.LogInformation("[EmailSender][ExecuteUsingSendGrid]");
            _logger.LogInformation("apiKey: '" + apiKey + "'");

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("LMSExtRegAdmin@gsa.gov", "LMS Registration and Enrollment"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }
    }
}
