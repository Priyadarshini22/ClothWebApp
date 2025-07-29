using System.Net;
using System.Net.Mail;

namespace ECommerceApp.Repository
{
    public interface IEmailRepository
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false);
    }


    public class EmailRepository : IEmailRepository
         {
             private readonly IConfiguration _configuration;
     
             public EmailRepository(IConfiguration configuration)
             {
                 _configuration = configuration;
             }
     
             public Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
             {
                 string? mailServer = _configuration["EmailSettings:MailServer"];
                 string? fromEmail = _configuration["EmailSettings:FromEmail"];
                 string? password = _configuration["EmailSettings:Password"];
                 string? senderName = _configuration["EmailSettings:SenderName"];
                 int port = int.Parse(_configuration["EmailSettings:MailPort"]);
     
                 var client = new SmtpClient(mailServer, port)
                 {
                     Credentials = new NetworkCredential(fromEmail, password),
                     EnableSsl = true,
                 };
     
                 var fromAddress = new MailAddress(fromEmail, senderName);
                 var mailMessage = new MailMessage
                 {
                     From = fromAddress,
                     Subject = subject,
                     Body = body,
                     IsBodyHtml = isBodyHtml
                 };
                 mailMessage.To.Add(toEmail);
     
                 return client.SendMailAsync(mailMessage);
             }
         }

}
