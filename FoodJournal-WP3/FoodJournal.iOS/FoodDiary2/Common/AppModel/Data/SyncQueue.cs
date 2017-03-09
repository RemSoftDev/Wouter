using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FoodJournal.FoodJournalService;
using FoodJournal.Model;

namespace FoodJournal.AppModel.Data
{

	#region Data

	[DataContract]
	public class SyncState
	{

		[DataMember]
		public List<PendingPost> PostQueue = new List<PendingPost> ();

		[DataMember]
		public List<ConnectedApp> Apps = new List<ConnectedApp> ();

		[DataMember]
		public int Skip;
		// used for chunked downloading

		public List<ConnectedApp> GetApps ()
		{
			#if DEBUG

//			if (Apps.Count != 1 || !Apps [0].InstanceGuid.ToString ().StartsWith ("e2c1e9c3")) {
//				Apps.Clear ();
//				Apps.Add (new ConnectedApp () { InstanceGuid = "e2c1e9c3-4dcb-4b23-98b8-b05b981fa977" });// new Guid (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1) });
//			}
			#endif
			if (Apps.Count == 0) {
				Apps.Add (new ConnectedApp () { InstanceGuid = AppStats.Current.AppInstance });// new Guid (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1) });
			}
			return Apps;
		}

	}

	/// <summary>
	/// Contains information about the data for another app instance
	/// </summary>
	[DataContract]
	public class ConnectedApp
	{

		[DataMember]
		public String InstanceGuid;

		[DataMember]
		public DateTime LastestChange = DateTime.MinValue;

		[DataMember]
		public DateTime SetDate = DateTime.MinValue;

	}

	/// <summary>
	/// Stores information about one pending post transaction into local storage
	/// </summary>
	[DataContract]
	public class PendingPost
	{

		public enum SyncState
		{
			pending,
			synced,
			retry,
			abandoned
		}

		[DataMember]
		public string Container;
		[DataMember]
		public string DocumentType;
		[DataMember]
		public string DocumentID;
		[DataMember]
		public SyncState State;
		[DataMember]
		public int Retry = 0;
		[DataMember]
		public DateTime Updated;

		/// <summary>
		/// Note that content isn't stored with the sync state
		/// </summary>
		private string content;

		public string Content {
			set { content = value; }
			get {
				if (content == null)
					content = LocalDB.Read (Container, DocumentType, DocumentID);
				return content;
			}

		}

		public Package ToPackage ()
		{
			return new Package () { Type = DocumentType, Key = DocumentID, Contents = Content, ClientTime = Updated };
		}

	}
	#endregion

	/// <summary>
	/// Sync queue tracks the sync status for local DB records.
	/// </summary>
	public static class SyncQueue
	{

		public const string CONTAINER = "Sync";

		private static readonly AutoResetEvent ThreadSync = new AutoResetEvent (true);

		private const int RetentionMaxDays = 14;
		private const int MAXRETRY = 3;

		private const int LargeMessage = 5000;
		#if ANDROID
		private const int TotalMessageSizeLimit = 20000;
		// 3/26/15, down from 48000, bc Android was puking on large messages
		#else
        private const int TotalMessageSizeLimit = 48000;
#endif

		private static bool InHere = false;
		private static bool Loading = false;
		private static bool PumpAgain;

		private static SyncState state = null;

		private static void LoadState ()
		{
			if (state == null) {
				state = (SyncState)LocalDB.ReadDataContract (CONTAINER, "Set", "All", typeof(SyncState));
				if (state == null)
					state = new SyncState ();
			}
		}

		private static bool Saving = false;
		private static void SaveState ()
		{
			if (state == null)
				return;

			if (Saving)
				return;

			Saving = true;

			// we can now only save state every 2 seconds.
			BackgroundTask.Start (2000, async () => {				
				try {
					ThreadSync.WaitOne (); // Wait till we have a sync signal, and clear the signal
					LocalDB.WriteDataContract (CONTAINER, "Set", "All", state);
				} catch (Exception ex) {
					LittleWatson.ReportException (ex);
				} finally {
					Saving = false;
					ThreadSync.Set (); // Set the signal for the next thread to go if it comes to it 
				}
			});
		}

		public static void Post (string Container, string DocumentType, string DocumentID, string Content)
		{

//			#if DEBUG
//			if (FoodJournal.ViewModels.ScreenshotVM.InScreenshot)
//				return;
//			#endif


			if (Container == CONTAINER)
				return; // Sync state is not synced

			try {
				ThreadSync.WaitOne (); // Wait till we have a sync signal, and clear the signal

				LoadState ();

				state.PostQueue.RemoveAll (x => x.Container == Container && x.DocumentType == DocumentType && x.DocumentID == DocumentID);
				state.PostQueue.Add (new PendingPost () {
					Container = Container,
					DocumentType = DocumentType,
					DocumentID = DocumentID,
					Content = Content,
					State = PendingPost.SyncState.pending,
					Updated = DateTime.Now
				});

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			} finally {
				ThreadSync.Set (); // Set the signal for the next thread to go if it comes to it 
			}

			SaveState ();
			StartPump ();

		}


