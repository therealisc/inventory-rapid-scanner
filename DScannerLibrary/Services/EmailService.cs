using System.Net;
using System.Net.Mail;

namespace DScannerLibrary.Services;

public class EmailService
{
    public async Task SendMailAsync(string key)
    {
	var sender = "ioan.scafa@gmail.com";
	var password = key;

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(sender, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(sender),
            Subject = "Renew Life!",
            Body = "<a href=\"https://sihastria.mmb.ro\">Here we go out of matrix</a> " + 
	           "<a href=\"https://daniilsihastrul.mmb.ro\">Change the report</a>",
            IsBodyHtml = true,
        };

	//mailMessage.Attachments.Add(new Attachment("Saga.db"));

        mailMessage.To.Add("therealisc@proton.me");
        //mailMessage.CC.Add("luminita_scafa@yahoo.com");

        await smtpClient.SendMailAsync(mailMessage);
    }
}
