using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
//using EASendMail;

//namespace Charismatech.MessageQueueClasses
//{
//    public static class IO
//    {
//        public static string DBConnectionString;
//        public static void SendMMS(string Number, string BodyText, List<AttachmentFileInfo> Attachments)
//        {
//            string attachURL = "";
//            if (Attachments != null && Attachments.Count > 0)
//                attachURL = ConfigurationManager.AppSettings["WebServerAddress"] + "/attachments/" + Attachments[0].UniqueFileNameNoPath;
//            DoSMSMMS(Number, BodyText, true, attachURL, ""); //TODO implement From
//        }

//        public static void SendSMS(string Number, string BodyText)
//        {
//           // DoSMSMMS(Number, BodyText, false, "", "");
//        }

//        //private static void DoSMSMMS(string Numbers, string BodyText, bool MMS, string attachmentURL, string From)
//        //{
//        //    WebClient client;
//        //    ConfigurationValues configValues = new ConfigurationValues();
//        //    if (configValues.getLatestConfigurationValueByKey("Testing Cellphone Number") != "")
//        //    {
//        //        Numbers = configValues.getLatestConfigurationValueByKey("Testing Cellphone Number"); //Allows all sms to be sent to a specified testing number in the config table
//        //    }
//        //    BodyText = BodyText.TrimEnd();
//        //    Numbers = Numbers.Replace(" ", "");
//        //    string[] nums = Numbers.Split(';');
//        //    // Number = Number.Remove(Number.Length - 1, 1);
//        //    for (int i = 0; i < nums.Length; i++)
//        //    {
//        //        client = new WebClient();
//        //        // Add a user agent header in case the requested URI contains a query.
//        //        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
//        //        client.QueryString.Add("user", configValues.getLatestConfigurationValueByKey("SmsUser"));
//        //        client.QueryString.Add("password", configValues.getLatestConfigurationValueByKey("SmsPassword"));
//        //        client.QueryString.Add("api_id", configValues.getLatestConfigurationValueByKey("SmsApiId"));

//        //        string num = nums[i];
//        //        if (num.StartsWith("0"))
//        //            num = "27" + num.Remove(0, 1);
//        //        // Number = Number.Remove(Number.Length - 1, 1);
//        //        client.QueryString.Add("to", num);
//        //        client.QueryString.Add("concat", "3"); //Max 3 SMSs
//        //        string baseurl;
//        //        if (MMS)
//        //        {
//        //            client.QueryString.Add("si_id", "1");
//        //            if (attachmentURL != "")
//        //            {
//        //                attachmentURL = System.Web.HttpUtility.HtmlEncode(attachmentURL);
//        //                //attachmentURL = attachmentURL.Replace("_", "%5F");
//        //                client.QueryString.Add("si_url", attachmentURL);
//        //                if (BodyText.Trim() == "") BodyText = "MMS from " + From;
//        //            }
//        //            client.QueryString.Add("si_text", BodyText);
//        //            baseurl = "http://api.clickatell.com/mms/si_push";
//        //        }
//        //        else //SMS
//        //        {
//        //            client.QueryString.Add("text", BodyText);

//        //            baseurl = "http://api.clickatell.com/http/sendmsg";
//        //        }
//        //        Stream data = client.OpenRead(baseurl);
//        //        StreamReader reader = new StreamReader(data);
//        //        string s = reader.ReadToEnd();
//        //        data.Close();
//        //        reader.Close();
//        //        if (s.Contains("ERR"))
//        //            throw new Exception("Error:IO.DoSMSMMS:" + s);
//        //    }
//        //}

//        public static void AddAttachments(MailMessage msg, List<AttachmentFileInfo> Attachments)
//        {
//            msg.Attachments.Clear();
//            foreach (AttachmentFileInfo afi in Attachments)
//            {
//                // Create a new attachment, and
//                // add the attachment to the supplied
//                // message.
//                FileStream fs = new FileStream(afi.UniqueFileName, FileMode.Open, FileAccess.Read);
//                Attachment att = new Attachment(fs, afi.OriginalName);
//                msg.Attachments.Add(att);
//            }

//        }

