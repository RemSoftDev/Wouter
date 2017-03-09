using FoodJournal.AppModel;
using FoodJournal.AppModel.UI;
using FoodJournal.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FoodJournal.Logging
{

	public class EventData
	{

		public string Name;
		private const int MaxLength = 50;

		public DateTime LastStart = DateTime.MinValue;
		private string Line;
		private int count = 0;

		public EventData(string Name){this.Name = Name;}

		public void Add (string Value)
		{
			if (count > MaxLength)
				return;
			if (count == MaxLength)
				Line += ",*";
			else if (count > 0)
				Line += "," + Value;
			else
				Line = Value;
			count++;				
		}

		public override string ToString ()
		{
			return string.Format ("{0}:{1}", Name, Line);
		}

	}

    [DataContract]
    public class SessionLog
    {

        private static DateTime FirstRecordableTime = DateTime.Now;

        [DataMember]
        public DateTime Time = DateTime.Now;

        [DataMember]
		public String Entries { get { return PerformanceSummary() + EntryBuilder.ToString() + LogFooter; } set { } }
        public StringBuilder EntryBuilder = new StringBuilder();
        public string LogFooter { get { return string.Format("{0},{1},{2},{3}", LogEntryType.Footer.ToString(), (DateTime.Now - FirstRecordableTime).Seconds, "Open Screen", null); } }

        private static SessionLog me = new SessionLog();
        private SessionLog() { }

        public static PerformanceScope NewScope(string Name, string Value)
        {
            return new PerformanceScope(Name, Value);
        }

        #region RecordEntry

        public void RecordEntry(LogEntryType type, string key, string value1, string value2, string value3, string value4)
        {

            try
            {

                string line = string.Format("{0},{1},{2}", type.ToString(), (DateTime.Now - FirstRecordableTime).Seconds, key);

                int commas = 0;
                if (value1 != null) commas = 1;
                if (value2 != null) commas = 2;
                if (value3 != null) commas = 3;
                if (value4 != null) commas = 4;

                if (commas >= 1) line += "," + String(value1);
                if (commas >= 2) line += "," + String(value2);
                if (commas >= 3) line += "," + String(value3);
                if (commas >= 4) line += "," + String(value4);

                EntryBuilder.AppendLine(line);

            }
            catch (Exception ex) { EntryBuilder.AppendLine("ERROR Writing Log Entry: " + ex.Message); }

        }

        protected string String(string value)
        {
            if (value == null) return "";
            value = value.Replace(",", "|");
            if (value.Contains("\"") || value.Contains("\n"))
                return string.Format("\"{0}\"", value.Replace("\"", "\"\""));
            return value;
        }

        #endregion

		[Conditional("DEBUG")]
		public static void Debug(string value) { 

#if !WINDOWS_PHONE
			if (FoodJournal.Runtime.Navigate.navigationContext != null)
			FoodJournal.Runtime.Navigate.navigationContext.RunOnUiThread (() => {
				Android.Util.Log.WriteLine (Android.Util.LogPriority.Debug, "SessionLog", value);
			});
#endif
		
		}

        public static void RecordMilestone(string milestone, string value) { me.RecordEntry(LogEntryType.Milestone, milestone, value, null, null, null); Messages.MessageQueue.Push(new Messages.MilestoneMessage(milestone, value)); }
        public static void ReportException(Exception ex, string CaughtIn) { me.RecordEntry(LogEntryType.Exception, CaughtIn, ex.Message, null, null, null); Messages.MessageQueue.Push(new Messages.ExceptionMessage(ex, CaughtIn)); }
		public static void ReportEscalation(string message, string caughtin, string details) { me.RecordEntry(LogEntryType.Exception, message, null, null, null, null); Messages.MessageQueue.Push(new Messages.ExceptionMessage(message, caughtin, null, details)); }
        public static void RecordTrace(string message) { me.RecordEntry(LogEntryType.Trace, message, null, null, null, null); }
        public static void RecordTraceValue(string message, string value) { me.RecordEntry(LogEntryType.Trace, message, value, null, null, null); }
        public static void RecordTraceValue(string message, string value1, string value2) { me.RecordEntry(LogEntryType.Trace, message, value1, value2, null, null); }
        public static void RecordPerformance(string name, string value, TimeSpan duration) { if (AppStats.Current.RecordPerf) me.RecordEntry(LogEntryType.Exception, name, duration.TotalMilliseconds.ToString() + " ms", value, null, null); }
        public static void RecordNewEntry(string Text, string Source, string Query, string LastMiss, int ResultRankId) { me.RecordEntry(LogEntryType.Entry, Text, Source, Query, LastMiss, ResultRankId.ToString()); }
        public static void RecordQuery(string Query, bool IsSuccess, string Previous) { me.RecordEntry(LogEntryType.Query, IsSuccess ? "H" : "M", Query, Previous, null, null); }



		private Dictionary<string,EventData> events = new Dictionary<string,EventData>();
		private EventData GetEvent(string Name) {EventData result=null; if (!events.TryGetValue(Name, out result)) {result = new EventData(Name); events.Add(Name,result);} return result;}

		public static void StartPerformance(string Name)
		{
#if DEBUG
            var Event = me.GetEvent (Name);
			if (Event.LastStart == DateTime.MinValue) Event.LastStart = DateTime.Now;
			else Event.Add("+"); // starting an event thats already in progress
#endif
        }

		public static void EndPerformance(string Name)
		{
#if DEBUG
            var Event = me.GetEvent (Name);
			if (Event.LastStart == DateTime.MinValue) Event.Add ("-"); // ending an event was already ended
			else Event.Add (((int)DateTime.Now.Subtract (Event.LastStart).TotalMilliseconds).ToString ());
			Event.LastStart = DateTime.MinValue;

			#if DEBUG
			Debug(me.PerformanceSummary());
#endif
#endif
        }

		private string PerformanceSummary()
		{
#if DEBUG
            StringBuilder sb = new StringBuilder ();
			foreach (var evnt in events.Values)
				sb.AppendLine (evnt.ToString ());
			return sb.ToString ();
#else
            return "";
#endif
        }

        #region Debug

#if DEBUG
        public void DumpLog()
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(this.GetType());

                    var settings = new XmlWriterSettings { Indent = true };
                    using (var w = XmlWriter.Create(memoryStream, settings))
                        serializer.WriteObject(w, this);

                    memoryStream.Position = 0;
                    string result = new StreamReader(memoryStream).ReadToEnd();

                    Debug(result);
                }
            }
            catch (Exception ex)
            {
                Debug(ex.Message);
            }
        }
#endif

        #endregion

		public static void Push() { MessageQueue.Push(me); me = new SessionLog ();}

    }
}
