using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FoodJournalServiceWebRole.FoodDatabaseService
{

    [ServiceContract]
    public interface IFoodDatabaseService
    {

        [OperationContract]
        QueryResponse Query(string AppInstance, string Secret, string Culture, string Query, string RequestData);

    }

}
