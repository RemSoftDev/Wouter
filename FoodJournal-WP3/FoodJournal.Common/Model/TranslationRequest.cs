using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Model
{
    [DataContract(Name="Translation")]
    public class TranslationRequest
    {

        [DataMember(Name="Auto")]
        public string AutoTranslation { get; set; }

        [DataMember]
        public string Corrected { get; set; }

        [DataMember]
        public string English { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsGood { get; set; }

    }
}
