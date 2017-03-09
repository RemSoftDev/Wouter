using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DailyJournalServiceRole
{

    [ServiceContract]
    public interface IDailyJournalService
    {

        [OperationContract]
        bool Feedback(Guid appinstance, string email, string feedback);

        [OperationContract]
        bool ExceptionReport(Guid appinstance, string report);

        [OperationContract]
        bool PerformanceReport(Guid appinstance, string report);

        [OperationContract]
        bool EmailReport(Guid appinstance, string email, string report, int days);

        [OperationContract]
        bool Query(Guid appinstance, string query, string locale, int USDACount, int NutritionIXCount, int FatsecretCount, string details);

        [OperationContract]
        bool AppInstance(Guid appinstance, string istrial, string wpmodel, string wpversion, string locale, string timezone, string themecolor, string details);

    }

/*
    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    } 
 */

}