//        public static void SendEmail(string FromAddress, List<string> ToAddresses, string EmailSubject, string BodyText, List<AttachmentFileInfo> Attachments, List<string> CCAddresses, bool IsBodyHtml)
//        {
//            string errorAddress = "";
//            try
//            {
//                ConfigurationValues configValues = new ConfigurationValues();
//                if (configValues.getLatestConfigurationValueByKey("Admin Email Address") == null || configValues.getLatestConfigurationValueByKey("Admin Email Address") == "")
//                    throw new Exception("SendEmail: No Admin Email Address was found in the Configuration table.");

//                if (FromAddress == "")
//                {
//                    FromAddress = configValues.getLatestConfigurationValueByKey("Admin Email Address");  //Send from admin by default
//                }
//                if (ToAddresses == null || ToAddresses.Count == 0)
//                {
//                    ToAddresses.Add(configValues.getLatestConfigurationValueByKey("Admin Email Address"));  //Send to admin by default
//                    BodyText = "Please note that the intended recipient has no email address.  " + "\n\n" + BodyText;
//                }

//                //try
//                //{
//                //    
//                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//                MailMessage msg = new MailMessage
//                {
//                    From = new MailAddress(FromAddress)
//                };
//                msg.ReplyToList.Add(FromAddress);
//                msg.Subject = EmailSubject;
//                msg.Body = BodyText;
//                msg.IsBodyHtml = IsBodyHtml;
//                foreach (string s in ToAddresses)
//                {
//                    if (s != null && s != "")
//                    {
//                        errorAddress += s + ";";
//                        msg.To.Add(new MailAddress(s));
//                    }
//                }
//                if (CCAddresses != null)
//                {
//                    foreach (string s in CCAddresses)
//                    {
//                        if (s != null && s != "")
//                        {
//                            errorAddress = s;
//                            msg.CC.Add(s);
//                            msg.CC.Add(new MailAddress(s));
//                        }
//                    }
//                }
//                if (Attachments != null)
//                {
//                    AddAttachments(msg, Attachments);
//                }
//                string BCC = configValues.getLatestConfigurationValueByKey("BCC Addresses");
//                if (BCC != null && BCC != "")
//                    msg.Bcc.Add(BCC);
//                // Queue the task.
//                //StartBackgroundThread(delegate { doSendThread(msg); });
//                // ConfigurationValues configValues = new ConfigurationValues();
//                //try
//                //{
//                if (configValues.getLatestConfigurationValueByKey("Testing Email Address") != "")
//                {
//                    msg.To.Clear();
//                    msg.To.Add(configValues.getLatestConfigurationValueByKey("Testing Email Address")); //Allows all emails to be sent to a specified testing email address
//                    msg.CC.Clear(); msg.Bcc.Clear();
//                }


//                msg.ReplyToList.Add(msg.From);
//                msg.Sender = msg.From;


//                SmtpClient mailClient = new SmtpClient(configValues.getLatestConfigurationValueByKey("SMTPServer"), 587)//Convert.ToInt32(configValues.getLatestConfigurationValueByKey("SMTPPortNumber")));//587
//                {
//                    UseDefaultCredentials = false
//                };
               
//                if (configValues.getLatestConfigurationValueByKey("SMTPUseDefaultCredentials") == "true")
//                    mailClient.UseDefaultCredentials = true;
//                else
//                {
//                    mailClient.UseDefaultCredentials = false;
//                    mailClient.Credentials = new NetworkCredential(configValues.getLatestConfigurationValueByKey("SMTPLogin"),
//                                                                configValues.getLatestConfigurationValueByKey("SMTPPassword"));
//                }
//                mailClient.EnableSsl = true;
//                mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                
//                mailClient.Send(msg);
//                //SmtpClient client = new SmtpClient(configValues.getLatestConfigurationValueByKey("SMTPServer"));

//                //client.Credentials = new NetworkCredential(configValues.getLatestConfigurationValueByKey("SMTPLogin"),
//                //        configValues.getLatestConfigurationValueByKey("SMTPPassword"));
//                //SmtpServer Server = new SmtpServer(configValues.getLatestConfigurationValueByKey("SMTPServer"));

