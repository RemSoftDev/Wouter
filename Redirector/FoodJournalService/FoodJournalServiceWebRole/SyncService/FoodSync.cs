using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FoodJournalServiceWebRole.SyncService
{
    public class FoodSyncController : ApiController
    {

        public const int PAGESIZE = 10;

        // GET api/<controller>
        public IEnumerable<Package> Get(Guid id, DateTime ClientTime, String Version = null, String UserID = null, String LastSync = null, int Skip = 0)
        {

            List<Package> result = new List<Package>();
            TimeSpan clientLag = ClientTime.GetClientLag();

            // Get is called on app boot.

            InstanceTable table = new InstanceTable();
            InstanceInfo instanceInfo = table.GetInstanceInfo(id);

            if (id.ToKey() == "00000000-0000-0000-0000-000000000003" && LastSync == null)
                instanceInfo = null;

            if (instanceInfo != null && !instanceInfo.HasConnectedApps)
                return result;

            if (instanceInfo != null && instanceInfo.UserID != null && UserID != null && instanceInfo.UserID != UserID)
                return result;

            if (instanceInfo != null && UserID == null)
                UserID = instanceInfo.UserID;

            if (String.IsNullOrEmpty(UserID))
            {
                // V15: we need to get the UserID from AppStats

                Leaf AppStats = table.GetLeaf(id, "AppStats-All");
                if (AppStats == null)
                    return result;

                var r = new System.Text.RegularExpressions.Regex(@"\<UserId\>(?<userid>[^\<]*)\<\/UserId\>");
                var m = r.Match(AppStats.Contents);
                if (!m.Success)
                    return result;

                UserID = m.Groups["userid"].Value;              
                if (String.IsNullOrEmpty(UserID))
                    return result;

                if (instanceInfo != null)
                    instanceInfo.UserID = UserID;

            }

            ConnectedAppSet set;
            if (instanceInfo != null)
            {
                set = new ConnectedAppSet(instanceInfo);
            }
            else
            {
                instanceInfo = new InstanceInfo(id, UserID);

                UserTable users = new UserTable();
                var SyncSet = users.SaveUser(UserID, id, DateTime.Now);

                if (String.IsNullOrEmpty(Version))
                {
                    // Old versions dont merge well, so don't return anything.
                    table.SaveInstanceInfo(instanceInfo);
                    return result;
                }

                set = new ConnectedAppSet(instanceInfo);

                if (SyncSet != null)
                {
                    // New UserID, first time its registered. 

                    if (SyncSet.Contains(id))
                        SyncSet.Remove(id);

                    string latest = table.GetLatestChangeID(SyncSet);

                    // Linking to a previous instance here:
                    if (latest != null)
                        set.Add(new Guid(latest));

                }
            }

            set.UpdateLastSync(LastSync, clientLag);

            List<Leaf> changes = new List<Leaf>();
            foreach (var App in set)
            {
                if (App.InstanceGuid != id) // don't get changes for "myself"
                    changes.AddRange(table.GetChanges(App.InstanceGuid, App.LastestChange));
            }

            foreach (var leaf in changes.OrderBy(x => x.ServerTime).Skip(Skip).Take(PAGESIZE))
                result.Add(Package.FromLeaf(leaf, clientLag));

            if (changes.Count > PAGESIZE + Skip)
                result.Add(new Package() { Type = "Continuation" });

            instanceInfo.HasConnectedApps = set.HasApps;
            table.SaveInstanceInfo(instanceInfo);

            return result;
        }

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<controller>
        public string[] Post(Guid id, DateTime ClientTime, [FromBody]Package[] packages)
        {
            List<string> result = new List<string>();

            TimeSpan clientLag = ClientTime.GetClientLag();

            List<Leaf> leaves = new List<Leaf>();
            foreach (Package package in packages)
                leaves.Add(package.ToLeaf(id, clientLag));

            if (leaves.Count>0)
            {
                InstanceTable table = new InstanceTable();
                table.SaveLeaves(leaves);
            }

            return result.ToArray();
        }

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}