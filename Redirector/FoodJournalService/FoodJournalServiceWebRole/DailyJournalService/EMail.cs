using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace DailyJournalServiceRole
{
    public class EMail
    {

        public static string DAILYJOURNALEMAIL = "daily.journal@outlook.com";

        public static void Send(string to, string subject, string body)
        {

            try
            {
                MailMessage mailMsg = new MailMessage();

                // To
                mailMsg.To.Add(new MailAddress(to));

                // From
                mailMsg.From = new MailAddress(DAILYJOURNALEMAIL, "Daily Journal");

                // Subject and multipart/alternative Body
                mailMsg.Subject = subject;
                //string text = body;
                string html = @"<p>" + body.Replace("\r\n","</br>") + "</p>";
                //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("azure_84062dec2c347082756b78bdd65d27ed@azure.com", "lr2uy2ne");
                smtpClient.Credentials = credentials;

                smtpClient.Send(mailMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


        }
    }
}