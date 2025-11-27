using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using MailKit.Net.Imap;
using MailKit;
using ReadSharp;
using MailKit.Net.Pop3;
using LifeLineEmailReceiverService;
using MailKit.Search;
using MailKit.Security;

namespace Charismatech.MessageQueueClasses
{
    public sealed class MessageReceiveSingleton
    {
        public int RecordsProcessed { get; set; }
        //public int RecordsProcessed;
        static readonly MessageReceiveSingleton instance = new MessageReceiveSingleton();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static MessageReceiveSingleton()
        {
        }

        MessageReceiveSingleton()
        {

        }

        public static MessageReceiveSingleton Instance
        {
            get
            {
                return instance;
            }
        }
        //private static string _SMTPServer = "outlook.office365.com"; //ConfigurationManager.AppSettings["POP3Server"];
        private static string _SMTPServer = "smtp.office365.com";// "mail.charismatech.co.za"; 
        private static int _SMTPPort = 993;//Convert.ToInt32(ConfigurationManager.AppSettings["POP3Port"]);
        private static bool _SMTPUseSSL = true;
        //private static string _SMTPUsername = "chris@lifelinepmb.co.za"; //ConfigurationManager.AppSettings["POP3Username"];
        private static string _SMTPUsername = "projects@adtrp.com"; //ConfigurationManager.AppSettings["POP3Username"];
        private static string _SMTPPassword = "D(173691557380ur"; //ConfigurationManager.AppSettings["POP3Password"];

        //private static string _POP3Server = "mail.lifelinepmb.org.za"; //ConfigurationManager.AppSettings["POP3Server"];
        //private static int _POP3Port = 110;//Convert.ToInt32(ConfigurationManager.AppSettings["POP3Port"]);
        //private static bool _POP3UseSSL = false;//Convert.ToBoolean(ConfigurationManager.AppSettings["POP3UseSSL"]);
        //private static string _POP3Username = "counsellor@lifelinepmb.org.za"; //ConfigurationManager.AppSettings["POP3Username"];
        //private static string _POP3Password = "CCC@@@LLL2020"; //ConfigurationManager.AppSettings["POP3Password"];

        public static string FromEmail = "service@charismatech.co.za";
        public static string BccEmail = "service@charismatech.co.za";
        public static string ErrorEmails = "service@charismatech.co.za";

        public static void WriteToEventLog(string sEvent)
        {
            Data.WriteToEventLog(sEvent);
            
        }
        public void ProcessEmailsIMAP()
        {
            WriteToEventLog("Processing emails IMAP...");
            //string _cloudUploadedFilesContainerName = "pop3-messages";
            //string _storageConnectionString = "";//ConfigurationManager.AppSettings["AzureWebJobsStorage"];

            using (var client = new ImapClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                
                bool error = false;
                try
                {
                    //client.Connect(_SMTPServer, _SMTPPort, _SMTPUseSSL,);
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    //client.Connect(_SMTPServer, _SMTPPort, SecureSocketOptions.StartTls);
                    client.Connect(_SMTPServer, _SMTPPort, SecureSocketOptions.Auto);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.AuthenticationMechanisms.Remove("NTLM");
                    client.Authenticate(_SMTPUsername, _SMTPPassword);
                    //client.Connect("outlook.office365.com", 993, true);
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //client.Authenticate(userName + @"\" + sharedMailboxAlias, password);

                }
                catch (Exception ex)
                {
                    error = true;
                    WriteToEventLog(ex.Message);
                    Data.SendErrorEmail(ex.Message);
                }

                if (!error)
                {
                    try
                    {
                        #region read and process messages
                        var inbox = client.Inbox;
                        inbox.Open(FolderAccess.ReadWrite);
                        //WriteToEventLog(String.Format("Total messages: {0}", inbox.Count));
                        

                        var dtFrom = DateTime.Now.AddDays(-60); //Two months
                        var query = SearchQuery.DeliveredAfter(dtFrom).And(SearchQuery.NotSeen); //.SubjectContains("").Or(SearchQuery.SubjectContains(""));
                        var orderBy = new[] { OrderBy.Arrival };
                        var uids = client.Inbox.Search(query);
                        WriteToEventLog(String.Format("Recent messages: {0}", uids.Count));
                        //for (int i = 0; i < inbox.Count; i++)
                        foreach (var uid in uids)
                        {
                            string strMessageGUID = Guid.NewGuid().ToString();
                            var message = client.Inbox.GetMessage(uid);
                            //if (message)
                            var emailMessage = new EmailMessage();
                            emailMessage.EmailDate = message.Date.Date;
                            emailMessage.EmailSubject = message.Subject;
                            //WriteToEventLog($"Got Message: {message.Subject}");
                            // Get all From Addresses
                            string strFromAddresses = "";
                            foreach (var item in message.From.Mailboxes)
                            {
                                strFromAddresses += item.Address + ";";
                            }
                            emailMessage.EmailFrom = strFromAddresses;
                            // Get all To Addresses
                            string strToAddresses = "";
                            foreach (var item in message.To.Mailboxes)
                            {
                                strToAddresses += item.Address + ";";
                            }
                            emailMessage.EmailTo = strToAddresses;

                            emailMessage.BodyText = message.GetTextBody(MimeKit.Text.TextFormat.Text);
                            if ((emailMessage.BodyText == null || emailMessage.BodyText == "") && message.HtmlBody != "")
                                emailMessage.BodyText = HtmlUtilities.ConvertToPlainText(message.HtmlBody);
                            if (message.From.ToString().Contains("no-reply"))
                                client.Inbox.SetFlags(uid, MessageFlags.Seen, true); //Ignore spam
                            else
                            {
                                Int64 crId = Data.CreateContactRecord(emailMessage);
                                if (crId > 0) //Successfully created
                                {
                                    //message.Delete
                                    //Message has been read
                                    client.Inbox.SetFlags(uid, MessageFlags.Seen, true);
                                }
                            }
                        }
                        WriteToEventLog("client.Disconnect");
                        client.Disconnect(true);
                        #endregion read and process messages
                    }
                    catch (Exception ex)
                    {
                        WriteToEventLog(ex.Message);
                        Data.SendErrorEmail(ex.Message);
                    }
                }
            }
        }


