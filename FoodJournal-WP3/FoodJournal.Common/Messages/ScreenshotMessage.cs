using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{
    [DataContract]
    public class ScreenshotMessage
    {

        public ScreenshotMessage() { }

        public ScreenshotMessage(string Screen, string Contents)
        {
            this.Screen = Screen;
            this.Contents = Contents;
        }

        [DataMember]
        public string Screen;

        [DataMember]
        public string Contents;

    }
}
