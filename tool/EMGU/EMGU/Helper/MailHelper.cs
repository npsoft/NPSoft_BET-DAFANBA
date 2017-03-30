using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SpiralEdge.Helper
{
    public class MailHelper
    {
        public static string SendEmail(string fromUsername, string fromPassword, string fromDisplayname, string to, string subject, string body, string attachment)
        {
            try
            {
                MailMessage msg = new MailMessage();

                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Priority = MailPriority.High;

                msg.From = new MailAddress(fromUsername, fromDisplayname, System.Text.Encoding.UTF8);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = body;
                if (!string.IsNullOrEmpty(attachment)) { msg.Attachments.Add(new Attachment(attachment)); }

                SmtpClient client = new SmtpClient();
                client.Credentials = new NetworkCredential(fromUsername, fromPassword);
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);

                return "true";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
