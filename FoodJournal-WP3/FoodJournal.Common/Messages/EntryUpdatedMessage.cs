using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{
    [DataContract]
    public class EntryUpdatedMessage
    {

        public EntryUpdatedMessage() { }

        public EntryUpdatedMessage(Entry entry)
        {
            try
            {
                Date = entry.Date;
                Period = entry.Period;
                TotalAmount = entry.TotalAmount.ToStorageString();
                Nutrition = entry.NutritionSummary;

				ItemText = entry.Item.Text;
                ItemSource = entry.Item.SourceID;
                ItemDescription = entry.Item.Description;
                ItemPropertyValues = entry.Item.Values.ToString();
                ItemServingSizes = entry.Item.ServingSizes.ToString();
                ItemLastModifed = entry.Item.LastChanged;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        [DataMember]
        public DateTime Date;

        [DataMember]
        public Period Period;

        [DataMember]
        public string TotalAmount;

        [DataMember]
        public string Nutrition;

        [DataMember]
        public string ItemText;

        [DataMember]
        public string ItemSource;

        [DataMember]
        public string ItemDescription;

        [DataMember]
        public string ItemPropertyValues;

        [DataMember]
        public string ItemServingSizes;

        [DataMember]
        public DateTime ItemLastModifed;


    }
}
