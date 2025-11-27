using Charismatech.MessageQueueClasses;
using LifeLineEmailReceiverService;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LifeLineEmailReceiverService
{
    public static class Data
    {

        public static void WriteToEventLog(string sEvent)
        {

            string sSource;
            string sLog;
            sSource = "LifelineEmailReceiver";
            sLog = "Application";

            try
            {
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Information, 234);
            }
            catch (Exception ex)
            {
                try
                {
                    SqlDb.execSQL("insert into ErrorLog (DateOFError, Message) values (getdate(), @Message)", new SqlParameter("Message", sEvent + Environment.NewLine + ex.Message));
                }
                catch { }
                try
                {
                    Data.SendErrorEmail(sEvent + Environment.NewLine + ex.Message);
                }
                catch { }

            }

        }
        public static Int64 CreateContactRecord(EmailMessage message) //, ILogger log)
        {
            Int64 ret = 0;
            Int64 issueId = 0;
            int memberId = 0;
            Int64 contactRecordId = 0;

            using (SqlConnection connection = getSqlConnection())
            {

                memberId = GetOrCreateMember(connection, message);

                if (memberId > 0)
                {
                    issueId = GetOrCreateIssue(connection, memberId, message);

                    if (issueId > 0)
                    {
                        try
                        {
                            contactRecordId = InsertContactRecord(connection, issueId, message);
                            if (contactRecordId > 0)
                            {
                                WriteToEventLog(String.Format("ContactRecord created with Id {0}", contactRecordId.ToString()));
                                ret = contactRecordId;
                            }
                        }
                        catch (Exception ex)
                        {
                            SendErrorEmail(ex.Message);
                            WriteToEventLog(ex.Message);
                        }
                    }
                }
                else
                {
                    string msg = GetMessageText();
                    if (msg != "" && !message.EmailFrom.StartsWith("no-reply"))
                        ret = SendEmailAutoResponse(connection, message, msg);

                }
                //Console.ReadLine();
                return ret;
            }
        }

        private static SqlConnection getSqlConnection()
        {
            //string connectionString =
            //    "Server=tcp:lifelinepmbportalserver.database.windows.net,1433;" +
            //    "Database=LifeLineProduction;User ID=charismatech@lifelinepmb.co.za@lifelinepmbportalserver;" +
            //    "Password=Mug90918;Encrypt=True;TrustServerCertificate=False;" +
            //    "Connection Timeout=30;MultipleActiveResultSets=true";
            string connectionString =
                "Server=localhost;Database=LifeLineProduction;User ID=LifelineWebUser;Password=Mug90918;Connection Timeout=60;MultipleActiveResultSets=true";
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        private static long InsertContactRecord(SqlConnection connection, long issueId, EmailMessage message)
        {
            long ret = 0;
            string sql = "insert into ContactRecord([TenantId], [IssueId], [ContactTypeId]" +
                                ", [MessageDetails]" +
                                ", [MessageDetailsEdited]" +
                                ", [ContactDate], [DateToFollowUp]" +
                                ", [CreatorUserName], [CreationTime]" +
                                ", [LastModifierUserName], [LastModificationTime])" +
                                " values" +
                                " (@TenantId, @IssueId, @ContactTypeId" +
                                ", @MessageDetails, @MessageDetailsEdited" +
                                ", @ContactDate, @DateToFollowUp" +
                                ", @CreatorUserName, getdate()" +
                                ", @LastModifierUserName,  getdate()); select @@identity;";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TenantId", 1);
            command.Parameters.AddWithValue("@IssueId", issueId);
            command.Parameters.AddWithValue("@ContactTypeId", 1); //Inbound
            command.Parameters.AddWithValue("@MessageDetails", message.BodyText == null ? message.EmailSubject : message.BodyText);
            command.Parameters.AddWithValue("@MessageDetailsEdited", "");
            command.Parameters.AddWithValue("@ContactDate", DateTime.Now);
            command.Parameters.AddWithValue("@DateToFollowUp", DateTime.Now.AddDays(1));
            command.Parameters.AddWithValue("@CreatorUserName", "EmailReceiver");
            command.Parameters.AddWithValue("@LastModifierUserName", "EmailReceiver");

            object res = command.ExecuteScalar();
            if (IsNumeric(res.ToString()) && Convert.ToInt64(res) > 0)
            {
                ret = Convert.ToInt64(res);
            }
            return ret;
        }


        private static long SendEmailAutoResponse(SqlConnection connection, EmailMessage message, string newMessage)
        {
            string body = newMessage;
            if (message.BodyText != null)
                body += Environment.NewLine + Environment.NewLine + Environment.NewLine + message.BodyText;

            string subject = "Lifeline counselling request ";
            subject += (message.EmailSubject == null || message.EmailSubject == "") ? "" : " - " + message.EmailSubject;
            //long ret = 0;

            //Send the auto-response to the Unrecognised Sender
            return InsertMessageAndMessageQueueItem(connection, MessageReceiveSingleton.FromEmail, message.EmailFrom, MessageReceiveSingleton.BccEmail
                , subject, body);
        }
        public static void SendErrorEmail(string message)
        {
            Data.InsertMessageAndMessageQueueItem(MessageReceiveSingleton.FromEmail, MessageReceiveSingleton.ErrorEmails, MessageReceiveSingleton.BccEmail, "LifeLine Email Receiver Error"
                        , "The LifeLine Email Receiver enouncountered an error:" + Environment.NewLine + Environment.NewLine + message);
        }

        private static long InsertMessageAndMessageQueueItem(string emailFrom, string emailTo, string emailBcc, string emailSubject, string emailBody)
        {
            using (SqlConnection connection = getSqlConnection())
            {
                return InsertMessageAndMessageQueueItem(connection, emailFrom, emailTo, emailBcc, emailSubject, emailBody);
            }
        }
        private static long InsertMessageAndMessageQueueItem(SqlConnection connection
            , string emailFrom, string emailTo, string emailBcc, string emailSubject, string emailBody)
        {

            long ret = 0;
            #region insert SQL
            string sql = " insert  into Message( " +
               " MessageTypeId, " +
           " [To], " +
           " CC, " +
           " BCC, " +
           " [From], " +
           " Subject, " +
           " Body, " +
           " IsBodyHtml, " +
           " RecipientIds, " +
           " Context, " +
           " DateQueuedToSend, " +
           //" DateArchived, " +
           //" UserIdArchivedBy, " +
           " CreatorUserName, " +
           " CreationTime, " +
           " LastModifierUserName, " +
           " LastModificationTime, " +
           //" DeleterUserName, " +
           //" DeletionTime, " +
           " TenantId, " +
           //" MemberId, " +
           //" RoleIdRecipients, " +
           //" UserIdRecipient, " +
           //" latestContactRecordId, " +
           //" DateRead, " +
           " IsArchive, " +
           " OrganisationId " +
           ") values ( " +
           " @MessageTypeId, " +
           " @To, " +
           " @CC, " +
           " @BCC, " +
           " @From, " +
           " @Subject, " +
           " @Body, " +
           " @IsBodyHtml, " +
           " @RecipientIds, " +
           " @Context, " +
           " @DateQueuedToSend, " +
           //// " @DateArchived, " +
           // " @UserIdArchivedBy, " +
           " @CreatorUserName, " +
           " @CreationTime, " +
           " @LastModifierUserName, " +
           " @LastModificationTime, " +
           // " @DeleterUserName, " +
           //" @DeletionTime, " +
           " @TenantId, " +
           //" @MemberId, " +
           //" @RoleIdRecipients, " +
           //" @UserIdRecipient, " +
           //" @latestContactRecordId, " +
           //" @DateRead, " +
           " @IsArchive, " +
           " @OrganisationId " +
           "); select @@identity; ";
            #endregion insert  SQL
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("MessageTypeId", 1);
            command.Parameters.AddWithValue("To", emailTo);
            command.Parameters.AddWithValue("CC", "");
            command.Parameters.AddWithValue("BCC", emailBcc);
            command.Parameters.AddWithValue("From", emailFrom);
            command.Parameters.AddWithValue("Subject", emailSubject);
            command.Parameters.AddWithValue("Body", emailBody);
            command.Parameters.AddWithValue("IsBodyHtml", 1);
            command.Parameters.AddWithValue("RecipientIds", "");
            command.Parameters.AddWithValue("Context", "");
            command.Parameters.AddWithValue("DateQueuedToSend", DateTime.Now);
            command.Parameters.AddWithValue("CreatorUserName", "EmailEngine");
            command.Parameters.AddWithValue("CreationTime", DateTime.Now);
            command.Parameters.AddWithValue("LastModifierUserName", "EmailEngine");
            command.Parameters.AddWithValue("LastModificationTime", DateTime.Now);
            command.Parameters.AddWithValue("TenantId", 1);
            command.Parameters.AddWithValue("IsArchive", 0);
            command.Parameters.AddWithValue("OrganisationId", 1);


            object res = command.ExecuteScalar();
            if (IsNumeric(res.ToString()) && Convert.ToInt64(res) > 0)
            {
                long newId = Convert.ToInt64(res);

                #region insert MessageQueueItem SQL
                sql = " insert  into MessageQueueItem( " +
                " MessageId, " +
                " [To], " +
                " BCC, " +
                " Subject, " +
                " Body, " +
                " IsBodyHtml, " +
                " SendAttemptMax, " +
                " CountSendAttempt, " +
                //" ErrorMessage, " +
                //" DateSent, " +
                //" Context, " +
                //" PersonId, " +
                //" EmployeeDeclarationId, " +
                //" ContextTable, " +
                //" ContextKeyName, " +
                //" ContextKey, " +
                //" ContextDateFieldName, " +
                //" CC, " +
                " TenantId, " +
                " OrganisationId, " +
                " UserNameModifiedBy " +
                ") values ( " +

                " @MessageId, " +
                " @To, " +
                " @BCC, " +
                " @Subject, " +
                " @Body, " +
                " @IsBodyHtml, " +
                " @SendAttemptMax, " +
                " @CountSendAttempt, " +
                //" @ErrorMessage, " +
                //" @DateSent, " +
                //" @Context, " +
                //" @PersonId, " +
                //" @EmployeeDeclarationId, " +
                //" @ContextTable, " +
                //" @ContextKeyName, " +
                //" @ContextKey, " +
                //" @ContextDateFieldName, " +
                //" @CC, " +
                " @TenantId, " +
                " @OrganisationId, " +
                " @UserNameModifiedBy " +
                "); select @@identity; ";
                #endregion insert  MessageQueueItem SQL
                command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("MessageId", newId);
                command.Parameters.AddWithValue("To", emailTo);
                command.Parameters.AddWithValue("BCC", emailBcc);
                command.Parameters.AddWithValue("Subject", emailSubject);

                command.Parameters.AddWithValue("Body", emailBody);
                command.Parameters.AddWithValue("IsBodyHtml", 1);
                command.Parameters.AddWithValue("SendAttemptMax", 5);
                command.Parameters.AddWithValue("CountSendAttempt", 0);
                command.Parameters.AddWithValue("TenantId", 1);
                command.Parameters.AddWithValue("OrganisationId", 1);
                command.Parameters.AddWithValue("UserNameModifiedBy", "Emailengine");


                object res1 = command.ExecuteScalar();
                if (IsNumeric(res1.ToString()) && Convert.ToInt64(res1) > 0)
                {
                    ret = Convert.ToInt64(res1);
                }
            }
            return ret;
        }

        public static string GetMessageText()
        {
            StringBuilder sb = new StringBuilder();
            string textFile = @"Email.html";
            if (File.Exists(textFile))
            {

                // Read a text file line by line.  
                string[] lines = File.ReadAllLines(textFile);
                foreach (string line in lines)
                {
                    sb.AppendLine(line);
                }

            }
            return sb.ToString();
        }

        private static long GetOrCreateIssue(SqlConnection connection, int memberId, EmailMessage message)
        {
            Int64 ret = 0;
            /* Get the most recent Issue */
            string sql = "select max(Id) from (select 0 as Id " +
                       "UNION select Id from Issue " +
                       "where MemberId=@MemberId and DateResolved is null " +
                       " ) as t1 " +
                       ""; //TODO: support up to 5 emails

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@MemberId", memberId);
            ret = Convert.ToInt64(command.ExecuteScalar());
            if (ret == 0) //Issue not found
            {
                sql = "insert into Issue ([MemberId], [Title], [Description]" +
                    ", [IssueTypeId], [DateDeadline], [IssueResolutionTypeId]" +
                    ", [DateCreated], [UserNameCreatedBy], [DateModified], [UserNameModifiedBy])" +
                    "values " +
                    " (@MemberId, @Title, @Description" +
                    ", @IssueTypeId, @DateDeadline, @IssueResolutionTypeId" +
                    ", getdate(), @UserNameCreatedBy, getdate(), @UserNameModifiedBy" +
                    "); select @@identity";
                command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@MemberId", memberId);
                command.Parameters.AddWithValue("@Title", message.EmailSubject);
                command.Parameters.AddWithValue("@Description", message.BodyText);
                command.Parameters.AddWithValue("@IssueTypeId", 11); //None
                command.Parameters.AddWithValue("@DateDeadline", DateTime.Now.AddDays(1));
                command.Parameters.AddWithValue("@IssueResolutionTypeId", 1); //Open
                command.Parameters.AddWithValue("@UserNameCreatedBy", "EmailReceiver");
                command.Parameters.AddWithValue("@UserNameModifiedBy", "EmailReceiver");

                ret = Convert.ToInt64(command.ExecuteScalar());
            }
            return ret;
        }

        private static int GetOrCreateMember(SqlConnection connection, EmailMessage message)
        {
            Int32 ret = 0;
            string sql = "select max(Id) from (select 0 as Id " +
                                                "UNION select Id from Member where Email=@Email ) as t1 "; //TODO: support up to 5 emails

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", message.EmailFrom.Replace(";", ""));
            ret = Convert.ToInt32(command.ExecuteScalar());

            //Not creating members this way at this stage: not enough info
            //if (ret == 0) //Member not found
            //{ 
            //    sql = "insert into Member ([MemberId], [Title], [Description]" +
            //        ", [IssueTypeId], [DateDeadline], [IssueResolutionTypeId]" +
            //        ", [DateCreated], [UserNameCreatedBy], [DateModified], [UserNameModifiedBy])" +
            //        "values " +
            //        " (@MemberId, @Title, @Description" +
            //        ", @IssueTypeId, @DateDeadline, @IssueResolutionTypeId" +
            //        ", getdate(), @UserNameCreatedBy, getdate(), @UserNameModifiedBy" +
            //        "); select @@identity";
            //    command = new SqlCommand(sql, connection);
            //    command.Parameters.AddWithValue("@MemberId", memberId);
            //    command.Parameters.AddWithValue("@Title", memberId);
            //    command.Parameters.AddWithValue("@Description", memberId);
            //    command.Parameters.AddWithValue("@IssueTypeId", memberId);
            //    command.Parameters.AddWithValue("@DateDeadline", memberId);
            //    command.Parameters.AddWithValue("@IssueResolutionTypeId", memberId);
            //    command.Parameters.AddWithValue("@UserNameCreatedBy", memberId);
            //    command.Parameters.AddWithValue("@UserNameModifiedBy", memberId);

            //    ret = Convert.ToInt64(command.ExecuteScalar());
            //}
            return ret;
        }

        public static bool IsNumeric(object inValue)
        {
            bool bValid = false;
            try
            {
                decimal d = Convert.ToDecimal(inValue);
                bValid = true;
            }
            catch
            {

            }
            return bValid;
        }


    }
}