//                //client.DeliveryMethod = SmtpClient();
//                //client.Send(msg);
//            }
//            catch (Exception ex)
//            {
//                //if (ex.Message.cont)
//                throw new Exception("SendEmail error: Subject=" + EmailSubject + ", From=" + FromAddress + ", To=" + errorAddress + ", Cc=" + CCAddresses + " ErrorMessage:" + ex.Message);
//            }
//            //doSend(msg);

//            //catch (Exception ex)
//            //{
//            //    if (ex.Message.Contains("not in the form required for an e-mail address"))
//            //        SendEmail(configValues.getLatestConfigurationValueByKey("Admin Email Address"), configValues.getLatestConfigurationValueByKey("Admin Email Address"), "Error Mail", ex.Message + " " + errorAddress, false);
//            //}
//        }

//        private static SmtpDeliveryMethod SmtpClient()
//        {
//            throw new NotImplementedException();
//        }

//        public static void SendEmail(string FromAddress, string ToAddress, string EmailSubject, string BodyText, List<AttachmentFileInfo> Attachments, List<string> CCAddresses, bool IsBodyHtml)
//        {
//            //Remove spaces
//            ToAddress = ToAddress.Trim().Replace(" ", "");
//            FromAddress = FromAddress.Trim().Replace(" ", "");

//            //Replace commas with semicolons            
//            ToAddress = ToAddress.Trim().Replace(",", ";");
//            FromAddress = FromAddress.Trim().Replace(",", ";");
//            string[] arr = ToAddress.Split(';');
//            List<string> ToAddresses = new List<string>();
//            for (int i = 0; i < arr.Length; i++)
//            {
//                // person.loadByIDNum(arr[i].Trim(), 0);
//                string email = arr[i];
//                if (email.Contains('<'))   //Email addresses may be specified in the format: Recipient Name <email@email.com>
//                    email = email.Split('<')[1].TrimEnd('>');

//                if (email != "")
//                {
//                    if (email != "" && email.Contains("@") && email.Contains("."))
//                    {
//                        ToAddresses.Add(email);
//                    }
//                }
//            }
//            SendEmail(FromAddress, ToAddresses, EmailSubject, BodyText, Attachments, CCAddresses, IsBodyHtml);
//        }

//        public static void StartBackgroundThread(ThreadStart threadStart)
//        {
//            if (threadStart != null)
//            {
//                Thread thread = new Thread(threadStart)
//                {
//                    IsBackground = true
//                };
//                thread.Start();
//            }
//        }

//        // This thread procedure performs the task.
//        static void DoSendThread(Object msg)
//        {
//            // No state object was passed to QueueUserWorkItem, so 
//            // stateInfo is null.
//            DoSend((MailMessage)msg);
//        }


//        //internal static void SendEmail(string FromAddress, string ToAddress, string EmailSubject, string BodyText, bool IsBodyHtml)
//        //{
//        //    SendEmail(FromAddress, ToAddress, EmailSubject, BodyText, IsBodyHtml, "", "");
//        //}

//        //internal static void SendEmail(string FromAddress, string ToAddress, string EmailSubject, string BodyText)
//        //{
//        //    // if (FromAddress == "")
//        //    var configValues = new ConfigurationValues();
//        //    FromAddress = configValues.getLatestConfigurationValueByKey("DefaultFromEmail");
//        //    try
//        //    {
//        //        //using (MailMessage msg = new MailMessage(FromAddress, ToAddress, EmailSubject, BodyText))
//        //        //{
//        //        MailMessage msg = new MailMessage(FromAddress, ToAddress, EmailSubject, BodyText);
//        //        StartBackgroundThread(delegate { SendEmailThread(msg); });
//        //        //   DoSend(msg);


//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string emailAdd = configValues.getLatestConfigurationValueByKey("BccEmail");
//        //        if (ex.Message.Contains("not in the form required for an e-mail address"))
//        //            SendEmail(emailAdd, emailAdd, "Charismatech Error Mail - invalid email address", ex.Message + " " + ToAddress);
//        //    }
//        //}

