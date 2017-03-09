using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournalServiceWebRole.SyncService
{

    public class Leaf : TableEntity
    {

        private string type;
        private string key;

        public Leaf() { }
        public Leaf(Guid AppinstanceID) { this.PartitionKey = AppinstanceID.ToKey(); }
        public Leaf(Guid AppinstanceID, String Type, String Key, String Contents, DateTime ServerTime) { this.PartitionKey = AppinstanceID.ToKey(); this.type = Type; this.key = Key; setRowKey(); this.Contents = Contents; this.ServerTime = ServerTime; }

        private void setRowKey() { this.RowKey = type + "-" + key; }

        public string Type { get { return type; } set { type = value; setRowKey(); } }
        public string Key { get { return key; } set { key = value; setRowKey(); } }
        public string Contents { get; set; }
        public DateTime ServerTime { get; set; }

    }

}
