using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;

namespace FoodJournalServiceWebRole.SyncService
{
    public class InstanceTable
    {

        /// User sync works as follows:
        /// 
        /// UserID Format (Android) : [DeviceID]-[AndroidId]
        /// 
        /// When a new install happens, the app sends a "Register UserID" message.
        ///  * The UserID, together with the instanceID is stored in the User Table.
        ///  * Any previous appids that were registered with that userID are determined, and the latests one is stored in the "to be sent to app" queue:
        ///     -> Sync [old instance ID] / StartDate = DateTime.MinValue
        ///     
        /// * When the app receives this message, sends back a message that the message is received -> Removes it from the "to be sent to the app" queue
        /// 
        /// * Other control messages
        /// * Delete all entries
        /// * Mark as paid? (We could just do this by updating the appstats?)
        /// 
        /// The App will only ever have to sync data from other AppInstances.
        /// The UserID Should be compatible though (either by it being the same, or by it having been connected prior)
        /// 
        /// The service would be able to tell the app when a sync is needed.
        /// The way the service knows is by looking up the AppInstance's (or Users) connections. When any of the connected instances are updated after Latest sync, a pull is needed.
        /// "Latest pull" can be stored inside the app, or inside the service
        /// It could also be determined on the storage of an update. <- this may be the best way to do it, but at least for "re-install" scenario, we'll use thisone.
        /// 
        /// 
        /// Communication:
        /// * On first install -> Register the userid / appid combination
        /// * On start of app -> Check if new sync is needed -> Don't do it at every communication, bc its not needed (only for testing)
        /// 
        /// * On sync -> register sync needed on all other apps | 
        ///         This can be an agent job, although may not provide much benefit. We can store all syncids with the appid, and 
        ///         usually won't have many more. 1 extra select shouldnt bring down the server, and since the app isn't waiting on
        ///         it should be fine. No need to reply with "updates pending" or so, we'll just do that few x per app/day
        /// 


        /// Lets only sync on the start of the app.
        /// 1. We send a list of the know relationships
        ///     If this instance is not know, we'll find old versions based on userID, and sync it fully. We'll store the tree of related apps with the instance (and update all relationship for hose instances as well)
        ///     If the instance is known, we'll find all "last changed" times for all related instances. We'll send back all changes for the whole tree, order by servertime, and chunked.


        private CloudTable table;

        public InstanceTable()
        {
            
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("FoodDataStorageConnectionString"));

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=fooddata;AccountKey=rL+OPTL66egMyT8BjqzNg3a2o76pworU3RnmPjgYb3U0pHhUpQLRb3J0gVHAxmd/4pYw17rY7vgX2ZAY0+HpuQ=="); 
            
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            table = tableClient.GetTableReference("InstanceData");
            table.CreateIfNotExists();

        }

        public void SaveLeaves(List<Leaf> leaves)
        {
            var batchOp = new TableBatchOperation();

            foreach (Leaf leaf in leaves)
                batchOp.InsertOrReplace(leaf);

            var result = table.ExecuteBatch(batchOp);

        }

        public void SaveInstanceInfo(InstanceInfo instanceInfo)
        {
            var batchOp = new TableBatchOperation();
            batchOp.InsertOrMerge(instanceInfo);
            var result = table.ExecuteBatch(batchOp);
        }


        public List<Leaf> GetChanges(Guid InstanceID, DateTime SinceServerTime)
        {
            //TableOperation retrieveOperation = TableOperation.Retrieve<Leaf>(InstanceID.ToString(), "All");

            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, InstanceID.ToKey());
            query = TableQuery.CombineFilters(query, TableOperators.And,
                                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, InstanceInfo.ROWKEY));
            
            if (SinceServerTime > DateTime.MinValue)
                query = TableQuery.CombineFilters(query, TableOperators.And, 
                                                    TableQuery.GenerateFilterConditionForDate("ServerTime", QueryComparisons.GreaterThanOrEqual, SinceServerTime));

            var exQuery = new TableQuery<Leaf>().Where(query);
            var results = table.ExecuteQuery<Leaf>(exQuery);

            return results.OrderBy(x=> x.ServerTime).ToList();
        }

        public Leaf GetLeaf(Guid InstanceID, String RowKey)
        {
            
            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, InstanceID.ToKey());            
            query = TableQuery.CombineFilters(query, TableOperators.And, 
                                                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey));

            var exQuery = new TableQuery<Leaf>().Where(query);
            var results = table.ExecuteQuery<Leaf>(exQuery);

            return results.FirstOrDefault();
        }

        public InstanceInfo GetInstanceInfo(Guid InstanceID)
        {

            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, InstanceID.ToKey());
            query = TableQuery.CombineFilters(query, TableOperators.And,
                                                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, InstanceInfo.ROWKEY));

            var exQuery = new TableQuery<InstanceInfo>().Where(query);
            var results = table.ExecuteQuery<InstanceInfo>(exQuery);

            return results.FirstOrDefault();

        }

        /// <summary>
        /// Finds the AppInstance that has the latest change from a set
        /// </summary>
        public String GetLatestChangeID(List<Guid> set)
        {

            if (set == null || set.Count == 0) return null;

            string query="";
            
            foreach (Guid instance in set)
            {
                string SubQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, instance.ToKey());

                if (query.Length == 0)
                    query = SubQuery;
                else
                    query = TableQuery.CombineFilters(query, TableOperators.Or, SubQuery);
            }

            query = TableQuery.CombineFilters(query, TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, InstanceInfo.ROWKEY)); // Note that AND Takes precedence on or

            var exQuery = new TableQuery<Leaf>().Where(query);
            var results = table.ExecuteQuery<Leaf>(exQuery).OrderByDescending(x => x.ServerTime).Take(1);

            var latest = results.FirstOrDefault();

            if (latest == null)
                return null;

            // could be null (but very unlikely)
            return latest.PartitionKey;
        }


    }
}