//        //internal static void SendEmail(string FromAddress, string ToAddress, string EmailSubject, string BodyText, bool IsBodyHtml,
//        //                                        string CC, string BCC)
//        //{
//        //    MailMessage msg = null;
//        //    ConfigurationValues configValues = new ConfigurationValues();
//        //    if (FromAddress == "")
//        //    {
//        //        FromAddress = configValues.getLatestConfigurationValueByKey("Admin Email Address");  //Send from admin by default
//        //    }
//        //    try
//        //    {
//        //        msg = new MailMessage(FromAddress, ToAddress, EmailSubject, BodyText);
//        //        msg.From = new MailAddress(FromAddress);
//        //        msg.ReplyToList.Add(FromAddress);
//        //        msg.IsBodyHtml = IsBodyHtml;
//        //        if (CC != "") msg.CC.Add(CC);
//        //        if (BCC != "") msg.Bcc.Add(BCC);
//        //        // Queue the task.
//        //        StartBackgroundThread(delegate { doSendThread(msg); });

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        AddEmailErrorRecord(msg, ex);
//        //    }

//        //    //SmtpClient smtpcli = new SmtpClient("smtp.gmail.com", 587); //use this PORT!
//        //    //smtpcli.EnableSsl = true;

//        //    //smtpcli.Credentials = new NetworkCredential("GmailAccount@gmail.com", "GmailAccountPassword");
//        //}
//#pragma warning disable IDE0060 // Remove unused parameter
//        static void DoSend(MailMessage msg)
//#pragma warning restore IDE0060 // Remove unused parameter
//        {
//            //ConfigurationValues configValues = new ConfigurationValues();
//            ////try
//            ////{
//            //    if (configValues.getLatestConfigurationValueByKey("Testing Email Address") != "")
//            //    {
//            //        msg.To.Clear();
//            //        msg.To.Add(configValues.getLatestConfigurationValueByKey("Testing Email Address")); //Allows all emails to be sent to a specified testing email address
//            //        msg.CC.Clear(); msg.Bcc.Clear();
//            //    }


//            //    msg.ReplyToList.Add(msg.From);
//            //    msg.Sender = msg.From;

//            //    SmtpClient client = new SmtpClient(configValues.getLatestConfigurationValueByKey("SMTPServer"), Convert.ToInt32(configValues.getLatestConfigurationValueByKey("SMTPPortNumber")));//587
//            //    client.UseDefaultCredentials = false;
//            //    client.Credentials = new NetworkCredential(configValues.getLatestConfigurationValueByKey("SMTPLogin"), configValues.getLatestConfigurationValueByKey("SMTPPassword"));
//            //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
//            //    client.Send(msg);

//            //}
//            //catch (Exception ex)
//            //{
//            //    if (ex.Message.Contains("The specified string is not in the form required for an e-mail address"))
//            //        SendEmail(configValues.getLatestConfigurationValueByKey("Admin Email Address"), configValues.getLatestConfigurationValueByKey("Admin Email Address"), "Error Mail", ex.Message + "  Email Address:" + msg.To, false);
//            //    else if (ex.Message.ToLower().Contains("too many connections from your host"))
//            //    {
//            //        try
//            //        {
//            //            System.Threading.Thread.Sleep(1000);  //Block the thread for 1000 ms
//            //            SmtpClient smtpClient = new SmtpClient();
//            //            smtpClient.Send(msg);
//            //        }
//            //        catch (Exception ex1)
//            //        {
//            //            AddEmailErrorRecord(msg, ex1);
//            //        }
//            //    }
//            //    else
//            //    {
//            //        AddEmailErrorRecord(msg, ex);
//            //    }
//            //}
//        }

//        static void AddEmailErrorRecord(MailMessage msg, Exception ex)
//        {
//            string sql = "insert into EmailError (ToAddresses, BCCAdresses, FromAddress, [Subject], ErrorMessage, RecordDate)" +
//                       " values (@ToAddresses, @BCCAdresses, @FromAddress, @Subject, @ErrorMessage, @RecordDate ) ";
//            SqlDb.execSQL(sql, new SqlParameter("ToAddresses", msg.To.ToString()),
//               new SqlParameter("FromAddress", msg.From.ToString()),
//               new SqlParameter("BCCAdresses", msg.Bcc.ToString()),
//               new SqlParameter("Subject", msg.Subject.ToString()),
//               new SqlParameter("RecordDate", DateTime.Now),
//               new SqlParameter("ErrorMessage", ex.Message));
//        }

