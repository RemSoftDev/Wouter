using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournalServiceWebRole.SyncService
{

    public class InstanceInfo : TableEntity
    {

        public const string ROWKEY = "InstanceInfo";

        private string type;
        private string key;

        public InstanceInfo() { }
        public InstanceInfo(Guid AppinstanceID, String UserId) { this.PartitionKey = AppinstanceID.ToKey(); this.RowKey = ROWKEY; this.UserID = UserId; this.ServerTime = DateTime.Now; }

        public bool HasConnectedApps { get; set; }
        public string UserID { get; set; }
        public DateTime ServerTime { get; set; }
        public String ConnectedApps { get; set; }

    }

}
