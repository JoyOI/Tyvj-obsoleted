using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Net.Mail;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Helpers
{
    public static class SMTP
    {
        public static string ServerHost;
        public static int Port;
        static SmtpClient SmtpClient = null;
        public static void Send(string Target, string Title, string Content)
        {
            System.Net.Mail.SmtpClient client = new SmtpClient("smtp.exmail.qq.com");

            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("ssx@qbxt.cn", "tsba940426");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailAddress addressFrom = new MailAddress("ssx@qbxt.cn", "Tyvj");
            MailAddress addressTo = new MailAddress(Target, "");

            System.Net.Mail.MailMessage message = new MailMessage(addressFrom, addressTo);
            message.Sender = new MailAddress("ssx@qbxt.cn");
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;
            message.Subject = Title;
            message.Body = Content;

            client.Send(message);
        }
    }
}
