using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace iPMCloud.Mobile
{
    public class MailManager
    {
        private static string smtpUser = "service@ipm-cloud.de";
        private static string smtpPasswort = "Marnad10+";
        private static string smtpServer = "smtp.strato.de";
        private static int serverPort = 587;//465
        //private static bool mailSent = false;
        //private SmtpClient smtpclient;

        public static void SendMailDirect(String empfaengerMailAdresse, String kopie, String betreff,
            String nachricht, String absenderMailAdresse, Attachment attach, List<Attachment> attachList = null)
        {
            try
            {
                MailMessage message = new MailMessage();
                MailAddress from = new MailAddress(absenderMailAdresse);
                if (kopie != null)
                {
                    message.Bcc.Add(kopie);
                }
                message.To.Add(empfaengerMailAdresse);
                message.From = from;
                message.Subject = betreff;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Body = nachricht;
                if (attach != null && attachList == null)
                {
                    message.Attachments.Add(attach);
                }
                else if (attachList != null)
                {
                    foreach (Attachment att in attachList)
                    {
                        message.Attachments.Add(att);
                    }
                }
                SmtpClient client = new SmtpClient(smtpServer, serverPort);
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(smtpUser, smtpPasswort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = nc;
                client.SendCompleted += (s, e) =>
                {
                    client.Dispose();
                    message.Dispose();
                };
                client.SendAsync(message, null);
                //client.Send(mail, null);
            }
            catch (Exception e)
            {
                Object ee = e;
            }

        }




        public static void SendMailByStream(String to, String bcc, String betreff, String nachricht, String absenderMailAdresse, string name, string attachFilename, List<string> attachFilenames = null)
        {
            try
            {
                //var t = DateTime.Now.Ticks;
                string tempFile = attachFilename.Replace(".pdf", "") + DateTime.Now.Ticks + ".pdf";
                File.Copy(attachFilename, tempFile);
                using (var fileStream = File.OpenRead(tempFile))
                using (var mailClient = new SmtpClient(smtpServer, serverPort))
                using (var message = new MailMessage(absenderMailAdresse, to, betreff, nachricht))
                {
                    NetworkCredential nc = new NetworkCredential(smtpUser, smtpPasswort);
                    mailClient.EnableSsl = true;
                    mailClient.Credentials = nc;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.IsBodyHtml = true;
                    if (!String.IsNullOrWhiteSpace(bcc))
                    {
                        message.Bcc.Add(new MailAddress(bcc));
                    }
                    message.Attachments.Add(new Attachment(fileStream, name, MediaTypeNames.Application.Pdf));


                    mailClient.Send(message);
                }
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}