using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodJournalServiceWebRole.SyncService
{
    public class Package
    {

        public Guid AppInstance { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Contents { get; set; }
        public DateTime ClientTime { get; set; }

        public Leaf ToLeaf(Guid AppinstanceID, TimeSpan ClientLag) { return new Leaf(AppinstanceID, Type, Key, Contents, ClientTime + ClientLag); }
        public static Package FromLeaf(Leaf leaf, TimeSpan ClientLag) { return new Package() { AppInstance = new Guid(leaf.PartitionKey), Type = leaf.Type, Key = leaf.Key, Contents = leaf.Contents, ClientTime = leaf.ServerTime.ToClientTime(ClientLag) }; }
    }

}