using System;
using Android.App;
using Android.Content;
using Android.Preferences;
using FoodJournal.Resources;
using Android.Support.V4.App;
using FoodJournal.Android15.Activities;
using Android.OS;
using FoodJournal.Logging;

namespace FoodJournal.Android15
{

	// scheduled task to run: show notification
	[BroadcastReceiver]
	public class AlarmReceiver : BroadcastReceiver
	{

		public static String GetRingtone ()
		{
			return Android.Provider.Settings.System.DefaultNotificationUri.ToString ();
//			var sp = PreferenceManager.GetDefaultSharedPreferences(this);
//			return sp.GetString(RemindMe.RINGTONE_PREF, DEFAULT_NOTIFICATION_URI.toString());
		}

		override public void OnReceive (Context context, Intent intent)
		{

			try {
				Reminder r = Reminder.FromIntent (intent);

				Intent resultIntent = new Intent (context, typeof(Splash));
				PendingIntent resultPendingIntent = PendingIntent.GetActivity (context, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
//			//resultIntent.PutExtras(valuesForActivity); // Pass some values to SecondActivity.
//			TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
//			stackBuilder.AddParentStack(Class.FromType(typeof(Splash)));
//			stackBuilder.AddNextIntent(resultIntent);
//
//			PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);
				//	PendingIntent pi = PendingIntent.GetBroadcast (this, 0, i, PendingIntentFlags.UpdateCurrent);
//			// Build the notification
				NotificationCompat.Builder builder = new NotificationCompat.Builder (context)
				.SetAutoCancel (true) // dismiss the notification from the notification area when the user clicks on it
				.SetContentIntent (resultPendingIntent) // start up this activity when the user clicks the intent.
				.SetContentTitle (r.Title) // Set the title
				.SetDefaults ((int)(NotificationDefaults.Vibrate | NotificationDefaults.Sound))
//				.SetSound(Android.Net.Uri.Parse (GetRingtone ()))
//				.SetLargeIcon(Android.Graphics.Bitmap. Resource.Drawable.ic_launcher)
					//.SetLargeIcon (Resource.Drawable.ic_icon) // This is the icon to display
					.SetSmallIcon (Resource.Drawable.ic_icon) // This is the icon to display
				.SetContentText (r.Content); // the message to display.
					
//			Notification n = new Notification (Resource.Drawable.ic_launcher, r.Title, Java.Lang.JavaSystem.CurrentTimeMillis ());
//			PendingIntent pi = PendingIntent.GetActivity (context, 0, new Intent(context, typeof(Splash)), 0);
//
//			n.SetLatestEventInfo (context, r.Title, r.Content, pi);
//			// TODO check user preferences
//			n.Defaults |= NotificationDefaults.Vibrate;
//			n.Sound = Android.Net.Uri.Parse (GetRingtone ());
//			//      n.defaults |= Notification.DEFAULT_SOUND;       
//			n.Flags |= NotificationFlags.AutoCancel;       

				NotificationManager nm = (NotificationManager)
				context.GetSystemService (Context.NotificationService);

				int id = 0;
				nm.Notify ((int)id, builder.Build ());

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

	}


	// Setter receives the "boot completed" broadcast
	[BroadcastReceiver]
	[IntentFilter (new[]{ Intent.ActionBootCompleted })]
	public class AlarmSetter : BroadcastReceiver
	{

		override public void OnReceive (Context context, Intent intent)
		{
			Intent service = new Intent (context, typeof(AlarmService));
			service.SetAction (AlarmService.UPDATE);
			context.StartService (service);
		}

	}

	// Service used to Schedule or Cancel the Alarm (ie, "scheduled task")
	[Service]
	public class AlarmService : IntentService
	{

		private static String CREATE = "CREATE";
		private static String CANCEL = "CANCEL";
		public static String UPDATE = "UPDATE";

		private IntentFilter matcher;

		public AlarmService ()
		{
			//base(TAG);
			matcher = new IntentFilter ();
			matcher.AddAction (CREATE);
			matcher.AddAction (CANCEL);
			matcher.AddAction (UPDATE);
		}

		public static void StartCreate (Reminder r)
		{

			Context context = Application.Context;
			Intent service = new Intent (context, typeof(AlarmService));
			service.SetAction (CREATE);
			service.SetType (r.Name);
			r.ToIntent (service);
			context.StartService (service);
		}

		public static void StartCancel (string name)
		{
			Context context = Application.Context;
			Intent service = new Intent (context, typeof(AlarmService));
			service.SetAction (CANCEL);
			service.SetType (name);
			context.StartService (service);
		}

		public static void ResetNotifications()
		{
			Context context = Application.Context;
			NotificationManager nm = (NotificationManager)
				context.GetSystemService (Context.NotificationService);
			nm.CancelAll();
		}

		override protected void OnHandleIntent (Intent intent)
		{
			String action = intent.Action;

			if (matcher.MatchAction (action)) {          
				execute (action, intent);
			}
		}

		private void execute (String action, Intent thisintent)
		{
			try {
				if (action == UPDATE) {
					// querying UserSettings will refresh reminders
					var settings = UserSettings.Current;
				} else {

					AlarmManager am = (AlarmManager)GetSystemService (Context.AlarmService);

					Intent i = new Intent (this, typeof(AlarmReceiver));
					i.SetType (thisintent.Type);
					Reminder r = Reminder.FromIntent (thisintent);
					r.ToIntent (i);

					PendingIntent pi = PendingIntent.GetBroadcast (this, 0, i, 
						                  PendingIntentFlags.UpdateCurrent);

					if (CREATE.Equals (action))
					{
						//DateTime firstone = FoodJournal.Extensions.DateTimeExtensions.Combine (DateTime.Now, r.BeginTime);
						//if (firstone < DateTime.Now)
						//	firstone = firstone.AddDays (1);
						//long time = Java.Lang.JavaSystem.CurrentTimeMillis () + 1000 * 5; 
						long time = ((long)SystemClock.ElapsedRealtime ()) + ((long)r.BeginTime.Subtract (DateTime.Now).TotalMilliseconds);
						am.SetRepeating (AlarmType.ElapsedRealtimeWakeup, time, AlarmManager.IntervalDay, pi);
					//	am.Set (AlarmType.RtcWakeup, time, pi);
					} else if (CANCEL.Equals (action)) {
						am.Cancel (pi);
					}
				}
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

	}


}