        //public void ProcessEmailsPOP3()
        //{
        //    WriteToEventLog("Processing emails POP3...");

        //    using (var client = new Pop3Client())
        //    {
        //        // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
        //        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //        bool error = false;
        //        try
        //        {
        //            client.Connect(_POP3Server, _POP3Port, _POP3UseSSL);
        //            // Note: since we don't have an OAuth2 token, disable
        //            // the XOAUTH2 authentication mechanism.
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");
        //            client.Authenticate(_POP3Username, _POP3Password);
        //        }
        //        catch (Exception ex)
        //        {
        //            error = true;
        //            WriteToEventLog(ex.Message);
        //            Data.SendErrorEmail(ex.Message);
        //        }

        //        if (!error)
        //        {
        //            if (client.Count > 34)
        //            {
        //                for (int i = 34; i < client.Count; i++)
        //                {
        //                    string strMessageGUID = Guid.NewGuid().ToString();
        //                    var message = client.GetMessage(i);
        //                    var emailMessage = new EmailMessage();
        //                    emailMessage.EmailDate = message.Date.Date;
        //                    emailMessage.EmailSubject = message.Subject;
        //                    WriteToEventLog($"Got Message: {message.Subject}");
        //                    // Get all From Addresses
        //                    string strFromAddresses = "";
        //                    foreach (var item in message.From.Mailboxes)
        //                    {
        //                        strFromAddresses += item.Address + ";";
        //                    }
        //                    emailMessage.EmailFrom = strFromAddresses;
        //                    // Get all To Addresses
        //                    string strToAddresses = "";
        //                    foreach (var item in message.To.Mailboxes)
        //                    {
        //                        strToAddresses += item.Address + ";";
        //                    }
        //                    emailMessage.EmailTo = strToAddresses;


        //                    if (message.TextBody != null && message.TextBody != ""
        //                         && !message.TextBody.StartsWith("Email configuration settings for") && !message.From.ToString().Contains("test"))
        //                    {
        //                        emailMessage.BodyText = message.TextBody;
        //                        if ((emailMessage.BodyText == null || emailMessage.BodyText == "") && message.HtmlBody != "")
        //                            emailMessage.BodyText = HtmlUtilities.ConvertToPlainText(message.HtmlBody);
        //                        Int64 crId = Data.CreateContactRecord(emailMessage);
        //                        if (crId > 0) //Successfully created
        //                        {
        //                            client.DeleteMessage(i);
        //                        }
        //                    }
        //                    //TODO:else?
        //                }
        //            }
        //            WriteToEventLog("client.Disconnect");
        //            client.Disconnect(true);

