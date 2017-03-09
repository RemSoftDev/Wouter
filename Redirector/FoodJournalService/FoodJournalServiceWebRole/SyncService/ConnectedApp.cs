using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodJournalServiceWebRole.SyncService
{

    /// <summary>
    /// Contains information about the data for another app instance
    /// </summary>
    public class ConnectedApp
    {
        public ConnectedApp(String code) { var parts = code.Split('='); if (parts.Length != 2) throw new ArgumentException(); InstanceGuid = Guid.Parse(parts[0]); LastestChange = DateTime.Parse(parts[1]); }
        public ConnectedApp(Guid InstanceGuid, DateTime LastestChange) { this.InstanceGuid = InstanceGuid; this.LastestChange = LastestChange; }

        public Guid InstanceGuid;
        public DateTime LastestChange;

        public override string ToString() { return InstanceGuid.ToKey() + "=" + LastestChange.ToString("s"); }
    }

    public class ConnectedAppSet : IEnumerable<ConnectedApp>
    {

        private List<ConnectedApp> set = new List<ConnectedApp>();

        private InstanceInfo loadset;

        public ConnectedAppSet(InstanceInfo loadset) {
            if (loadset.ConnectedApps != null)
                foreach (string part in loadset.ConnectedApps.Split('|'))
                    if (part.Length >0 )
                        set.Add(new ConnectedApp(part));
            this.loadset = loadset;
        }

        public void Add(Guid newconnection)
        {
            foreach (var app in set)
                if (app.InstanceGuid == newconnection)
                    return;
            set.Add(new ConnectedApp(newconnection, DateTime.MinValue));
            loadset.ConnectedApps = ToString();
        }

        public void UpdateLastSync(String LastSync, TimeSpan ClientLag)
        {

            if (String.IsNullOrEmpty(LastSync)) return;

            foreach (var part in LastSync.Split('|'))
            {

                Guid id = Guid.Parse(part.Split('=')[0]);
                DateTime lastSync = DateTime.Parse(part.Split('=')[1]).ToServerTime(ClientLag);

                foreach (var app in set)
                    if (app.InstanceGuid == id)
                        if (app.LastestChange < lastSync)
                            app.LastestChange = lastSync;


            }

            loadset.ConnectedApps = ToString();

        }

        public bool HasApps { get { return set.Count > 0; } }


        public IEnumerator<ConnectedApp> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        public override string ToString()
        {
            String result="";
            foreach (var app in set)
            {
                if (result.Length > 0) result += "|";
                result += app.ToString();
            }
            return result;  
        }

    }

}