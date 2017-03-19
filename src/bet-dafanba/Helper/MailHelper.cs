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
        public string User { get; set; }
        public string Pass { get; set; }

        public MailHelper() { }

        public MailHelper(string user, string pass)
        {
            User = user;
            Pass = pass;
        }

        public string SendEmail(string[] emails, string subject, string body, string[] attachments = null, string displayName = null)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.SubjectEncoding = Encoding.UTF8;
                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Priority = MailPriority.High;

                msg.From = new MailAddress(User, displayName, Encoding.UTF8);
                foreach (string email in emails ?? Enumerable.Empty<string>())
                {
                    msg.To.Add(email);
                }
                msg.Subject = subject;
                msg.Body = body;
                foreach (string attachment in attachments ?? Enumerable.Empty<string>())
                {
                    msg.Attachments.Add(new Attachment(attachment));
                }

                SmtpClient client = new SmtpClient();
                client.Credentials = new NetworkCredential(User, Pass);
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
            }
            catch (Exception ex)
            {
                return string.Format("{0}{1}", ex.Message, ex.StackTrace);
            }
            return null;
        }
    }
}
