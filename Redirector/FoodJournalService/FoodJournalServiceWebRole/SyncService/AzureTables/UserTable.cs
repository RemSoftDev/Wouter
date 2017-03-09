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
    public class UserTable
    {

        private CloudTable table;

        public UserTable()
        {
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=fooddata;AccountKey=rL+OPTL66egMyT8BjqzNg3a2o76pworU3RnmPjgYb3U0pHhUpQLRb3J0gVHAxmd/4pYw17rY7vgX2ZAY0+HpuQ=="); 
            
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            table = tableClient.GetTableReference("UserData");
            table.CreateIfNotExists();

        }


        /// <summary>
        /// If this is a new user, returns the list of instances that should be linked.
        /// If this is an existing user, it returns null.
        /// </summary>
        public List<Guid> SaveUser(String UserID, Guid AppinstanceID, DateTime ServerTime)
        {

            if (!UserRow.IsValidUserID(UserID)) return null;

            var current = GetInstances(UserID);

            if (!current.Contains(AppinstanceID))
            {
                var batchOp = new TableBatchOperation();
                batchOp.Insert(new UserRow(UserID, AppinstanceID) { ServerTime = ServerTime });
                table.ExecuteBatch(batchOp);

                current.Add(AppinstanceID);
            }

            return current;
        }

        public List<Guid> GetInstances(String UserID)
        {

            if (UserRow.GetClass(UserID) == UserRow.UserIDClass.none) return new List<Guid>();

            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, UserID);
            
            string alternative = UserRow.GetAlternativeUserID(UserID);
            if (alternative != null)
                query = TableQuery.CombineFilters(query, TableOperators.Or,
                                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, alternative));

            var exQuery = new TableQuery<UserRow>().Where(query);
            var results = table.ExecuteQuery<UserRow>(exQuery).ToList();

            if (results.Count > 20)
                return new List<Guid>(); // if this is a very common userID, its probably not a unique user

            if (results.Count > 0)
                return results.Select(x => x.AppinstanceID).ToList();

            return new List<Guid>();
        }

    }
}