		public static void StartLoad ()
		{

//			#if DEBUG
//			if (FoodJournal.ViewModels.ScreenshotVM.InScreenshot)
//				return;
//			#endif

			if (Loading)
				return;
			
			Loading = true;

			// save after 1 second
			BackgroundTask.Start (1000, async () => {

				bool GoAgain = false;

				try {

					LoadState ();

					List<DateTime> ChangedDates = new List<DateTime> ();

					String LastSync = "";
					foreach (var app in state.GetApps())
						if (app.InstanceGuid != AppStats.Current.AppInstance) {
							if (LastSync.Length > 0)
								LastSync += "|";
							LastSync += app.InstanceGuid + "=" + app.LastestChange.ToString ("s");
						}


					List<Package> changes = await SyncService.Get (state.Skip, LastSync);
					foreach (var change in changes) {

						if (change.Type == "Continuation")
							GoAgain = true;
						else
							state.Skip++;

						try {

							System.Diagnostics.Debug.WriteLine (string.Format ("Applying remote changes {0} {1} {2}", change.Type, change.Key, change.ClientTime));

							switch(change.Type)
							{ 
							case ("Recent"):
								// this should get re-calculated so skip
								break;
							case ("Continuation"):
								// no work here
								break;
							case ("Day"):

								DateTime date = DateTime.Parse (change.Key);

								FoodJournalNoSQL.LoadDay (date, change.Contents);

								if (!ChangedDates.Contains (date))
									ChangedDates.Add (date);

								break;
							case ("Items"): 

								FoodJournalNoSQL.LoadItems(true, change.Contents);
								break;

							case ("UserSettings"):

								UserSettings.ReplaceWith(change.Contents);

								break;
							case ("AppStats"):

								if (change.Contents.Contains("<InstalledProductKind>Paid</InstalledProductKind>"))
									AppStats.Current.RegisterPurchase("Reinstall");
									
								break;
							default:
								System.Diagnostics.Debug.WriteLine (string.Format ("Not applying remote changes {0} {1} {2}", change.Type, change.Key, change.ClientTime));
								break;
							}

						} catch (Exception ex) {
							LittleWatson.ReportException (ex, string.Format ("Applying changes {0} {1} {2}", change.Type, change.Key, change.ClientTime));
						}

						bool found = false;
						foreach (var app in state.Apps)
							if (app.InstanceGuid == change.AppInstance)
							{
								found = true;
								app.SetDate = change.ClientTime;
							}

						if (!found)
							state.Apps.Add(new ConnectedApp() {InstanceGuid = change.AppInstance, LastestChange= DateTime.MinValue, SetDate = change.ClientTime});

					}

					if (changes.Count == 0 || !GoAgain) {
						state.Skip = 0;
						foreach (var app in state.Apps)
							app.LastestChange = app.SetDate;
					}								
					foreach (DateTime date in ChangedDates)
						FoodJournalNoSQL.StartSaveDayFull (date);

					SaveState ();

				} catch (Exception ex) {
					LittleWatson.ReportException (ex, "Saving state");
				} finally {
					Loading = false;
				}

				if (GoAgain)
					StartLoad ();

			}
			);

		}

		public static void StartPump ()
		{
			Task.Run (() => {
				Pump (Platform.GetConnectionType ());
			});
		}

		private static void Pump (Platform.NetworkConnectionType connectionType)
		{

//			#if DEBUG
//			if (FoodJournal.ViewModels.ScreenshotVM.InScreenshot)
//				return;
//			#endif

			try {

				if (connectionType == Platform.NetworkConnectionType.None)
					return;

				if (InHere) {
					PumpAgain = true;
					SessionLog.RecordTrace ("Attempting to sync while already syncing");
					return;
				}

				InHere = true;
				PumpAgain = false;

				var messages = new List<Package> ();
				var working = new List<PendingPost> ();
				List<PendingPost> PostQueue = null;

				try {
					ThreadSync.WaitOne (); // Wait till we have a sync signal, and clear the signal
					PostQueue = state.PostQueue.Where (x => x.State == PendingPost.SyncState.pending || x.State == PendingPost.SyncState.retry).ToList ();
				} catch (Exception ex) {
					LittleWatson.ReportException (ex);
				} finally {
					ThreadSync.Set (); // Set the signal for the next thread to go if it comes to it 
				}

				int totallength = 0;
				foreach (var item in PostQueue) {
					var pkg = item.ToPackage ();
					if (messages.Count > 0 && totallength + pkg.Contents.Length > TotalMessageSizeLimit) {
						PumpAgain = true;
						break; 
					} else {
						messages.Add (pkg);
						working.Add (item);
						totallength += pkg.Contents.Length;

						System.Diagnostics.Debug.WriteLine (string.Format ("Posting {0} {1} {2}", pkg.Type, pkg.Key, pkg.ClientTime));

					}
				}

				if (messages.Count > 0) {

					try {

						SyncService.Post (messages);

					} catch (Exception ex) {
						
						ReportQueueException (ex);

						foreach (var i in working.ToArray()) {
							if (i.Retry >= MAXRETRY) {
								//i.State = PendingPost.SyncState.abandoned;
								state.PostQueue.Remove (i);
							} else {
								i.State = PendingPost.SyncState.retry;
								i.Retry++;
								working.Remove (i);
							}
						}

					}

					try {
						ThreadSync.WaitOne (); // Wait till we have a sync signal, and clear the signal
						foreach (var i in working)
							state.PostQueue.Remove (i);
					} catch (Exception ex) {
						LittleWatson.ReportException (ex);
					} finally {
						ThreadSync.Set (); // Set the signal for the next thread to go if it comes to it 
					}


					SaveState ();

				}

			} catch (Exception ex) {
				ReportQueueException (ex);
			}

			InHere = false;

			if (PumpAgain)
				StartPump ();

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
	}

}