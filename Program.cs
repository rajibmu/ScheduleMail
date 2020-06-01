using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace ScheduleMail
{
    class MailReminder
    {
        static void Main(string[] args)
        {
            var currentTime = DateTime.Now.ToShortTimeString();
            MailReminder mailReminder = new MailReminder();
            if (string.Compare(currentTime, "15:49") > 0)
            {
                string messageText = "";
                List<MailAddress> mailTo = mailReminder.GetMailAddressFromDB();
                mailReminder.SendMail(messageText, mailTo);
            }
        }

        private List<MailAddress> GetMailAddressFromDB()
        {
            List<MailAddress> mailto = new List<MailAddress>(); ;
            SqlCommand cmd = null;
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            string queryString = @" SELECT EmailAddress FROM [UserDB].[dbo].[UserEmailInfo]
                                    WHERE ISNULL([EmailActionTaken],'N') <> 'Y'  
                                    AND ([EmailSendOn] IS NULL OR  DateAdd(d,3,[EmailSendOn]) < GetDate())";

            using (SqlConnection connection =
                       new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                cmd = new SqlCommand(queryString);
                cmd.Connection = connection;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    mailto.Add(new MailAddress(reader["EmailAddress"].ToString()));
                }
                reader.Close();
            }

            return mailto;
        }

        protected void SendMail(string messageText, List<MailAddress> mailTo)
        {

            //Mail notification
            MailMessage message = new MailMessage();
            foreach (var to in mailTo)
            {
                message.To.Add(to);
            }
            message.Subject = "Reminder Mail";
            message.Body = "<html><body>" + messageText + "</body></html>";
            message.From = new MailAddress("MyEmail@mail.com");

            // Email Address from where you send the mail
            var fromAddress = "helpdesk@mail.com";
            //Password of your mail address
            const string fromPassword = "password";

            // smtp settings
            var smtp = new System.Net.Mail.SmtpClient();
            {
                smtp.Host = "smtp.mail.com";
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                smtp.Timeout = 20000;
            }
            // Passing values to smtp object        
            smtp.Send(message);
        }
    }

}
