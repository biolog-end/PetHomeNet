using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PetHome.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendRegistrationConfirmationAsync(string email, string confirmationCode);
        Task SendPasswordResetAsync(string email, string resetCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUsername),
                Subject = subject,
                Body = GenerateEmailBody(subject, message),
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }

        private string GenerateEmailBody(string subject, string message)
        {
            //var baseTemplate = File.ReadAllText("Services/EmailTemplates/EmailTemplateBase.html");
            var baseTemplate = "<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>{0}</title>\r\n    <style>\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            line-height: 1.6;\r\n            color: #395874;\r\n            background-color: #F2E8DF;\r\n        }\r\n\r\n        .container {\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            padding: 20px;\r\n            background-color: #FFFFFF;\r\n        }\r\n\r\n        .header {\r\n            background-color: #6DD3E1;\r\n            color: #395874;\r\n            padding: 20px;\r\n            text-align: center;\r\n        }\r\n\r\n        .content {\r\n            padding: 20px;\r\n        }\r\n\r\n        .footer {\r\n            background-color: #F2AD72;\r\n            color: #26110C;\r\n            padding: 10px;\r\n            text-align: center;\r\n            font-size: 0.8em;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <h1>{{subject}}</h1>\r\n        </div>\r\n        <div class=\"content\">\r\n            {{content}}\r\n        </div>\r\n        <div class=\"footer\">\r\n            © 2023 PetHome. All rights reserved.\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>";
            baseTemplate = baseTemplate.Replace("{{subject}}", subject);
            baseTemplate = baseTemplate.Replace("{{content}}", message);
            return baseTemplate;
        }

        public async Task SendRegistrationConfirmationAsync(string email, string confirmationCode)
        {
            //var template = File.ReadAllText("Services/EmailTemplates/RegistrationConfirmation.html");
            var template = "<p>Dear User,</p>\r\n\r\n<p>Thank you for registering with PetHome. To complete the registration process, please enter the following confirmation code:</p>\r\n\r\n<p style=\"text-align: center; font-size: 24px; letter-spacing: 5px; color: #FF7700; font-weight: bold;\">{{code}}</p>\r\n\r\n<p>If you did not register on our site, please ignore this message.</p>\r\n\r\n<p>Sincerely,<br>The PetHome Team</p>\r\n";
            var message = template.Replace("{{code}}", confirmationCode);
            await SendEmailAsync(email, "PetHome registration confirmation", message);
        }

        public async Task SendPasswordResetAsync(string email, string resetCode)
        {
            //var template = File.ReadAllText("Services/EmailTemplates/PasswordReset.html");
            var template = "<p>Dear User,</p>\r\n\r\n<p>You have requested a password reset for your PetHome account. To continue the password reset process, please use the following code:</p>\r\n\r\n<p style=\"text-align: center; font-size: 24px; letter-spacing: 5px; color: #FF7700; font-weight: bold;\">{{code}}</p>\r\n\r\n<p>If you did not request a password reset, please ignore this message and ensure your account is secure.</p>\r\n\r\n<p>Sincerely,<br>The PetHome Team</p>\r\n";
            var message = template.Replace("{{code}}", resetCode);
            await SendEmailAsync(email, "Resetting your password in PetHome", message);
        }
    }
}