//        public static void AddAttachmentFromStream(MailMessage message, String dataString, String attachmentName)
//        {
//            using (MemoryStream stream = new MemoryStream(UTF32Encoding.Default.GetBytes(dataString)) { Position = 0 })
//            {
//                // Create a new attachment, and
//                // add the attachment to the supplied
//                // message.
//                Attachment att = new Attachment(stream, attachmentName);
//                message.Attachments.Add(att);
//            }
//        }

//        internal static void SendWhatsApp(string p1, string p2, List<AttachmentFileInfo> attachments)
//        {
//            throw new Exception("SendWhatsApp not implemented yet");
//            //WhatsAppApi.WhatsApp wa = new WhatsAppApi.WhatsApp("", "", "", true, true);
//            //wa.Get.SendCreateGroupChat()
//            //https://www.youtube.com/watch?v=4i4G7GBSgnI
//            //wa.PollMessages()
//            //http://www.apache.org/licenses/LICENSE-2.0 : needs to be used
//        }
//    }

//    public class ConfigurationValues
//    {
//        public DataSet getConfigurationValues()
//        {
//            return SqlDb.GetDataSet("select * from ConfigurationValue order by ConfigurationKey, EffectiveDate ");
//        }

//        public string getLatestConfigurationValueByKey(string configKey)
//        {
//            try
//            {
//                string sql = "select top 1 isnull(ConfigurationValue,'') from ConfigurationValue ";
//                sql += " where ConfigurationKey = @configKey ";
//                sql += " and getdate() >= EffectiveDate "; //Must be a record whose effective date is prior to equal to today
//                sql += " order by EffectiveDate desc ";  //most recent
//                return getValue(sql, configKey);
//            }
//            catch
//            {
//                string sql = "select top 1 isnull(ConfigurationValues,'') from ConfigurationValue ";
//                sql += " where ConfigurationKey = @configKey ";
//                sql += " and getdate() >= EffectiveDate "; //Must be a record whose effective date is prior to equal to today
//                sql += " order by EffectiveDate desc ";  //most recent
//                return getValue(sql, configKey);
//            }

//        }

//        private string getValue(string sql, string configKey)
//        {
//            bool hasRows = false;
//            using (DataSet ds = SqlDb.GetDataSet(sql, out hasRows,
//                      new SqlParameter("configKey", configKey)))
//            {
//                if (hasRows)
//                {
//                    DataRow dr = ds.Tables[0].Rows[0];
//                    string ret = dr[0].ToString();
//                    return ret;
//                }
//                else
//                {
//                    throw new Exception("ConfigurationValues.getLatestConfigurationValueByKey: No Active Configuration value record for " + configKey);
//                }
//            }
//        }
//    }
//    public class AttachmentFileInfo
//    {
//        private string attachmentPath;

//        public string AttachmentPath
//        {
//            get { return attachmentPath; }
//            set
//            {
//                attachmentPath = value;
//                if (!attachmentPath.EndsWith("\\"))
//                {
//                    attachmentPath += "\\";
//                }
//            }
//        }

//        public AttachmentFileInfo(string attachmentPath)
//        {
//            AttachmentPath = attachmentPath;
//        }

//        //  private string uniqueFileName;
//        public string UniqueFileName
//        {
//            get { return AttachmentPath + uniqueFileNameNoPath; }
//            // set { uniqueFileName = value; }
//        }

//        private string uniqueFileNameNoPath;
//        public string UniqueFileNameNoPath   //Always in the attachments folder
//        {
//            get { return uniqueFileNameNoPath; }
//            set { uniqueFileNameNoPath = value; }
//        }

//        private string originalName;
//        public string OriginalName
//        {
//            get { return originalName; }
//            set { originalName = value; }
//        }

//    }

