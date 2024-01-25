using System.Net;
using System.Net.Mail;

namespace BlogApp.Services;

public class EmailService
{
    public bool Send(
        string toName,
        string toEmail,
        string subject,
        string body)
    {
        var smtpClient = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port)
        {
            Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password),

            DeliveryMethod = SmtpDeliveryMethod.Network,

            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(Configuration.Smtp.UserName, "MiguelFarias"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
            To = { new MailAddress(toEmail, toName) }
        };

        try
        {
            smtpClient.Send(mail);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            Console.WriteLine(e.StackTrace);

            return false;
        }
    }
}