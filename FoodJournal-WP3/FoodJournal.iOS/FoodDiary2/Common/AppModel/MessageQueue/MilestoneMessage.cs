using FoodJournal.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{
    [DataContract]
    public class MilestoneMessage
    {

        public MilestoneMessage() { }

        public MilestoneMessage(string milestone, string value)
        {
            Milestone = milestone;
            Value = value;
        }

        [DataMember]
        public string Milestone;

        [DataMember]
        public string Value;

    }
}
