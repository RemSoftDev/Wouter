using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FoodJournalServiceWebRole.FoodJournalService
{
    [ServiceContract]
    public interface IFoodJournalService
    {

        //Returns the list of messages that should be considered processed
        [OperationContract]
        Identifier[] Push(string AppInstance, string Culture, string Version, Message[] messages);

        //[OperationContract]
        //Message[] Push2(string AppInstance, string Culture, string Version, Message[] messages);

    }

}
