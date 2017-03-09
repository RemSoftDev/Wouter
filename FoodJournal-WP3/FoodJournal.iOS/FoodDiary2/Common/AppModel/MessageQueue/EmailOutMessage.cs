using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;
#endif

namespace FoodJournal.Messages
{
    [DataContract]
    public class EmailOutMessage
    {

        public EmailOutMessage() { }

        public EmailOutMessage(string To, string Subject, string BodyHtml)
        {
            this.To= To;
            this.Subject = Subject;
            this.BodyHtml = BodyHtml;
        }

        [DataMember]
        public string To;

        [DataMember]
        public string Subject;

        [DataMember]
        public string BodyHtml;

    }
}
