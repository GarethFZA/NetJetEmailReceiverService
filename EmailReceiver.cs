using Charismatech.MessageQueueClasses;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace LifeLineEmailReceiverService
{
    public partial class EmailReceiver : ServiceBase
    {
        public EmailReceiver()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToEventLog("Starting with an interval of " + ConfigurationManager.AppSettings["TimerInterval"] + "ms");
            StartBackgroundThread(delegate { ReceiveEmail(); });
        }

        protected override void OnStop()
        {
        }

        public static void StartBackgroundThread(ThreadStart threadStart)
        {
            if (threadStart != null)
            {
                Thread thread = new Thread(threadStart);
                thread.IsBackground = true;
                thread.Start();
            }
        }
        public void ReceiveEmail()
        {
            while (true)
            {
                MessageReceiveSingleton.Instance.ProcessEmailsIMAP();
                //MessageReceiveSingleton.Instance.ProcessEmailsPOP3();
                System.Threading.Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]));  //Block the thread for a while
            }

        }


        static void WriteToEventLog(string sEvent)
        {
            Data.WriteToEventLog(sEvent);
        }
    }
}
