using System.Net;
using System.Net.Mail;

namespace DScannerLibrary.Services;

public class EmailService
{
    public async Task SendMailAsync()
    {
	var sender = "ioan.scafa@gmail.com";
	var password = "";

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(sender, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(sender),
            Subject = "Error check!",
            Body = "<a href=\"https://sihastria.mmb.ro\">sihastria.mmb.ro</a>",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(sender);
        //mailMessage.CC.Add(senderEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
