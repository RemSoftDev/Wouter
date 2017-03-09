using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FoodJournalServiceWebRole.FoodJournalService
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string Key;

        [DataMember]
        public string MessageType;
        
        [DataMember]
        public string Body;
    }

    [DataContract]
    public class Identifier
    {
        [DataMember]
        public string Key;
    }
}