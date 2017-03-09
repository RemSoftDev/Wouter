using FoodJournal.Logging;
using FoodJournal.FoodJournalService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FoodJournal.DataModel;
using FoodJournal.AppModel;
using FoodJournal.Model;

namespace FoodJournal.Messages
{
	public class MessageQueue
	{


		private const int RetentionMaxDays = 14;

		private const int LargeMessage = 5000;
		#if ANDROID
		private const int TotalMessageSizeLimit = 20000;
		// 3/26/15, down from 48000, bc Android was puking on large messages
		#else
		private const int TotalMessageSizeLimit = 48000;
		#endif

		private static bool InHere = false;
		private static bool PumpAgain;

		public static void Truncate ()
		{
			try {
				var ExpiredRows = DateTime.Now.Date.AddDays (-1 * RetentionMaxDays); 
				var DeleteCount = MessageQueueDB.DeleteWhere (m => m.Created < ExpiredRows && m.Processed != null);
				if (DeleteCount > 0)
					SessionLog.RecordTraceValue ("Truncating Message Queue Rows", DeleteCount.ToString ());
			} catch (Exception ex) {
				ReportQueueException (ex);
			}
		}

		public static void SendEmail (string to, string subject, string bodyhtml)
		{
			try {
				var email = new EmailOutMessage (to, subject, bodyhtml);
				MessageQueue.Push (email);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		public static void Push (object Message)
		{
			try {

				SessionLog.StartPerformance("Push");

				Truncate ();

				var msg = new MessageQueueRow ();
				msg.MessageType = Message.GetType ().Name;
				msg.IsClientIncomming = false;
				msg.Created = DateTime.Now;
				msg.Message = Serialize (Message);

				MessageQueueDB.Insert (msg);

				StartPump ();

			} catch (Exception ex) {
				ReportQueueException (ex);
			}
		}

		public static void StartPump ()
		{
			Task.Run (() => {
				Pump (Platform.GetConnectionType ());
			});
		}

		private static void Pump (Platform.NetworkConnectionType connectionType)
		{
			try {

				if (connectionType == Platform.NetworkConnectionType.None)
					return;

				if (InHere) {
					PumpAgain = true;
					SessionLog.RecordTrace ("Attempting to pump while already pumping");
					return;
				}

				InHere = true;
				PumpAgain = false;

				var messages = new List<Message> ();
				var infomessages = new List<String> ();

				int totallength = 0;
				foreach (var item in MessageQueueDB.SelectWhere(m => m.Processed == null)) {
					var msg = new Message () {
						Key = item.Id.ToString (),
						MessageType = item.MessageType,
						Body = item.Message
					};
					if (totallength + msg.Body.Length > TotalMessageSizeLimit) {
						if (messages.Count == 0) {
							messages.Add (msg);
							infomessages.Add ("Msg " + msg.Key + " truncated from " + msg.Body.Length.ToString ());
							msg.Body = msg.Body.Substring (0, TotalMessageSizeLimit);
						}
						PumpAgain = true;
						break;
					} else {
						messages.Add (msg);
						totallength += msg.Body.Length;

						#if ANDROID
						try {
							// 3/25/15: no more retry for large messages
							// trying this for Android first, if successfull we may as well apply to WinPhone
							if (msg.Body.Length > LargeMessage) {
								item.Processed = DateTime.Now;
								MessageQueueDB.Update (item);
							}
						} catch (Exception ex) {
							ReportQueueException (ex);
						}
						#endif

					}
				}

				if (messages.Count > 0) {

					foreach (var info in infomessages)
						messages.Add (new Message () { Key = "0", MessageType = "Info", Body = info });

					var svc = Services.NewServiceClient ();
					svc.PushCompleted += (x, y) => {

						InHere = false;

						// mark committed messages as processed in the database
						if (y.Error != null) {
							ReportQueueException (y.Error);

							 

						} else {
							if (y.Result != null)
								try {

									foreach (var result in y.Result) {
										int i;
										if (result.Key.Length > 8)
											ParseLongResult (result.Key);
										else if (int.TryParse (result.Key, out i) && i > 0)
											foreach (var m in MessageQueueDB.SelectWhere(m => m.Id == i)) {
												m.Processed = DateTime.Now;
												MessageQueueDB.Update (m);
											}
									}

								} catch (Exception ex) {
									LittleWatson.ReportException (ex);
								}
						}
						InHere = false;
						if (PumpAgain)
							StartPump ();
						else
							SessionLog.EndPerformance("Push");
						
					};

					var upd = AppStats.Current.CultureSettingsLastUpdated.ToString ("yyyy/MM/dd");
					svc.PushAsync (AppStats.Current.AppInstance, AppStats.Current.Culture, AppStats.Current.Version + "?" + upd, messages);
					return;

				}

			} catch (Exception ex) {
				ReportQueueException (ex);
			}

			InHere = false;

			return;
		}

		private static void ReportQueueException (Exception ex)
		{
			try {
				var svc = Services.NewServiceClient ();
				var msg = new FoodJournalService.Message () { MessageType = "QueueException", Body = ex.Message };
				svc.PushAsync (AppStats.Current.AppInstance, AppStats.Current.Culture, AppStats.Current.Version, new List<Message> () { msg });
			} catch {
			}
		}

		private static void ParseLongResult (string result)
		{
			try {
				AppStats.Current.CultureSettings = (CultureSettings)Deserialize (result, typeof(CultureSettings));
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		//TODO: deal with UTF16?
		private static string Serialize (object obj)
		{
			using (MemoryStream memoryStream = new MemoryStream ()) {
				DataContractSerializer serializer = new DataContractSerializer (obj.GetType ());

				var settings = new XmlWriterSettings { Indent = true };
				using (var w = XmlWriter.Create (memoryStream, settings))
					serializer.WriteObject (w, obj);

				memoryStream.Position = 0;

				return new StreamReader (memoryStream).ReadToEnd ();
			}
		}

		//TODO: deal with UTF16?
		private static object Deserialize (string xml, Type toType)
		{
			using (MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml))) {
				XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader (memoryStream, new XmlDictionaryReaderQuotas ());
				DataContractSerializer serializer = new DataContractSerializer (toType);
				return serializer.ReadObject (reader);
			}
		}
	}
}
