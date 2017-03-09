using FoodJournal.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace FoodJournalServiceWebRole.FoodJournalService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FoodJournalService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select FoodJournalService.svc or FoodJournalService.svc.cs at the Solution Explorer and start debugging.
    public class FoodJournalService : IFoodJournalService
    {

        private const int SettingsTimeoutHours = 1;
        private static DateTime LastSettingsQuery = DateTime.MinValue;
        private static Dictionary<string, DateTime> SettingsUpdated = new Dictionary<string, DateTime>();
        private static Dictionary<string, string> Settings = new Dictionary<string, string>();

        public Identifier[] Push(string AppInstance, string Culture, string Version, Message[] messages)
        {
            try
            {

                DateTime lastSettingsDate = DateTime.MaxValue;

                if (Version != null)
                {
                    var pos = Version.IndexOf('?');
                    if (pos > 0)
                    {
                        lastSettingsDate = DateTime.MinValue;
                        DateTime.TryParse(Version.Substring(pos + 1), out lastSettingsDate);
                        Version = Version.Substring(0, pos);
                    }
                }

                var result = new List<Identifier>();
                using (var db = new DailyLog_dbEntities())
                {
                    //db.Database.CommandTimeout = 120;
                    foreach (var msg in messages)
                    {

                        try{
                            if (msg.MessageType == "MilestoneMessage" && msg.Body.Contains("New Install"))
                            {
                                OperationContext context = OperationContext.Current;
                                MessageProperties prop = context.IncomingMessageProperties;
                                RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                                string ip = endpoint.Address;
                                msg.Body = msg.Body.Replace("<Value>" + Version + "</Value>", "<Value>" + ip + "</Value>");
                            }
                        } catch {}

                        var MQ = new MessageQueue();
                        MQ.AppInstance = AppInstance;
                        MQ.ClientKey = msg.Key;
                        MQ.MessageType = msg.MessageType;
                        MQ.IsServerIncoming = true;
                        MQ.Message = msg.Body;
                        MQ.Created = DateTime.Now;
                        MQ.Version = Version;
                        MQ.Culture = Culture;
                        db.MessageQueues.Add(MQ);
                        result.Add(new Identifier() { Key = msg.Key });
                    }
                    db.SaveChanges();
                }

                foreach (var msg in messages)
                    if (msg !=null && msg.MessageType == "EmailOutMessage")
                        ProcessEmailMessage(AppInstance, msg);

                if (lastSettingsDate < DateTime.MaxValue)
                {
                    var settings = GetSettingsUpdate(Culture, lastSettingsDate);
                    if (settings != null)
                        result.Add(new Identifier() { Key = settings });
                }

                return result.ToArray();
            }
            catch (Exception ex) { ServerException.Report(ex); }
            return null;
        }

        private void ProcessEmailMessage(string AppInstance, Message msg)
        {
            try
            {

                // make sure this is not a dupe
                using (var db = new DailyLog_dbEntities())
                {
                    var rows = db.MessageQueues.Where(q => q.AppInstance == AppInstance && q.ClientKey == msg.Key);
                    //if (rows.Count() == 0)
                    //{
                    //    ServerException.Report(new Exception(string.Format("EmailOut message not found in the q: {0} / {1}", AppInstance, msg.Key)));
                    //    return; // try the next time (to make sure no dupe e-mails are sent)
                    //}

                    //if (rows.Where(q => q.Processed != null).Count() > 0)
                    //    return; // already processed

                    EmailOutMessage email = (EmailOutMessage)Util.Deserialize(msg.Body, typeof(EmailOutMessage));
                    EMail.Send(email);

                    rows.First().Processed = DateTime.Now;
                    db.SaveChanges();

                }

            }
            catch (Exception ex) { ServerException.Report(ex); }
        }


        private string GetSettingsUpdate(string Culture, DateTime Lastupdate)
        {
            try
            {
                if (LastSettingsQuery.AddHours(SettingsTimeoutHours) < DateTime.Now)
                {
                    // requery DB for settings
                    using (var db = new DailyLog_dbEntities())
                    {
                        foreach (var setting in db.Settings.ToList())
                        {
                            SettingsUpdated[setting.Culture] = setting.LastUpdated;
                            Settings[setting.Culture] = setting.Settings;
                        }
                    }

                    LastSettingsQuery = DateTime.Now;
                }

                if (SettingsUpdated.ContainsKey(Culture) && SettingsUpdated[Culture] > Lastupdate)
                    return Settings[Culture];

            }
            catch (Exception ex) { ServerException.Report(ex); }
            return null;
        }

        //public Message[] Pull(string AppInstance, Identifier[] processed)
        //{
        //    try
        //    {

        //        if (processed == null) processed = new Identifier[0];

        //        var result = new List<Message>();
        //        using (var db = new DailyLog_dbEntities())
        //        {

        //            // we could implement some chunking / throttling here, perhaps also sorting
        //            var q = db.MessageQueues.Where(m => (m.AppInstance == AppInstance
        //                                                && m.IsServerIncoming == false
        //                                                && m.Processed == null));

        //            foreach (var MQ in q)
        //            {

        //                if (processed.Count(i => i.Key == MQ.ClientKey) > 0)
        //                {
        //                    // mark the msg as processed by the client, dont return it
        //                    MQ.Processed = DateTime.Now;
        //                }
        //                else
        //                {
        //                    // found an unprocessed message, add to result
        //                    var msg = new Message();
        //                    msg.Key = MQ.Id.ToString();
        //                    msg.MessageType = MQ.MessageType;
        //                    msg.Body = MQ.Message;
        //                    result.Add(msg);
        //                }
        //            }
        //            if (db.ChangeTracker.HasChanges())
        //                db.SaveChanges();
        //        }
        //        return result.ToArray();
        //    }
        //    catch (Exception ex) { ServerException.Report(ex); }
        //    return null;
        //}

    }
}