        //        }
        //    }
        //}
        //public void ReceiveMessages()
        //{
        //    bool Debug = ConfigurationManager.AppSettings["debug"] == "true";
        //    List<MessageQueueItem> l = new List<MessageQueueItem>();
        //    try
        //    {
        //        if (Debug) WriteToEventLog("SendMessagesStart:ConfigKeys:" + ConfigurationManager.AppSettings.AllKeys.Count());
        //        //Loop through all the DBConnectionString elements in the app.config
        //        //ConfigurationManager.AppSettings["DBConnectionString"];
        //        foreach (string k in ConfigurationManager.AppSettings.AllKeys)
        //        {
        //            if (k.StartsWith("DBConnectionString"))//el.Name.StartsWith("DBConnectionString"))
        //            {
        //                string DBId = k.Replace("DBConnectionString", ""); //grab the suffix
        //                string attachmentPath = ConfigurationManager.AppSettings["AttachmentPath" + DBId];
        //                //if (Debug) WriteToEventLog("DBConn:" + attachmentPath);
        //                if (Debug) WriteToEventLog("AttachmentPath:" + attachmentPath);
        //                MessageQueueItems mqis = new MessageQueueItems(ConfigurationManager.AppSettings[k]);//el.Value);
        //                mqis.DebugMode = Debug;
        //                RecordsProcessed = 0;
        //                //WriteToEventLog("GetMessageQueueItemsPending...");
        //                try
        //                {
        //                    l = mqis.GetMessageQueueItemsPending(50);
        //                }
        //                catch (Exception ex)
        //                {
        //                    WriteToEventLog("GetMessageQueueItemsPending:Error:" + ex.Message);
        //                }
        //                if (Debug) WriteToEventLog("GetMessageQueueItemsPending:Count records" + l.Count());
        //                if (l.Count > 0)
        //                {
        //                    if (Debug) WriteToEventLog(ConfigurationManager.AppSettings[k].Split(';')[1]);  //Write the database name to the Log
        //                    if (Debug) WriteToEventLog("Messages to process in queue:" + l.Count.ToString());

        //                    foreach (MessageQueueItem mqi in l)
        //                    {
        //                        try
        //                        {
        //                            if (mqi.MessageType == MessageType.Email)
        //                            {
        //                                List<AttachmentFileInfo> attachments = mqi.GetAttachments(attachmentPath);
        //                                IO.SendEmail(mqi.From, mqi.To, mqi.Subject, mqi.Body, attachments, null, mqi.IsBodyHtml);
        //                            }
        //                            else if (mqi.MessageType == MessageType.SMS)
        //                            {
        //                                IO.SendSMS(mqi.To, mqi.Body);
        //                            }
        //                            else if (mqi.MessageType == MessageType.MMS)
        //                            {
        //                                List<AttachmentFileInfo> attachments = mqi.GetAttachments(attachmentPath);
        //                                IO.SendMMS(mqi.To, mqi.Body, attachments);
        //                            }
        //                            else if (mqi.MessageType == MessageType.WhatsApp)
        //                            {
        //                                List<AttachmentFileInfo> attachments = mqi.GetAttachments(attachmentPath);
        //                                IO.SendWhatsApp(mqi.To, mqi.Body, attachments);
        //                            }
        //                            mqi.ErrorMessage = null;
        //                            mqi.DateSent = DateTime.Now;
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                            WriteToEventLog("Charismatech.MessageQueueService error: " + ex.Message);
        //                            mqi.ErrorMessage = ex.Message;
        //                            if (ex.InnerException != null)
        //                                mqi.ErrorMessage += "->" + ex.InnerException.Message;

        //                            mqi.DateSent = null;
        //                        }
        //                        mqi.CountSendAttempt++;
        //                        mqi.Save(false);
        //                        RecordsProcessed++;
        //                    }
        //                    if (RecordsProcessed > 0)
        //                        WriteToEventLog("Charismatech.MessageQueueService records processed:" + RecordsProcessed.ToString());
        //                }
        //            }
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        WriteToEventLog(ex.Message);
        //    }

        //}
    }
}
