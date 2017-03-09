using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{
    [DataContract]
    public class ExceptionMessage
    {

        public ExceptionMessage() { }

        public ExceptionMessage(Exception ex,string CaughtIn)
        {
            Message = ex.Message;
            this.CaughtIn = CaughtIn;
            if (ex.InnerException != null) InnerException = ex.InnerException.Message;
            StackTrace = ex.StackTrace;
        }

		public ExceptionMessage(String Message,string CaughtIn, string InnerException, String StackTrace)
		{
			this.Message = Message;
			this.CaughtIn = CaughtIn;
			this.InnerException = InnerException;
			this.StackTrace = StackTrace;
		}


        [DataMember]
        public string Message;

        [DataMember]
        public string CaughtIn;

        [DataMember]
        public string InnerException;

        [DataMember]
        public string StackTrace;

    }
}
