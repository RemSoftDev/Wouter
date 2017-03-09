using FoodJournal.Model;
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
    public class TranslationsMessage
    {

        public TranslationsMessage() { }

        public TranslationsMessage(string Comments, TranslationRequest[] translations)
        {
            this.Comments= Comments;
            this.Translations= translations;
        }

        [DataMember]
        public string Comments;

        [DataMember]
        public TranslationRequest[] Translations;

    }
}
