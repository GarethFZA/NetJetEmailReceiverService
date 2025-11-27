using System;

namespace LifeLineEmailReceiverService
{
    public class EmailMessage
    {
        public DateTime EmailDate { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailSubject { get; set; }
        public string BodyText { get; set; }
    }
}
