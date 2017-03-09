using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using System.IO;
using System.Xml;
using System.ServiceModel;
using System.Xml.Serialization;
#endif


namespace FoodJournal.Model
{

    public enum AdProvider
    {
        Platform,
        Secondary,
        Free
    }
    
    [DataContract]
    public class CultureSettings
    {

        [DataMember]
        public string Culture { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember(Name = "TranslationRequests")]
        public TranslationRequest[] TranslationRequests { get; set; }

        [DataMember]
        public AdProvider[] AdProviders { get; set; }

        [DataMember]
        public FeedbackSettings[] FeedbackSettings { get; set; }

        [DataMember]
        public Sale[] Sales2 { get; set; }

        [DataMember]
        public string ExpiredVersion { get; set; }

        [DataMember]
        public int ExpiredMonths { get; set; }

		[DataMember]
		public int FreeTrialDays {get; set; }

		[DataMember]
		public string BackupDBVersion { get; set; }

#if DEBUG
        public static void Test()
        {

            CultureSettings c = new CultureSettings();
            c.FeedbackSettings = new FeedbackSettings[] { FoodJournal.Model.FeedbackSettings.Default };
            c.Sales2 = new Sale[] { new Sale() { ID="3s", SessionIDStart=3, SessionIDEnd=3, Title="1 time offer", SubTitle="25% off, click Ok to buy",  productId ="premium"}, 
                                   new Sale() { ID="2d", DayIDStart=2, DayIDEnd=2, Title="1 day sale", SubTitle="10% off, click Ok to buy",  productId ="premium"},
                                   new Sale() { ID="christmas", DateStart=new DateTime(2014,12,24), DateEnd=new DateTime(2014,12,26), Title="christmas sale", SubTitle="10% off, click Ok to buy",  productId ="premium"}};

            // c.ExpiredVersion = FoodJournal.AppModel.AppStats.Current.Version;
            c.ExpiredMonths= 3;

            c.AdProviders = new AdProvider[] { AdProvider.Secondary };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(c.GetType());

                var settings = new XmlWriterSettings { Indent = true };
                using (var w = XmlWriter.Create(memoryStream, settings))
                    serializer.WriteObject(w, c);

                //serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;

                string result = new StreamReader(memoryStream).ReadToEnd();
                System.Diagnostics.Debug.WriteLine(result);
            }

        }
#endif
    }

    [DataContract]
    public class FeedbackSettings
    {

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public int DaysTillStart { get; set; }
        [DataMember]
        public int NumberOfRequests { get; set; }
        [DataMember]
        public int NumberOfReAttempts { get; set; }
        [DataMember]
        public int NumberOfSessionsBetween { get; set; }

        public static FeedbackSettings Default { get { return new FeedbackSettings() { ID = "A", DaysTillStart = 3, NumberOfRequests = 3, NumberOfReAttempts = 1, NumberOfSessionsBetween = 7 }; } }

    }

    [DataContract]
    public class Sale
    {

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string SubTitle { get; set; }

        [DataMember]
        public int SessionIDStart { get; set; }
        [DataMember]
        public int SessionIDEnd { get; set; }

        [DataMember]
        public int DayIDStart { get; set; }
        [DataMember]
        public int DayIDEnd { get; set; }

        [DataMember]
        public DateTime DateStart { get; set; }
        [DataMember]
        public DateTime DateEnd { get; set; }

        [DataMember]
        public int NumberOfSessionsBetween { get; set; }

        [DataMember]
        public string productId { get; set; }        

    }

}
