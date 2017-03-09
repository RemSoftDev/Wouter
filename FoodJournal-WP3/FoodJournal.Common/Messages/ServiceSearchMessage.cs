using FoodJournal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FoodJournal.Messages
{
    [DataContract]
    public class ServiceSearchMessage
    {

        public ServiceSearchMessage() { }

        public ServiceSearchMessage(string Provider, string Query, int Count, int Milliseconds, List<SearchResultVM> Results, string RawResult)
        {
            this.Provider = Provider;
            this.Query = Query;
            this.Count = Count;
            this.Milliseconds = Milliseconds;
            foreach (var result in Results)
            {
                if (!string.IsNullOrEmpty(this.Results)) this.Results += "|";
                this.Results += result.Text;
            }
            this.RawResult = RawResult;
        }

        [DataMember(Order = 0)]
        public string Provider;

        [DataMember(Order = 1)]
        public string Query;

        [DataMember(Order = 2)]
        public int Count;

        [DataMember(Order = 3)]
        public int Milliseconds;

        [DataMember(Order = 4)]
        public string Results;

        [DataMember(Order = 5)]
        public string RawResult;

    }
}