//    //public static class IO_FIDDUT
//    //{




//    //    public static void StartBackgroundThread(ThreadStart threadStart)
//    //    {
//    //        if (threadStart != null)
//    //        {
//    //            Thread thread = new Thread(threadStart)
//    //            {
//    //                IsBackground = true
//    //            };
//    //            thread.Start();
//    //        }
//    //    }

//    //    static void SendEmailThread(Object messag)
//    //    {
//    //        DoSend((MailMessage)messag);
//    //    }
//    //    static void DoSend(MailMessage msg)
//    //    {
//    //        msg.ReplyToList.Add(msg.From);
//    //        msg.Sender = msg.From;
//    //        var configValues = new ConfigurationValues();
//    //        System.Collections.Specialized.NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
//    //        if (configValues.getLatestConfigurationValueByKey("EmailingIsLive") != "true") //Override email sending, just send to the BCC person
//    //        {
//    //            msg.To.Clear(); msg.To.Add(configValues.getLatestConfigurationValueByKey("BccEmail"));
//    //        }
//    //        else //Emailing is live, BCC to the BCC person
//    //        {
//    //            if (configValues.getLatestConfigurationValueByKey("BccEmail") != "")
//    //                msg.Bcc.Add(configValues.getLatestConfigurationValueByKey("BccEmail"));
//    //        }
//    //        //SmtpClient client = new SmtpClient(configValues.getLatestConfigurationValueByKey("SMTPServer"), Convert.ToInt32(configValues.getLatestConfigurationValueByKey("SMTPPortNumber")));//587
//    //        //client.UseDefaultCredentials = false;
//    //        //client.Credentials = new NetworkCredential(configValues.getLatestConfigurationValueByKey("SMTPLogin"), configValues.getLatestConfigurationValueByKey("SMTPPassword"));
//    //        //client.DeliveryMethod = SmtpDeliveryMethod.Network;
//    //        try
//    //        {
//    //            //SmtpClient smtpClient = new SmtpClient();
//    //            //
//    //            SmtpClient client = new SmtpClient(configValues.getLatestConfigurationValueByKey("SMTPServer"), Convert.ToInt32(configValues.getLatestConfigurationValueByKey("SMTPPortNumber")))
//    //            {
//    //                UseDefaultCredentials = false,
//    //                Credentials = new NetworkCredential(configValues.getLatestConfigurationValueByKey("SMTPLogin"), configValues.getLatestConfigurationValueByKey("SMTPPassword")),
//    //                DeliveryMethod = SmtpDeliveryMethod.Network
//    //            };//587
//    //            client.Send(msg);
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            if (ex.Message.ToLower().Contains("too many connections from your host"))
//    //            {
//    //                try
//    //                {
//    //                    System.Threading.Thread.Sleep(1000);  //Block the thread for 1000 ms
//    //                    SmtpClient smtpClient = new SmtpClient();
//    //                    smtpClient.Send(msg);
//    //                }
//    //                catch (Exception ex1)
//    //                {
//    //                    AddEmailErrorRecord(msg, ex1);
//    //                }
//    //            }
//    //            else
//    //            {
//    //                AddEmailErrorRecord(msg, ex);

//    //            }
//    //        }
//    //    }

//    //    static void AddEmailErrorRecord(MailMessage msg, Exception ex)
//    //    {
//    //        string sql = "insert into EmailError (ToAddresses, BCCAdresses, FromAddress, [Subject], ErrorMessage, RecordDate)" +
//    //                   " values (@ToAddresses, @BCCAdresses, @FromAddress, @Subject, @ErrorMessage, @RecordDate ) ";
//    //        SqlDb.execSQL(sql, new SqlParameter("ToAddresses", msg.To.ToString()),
//    //           new SqlParameter("FromAddress", msg.From.ToString()),
//    //           new SqlParameter("BCCAdresses", msg.Bcc.ToString()),
//    //           new SqlParameter("Subject", msg.Subject.ToString()),
//    //           new SqlParameter("RecordDate", DateTime.Now),
//    //           new SqlParameter("ErrorMessage", ex.Message));
//    //    }


//    //}
//}